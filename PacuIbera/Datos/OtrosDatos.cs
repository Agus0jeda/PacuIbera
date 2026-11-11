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
                    // Atributo de presentacion: conservar la consulta y sus cantidades decimales.
                    using (var unidades = new SqlCommand("SELECT Id, SeVendePorPeso FROM dbo.Producto;", conexion))
                    using (var reader = unidades.ExecuteReader())
                    {
                        var porPeso = new Dictionary<int, bool>();
                        while (reader.Read()) porPeso.Add(reader.GetInt32(0), reader.GetBoolean(1));
                        tabla.Columns.Add("SeVendePorPeso", typeof(bool));
                        foreach (DataRow fila in tabla.Rows)
                            fila["SeVendePorPeso"] = porPeso[Convert.ToInt32(fila["ProductoId"])];
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
            if (nuevoStockMinimo < 0 || nuevoStockMinimo > 9999999.999M
                || decimal.Round(nuevoStockMinimo, 3) != nuevoStockMinimo)
                throw new ArgumentException("El mínimo debe estar entre 0 y 9999999,999 y tener hasta tres decimales.");
            try
            {
                using (SqlConnection conexion = ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand("sp_ActualizarStockMinimoDeposito", conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProductoId", productoId);
                    var p = new SqlParameter("@StockMinimo", System.Data.SqlDbType.Decimal);
                    p.Precision = 10;
                    p.Scale = 3;
                    p.Value = nuevoStockMinimo;
                    cmd.Parameters.Add(p);
                    conexion.Open();
                    // Evitar que el procedimiento anterior redondee el minimo a dos decimales.
                    using (var verificar = new SqlCommand(@"
                        SELECT COUNT(*) FROM sys.parameters
                        WHERE object_id = OBJECT_ID(N'dbo.sp_ActualizarStockMinimoDeposito')
                          AND name = N'@StockMinimo' AND TYPE_NAME(user_type_id) = N'decimal'
                          AND precision = 10 AND scale = 3;", conexion))
                    {
                        if (Convert.ToInt32(verificar.ExecuteScalar()) != 1)
                            throw new InvalidOperationException("Debe aplicar el script de Stock mínimo decimal(10,3) en la base seleccionada antes de guardar.");
                    }
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("No se pudo guardar el mínimo: " + ex.Message, ex);
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
