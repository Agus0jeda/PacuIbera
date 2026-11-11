using Microsoft.Data.SqlClient;
using PacuIbera.Dominio;
using PacuIbera.UI.Common;
using System;
using System.Data;
using System.Globalization;

namespace Datos 
{
    public class CajaDatos : ConexionBD
    {
        public sealed record CajaAbiertaInfo(int Id, DateTime FechaHoraApertura, string NombreVendedor);
        public sealed record ResumenCierreCaja(
            int CajaId,
            decimal FondoInicial,
            decimal CobrosEfectivo,
            decimal Ingresos,
            decimal Retiros,
            decimal? EfectivoEsperado,
            decimal? EfectivoContado,
            decimal? Diferencia,
            bool Bloqueada,
            string Observacion);

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
        public ResumenCierreCaja? ObtenerResumenCierreCaja(int cajaId, int usuarioId)
        {
            if (cajaId <= 0 || usuarioId <= 0)
                throw new ArgumentException("No se pudo identificar la caja y el Vendedor activo.");

            using (SqlConnection conexion = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("dbo.sp_ResumenCajas", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Fecha", SqlDbType.Date).Value = DateTime.Today;
                cmd.Parameters.Add("@CajaId", SqlDbType.Int).Value = cajaId;
                cmd.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = usuarioId;

                conexion.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                if (!reader.Read()) return null;

                return new ResumenCierreCaja(
                    cajaId,
                    Convert.ToDecimal(reader["FondoCaja"], CultureInfo.InvariantCulture),
                    Convert.ToDecimal(reader["CobrosEfectivo"], CultureInfo.InvariantCulture),
                    Convert.ToDecimal(reader["IngresosCaja"], CultureInfo.InvariantCulture),
                    Convert.ToDecimal(reader["RetirosCaja"], CultureInfo.InvariantCulture),
                    reader["MontoEsperado"] == DBNull.Value ? null : Convert.ToDecimal(reader["MontoEsperado"], CultureInfo.InvariantCulture),
                    reader["EfectivoDeclarado"] == DBNull.Value ? null : Convert.ToDecimal(reader["EfectivoDeclarado"], CultureInfo.InvariantCulture),
                    reader["Diferencia"] == DBNull.Value ? null : Convert.ToDecimal(reader["Diferencia"], CultureInfo.InvariantCulture),
                    Convert.ToBoolean(reader["CajaBloqueadaCierre"], CultureInfo.InvariantCulture),
                    reader["ObservacionEfectivo"]?.ToString() ?? string.Empty);
            }
        }

        public ResumenCierreCaja CerrarCaja(int cajaId, decimal saldoFinal, int usuarioId)
        {
            if (cajaId <= 0 || usuarioId <= 0)
                throw new ArgumentException("No se pudo identificar la caja y el Vendedor activo.");
            if (saldoFinal < 0)
                throw new ArgumentOutOfRangeException(nameof(saldoFinal), "El efectivo contado no puede ser negativo.");

            using (SqlConnection conexion = ObtenerConexion())
            {
                using SqlCommand cmd = new SqlCommand("dbo.sp_CerrarCaja", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@CajaId", SqlDbType.Int).Value = cajaId;
                cmd.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = usuarioId;
                SqlParameter saldo = cmd.Parameters.Add("@SaldoFinal", SqlDbType.Decimal);
                saldo.Precision = 18;
                saldo.Scale = 2;
                saldo.Value = saldoFinal;

                conexion.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                if (!reader.Read())
                    throw new InvalidOperationException("El cierre no devolvió su resumen de confirmación.");

                return new ResumenCierreCaja(
                    Convert.ToInt32(reader["CajaId"], CultureInfo.InvariantCulture),
                    Convert.ToDecimal(reader["FondoInicial"], CultureInfo.InvariantCulture),
                    Convert.ToDecimal(reader["CobrosEfectivoConservados"], CultureInfo.InvariantCulture),
                    Convert.ToDecimal(reader["Ingresos"], CultureInfo.InvariantCulture),
                    Convert.ToDecimal(reader["Retiros"], CultureInfo.InvariantCulture),
                    Convert.ToDecimal(reader["EfectivoEsperado"], CultureInfo.InvariantCulture),
                    Convert.ToDecimal(reader["EfectivoContado"], CultureInfo.InvariantCulture),
                    Convert.ToDecimal(reader["Diferencia"], CultureInfo.InvariantCulture),
                    false,
                    string.Empty);
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
