using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Datos
{
    public class ReportesDatos : ConexionBD
    {
        public DataTable ObtenerTopProductos()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_ReporteTopProductos", con);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        public DataTable ObtenerMetodosPagoHoy()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_ReporteMetodosPago", con);
                cmd.CommandType = CommandType.StoredProcedure;
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

        // --- NUEVA FUNCIÓN PARA LOS VENCIMIENTOS ---
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
    }
}