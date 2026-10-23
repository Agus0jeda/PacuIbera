using Datos;
using Microsoft.Data.SqlClient;
using PacuIbera.Dominio;
using System;
using System.Data;

namespace PacuIbera.Datos
{
    public class PedidosDatos : ConexionBD
    {
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
