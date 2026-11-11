using Datos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace PacuIbera.Datos
{
    public sealed class RecepcionesDatos : ConexionBD
    {
        public enum EstadoRecepcion { NoDisponible, Disponible, Confirmada }

        private async Task<SqlConnection> AbrirRecepcionProtegidaAsync()
        {
            if (Environment.GetEnvironmentVariable("PACUIBERA_DB_PRUEBAS") != "1"
                || new SqlConnectionStringBuilder(CadenaGlobal).InitialCatalog != "PacuIberaDB_Pruebas")
                throw new InvalidOperationException("La recepción requiere el perfil PacuIbera - Pruebas.");
            var conexion = ObtenerConexion();
            try
            {
                await conexion.OpenAsync();
                using var cmd = new SqlCommand(@"
                    SELECT CASE WHEN DB_NAME()=N'PacuIberaDB_Pruebas' AND EXISTS
                    (SELECT 1 FROM sys.extended_properties e
                     WHERE e.class=1 AND e.major_id=OBJECT_ID(N'dbo.sp_ConfirmarRecepcionPedido') AND e.minor_id=0
                       AND e.name=N'PacuIbera.RecepcionPedido.ControlVencimiento'
                       AND CONVERT(nvarchar(64),e.value)=CONVERT(nvarchar(64),HASHBYTES('SHA2_256',
                           OBJECT_DEFINITION(OBJECT_ID(N'dbo.sp_ConfirmarRecepcionPedido'))),2))
                    THEN 1 ELSE 0 END;", conexion);
                if (Convert.ToInt32(await cmd.ExecuteScalarAsync()) != 1)
                    throw new InvalidOperationException("Antes de confirmar, debe instalarse en Pruebas el script Actualizar_sp_ConfirmarRecepcionPedido_ControlVencimiento_PacuIberaDB_Pruebas.sql. No se envió la confirmación.");
                return conexion;
            }
            catch { conexion.Dispose(); throw; }
        }

        private static async Task ValidarVencimientosAsync(SqlConnection conexion, int pedidoId, DataTable lotes)
        {
            using var cmd = new SqlCommand(@"
                SELECT TOP(1) p.Nombre
                FROM @Lotes l JOIN dbo.DetallePedidoProveedor d ON d.Id=l.DetallePedidoProveedorId
                JOIN dbo.Producto p ON p.Id=d.ProductoId
                WHERE d.PedidoId=@PedidoId AND
                    ((p.ControlaVencimiento=1 AND(l.TieneVencimiento=0 OR l.FechaVencimiento IS NULL))
                     OR(l.TieneVencimiento=1 AND l.FechaVencimiento IS NULL)
                     OR(l.TieneVencimiento=0 AND l.FechaVencimiento IS NOT NULL)
                     OR l.FechaVencimiento<CONVERT(date,SYSDATETIME()));", conexion);
            cmd.Parameters.Add("@PedidoId", SqlDbType.Int).Value=pedidoId;
            cmd.Parameters.Add(new SqlParameter("@Lotes", SqlDbType.Structured)
                { TypeName="dbo.RecepcionLoteEntrada", Value=lotes });
            if (await cmd.ExecuteScalarAsync() is string producto)
                throw new InvalidOperationException($"{producto}: revise el vencimiento. La configuración actual puede exigir una fecha por lote; no se admiten fechas vencidas.");
        }

        public async Task ValidarVencimientosAsync(int pedidoId, DataTable lotes)
        {
            using var conexion=await AbrirRecepcionProtegidaAsync();
            await ValidarVencimientosAsync(conexion,pedidoId,lotes);
        }

        private const string ConsultaHistorial = @"
            SELECT r.Id AS RecepcionId, r.PedidoId, p.RazonSocial AS Proveedor,
                   r.FechaRecepcion, LTRIM(RTRIM(CONCAT(u.Nombre, ' ', u.Apellido))) AS Usuario,
                   r.EstadoResultado AS Estado
            FROM dbo.RecepcionPedido r
            INNER JOIN dbo.PedidoProveedor pp ON pp.Id = r.PedidoId
            INNER JOIN dbo.Proveedor p ON p.Id = pp.ProveedorId
            INNER JOIN dbo.Usuario u ON u.Id = r.UsuarioReceptorId";

        public DataTable ObtenerHistorial()
        {
            using var conexion = ObtenerConexion();
            using var cmd = new SqlCommand(ConsultaHistorial + " ORDER BY r.FechaRecepcion DESC, r.Id DESC;", conexion);
            conexion.Open();
            using var reader = cmd.ExecuteReader();
            var tabla = new DataTable();
            tabla.Load(reader);
            return tabla;
        }

        public DataSet ObtenerDetalleHistorial(int recepcionId)
        {
            using var conexion = ObtenerConexion();
            using var cmd = new SqlCommand(ConsultaHistorial + @" WHERE r.Id = @RecepcionId;
                SELECT rd.Id AS DetalleRecepcionId, p.Nombre AS Producto,
                       rd.CantidadSolicitada AS Solicitado, rd.CantidadRecibida AS Recibido,
                       rd.CantidadFaltante AS Faltante
                FROM dbo.RecepcionDetalle rd
                INNER JOIN dbo.DetallePedidoProveedor d ON d.Id = rd.DetallePedidoProveedorId
                INNER JOIN dbo.Producto p ON p.Id = d.ProductoId
                WHERE rd.RecepcionId = @RecepcionId ORDER BY rd.Id;
                SELECT rl.Id AS Lote, rd.Id AS DetalleRecepcionId, p.Nombre AS Producto,
                       rl.Cantidad, rl.FechaVencimiento AS Vencimiento
                FROM dbo.RecepcionLote rl
                INNER JOIN dbo.RecepcionDetalle rd ON rd.Id = rl.RecepcionDetalleId
                INNER JOIN dbo.DetallePedidoProveedor d ON d.Id = rd.DetallePedidoProveedorId
                INNER JOIN dbo.Producto p ON p.Id = d.ProductoId
                WHERE rd.RecepcionId = @RecepcionId ORDER BY rd.Id, rl.Id;", conexion);
            cmd.Parameters.Add("@RecepcionId", SqlDbType.Int).Value = recepcionId;
            using var adapter = new SqlDataAdapter(cmd);
            var datos = new DataSet();
            adapter.Fill(datos);
            return datos;
        }

        public async Task ConfirmarAsync(int pedidoId, int usuarioId, DataTable detalles, DataTable lotes)
        {
            using var conexion = await AbrirRecepcionProtegidaAsync();
            await ValidarVencimientosAsync(conexion,pedidoId,lotes);
            using var cmd = new SqlCommand("dbo.sp_ConfirmarRecepcionPedido", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@PedidoId", SqlDbType.Int).Value = pedidoId;
            cmd.Parameters.Add("@UsuarioReceptorId", SqlDbType.Int).Value = usuarioId;
            cmd.Parameters.Add(new SqlParameter("@Detalles", SqlDbType.Structured)
            {
                TypeName = "dbo.RecepcionDetalleEntrada", Value = detalles
            });
            cmd.Parameters.Add(new SqlParameter("@Lotes", SqlDbType.Structured)
            {
                TypeName = "dbo.RecepcionLoteEntrada", Value = lotes
            });
            // Un solo envio. Sin reintentos automaticos ni escrituras auxiliares.
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<EstadoRecepcion> ConsultarEstadoAsync(int pedidoId)
        {
            using var conexion = ObtenerConexion();
            await conexion.OpenAsync();
            using var transaccion = conexion.BeginTransaction(IsolationLevel.Serializable);
            // Esperar a cualquier confirmacion en curso usando la misma cabecera.
            // UPDLOCK no escribe datos: evita leer Pendiente mientras el SP sigue ejecutandose.
            using var cmd = new SqlCommand(@"
                DECLARE @Estado nvarchar(30);
                SELECT @Estado = Estado FROM dbo.PedidoProveedor WITH (UPDLOCK, HOLDLOCK)
                WHERE Id = @PedidoId;
                SELECT CASE
                    WHEN EXISTS (SELECT 1 FROM dbo.RecepcionPedido WHERE PedidoId = @PedidoId) THEN 2
                    WHEN @Estado = N'Pendiente'
                     AND EXISTS (SELECT 1 FROM dbo.DetallePedidoProveedor WHERE PedidoId = @PedidoId)
                     AND NOT EXISTS (SELECT 1 FROM dbo.DetallePedidoProveedor
                                     WHERE PedidoId = @PedidoId AND CantidadRecibida <> 0) THEN 1
                    ELSE 0 END;", conexion, transaccion);
            cmd.Parameters.Add("@PedidoId", SqlDbType.Int).Value = pedidoId;
            var estado = (EstadoRecepcion)Convert.ToInt32(await cmd.ExecuteScalarAsync());
            await transaccion.CommitAsync();
            return estado;
        }
    }
}
