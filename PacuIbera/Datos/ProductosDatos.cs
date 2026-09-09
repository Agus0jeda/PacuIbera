using Datos;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace Datos
{
    public class ProductoDatos : ConexionBD
    {
        // Método para llenar el ComboBox
        public DataTable ObtenerCategorias()
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = ObtenerConexion())
            {
                // Traemos Id y Nombre de la tabla Categoria
                SqlCommand cmd = new SqlCommand("SELECT Id, Nombre FROM Categoria ORDER BY Nombre", conexion);
                conexion.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    tabla.Load(reader);
                }
            }
            return tabla;
        }

        // Método para agregar categoría nueva 
        public int RegistrarCategoria(string nombre)
        {
            using (SqlConnection conexion = ObtenerConexion())
            {
                // Usamos OUTPUT INSERTED.Id para que nos devuelva el numerito generado
                SqlCommand cmd = new SqlCommand("INSERT INTO Categoria (Nombre) OUTPUT INSERTED.Id VALUES (@Nombre)", conexion);
                cmd.Parameters.AddWithValue("@Nombre", nombre);
                conexion.Open();

                // Ejecuta y devuelve el ID nuevo
                return (int)cmd.ExecuteScalar();
            }
        }
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
     
        public void RegistrarProducto(string nombre, int categoriaId, decimal precioVenta, decimal stockMinimo, bool seVendePorPeso, string descripcion)
        {
            using (SqlConnection conexion = ObtenerConexion())
            {
                
                string query = @"INSERT INTO Producto (Nombre, CategoriaId, PrecioVenta, StockMinimo, SeVendePorPeso, Descripcion) 
                                 VALUES (@Nombre, @CategoriaId, @PrecioVenta, @StockMinimo, @SeVendePorPeso, @Descripcion)";

                SqlCommand cmd = new SqlCommand(query, conexion);
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@Nombre", nombre);
                cmd.Parameters.AddWithValue("@CategoriaId", categoriaId);
                cmd.Parameters.AddWithValue("@PrecioVenta", precioVenta);
                cmd.Parameters.AddWithValue("@StockMinimo", stockMinimo);
                cmd.Parameters.AddWithValue("@SeVendePorPeso", seVendePorPeso);
                cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrEmpty(descripcion) ? (object)DBNull.Value : descripcion);

                conexion.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}