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

        // --- NUEVO: Historial filtrado por Rol ---
        // --- SE MODIFICÓ PARA ACEPTAR FECHAS ---
        public DataTable ObtenerHistorialVentas(string rolActual, int idUsuarioActivo, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dt = new DataTable();
            using (Microsoft.Data.SqlClient.SqlConnection conexion = ObtenerConexion())
            {
                string query = @"SELECT V.Id AS [Nro Venta], V.FechaHora AS Fecha, 
                                    ISNULL(C.Nombre + ' ' + C.Apellido, 'Consumidor Final') AS Cliente, 
                                    U.Nombre + ' ' + U.Apellido AS Cajero, 
                                    V.Total, V.Estado
                             FROM Venta V
                             LEFT JOIN Cliente C ON V.ClienteId = C.Id
                             INNER JOIN Usuario U ON V.UsuarioId = U.Id
                             WHERE CAST(V.FechaHora AS DATE) >= CAST(@Desde AS DATE) 
                             AND CAST(V.FechaHora AS DATE) <= CAST(@Hasta AS DATE)";

                if (rolActual == "Vendedor") query += " AND V.UsuarioId = @UsuarioId";
                query += " ORDER BY V.FechaHora DESC";

                Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@Desde", fechaDesde);
                cmd.Parameters.AddWithValue("@Hasta", fechaHasta);
                if (rolActual == "Vendedor") cmd.Parameters.AddWithValue("@UsuarioId", idUsuarioActivo);

                Microsoft.Data.SqlClient.SqlDataAdapter da = new Microsoft.Data.SqlClient.SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        // --- NUEVA FUNCIÓN PARA SUMAR LA CAJA ---
        public DataTable ObtenerTotalesVentas(string rolActual, int idUsuarioActivo, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dt = new DataTable();
            using (Microsoft.Data.SqlClient.SqlConnection conexion = ObtenerConexion())
            {
                // Usamos VentaPago y agrupamos los IDs duplicados que tenés en tu tabla MetodoPago
                string query = @"
            SELECT 
                ISNULL(SUM(CASE WHEN P.MetodoPagoId IN (1, 1002) THEN P.MontoCobrado ELSE 0 END), 0) AS TotalEfectivo,
                ISNULL(SUM(CASE WHEN P.MetodoPagoId IN (2, 3, 1003, 1004) THEN P.MontoCobrado ELSE 0 END), 0) AS TotalTarjeta,
                ISNULL(SUM(CASE WHEN P.MetodoPagoId IN (4, 1005) THEN P.MontoCobrado ELSE 0 END), 0) AS TotalTransferencia,
                ISNULL(SUM(P.MontoCobrado), 0) AS TotalGeneral
            FROM Venta V
            INNER JOIN VentaPago P ON V.Id = P.VentaId
            WHERE V.Estado != 'Anulada' 
            AND CAST(V.FechaHora AS DATE) >= CAST(@Desde AS DATE) 
            AND CAST(V.FechaHora AS DATE) <= CAST(@Hasta AS DATE)";

                if (rolActual == "Vendedor") query += " AND V.UsuarioId = @UsuarioId";

                Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@Desde", fechaDesde);
                cmd.Parameters.AddWithValue("@Hasta", fechaHasta);
                if (rolActual == "Vendedor") cmd.Parameters.AddWithValue("@UsuarioId", idUsuarioActivo);

                Microsoft.Data.SqlClient.SqlDataAdapter da = new Microsoft.Data.SqlClient.SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        // --- NUEVO: Anulación con Auditoría ---
        public void AnularVentaConMotivo(int ventaId, int usuarioId, string motivo)
        {
            using (SqlConnection conexion = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_AnularVentaConMotivo", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@VentaId", ventaId);
                cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                cmd.Parameters.AddWithValue("@Motivo", motivo);

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