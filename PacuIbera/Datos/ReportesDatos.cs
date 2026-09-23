using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Datos
{
    public class ReportesDatos : ConexionBD
    {
        public DataTable ObtenerTopProductos(DateTime desde, DateTime hasta)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_ReporteTopProductos", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FechaDesde", desde);
                cmd.Parameters.AddWithValue("@FechaHasta", hasta);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        public DataTable ObtenerMetodosPago(DateTime desde, DateTime hasta)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_ReporteMetodosPago", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FechaDesde", desde);
                cmd.Parameters.AddWithValue("@FechaHasta", hasta);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        public DataTable ObtenerStockCritico()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_ReporteStockCritico", con);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        public DataTable ObtenerLotesVencidos()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_ReporteLotesVencidos", con);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        public DataTable ObtenerResumenGlobal(DateTime desde, DateTime hasta)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conexion = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_ReporteResumenGlobal", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FechaDesde", desde);
                cmd.Parameters.AddWithValue("@FechaHasta", hasta);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        public DataTable ObtenerRankingVendedores(DateTime desde, DateTime hasta)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conexion = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_ReporteRankingVendedores", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FechaDesde", desde);
                cmd.Parameters.AddWithValue("@FechaHasta", hasta);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        public DataTable ObtenerAuditoriaAnulaciones(DateTime desde, DateTime hasta)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conexion = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_ReporteAuditoria", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FechaDesde", desde);
                cmd.Parameters.AddWithValue("@FechaHasta", hasta);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        public DataTable ObtenerCurvaVentas(DateTime desde, DateTime hasta)
        {
            DataTable dt = new DataTable();
            using (Microsoft.Data.SqlClient.SqlConnection conexion = ObtenerConexion())
            {
                Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand("sp_AnalisisCurvaVentas", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FechaDesde", desde);
                cmd.Parameters.AddWithValue("@FechaHasta", hasta);
                Microsoft.Data.SqlClient.SqlDataAdapter da = new Microsoft.Data.SqlClient.SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        public DataTable ObtenerPerdidaVencimientos()
        {
            DataTable dt = new DataTable();
            using (Microsoft.Data.SqlClient.SqlConnection conexion = ObtenerConexion())
            {
                Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand("sp_AnalisisPerdidaVencimientos", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                Microsoft.Data.SqlClient.SqlDataAdapter da = new Microsoft.Data.SqlClient.SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }
    }
}