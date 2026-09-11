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
            string stringConexion = "Server=(localdb)\\MSSQLLocalDB; DataBase=PacuIberaDB; Integrated Security=True; TrustServerCertificate=True;";
            using (Microsoft.Data.SqlClient.SqlConnection conexion = new Microsoft.Data.SqlClient.SqlConnection(stringConexion))
            {
                // Filtramos con WHERE p.Activo = 1
                string query = @"SELECT p.Id, p.Nombre, c.Nombre AS Categoria, p.PrecioVenta, p.StockMinimo, p.SeVendePorPeso, p.Descripcion 
                                 FROM Producto p 
                                 LEFT JOIN Categoria c ON p.CategoriaId = c.Id
                                 WHERE p.Activo = 1";
                Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand(query, conexion);
                conexion.Open();
                using (Microsoft.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader())
                {
                    tabla.Load(reader);
                }
            }
            return tabla;
        }

        // 2. NUEVO: Método para Editar
        public void ModificarProducto(int id, string nombre, int categoriaId, decimal precioVenta, decimal stockMinimo, bool seVendePorPeso, string descripcion)
        {
            string stringConexion = "Server=(localdb)\\MSSQLLocalDB; DataBase=PacuIberaDB; Integrated Security=True; TrustServerCertificate=True;";
            using (Microsoft.Data.SqlClient.SqlConnection conexion = new Microsoft.Data.SqlClient.SqlConnection(stringConexion))
            {
                string query = @"UPDATE Producto SET Nombre = @Nombre, CategoriaId = @CategoriaId, PrecioVenta = @PrecioVenta, 
                                 StockMinimo = @StockMinimo, SeVendePorPeso = @SeVendePorPeso, Descripcion = @Descripcion 
                                 WHERE Id = @Id";
                Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@Id", id);
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

        // 3. NUEVO: Método para Baja Lógica
        public void EliminarProducto(int id)
        {
            string stringConexion = "Server=(localdb)\\MSSQLLocalDB; DataBase=PacuIberaDB; Integrated Security=True; TrustServerCertificate=True;";
            using (Microsoft.Data.SqlClient.SqlConnection conexion = new Microsoft.Data.SqlClient.SqlConnection(stringConexion))
            {
                // Baja lógica: no lo borramos, solo lo apagamos (Activo = 0)
                string query = "UPDATE Producto SET Activo = 0 WHERE Id = @Id";
                Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@Id", id);
                conexion.Open();
                cmd.ExecuteNonQuery();
            }
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