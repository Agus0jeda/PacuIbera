using Microsoft.Data.SqlClient;
using PacuIbera.Dominio;
using System.Data;

namespace PacuIbera.Datos
{
    public sealed partial class RemitosDatos
    {
        public static bool PuedePrepararResolucion => SesionActiva.IdUsuario > 0 && SesionActiva.Rol == "Administrador";
        public static IReadOnlyList<string> TiposResolucion { get; } = Array.AsReadOnly(new[]
        {
            "Recepción posterior", "Devolución depósito", "Pérdida documentada"
        });

        // Solo SELECT: valida el usuario real y devuelve el dia de negocio del servidor.
        public async Task<DateTime> ValidarAdministradorActivoAsync()
        {
            if (!PuedePrepararResolucion) throw new UnauthorizedAccessException("Solo Administrador puede preparar resoluciones.");
            using var conexion = ObtenerConexion();
            using var cmd = new SqlCommand(@"
                SELECT CONVERT(date,SYSDATETIME()) WHERE EXISTS
                (SELECT 1 FROM dbo.Usuario u INNER JOIN dbo.Rol r ON r.Id=u.RolId
                 WHERE u.Id=@UsuarioId AND u.Activo=1 AND r.Nombre=N'Administrador');", conexion);
            cmd.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = SesionActiva.IdUsuario;
            await conexion.OpenAsync();
            if (await cmd.ExecuteScalarAsync() is not DateTime fecha)
                throw new UnauthorizedAccessException("El usuario debe estar activo y tener rol Administrador.");
            return fecha.Date;
        }

        public async Task<List<RemitoInterno>> ListarRemitosConDiferenciasAsync()
        {
            await ValidarAdministradorActivoAsync();
            using var conexion = ObtenerConexion();
            using var cmd = new SqlCommand(Cabecera.Replace("{BLOQUEO}", "") + @"
                AND ri.Estado=N'Con diferencias'
                AND EXISTS (SELECT 1 FROM dbo.Usuario u INNER JOIN dbo.Rol r ON r.Id=u.RolId
                            WHERE u.Id=@UsuarioId AND u.Activo=1 AND r.Nombre=N'Administrador')
                AND EXISTS
                (SELECT 1 FROM dbo.RecepcionRemitoLote rr
                 INNER JOIN dbo.RecepcionRemito rec ON rec.Id=rr.RecepcionRemitoId AND rec.RemitoId=rr.RemitoId
                 WHERE rr.RemitoId=ri.Id AND rec.EstadoResultado=N'Con diferencias'
                   AND rr.CantidadDiferencia > ISNULL(
                       (SELECT SUM(d.Cantidad) FROM dbo.ResolucionDiferenciaDetalle d
                        INNER JOIN dbo.ResolucionDiferencia h ON h.Id=d.ResolucionId AND h.RemitoId=d.RemitoId
                        WHERE d.RecepcionRemitoLoteId=rr.Id AND d.RemitoId=ri.Id AND h.Estado=N'Aplicada'),0))
                ORDER BY ri.Id;", conexion);
            AgregarUsuario(cmd);
            await conexion.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            var resultado = new List<RemitoInterno>();
            while (await reader.ReadAsync()) resultado.Add(LeerCabecera(reader));
            return resultado;
        }

        public async Task<(DetalleRemitoInterno Remito, DateTime FechaServidor)> ObtenerRemitoParaResolucionAsync(int remitoId)
        {
            await ValidarAdministradorActivoAsync();
            var detalle = await ObtenerDetalleAsync(remitoId);
            if (detalle.Cabecera.Estado != "Con diferencias" || !detalle.Cabecera.FechaRecepcion.HasValue
                || detalle.Lotes.Count == 0 || detalle.Lotes.Any(l => !l.RecepcionRemitoLoteId.HasValue
                    || !l.Recibido.HasValue || !l.DiferenciaOriginal.HasValue || !l.Pendiente.HasValue))
                throw new InvalidOperationException("El remito no tiene una recepción definitiva con diferencias válidas.");
            if (detalle.Lotes.Any(l => l.Resuelto < 0 || l.Pendiente < 0
                || l.Enviado - l.Recibido!.Value != l.DiferenciaOriginal!.Value
                || l.DiferenciaOriginal!.Value - l.Resuelto != l.Pendiente!.Value))
                throw new InvalidOperationException("Las diferencias históricas no concilian. No se corregirán automáticamente.");
            if (!detalle.Lotes.Any(l => l.Pendiente > 0))
                throw new InvalidOperationException("El remito no tiene diferencias todavía pendientes.");
            return (detalle, await ValidarAdministradorActivoAsync());
        }

        internal static DataTable ConstruirResolucionEntrada(DetalleRemitoInterno origen,
            IEnumerable<ResolucionRemitoBorrador> registros, string tipo, string motivo, DateTime diaServidor)
        {
            if (origen.Cabecera.Id <= 0 || origen.Cabecera.Estado != "Con diferencias" || !origen.Cabecera.FechaRecepcion.HasValue)
                throw new InvalidOperationException("El remito no está disponible para resolver diferencias.");
            if (!TiposResolucion.Contains(tipo, StringComparer.Ordinal)) throw new InvalidOperationException("Seleccione un tipo de resolución válido.");
            if (string.IsNullOrWhiteSpace(motivo) || motivo.Length > 500)
                throw new InvalidOperationException("El motivo es obligatorio y admite hasta 500 caracteres.");
            var filas = registros.ToList();
            if (filas.Count == 0 || filas.Select(f => f.RecepcionRemitoLoteId).Distinct().Count() != filas.Count)
                throw new InvalidOperationException("Agregue al menos una diferencia, sin duplicados.");
            var originales = origen.Lotes.Where(l => l.RecepcionRemitoLoteId.HasValue)
                .ToDictionary(l => l.RecepcionRemitoLoteId!.Value);
            foreach (var f in filas)
            {
                if (!originales.TryGetValue(f.RecepcionRemitoLoteId, out var l)
                    || l.Id != f.RemitoLoteId || l.DepositoStockId != f.DepositoStockId
                    || l.SeVendePorPeso != f.SeVendePorPeso || l.Vencimiento != f.Vencimiento
                    || l.Enviado != f.Enviado || l.Recibido != f.Recibido || l.Resuelto != f.Resuelto || l.Pendiente != f.Pendiente)
                    throw new InvalidOperationException("Cambió una diferencia o su trazabilidad. Actualice el remito antes de revisar.");
                if (f.Cantidad <= 0 || f.Cantidad > f.Pendiente || f.Cantidad > 9999999.999M
                    || decimal.Round(f.Cantidad, 3) != f.Cantidad
                    || (!f.SeVendePorPeso && new[] { f.Cantidad, f.Enviado, f.Recibido, f.Resuelto, f.Pendiente }.Any(v => decimal.Truncate(v) != v)))
                    throw new InvalidOperationException($"{f.Producto}, lote {f.RemitoLoteId}: revise cantidad, pendiente y unidad de venta.");
                if (tipo == "Recepción posterior" && f.Vencimiento.HasValue && f.Vencimiento.Value.Date < diaServidor.Date)
                    throw new InvalidOperationException($"{f.Producto}: un lote vencido no puede ingresar al stock vendible.");
            }
            var tabla = new DataTable();
            var id = tabla.Columns.Add("RecepcionRemitoLoteId", typeof(int));
            tabla.Columns.Add("Cantidad", typeof(decimal));
            tabla.PrimaryKey = new[] { id };
            foreach (var f in filas) tabla.Rows.Add(f.RecepcionRemitoLoteId, f.Cantidad);
            return tabla;
        }

        // Solo construye el comando. NO abre la conexion ni llama Execute*.
        // La ejecucion protegida esta en ConfirmarResolucionRemitoAsync.
        internal static SqlCommand PrepararComandoResolucion(SqlConnection conexion, Guid claveOperacion,
            DetalleRemitoInterno origen, IEnumerable<ResolucionRemitoBorrador> filas, string tipo, string motivo, DateTime diaServidor)
        {
            if (!PuedePrepararResolucion) throw new UnauthorizedAccessException("Solo Administrador puede resolver diferencias.");
            if (claveOperacion == Guid.Empty) throw new ArgumentException("La clave de operación no puede estar vacía.");
            var entrada = ConstruirResolucionEntrada(origen, filas, tipo, motivo, diaServidor);
            var cmd = new SqlCommand("dbo.sp_ResolverDiferenciaRemito", conexion) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.Add("@RemitoId", SqlDbType.Int).Value = origen.Cabecera.Id;
            cmd.Parameters.Add("@ClaveOperacion", SqlDbType.UniqueIdentifier).Value = claveOperacion;
            cmd.Parameters.Add("@UsuarioResponsableId", SqlDbType.Int).Value = SesionActiva.IdUsuario;
            cmd.Parameters.Add("@Tipo", SqlDbType.NVarChar, -1).Value = tipo;
            cmd.Parameters.Add("@Motivo", SqlDbType.NVarChar, -1).Value = motivo;
            cmd.Parameters.Add(new SqlParameter("@Lotes", SqlDbType.Structured)
            { TypeName = "dbo.ResolucionDiferenciaEntrada", Value = entrada });
            return cmd;
        }
    }
}
