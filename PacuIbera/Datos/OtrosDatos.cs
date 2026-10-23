using Datos;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacuIbera.Datos
{
    public class OtrosDatos : ConexionBD
    {
        //  Obtener lista de Provincias para el ComboBox
        public DataTable ObtenerProvincias()
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_ObtenerProvincias", conexion);
                cmd.CommandType = CommandType.StoredProcedure;

                conexion.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    tabla.Load(reader);
                }
            }
            return tabla;
        }

        //  Obtener Localidades filtradas por la Provincia seleccionada
        public DataTable ObtenerLocalidadesPorProvincia(int provinciaId)
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_ObtenerLocalidadesPorProvincia", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ProvinciaId", provinciaId);

                conexion.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    tabla.Load(reader);
                }
            }
            return tabla;
        }
        public DataTable ObtenerRoles()
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_ObtenerRoles", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(tabla);
            }
            return tabla;
        }

        // Método para obtener stock del depósito central mediante SP
        public DataTable ObtenerStockDeposito()
        {
            DataTable tabla = new DataTable();
            try
            {
                using (SqlConnection conexion = ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand("sp_ObtenerStockDeposito", conexion))
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
                throw new ApplicationException("Error al obtener stock del depósito", ex);
            }
            return tabla;
        }

        public void ActualizarStockMinimoDeposito(int productoId, decimal nuevoStockMinimo)
        {
            try
            {
                using (SqlConnection conexion = ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand("sp_ActualizarStockMinimoDeposito", conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProductoId", productoId);
                    var p = new SqlParameter("@StockMinimo", System.Data.SqlDbType.Decimal);
                    p.Precision = 18;
                    p.Scale = 2;
                    p.Value = nuevoStockMinimo;
                    cmd.Parameters.Add(p);
                    conexion.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al actualizar stock mínimo del depósito", ex);
            }
        }

        public void TransferirDepositoALocal(int usuarioId, int productoId, int cantidad, DateTime? fechaVencimiento)
        {
            try
            {
                using (SqlConnection conexion = ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand("sp_TransferirDepositoALocal", conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                    cmd.Parameters.AddWithValue("@ProductoId", productoId);
                    cmd.Parameters.AddWithValue("@Cantidad", cantidad);
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
                throw new ApplicationException("Error al transferir del depósito al local", ex);
            }
        }

    }
}
