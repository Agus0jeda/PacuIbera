using Microsoft.Data.SqlClient;
using PacuIbera.Dominio;
using PacuIbera.UI.Common;
using System;
using System.Data;

namespace Datos 
{
    public class CajaDatos : ConexionBD
    {
        // 1. Verificar si hay caja abierta al loguearse
        public int VerificarCajaAbierta(int usuarioId)
        {
            int cajaId = 0; // Si devuelve 0, no hay caja abierta
            using (SqlConnection conexion = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_VerificarCajaAbierta", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);

                conexion.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        cajaId = Convert.ToInt32(reader["Id"]);
                    }
                }
            }
            return cajaId;
        }

        // 2. Registrar la apertura de una nueva caja
        public int AbrirCaja(int usuarioId, decimal saldoInicial)
        {
            int nuevaCajaId = 0;
            using (SqlConnection conexion = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_AbrirCaja", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                cmd.Parameters.AddWithValue("@SaldoInicial", saldoInicial);

                conexion.Open();
                // ExecuteScalar captura el SCOPE_IDENTITY() que devuelve el script SQL
                nuevaCajaId = Convert.ToInt32(cmd.ExecuteScalar());
            }
            return nuevaCajaId;
        }

        // 3. Registrar el cierre de caja
        public void CerrarCaja(int cajaId, decimal saldoFinal)
        {
            using (SqlConnection conexion = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_CerrarCaja", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CajaId", cajaId);
                cmd.Parameters.AddWithValue("@SaldoFinal", saldoFinal);

                conexion.Open();
                cmd.ExecuteNonQuery(); // No devuelve datos, solo actualiza la tabla
            }
        }

        public DataTable ObtenerResumenCajas(DateTime fecha)
        {
            DataTable dt = new DataTable();
            using (Microsoft.Data.SqlClient.SqlConnection conexion = ObtenerConexion())
            {
                Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand("sp_ResumenCajas", conexion);
                cmd.CommandType = CommandType.StoredProcedure;

                
                cmd.Parameters.AddWithValue("@Fecha", fecha);

                Microsoft.Data.SqlClient.SqlDataAdapter da = new Microsoft.Data.SqlClient.SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }
    }
}