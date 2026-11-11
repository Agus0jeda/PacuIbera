using Microsoft.Data.SqlClient;
using PacuIbera.Dominio;
using PacuIbera.UI.Common;
using System;
using System.Data;

namespace Datos 
{
    public class CajaDatos : ConexionBD
    {
        public sealed record CajaAbiertaInfo(int Id, DateTime FechaHoraApertura, string NombreVendedor);

        // 1. Verificar si hay caja abierta al loguearse
        public int VerificarCajaAbierta(int usuarioId)
            => ObtenerCajaAbierta(usuarioId)?.Id ?? 0;

        public CajaAbiertaInfo? ObtenerCajaAbierta(int usuarioId)
        {
            using (SqlConnection conexion = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT TOP (1) c.Id,c.FechaHoraApertura,
                           LTRIM(RTRIM(u.Nombre + ' ' + u.Apellido)) AS NombreVendedor
                    FROM dbo.Caja c
                    INNER JOIN dbo.Usuario u ON u.Id=c.UsuarioId
                    WHERE c.UsuarioId = @UsuarioId AND c.Estado = 'Abierta'
                    ORDER BY
                        CASE WHEN EXISTS (SELECT 1 FROM dbo.Venta v WHERE v.CajaId = c.Id) THEN 0 ELSE 1 END,
                        CASE WHEN EXISTS (SELECT 1 FROM dbo.Venta v WHERE v.CajaId = c.Id) THEN c.FechaHoraApertura END DESC,
                        c.FechaHoraApertura DESC,
                        c.Id DESC;", conexion);
                cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);

                conexion.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                    return new CajaAbiertaInfo(reader.GetInt32(0), reader.GetDateTime(1), reader.GetString(2));
            }
            return null;
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
