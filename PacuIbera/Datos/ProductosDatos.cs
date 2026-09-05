using Datos;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace Datos
{
    public class ProductoDatos : ConexionBD
    {
        // Método para llenar la grilla de productos
        public DataTable ObtenerProductos()
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_ObtenerProductos", conexion);
                cmd.CommandType = CommandType.StoredProcedure;

                conexion.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    tabla.Load(reader); // Carga los datos directamente en la tabla
                }
            }
            return tabla;
        }

        // Método para guardar un nuevo producto
        public void RegistrarProducto(string nombre, int categoriaId, decimal precioVenta, decimal stockMinimo, bool seVendePorPeso)
        {
            using (SqlConnection conexion = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_InsertarProducto", conexion);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Nombre", nombre);
                cmd.Parameters.AddWithValue("@CategoriaId", categoriaId);
                cmd.Parameters.AddWithValue("@PrecioVenta", precioVenta);
                cmd.Parameters.AddWithValue("@StockMinimo", stockMinimo);
                cmd.Parameters.AddWithValue("@SeVendePorPeso", seVendePorPeso);

                conexion.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}