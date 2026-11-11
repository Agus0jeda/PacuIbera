using Datos;
using Microsoft.Data.SqlClient;
using PacuIbera.Dominio;
using System.Data;

namespace PacuIbera.Datos
{
    // Consultas y borradores. La confirmacion de recepcion se implementa en su archivo parcial.
    public sealed partial class RemitosDatos : ConexionBD
    {
        public static bool PuedeConsultar => SesionActiva.IdUsuario > 0
            && SesionActiva.Rol is "Administrador" or "Vendedor" or "Gerente";
        public static bool PuedePrepararEmision => SesionActiva.IdUsuario > 0 && SesionActiva.Rol == "Administrador";
        public static bool PuedePrepararRecepcion => SesionActiva.IdUsuario > 0 && SesionActiva.Rol == "Vendedor";

        public async Task ValidarConsultaRemitosActivaAsync()
        {
            if (!PuedeConsultar) throw new UnauthorizedAccessException("Su rol no puede consultar remitos.");
            using var conexion = ObtenerConexion();
            using var cmd = new SqlCommand(@"
                SELECT COUNT(*) FROM dbo.Usuario u JOIN dbo.Rol r ON r.Id=u.RolId
                WHERE u.Id=@UsuarioId AND u.Activo=1 AND r.Nombre=@Rol
                  AND r.Nombre IN (N'Administrador',N'Vendedor',N'Gerente');", conexion);
            cmd.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = SesionActiva.IdUsuario;
            cmd.Parameters.Add("@Rol", SqlDbType.NVarChar, 30).Value = SesionActiva.Rol;
            await conexion.OpenAsync();
            if (Convert.ToInt32(await cmd.ExecuteScalarAsync()) != 1)
                throw new UnauthorizedAccessException("El usuario debe estar activo y tener permiso para consultar remitos.");
        }

        public async Task ValidarVendedorActivoAsync()
        {
            if (!PuedePrepararRecepcion) throw new UnauthorizedAccessException("Solo Vendedor puede preparar recepciones de remitos.");
            using var conexion = ObtenerConexion();
            using var cmd = new SqlCommand(@"
                SELECT COUNT(*) FROM dbo.Usuario u INNER JOIN dbo.Rol r ON r.Id=u.RolId
                WHERE u.Id=@UsuarioId AND u.Activo=1 AND r.Nombre=N'Vendedor';", conexion);
            cmd.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = SesionActiva.IdUsuario;
            await conexion.OpenAsync();
            if (Convert.ToInt32(await cmd.ExecuteScalarAsync()) != 1)
                throw new UnauthorizedAccessException("El usuario debe estar activo y tener rol Vendedor.");
        }

        public async Task<List<RemitoInterno>> ListarPendientesRecepcionAsync()
        {
            await ValidarVendedorActivoAsync();
            using var conexion = ObtenerConexion();
            using var cmd = new SqlCommand(Cabecera.Replace("{BLOQUEO}", "") + @"
                AND ri.Estado=N'En tránsito'
                AND NOT EXISTS (SELECT 1 FROM dbo.RecepcionRemito r WHERE r.RemitoId=ri.Id)
                AND EXISTS (SELECT 1 FROM dbo.Usuario u INNER JOIN dbo.Rol rol ON rol.Id=u.RolId
                            WHERE u.Id=@UsuarioId AND u.Activo=1 AND rol.Nombre=N'Vendedor')
                ORDER BY ri.Id;", conexion);
            AgregarUsuario(cmd);
            await conexion.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            var resultado = new List<RemitoInterno>();
            while (await reader.ReadAsync()) resultado.Add(LeerCabecera(reader));
            return resultado;
        }

        public async Task<DateTime> ObtenerFechaServidorRecepcionAsync()
        {
            if (!PuedePrepararRecepcion) throw new UnauthorizedAccessException("Solo Vendedor puede revisar una recepción.");
            using var conexion = ObtenerConexion();
            using var cmd = new SqlCommand(@"
                SELECT CONVERT(date,SYSDATETIME()) WHERE EXISTS
                (SELECT 1 FROM dbo.Usuario u INNER JOIN dbo.Rol r ON r.Id=u.RolId
                 WHERE u.Id=@UsuarioId AND u.Activo=1 AND r.Nombre=N'Vendedor');", conexion);
            cmd.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = SesionActiva.IdUsuario;
            await conexion.OpenAsync();
            object? fecha = await cmd.ExecuteScalarAsync();
            if (fecha is not DateTime dia)
                throw new UnauthorizedAccessException("El usuario debe seguir activo y tener rol Vendedor.");
            return dia.Date;
        }

        public async Task<(DetalleRemitoInterno Remito, DateTime FechaServidor)> ObtenerRemitoParaRecepcionAsync(int remitoId)
        {
            await ValidarVendedorActivoAsync();
            var detalle = await ObtenerDetalleAsync(remitoId);
            if (detalle.Cabecera.Estado != "En tránsito" || !detalle.Cabecera.FechaEmision.HasValue
                || detalle.Cabecera.FechaRecepcion.HasValue || detalle.Lotes.Count == 0
                || detalle.Lotes.Any(l => l.RecepcionRemitoLoteId.HasValue))
                throw new InvalidOperationException("El remito no está pendiente de recepción o no tiene lotes enviados.");
            if (detalle.Lotes.Any(l => l.Enviado <= 0 || l.Enviado > 9999999.999M
                || (!l.SeVendePorPeso && decimal.Truncate(l.Enviado) != l.Enviado)))
                throw new InvalidOperationException("Hay cantidades enviadas incompatibles con la unidad de venta. Revise el remito antes de recibir.");
            var dia = await ObtenerFechaServidorRecepcionAsync();
            return (detalle, dia);
        }

        internal static DataTable ConstruirLotesRecepcionEntrada(DetalleRemitoInterno origen,
            IEnumerable<LoteRecepcionRemitoBorrador> registros, DateTime diaServidor)
        {
            var filas = registros.ToList();
            if (origen.Cabecera.Id <= 0 || origen.Cabecera.Estado != "En tránsito" || !origen.Cabecera.FechaEmision.HasValue
                || origen.Cabecera.FechaRecepcion.HasValue || origen.Lotes.Count == 0
                || origen.Lotes.Any(l => l.RecepcionRemitoLoteId.HasValue))
                throw new InvalidOperationException("El remito no está pendiente de recepción.");
            if (filas.Count != origen.Lotes.Count || filas.Select(l => l.RemitoLoteId).Distinct().Count() != filas.Count)
                throw new InvalidOperationException("Debe representar todos los lotes enviados, una sola vez.");
            var originales = origen.Lotes.ToDictionary(l => l.Id);
            foreach (var fila in filas)
            {
                if (!originales.TryGetValue(fila.RemitoLoteId, out var lote)
                    || fila.RemitoDetalleId != lote.RemitoDetalleId || fila.DepositoStockId != lote.DepositoStockId
                    || fila.Enviado != lote.Enviado || fila.SeVendePorPeso != lote.SeVendePorPeso || fila.Vencimiento != lote.Vencimiento)
                    throw new InvalidOperationException("No se pueden cambiar los identificadores ni los datos originales de un lote.");
                if (fila.Enviado <= 0 || fila.Enviado > 9999999.999M || fila.Recibido < 0 || fila.Recibido > fila.Enviado
                    || decimal.Round(fila.Recibido, 3) != fila.Recibido
                    || (!fila.SeVendePorPeso && (decimal.Truncate(fila.Recibido) != fila.Recibido || decimal.Truncate(fila.Enviado) != fila.Enviado)))
                    throw new InvalidOperationException($"{fila.Producto}, lote {fila.RemitoLoteId}: revise cantidad recibida y unidad de venta.");
                if (fila.Vencimiento.HasValue && fila.Vencimiento.Value.Date < diaServidor.Date && fila.Recibido != 0)
                    throw new InvalidOperationException($"{fila.Producto}, lote {fila.RemitoLoteId}: está vencido; indique recibido cero.");
            }
            var tabla = new DataTable();
            var id = tabla.Columns.Add("RemitoLoteId", typeof(int));
            tabla.Columns.Add("CantidadRecibida", typeof(decimal));
            tabla.PrimaryKey = new[] { id };
            foreach (var fila in filas) tabla.Rows.Add(fila.RemitoLoteId, fila.Recibido);
            return tabla;
        }

        // Construye el comando; la ejecucion protegida esta en ConfirmarRecepcionRemitoAsync.
        internal static SqlCommand PrepararComandoRecepcionRemito(SqlConnection conexion, Guid claveOperacion,
            DetalleRemitoInterno origen, IEnumerable<LoteRecepcionRemitoBorrador> registros, DateTime diaServidor)
        {
            if (!PuedePrepararRecepcion) throw new UnauthorizedAccessException("Solo Vendedor puede preparar la recepción.");
            if (claveOperacion == Guid.Empty) throw new ArgumentException("La clave de operación no puede estar vacía.");
            var entrada = ConstruirLotesRecepcionEntrada(origen, registros, diaServidor);
            var cmd = new SqlCommand("dbo.sp_ConfirmarRecepcionRemito", conexion) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.Add("@RemitoId", SqlDbType.Int).Value = origen.Cabecera.Id;
            cmd.Parameters.Add("@ClaveOperacion", SqlDbType.UniqueIdentifier).Value = claveOperacion;
            cmd.Parameters.Add("@UsuarioReceptorId", SqlDbType.Int).Value = SesionActiva.IdUsuario;
            cmd.Parameters.Add(new SqlParameter("@Lotes", SqlDbType.Structured)
            {
                TypeName = "dbo.RecepcionRemitoLoteEntrada", Value = entrada
            });
            return cmd;
        }

        public async Task<List<ProductoDisponibleRemito>> ObtenerProductosEmisionAsync()
        {
            if (!PuedePrepararEmision) throw new UnauthorizedAccessException("Solo Administrador puede preparar remitos.");
            using var conexion = ObtenerConexion();
            using var cmd = new SqlCommand(@"
                SELECT p.Id,p.Nombre,p.SeVendePorPeso,ISNULL(SUM(ds.StockActual),0) AS Disponible
                FROM dbo.Producto p LEFT JOIN dbo.DepositoStock ds ON ds.ProductoId=p.Id AND ds.Activo=1
                    AND ds.StockActual>0 AND (ds.FechaVencimiento IS NULL OR ds.FechaVencimiento>=CONVERT(date,SYSDATETIME()))
                WHERE p.Activo=1 AND EXISTS
                    (SELECT 1 FROM dbo.Usuario u INNER JOIN dbo.Rol r ON r.Id=u.RolId
                     WHERE u.Id=@UsuarioId AND u.Activo=1 AND r.Nombre=N'Administrador')
                GROUP BY p.Id,p.Nombre,p.SeVendePorPeso ORDER BY p.Nombre,p.Id;", conexion);
            cmd.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = SesionActiva.IdUsuario;
            await conexion.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            var productos = new List<ProductoDisponibleRemito>();
            while (await reader.ReadAsync())
                productos.Add(new(reader.GetInt32(0), reader.GetString(1), reader.GetBoolean(2), reader.GetDecimal(3)));
            return productos;
        }

        internal static DataTable ConstruirProductosEntrada(IEnumerable<ProductoEmisionRemito> productos)
        {
            var filas = productos.ToList();
            if (filas.Count == 0) throw new InvalidOperationException("Agregue al menos un producto.");
            if (filas.Select(p => p.ProductoId).Distinct().Count() != filas.Count)
                throw new InvalidOperationException("No se permiten productos duplicados.");
            foreach (var p in filas)
            {
                if (p.ProductoId <= 0 || p.Cantidad <= 0 || p.Cantidad > 9999999.999M
                    || p.Cantidad > p.StockDisponible || decimal.Round(p.Cantidad, 3) != p.Cantidad
                    || (!p.SeVendePorPeso && (decimal.Truncate(p.Cantidad) != p.Cantidad
                        || decimal.Truncate(p.StockDisponible) != p.StockDisponible)))
                    throw new InvalidOperationException($"{p.Producto}: revise cantidad, unidad de venta y stock disponible.");
            }
            var tabla = new DataTable();
            var id = tabla.Columns.Add("ProductoId", typeof(int));
            tabla.Columns.Add("Cantidad", typeof(decimal));
            tabla.PrimaryKey = new[] { id };
            foreach (var p in filas) tabla.Rows.Add(p.ProductoId, p.Cantidad);
            return tabla;
        }

        // Construye el comando. EmitirRemitoAsync verifica destino y persiste el intento antes de ejecutarlo.
        internal static SqlCommand PrepararComandoEmision(SqlConnection conexion, Guid claveOperacion,
            IEnumerable<ProductoEmisionRemito> productos)
        {
            if (!PuedePrepararEmision) throw new UnauthorizedAccessException("Solo Administrador puede emitir remitos.");
            if (claveOperacion == Guid.Empty) throw new ArgumentException("La clave de operación no puede estar vacía.");
            var tabla = ConstruirProductosEntrada(productos);
            var cmd = new SqlCommand("dbo.sp_EmitirRemitoInterno", conexion) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.Add("@ClaveOperacion", SqlDbType.UniqueIdentifier).Value = claveOperacion;
            cmd.Parameters.Add("@UsuarioEmisorId", SqlDbType.Int).Value = SesionActiva.IdUsuario;
            cmd.Parameters.Add(new SqlParameter("@Productos", SqlDbType.Structured)
            {
                TypeName = "dbo.RemitoProductoEntrada", Value = tabla
            });
            return cmd;
        }

        // No existe asignacion por vendedor ni sucursales: todos los emitidos van al unico local.
        private const string Acceso = @"
            EXISTS (SELECT 1 FROM dbo.Usuario lector INNER JOIN dbo.Rol rol ON rol.Id=lector.RolId
                    WHERE lector.Id=@UsuarioId AND lector.Activo=1
                      AND (rol.Nombre IN (N'Administrador',N'Gerente') OR (rol.Nombre=N'Vendedor'
                           AND ri.FechaEmision IS NOT NULL
                           AND ri.Estado IN (N'En tránsito',N'Recibido',N'Con diferencias',N'Cerrado'))))";

        private const string Cabecera = @"
            SELECT ri.Id,ri.Estado,ri.FechaCreacion,ri.FechaEmision,ri.FechaCancelacion,ri.MotivoCancelacion,
                LTRIM(RTRIM(CONCAT(creador.Nombre,' ',creador.Apellido))) AS Creador,
                LTRIM(RTRIM(CONCAT(emisor.Nombre,' ',emisor.Apellido))) AS Emisor,
                LTRIM(RTRIM(CONCAT(receptor.Nombre,' ',receptor.Apellido))) AS Receptor,
                LTRIM(RTRIM(CONCAT(cancelador.Nombre,' ',cancelador.Apellido))) AS Cancelador,
                recep.FechaRecepcion
            FROM dbo.RemitoInterno ri {BLOQUEO}
            INNER JOIN dbo.Usuario creador ON creador.Id=ri.UsuarioCreadorId
            LEFT JOIN dbo.Usuario emisor ON emisor.Id=ri.UsuarioEmisorId
            LEFT JOIN dbo.Usuario cancelador ON cancelador.Id=ri.UsuarioCanceladorId
            LEFT JOIN dbo.RecepcionRemito recep ON recep.RemitoId=ri.Id
            LEFT JOIN dbo.Usuario receptor ON receptor.Id=recep.UsuarioReceptorId
            WHERE " + Acceso;

        private static void AgregarUsuario(SqlCommand cmd)
        {
            if (!PuedeConsultar) throw new UnauthorizedAccessException("Su rol no tiene acceso a la consulta de remitos.");
            cmd.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = SesionActiva.IdUsuario;
        }

        public async Task<List<RemitoInterno>> ListarAsync(string? estado = null, int? numero = null)
        {
            using var conexion = ObtenerConexion();
            using var cmd = new SqlCommand(Cabecera.Replace("{BLOQUEO}", "") + @"
                AND (@Estado IS NULL OR ri.Estado=@Estado) AND (@Numero IS NULL OR ri.Id=@Numero)
                ORDER BY ri.Id DESC;", conexion);
            AgregarUsuario(cmd);
            cmd.Parameters.Add("@Estado", SqlDbType.NVarChar, 30).Value = (object?)estado ?? DBNull.Value;
            cmd.Parameters.Add("@Numero", SqlDbType.Int).Value = (object?)numero ?? DBNull.Value;
            await conexion.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            var resultado = new List<RemitoInterno>();
            while (await reader.ReadAsync()) resultado.Add(LeerCabecera(reader));
            return resultado;
        }

        public async Task<DetalleRemitoInterno> ObtenerDetalleAsync(int remitoId)
        {
            using var conexion = ObtenerConexion();
            await conexion.OpenAsync();
            using var transaccion = conexion.BeginTransaction();
            // Lectura coherente con los SP: la cabecera UPDLOCK/HOLDLOCK impide que
            // cambie este remito entre los SELECT. No se modifica ninguna fila.
            using var cmd = new SqlCommand(Cabecera.Replace("{BLOQUEO}", "WITH (UPDLOCK,HOLDLOCK)") + @"
                AND ri.Id=@RemitoId;", conexion, transaccion);
            AgregarUsuario(cmd);
            cmd.Parameters.Add("@RemitoId", SqlDbType.Int).Value = remitoId;
            DetalleRemitoInterno resultado;
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (!await reader.ReadAsync())
                    throw new InvalidOperationException("El remito no existe o no está disponible para su usuario.");
                resultado = new DetalleRemitoInterno { Cabecera = LeerCabecera(reader) };
            }
            cmd.CommandText = @"
                SELECT rd.Id,rd.ProductoId,p.Nombre AS Producto,p.SeVendePorPeso,rd.CantidadEnviada
                FROM dbo.RemitoDetalle rd INNER JOIN dbo.Producto p ON p.Id=rd.ProductoId
                WHERE rd.RemitoId=@RemitoId ORDER BY rd.Id;

                SELECT rl.Id,rl.RemitoDetalleId,rl.DepositoStockId,rl.FechaIngresoOrigen,
                       rl.FechaVencimiento,p.Nombre AS Producto,p.SeVendePorPeso,rl.CantidadEnviada,
                       rr.Id AS RecepcionRemitoLoteId,rr.LoteLocalId,rr.CantidadRecibida,
                       rr.CantidadDiferencia,rr.Observacion,ISNULL(res.Cantidad,0) AS Resuelto,
                       CASE WHEN rr.Id IS NULL THEN NULL ELSE
                           CONVERT(decimal(18,3),rr.CantidadDiferencia)-
                           CONVERT(decimal(18,3),ISNULL(res.Cantidad,0)) END AS Pendiente
                FROM dbo.RemitoLote rl
                INNER JOIN dbo.RemitoDetalle rd ON rd.Id=rl.RemitoDetalleId AND rd.RemitoId=rl.RemitoId
                INNER JOIN dbo.Producto p ON p.Id=rd.ProductoId
                LEFT JOIN dbo.RecepcionRemitoLote rr ON rr.RemitoLoteId=rl.Id AND rr.RemitoId=rl.RemitoId
                OUTER APPLY (SELECT SUM(d.Cantidad) AS Cantidad FROM dbo.ResolucionDiferenciaDetalle d
                             INNER JOIN dbo.ResolucionDiferencia h ON h.Id=d.ResolucionId AND h.RemitoId=d.RemitoId
                             WHERE d.RecepcionRemitoLoteId=rr.Id AND d.RemitoId=@RemitoId AND h.Estado=N'Aplicada') res
                WHERE rl.RemitoId=@RemitoId ORDER BY rd.Id,rl.Id;

                SELECT h.Id AS ResolucionId,h.Tipo,h.Estado,h.FechaResolucion,h.Motivo,
                       LTRIM(RTRIM(CONCAT(u.Nombre,' ',u.Apellido))) AS Responsable,
                       d.RecepcionRemitoLoteId,rl.Id AS RemitoLoteId,p.Nombre AS Producto,p.SeVendePorPeso,
                       d.Cantidad,d.LoteLocalDestinoId,d.DepositoStockDestinoId,rl.FechaVencimiento
                FROM dbo.ResolucionDiferencia h
                INNER JOIN dbo.ResolucionDiferenciaDetalle d ON d.ResolucionId=h.Id AND d.RemitoId=h.RemitoId
                INNER JOIN dbo.RecepcionRemitoLote rr ON rr.Id=d.RecepcionRemitoLoteId AND rr.RemitoId=d.RemitoId
                INNER JOIN dbo.RemitoLote rl ON rl.Id=rr.RemitoLoteId AND rl.RemitoId=rr.RemitoId
                INNER JOIN dbo.RemitoDetalle rd ON rd.Id=rl.RemitoDetalleId AND rd.RemitoId=rl.RemitoId
                INNER JOIN dbo.Producto p ON p.Id=rd.ProductoId
                INNER JOIN dbo.Usuario u ON u.Id=h.UsuarioResponsableId
                WHERE h.RemitoId=@RemitoId ORDER BY h.FechaResolucion,h.Id,d.Id;";
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync()) resultado.Productos.Add(new RemitoProductoDetalle
                {
                    Id = Entero(reader,"Id"), ProductoId = Entero(reader,"ProductoId"),
                    Producto = Texto(reader,"Producto"), SeVendePorPeso = reader.GetBoolean(reader.GetOrdinal("SeVendePorPeso")),
                    Enviado = reader.GetDecimal(reader.GetOrdinal("CantidadEnviada"))
                });
                await reader.NextResultAsync();
                while (await reader.ReadAsync()) resultado.Lotes.Add(new RemitoLoteDetalle
                {
                    Id = Entero(reader,"Id"), RemitoDetalleId = Entero(reader,"RemitoDetalleId"),
                    DepositoStockId = Entero(reader,"DepositoStockId"),
                    RecepcionRemitoLoteId = Opcional<int>(reader,"RecepcionRemitoLoteId"),
                    LoteLocalId = Opcional<int>(reader,"LoteLocalId"), Producto = Texto(reader,"Producto"),
                    SeVendePorPeso = reader.GetBoolean(reader.GetOrdinal("SeVendePorPeso")),
                    FechaIngresoOrigen = Opcional<DateTime>(reader,"FechaIngresoOrigen"),
                    Vencimiento = Opcional<DateTime>(reader,"FechaVencimiento"),
                    Enviado = reader.GetDecimal(reader.GetOrdinal("CantidadEnviada")),
                    Recibido = Opcional<decimal>(reader,"CantidadRecibida"),
                    DiferenciaOriginal = Opcional<decimal>(reader,"CantidadDiferencia"),
                    Resuelto = reader.GetDecimal(reader.GetOrdinal("Resuelto")),
                    Pendiente = Opcional<decimal>(reader,"Pendiente"), Observacion = reader.IsDBNull(reader.GetOrdinal("Observacion")) ? null : Texto(reader,"Observacion")
                });
                await reader.NextResultAsync();
                while (await reader.ReadAsync()) resultado.Resoluciones.Add(new RemitoResolucionDetalle
                {
                    ResolucionId = Entero(reader,"ResolucionId"), RecepcionRemitoLoteId = Entero(reader,"RecepcionRemitoLoteId"),
                    RemitoLoteId = Entero(reader,"RemitoLoteId"), Producto = Texto(reader,"Producto"),
                    SeVendePorPeso = reader.GetBoolean(reader.GetOrdinal("SeVendePorPeso")),
                    Tipo = Texto(reader,"Tipo"), Estado = Texto(reader,"Estado"), Responsable = Texto(reader,"Responsable"),
                    Fecha = reader.GetDateTime(reader.GetOrdinal("FechaResolucion")), Motivo = Texto(reader,"Motivo"),
                    Cantidad = reader.GetDecimal(reader.GetOrdinal("Cantidad")),
                    LoteLocalDestinoId = Opcional<int>(reader,"LoteLocalDestinoId"),
                    DepositoStockDestinoId = Opcional<int>(reader,"DepositoStockDestinoId"),
                    Vencimiento = Opcional<DateTime>(reader,"FechaVencimiento")
                });
            }
            foreach (var producto in resultado.Productos)
            {
                var lotes = resultado.Lotes.Where(l => l.RemitoDetalleId == producto.Id).ToList();
                bool recibidos = lotes.Count > 0 && lotes.All(l => l.Recibido.HasValue);
                producto.Recibido = recibidos ? lotes.Sum(l => l.Recibido!.Value) : null;
                producto.DiferenciaOriginal = recibidos ? lotes.Sum(l => l.DiferenciaOriginal!.Value) : null;
                producto.Resuelto = lotes.Sum(l => l.Resuelto);
                producto.Pendiente = recibidos ? lotes.Sum(l => l.Pendiente!.Value) : null;
            }
            await transaccion.CommitAsync();
            return resultado;
        }

        private static int Entero(SqlDataReader r, string c) => r.GetInt32(r.GetOrdinal(c));
        private static string Texto(SqlDataReader r, string c) => r.IsDBNull(r.GetOrdinal(c)) ? "" : r.GetString(r.GetOrdinal(c));
        private static T? Opcional<T>(SqlDataReader r, string c) where T : struct =>
            r.IsDBNull(r.GetOrdinal(c)) ? null : r.GetFieldValue<T>(r.GetOrdinal(c));
        private static RemitoInterno LeerCabecera(SqlDataReader r) => new()
        {
            Id = Entero(r,"Id"), Estado = Texto(r,"Estado"), Creador = Texto(r,"Creador"),
            FechaCreacion = r.GetDateTime(r.GetOrdinal("FechaCreacion")), Emisor = Texto(r,"Emisor"),
            FechaEmision = Opcional<DateTime>(r,"FechaEmision"), Receptor = Texto(r,"Receptor"),
            FechaRecepcion = Opcional<DateTime>(r,"FechaRecepcion"), Cancelador = Texto(r,"Cancelador"),
            FechaCancelacion = Opcional<DateTime>(r,"FechaCancelacion"),
            MotivoCancelacion = r.IsDBNull(r.GetOrdinal("MotivoCancelacion")) ? null : Texto(r,"MotivoCancelacion")
        };
    }
}
