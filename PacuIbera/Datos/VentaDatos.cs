using Datos;
using System;
using System.Data;
using Microsoft.Data.SqlClient;


namespace PacuIbera.Datos
{
    public class VentaDatos : ConexionBD
    {
        public void RegistrarVentaCompleta(int cajaId, int usuarioId, int clienteId, decimal total, decimal pagoEfectivo, decimal pagoTransferencia, decimal pagoTarjeta, DataTable carrito)
        {
            using (SqlConnection conexion = ObtenerConexion())
            {
                conexion.Open();
                // Iniciamos una TRANSACCIÓN: Si algo falla a la mitad, se deshace todo
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        // 1. Crear la Venta Principal
                        SqlCommand cmdVenta = new SqlCommand("sp_RegistrarVenta", conexion, transaccion);
                        cmdVenta.CommandType = CommandType.StoredProcedure;
                        cmdVenta.Parameters.AddWithValue("@CajaId", cajaId);
                        cmdVenta.Parameters.AddWithValue("@UsuarioId", usuarioId);

                        if (clienteId == 0) // Si es Consumidor Final, pasamos NULL
                            cmdVenta.Parameters.AddWithValue("@ClienteId", DBNull.Value);
                        else
                            cmdVenta.Parameters.AddWithValue("@ClienteId", clienteId);

                        cmdVenta.Parameters.AddWithValue("@Total", total);

                        // ExecuteScalar nos devuelve el VentaId que se acaba de crear en la tabla
                        int ventaId = Convert.ToInt32(cmdVenta.ExecuteScalar());

                        // 2. Registrar los Pagos Mixtos
                        // Según tu script: 1 = Efectivo, 2 = Tarjeta, 4 = Transferencia
                        if (pagoEfectivo > 0) InsertarPago(conexion, transaccion, ventaId, 1, pagoEfectivo);
                        if (pagoTarjeta > 0) InsertarPago(conexion, transaccion, ventaId, 2, pagoTarjeta);
                        if (pagoTransferencia > 0) InsertarPago(conexion, transaccion, ventaId, 4, pagoTransferencia);

                        // 3. Guardar el detalle del carrito y descontar el stock
                        foreach (DataRow fila in carrito.Rows)
                        {
                            SqlCommand cmdDetalle = new SqlCommand("sp_RegistrarDetalleVentaFIFO", conexion, transaccion);
                            cmdDetalle.CommandType = CommandType.StoredProcedure;
                            cmdDetalle.Parameters.AddWithValue("@VentaId", ventaId);
                            cmdDetalle.Parameters.AddWithValue("@ProductoId", Convert.ToInt32(fila["IdProducto"]));
                            cmdDetalle.Parameters.AddWithValue("@CantidadRequerida", Convert.ToDecimal(fila["Cantidad"]));
                            cmdDetalle.Parameters.AddWithValue("@PrecioUnitario", Convert.ToDecimal(fila["Precio"]));

                            cmdDetalle.ExecuteNonQuery();
                        }

                        // Si llegó hasta acá sin explotar, confirmamos todos los cambios en la BD
                        transaccion.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Si falta stock o hay un error, cancelamos todo para no ensuciar la base de datos
                        transaccion.Rollback();
                        throw new Exception("Error al procesar la venta: " + ex.Message);
                    }
                }
            }
        }

        private void InsertarPago(SqlConnection con, SqlTransaction tr, int ventaId, int metodoPagoId, decimal monto)
        {
            SqlCommand cmd = new SqlCommand("sp_RegistrarPagoVenta", con, tr);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@VentaId", ventaId);
            cmd.Parameters.AddWithValue("@MetodoPagoId", metodoPagoId);
            cmd.Parameters.AddWithValue("@MontoCobrado", monto);
            cmd.ExecuteNonQuery();
        }

        public DataTable ObtenerHistorialVentas()
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_ObtenerHistorialVentas", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                conexion.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    tabla.Load(reader);
                }
            }
            return tabla;
        }

        public void AnularVenta(int idVenta)
        {
            using (SqlConnection conexion = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_AnularVenta", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@VentaId", idVenta);
                conexion.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable ObtenerDetalleVenta(int idVenta)
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_ObtenerDetalleVenta", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@VentaId", idVenta);
                conexion.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    tabla.Load(reader);
                }
            }
            return tabla;
        }
    }
}