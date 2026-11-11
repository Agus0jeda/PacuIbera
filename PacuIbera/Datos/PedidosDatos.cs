using Datos;
using Microsoft.Data.SqlClient;
using PacuIbera.Dominio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PacuIbera.Datos
{
    public class PedidosDatos : ConexionBD
    {
        public sealed class ItemPedido
        {
            public int ProductoId { get; init; }
            public decimal Cantidad { get; init; }
            public bool SeVendePorPeso { get; init; }
            public decimal PrecioCosto { get; init; }
        }

        public DataTable ObtenerPedidos()
        {
            DataTable tabla = new DataTable();
            try
            {
                using (SqlConnection conexion = ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand("sp_ObtenerPedidosProveedor", conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conexion.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        tabla.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log o rethrow según convención
                throw new ApplicationException("Error al obtener pedidos", ex);
            }
            return tabla;
        }

        // Lectura exclusiva de Deposito; conserva la consulta usada por Compras.
        public DataTable ObtenerPedidosPorRecibir()
        {
            DataTable pedidos = ObtenerPedidos();
            var elegibles = new HashSet<int>();
            using (SqlConnection conexion = ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT pp.Id FROM dbo.PedidoProveedor pp
                WHERE pp.Estado = N'Pendiente'
                  AND NOT EXISTS (SELECT 1 FROM dbo.RecepcionPedido r WHERE r.PedidoId = pp.Id)
                  AND EXISTS (SELECT 1 FROM dbo.DetallePedidoProveedor d WHERE d.PedidoId = pp.Id)
                  AND NOT EXISTS (SELECT 1 FROM dbo.DetallePedidoProveedor d
                                  WHERE d.PedidoId = pp.Id AND d.CantidadRecibida <> 0);", conexion))
            {
                conexion.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                    while (reader.Read()) elegibles.Add(reader.GetInt32(0));
            }
            DataTable resultado = pedidos.Clone();
            foreach (DataRow fila in pedidos.Rows)
                if (fila.Field<string>("Estado") == "Pendiente" && elegibles.Contains(fila.Field<int>("Id")))
                    resultado.ImportRow(fila);
            return resultado;
        }

        // Solo lectura: incluye la unidad de venta sin cambiar la consulta de detalle existente.
        public DataTable ObtenerDetalleParaRecepcion(int pedidoId)
        {
            var tabla = new DataTable();
            using (SqlConnection conexion = ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT d.Id AS DetallePedidoProveedorId, p.Nombre AS Producto,
                       d.Cantidad, p.SeVendePorPeso, p.ControlaVencimiento
                FROM dbo.DetallePedidoProveedor d
                INNER JOIN dbo.Producto p ON p.Id = d.ProductoId
                WHERE d.PedidoId = @PedidoId ORDER BY d.Id;", conexion))
            {
                cmd.Parameters.Add("@PedidoId", SqlDbType.Int).Value = pedidoId;
                conexion.Open();
                using (SqlDataReader reader = cmd.ExecuteReader()) tabla.Load(reader);
            }
            return tabla;
        }

        public DataTable ObtenerCabeceraPedido(int pedidoId)
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT
                    pp.Id,
                    p.RazonSocial AS Proveedor,
                    LTRIM(RTRIM(CONCAT(ISNULL(u.Nombre, ''), ' ', ISNULL(u.Apellido, '')))) AS UsuarioSolicitante,
                    pp.FechaPedido,
                    pp.Estado
                FROM dbo.PedidoProveedor pp
                INNER JOIN dbo.Proveedor p ON p.Id = pp.ProveedorId
                INNER JOIN dbo.Usuario u ON u.Id = pp.UsuarioId
                WHERE pp.Id = @PedidoId;", conexion))
            {
                cmd.Parameters.Add("@PedidoId", SqlDbType.Int).Value = pedidoId;
                conexion.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    tabla.Load(reader);
                }
            }
            return tabla;
        }

        public void GuardarPedidoAtomico(int proveedorId, int usuarioId, DateTime fechaPedido, IEnumerable<ItemPedido> items)
        {
            List<ItemPedido> itemsLista = items?.ToList() ?? new List<ItemPedido>();
            if (itemsLista.Count == 0)
                throw new ApplicationException("El pedido debe contener al menos un producto.");

            using (SqlConnection conexion = ObtenerConexion())
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        ValidarProveedorYProductos(conexion, transaccion, proveedorId, itemsLista);
                        ValidarSoporteCantidadDecimal(conexion, transaccion, itemsLista);
                        int pedidoId = CrearPedidoEnTransaccion(conexion, transaccion, proveedorId, usuarioId, fechaPedido);

                        foreach (ItemPedido item in itemsLista)
                            AgregarItemEnTransaccion(conexion, transaccion, pedidoId, item);

                        transaccion.Commit();
                    }
                    catch
                    {
                        if (transaccion.Connection != null)
                            transaccion.Rollback();
                        throw;
                    }
                }
            }
        }

        private static void ValidarProveedorYProductos(SqlConnection conexion, SqlTransaction transaccion, int proveedorId, IReadOnlyCollection<ItemPedido> items)
        {
            if (items.Any(item => item.Cantidad <= 0 || item.Cantidad > 9999999.999M || item.PrecioCosto < 0))
                throw new ApplicationException("El pedido contiene una cantidad o precio de costo inválido.");
            if (items.Any(item => !item.SeVendePorPeso && decimal.Truncate(item.Cantidad) != item.Cantidad))
                throw new ApplicationException("Los productos vendidos por unidad solo admiten cantidades enteras.");

            using (SqlCommand cmd = new SqlCommand(@"
                SELECT COUNT(*)
                FROM dbo.Proveedor p
                WHERE p.Id = @ProveedorId AND p.Activo = 1;

                SELECT COUNT(*)
                FROM dbo.ProveedorProducto pp
                INNER JOIN dbo.Producto p ON p.Id = pp.ProductoId AND p.Activo = 1
                WHERE pp.ProveedorId = @ProveedorId
                  AND pp.Activo = 1
                  AND pp.ProductoId IN (" + string.Join(",", items.Select((_, indice) => "@ProductoId" + indice)) + ");", conexion, transaccion))
            {
                cmd.Parameters.Add("@ProveedorId", SqlDbType.Int).Value = proveedorId;
                int indice = 0;
                foreach (ItemPedido item in items)
                    cmd.Parameters.Add("@ProductoId" + indice++, SqlDbType.Int).Value = item.ProductoId;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read() || reader.GetInt32(0) != 1)
                        throw new ApplicationException("El proveedor seleccionado no está activo o no existe.");

                    if (!reader.NextResult() || !reader.Read() || reader.GetInt32(0) != items.Count)
                        throw new ApplicationException("Uno o más productos ya no están asociados activamente al proveedor seleccionado.");
                }
            }
        }

        private static void ValidarSoporteCantidadDecimal(SqlConnection conexion, SqlTransaction transaccion, IReadOnlyCollection<ItemPedido> items)
        {
            if (!items.Any(item => decimal.Truncate(item.Cantidad) != item.Cantidad))
                return;

            using (SqlCommand cmd = new SqlCommand(@"
                SELECT TYPE_NAME(p.user_type_id)
                FROM sys.parameters p
                INNER JOIN sys.objects o ON o.object_id = p.object_id
                WHERE o.name = N'sp_AgregarItemPedidoProveedor'
                  AND SCHEMA_NAME(o.schema_id) = N'dbo'
                  AND p.name = N'@Cantidad';", conexion, transaccion))
            {
                object resultado = cmd.ExecuteScalar();
                if (!string.Equals(resultado?.ToString(), "decimal", StringComparison.OrdinalIgnoreCase))
                    throw new ApplicationException("La base de pruebas todavía no permite cantidades decimales para los pedidos. Aplique el script SQL preparado antes de guardar cantidades fraccionarias.");
            }
        }

        private static int CrearPedidoEnTransaccion(SqlConnection conexion, SqlTransaction transaccion, int proveedorId, int usuarioId, DateTime fechaPedido)
        {
            using (SqlCommand cmd = new SqlCommand("sp_CrearPedidoProveedor", conexion, transaccion))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ProveedorId", SqlDbType.Int).Value = proveedorId;
                cmd.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = usuarioId;
                cmd.Parameters.Add("@FechaPedido", SqlDbType.DateTime2).Value = fechaPedido;
                SqlParameter outId = new SqlParameter("@NuevoPedidoId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(outId);
                cmd.ExecuteNonQuery();
                return Convert.ToInt32(outId.Value);
            }
        }

        private static void AgregarItemEnTransaccion(SqlConnection conexion, SqlTransaction transaccion, int pedidoId, ItemPedido item)
        {
            using (SqlCommand cmd = new SqlCommand("sp_AgregarItemPedidoProveedor", conexion, transaccion))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@PedidoId", SqlDbType.Int).Value = pedidoId;
                cmd.Parameters.Add("@ProductoId", SqlDbType.Int).Value = item.ProductoId;
                cmd.Parameters.Add("@Cantidad", SqlDbType.Decimal).Value = item.Cantidad;
                cmd.Parameters["@Cantidad"].Precision = 10;
                cmd.Parameters["@Cantidad"].Scale = 3;
                cmd.Parameters.Add("@PrecioCosto", SqlDbType.Decimal).Value = item.PrecioCosto;
                cmd.Parameters["@PrecioCosto"].Precision = 18;
                cmd.Parameters["@PrecioCosto"].Scale = 2;
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable ObtenerDetallePedido(int pedidoId)
        {
            DataTable tabla = new DataTable();
            try
            {
                using (SqlConnection conexion = ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand("sp_ObtenerDetallePedidoProveedor", conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PedidoId", pedidoId);
                    conexion.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        tabla.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al obtener detalle del pedido", ex);
            }
            return tabla;
        }

        public int CrearPedido(int proveedorId, int usuarioId, DateTime fechaPedido)
        {
            try
            {
                using (SqlConnection conexion = ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand("sp_CrearPedidoProveedor", conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProveedorId", proveedorId);
                    cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                    cmd.Parameters.AddWithValue("@FechaPedido", fechaPedido);

                    // output id
                    SqlParameter outId = new SqlParameter("@NuevoPedidoId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(outId);

                    conexion.Open();
                    cmd.ExecuteNonQuery();

                    return Convert.ToInt32(outId.Value);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al crear pedido", ex);
            }
        }

        public void AgregarItemPedido(int pedidoId, int productoId, int cantidad, decimal precioCosto)
        {
            try
            {
                using (SqlConnection conexion = ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand("sp_AgregarItemPedidoProveedor", conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PedidoId", pedidoId);
                    cmd.Parameters.AddWithValue("@ProductoId", productoId);
                    cmd.Parameters.AddWithValue("@Cantidad", cantidad);
                    cmd.Parameters.AddWithValue("@PrecioCosto", precioCosto);
                    conexion.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al agregar item al pedido", ex);
            }
        }

        public void RecepcionarItem(int detalleId, int cantidadIngresada, DateTime? fechaVencimiento)
        {
            try
            {
                using (SqlConnection conexion = ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand("sp_RecepcionarItemPedido", conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DetalleId", detalleId);
                    cmd.Parameters.AddWithValue("@CantidadIngresada", cantidadIngresada);
                    if (fechaVencimiento.HasValue)
                        cmd.Parameters.AddWithValue("@FechaVencimiento", fechaVencimiento.Value);
                    else
                        cmd.Parameters.AddWithValue("@FechaVencimiento", DBNull.Value);
                    conexion.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al recepcionar item", ex);
            }
        }
    }
}
