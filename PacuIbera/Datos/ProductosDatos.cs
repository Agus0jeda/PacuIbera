using System;
using System.Data;
using Microsoft.Data.SqlClient;

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
                SqlCommand cmd = new SqlCommand("SELECT Id, Nombre FROM Categoria ORDER BY Nombre", conexion);
                conexion.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    tabla.Load(reader);
                }
            }
            return tabla;
        }

        public void RegistrarIngresoLote(int productoId, decimal cantidad, DateTime fechaVencimiento)
        {
            string stringConexion = "Server=(localdb)\\MSSQLLocalDB; DataBase=PacuIberaDB; Integrated Security=True; TrustServerCertificate=True;";
           // string stringConexion = "Server=AGUS\\SQLEXPRESS; DataBase=PacuIberaDB; Integrated Security=True; TrustServerCertificate=True;";

            using (SqlConnection conexion = new SqlConnection(stringConexion))
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        // 1. Insertamos el lote nuevo
                        string queryLote = @"INSERT INTO Lote (ProductoId, FechaIngreso, FechaVencimiento, StockInicial, StockActual, Activo) 
                                             VALUES (@ProdId, GETDATE(), @Venc, @Cant, @Cant, 1)";
                        SqlCommand cmdLote = new SqlCommand(queryLote, conexion, transaccion);
                        cmdLote.Parameters.AddWithValue("@ProdId", productoId);
                        cmdLote.Parameters.AddWithValue("@Venc", fechaVencimiento);
                        cmdLote.Parameters.AddWithValue("@Cant", cantidad);
                        cmdLote.ExecuteNonQuery();

                        // 2. Sumamos esa cantidad al total del producto
                        string queryProd = "UPDATE Producto SET StockActual = StockActual + @Cant WHERE Id = @ProdId";
                        SqlCommand cmdProd = new SqlCommand(queryProd, conexion, transaccion);
                        cmdProd.Parameters.AddWithValue("@ProdId", productoId);
                        cmdProd.Parameters.AddWithValue("@Cant", cantidad);
                        cmdProd.ExecuteNonQuery();

                        transaccion.Commit();
                    }
                    catch
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
            }
        }

        // Método para agregar categoría nueva 
        public int RegistrarCategoria(string nombre)
        {
            using (SqlConnection conexion = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("INSERT INTO Categoria (Nombre) OUTPUT INSERTED.Id VALUES (@Nombre)", conexion);
                cmd.Parameters.AddWithValue("@Nombre", nombre);
                conexion.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        // Método para llenar la grilla de productos
        public DataTable ObtenerProductos()
        {
            DataTable tabla = new DataTable();
            string stringConexion = "Server=(localdb)\\MSSQLLocalDB; DataBase=PacuIberaDB; Integrated Security=True; TrustServerCertificate=True;";
           // string stringConexion = "Server=AGUS\\SQLEXPRESS; DataBase=PacuIberaDB; Integrated Security=True; TrustServerCertificate=True;";

            using (SqlConnection conexion = new SqlConnection(stringConexion))
            {
                string query = @"SELECT p.Id, p.Nombre, c.Nombre AS Categoria, p.PrecioVenta, 
                                        p.StockActual, p.StockMinimo, p.SeVendePorPeso, p.Descripcion,
                                        (SELECT COUNT(*) FROM Lote l WHERE l.ProductoId = p.Id AND l.Activo = 1 AND l.FechaVencimiento <= DATEADD(day, 15, GETDATE())) AS LotesPorVencer,
                                        (SELECT MIN(FechaVencimiento) FROM Lote l WHERE l.ProductoId = p.Id AND l.Activo = 1 AND l.StockActual > 0) AS ProximoVencimiento
                                 FROM Producto p 
                                 LEFT JOIN Categoria c ON p.CategoriaId = c.Id
                                 WHERE p.Activo = 1";
                SqlCommand cmd = new SqlCommand(query, conexion);
                conexion.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    tabla.Load(reader);
                }
            }
            return tabla;
        }

        // Método para ver los Lotes de un producto específico
        public DataTable ObtenerLotesPorProducto(int productoId)
        {
            DataTable tabla = new DataTable();
            string stringConexion = "Server=(localdb)\\MSSQLLocalDB; DataBase=PacuIberaDB; Integrated Security=True; TrustServerCertificate=True;";
            //string stringConexion = "Server=AGUS\\SQLEXPRESS; DataBase=PacuIberaDB; Integrated Security=True; TrustServerCertificate=True;";

            using (Microsoft.Data.SqlClient.SqlConnection conexion = new Microsoft.Data.SqlClient.SqlConnection(stringConexion))
            {
                string query = @"SELECT Id AS [Lote N°], FechaIngreso AS [Ingreso], FechaVencimiento AS [Vencimiento], StockActual AS [Quedan] 
                                 FROM Lote WHERE ProductoId = @ProdId AND Activo = 1 ORDER BY FechaVencimiento ASC";
                Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@ProdId", productoId);
                conexion.Open();
                using (Microsoft.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader())
                {
                    tabla.Load(reader);
                }
            }
            return tabla;
        }

        public void ModificarProducto(int id, string nombre, int categoriaId, decimal precioVenta, decimal stockMinimo, bool seVendePorPeso, string descripcion, decimal stockNuevo, DateTime? fechaVencimiento)
        {
            string stringConexion = "Server=(localdb)\\MSSQLLocalDB; DataBase=PacuIberaDB; Integrated Security=True; TrustServerCertificate=True;";
            //string stringConexion = "Server=AGUS\\SQLEXPRESS; DataBase=PacuIberaDB; Integrated Security=True; TrustServerCertificate=True;";

            using (Microsoft.Data.SqlClient.SqlConnection conexion = new Microsoft.Data.SqlClient.SqlConnection(stringConexion))
            {
                conexion.Open();

                // Actualizamos los datos. Al StockActual que ya tiene, le SUMAMOS el stockNuevo
                string query = @"UPDATE Producto SET Nombre = @Nombre, CategoriaId = @CategoriaId, PrecioVenta = @PrecioVenta, 
                                 StockMinimo = @StockMinimo, SeVendePorPeso = @SeVendePorPeso, Descripcion = @Descripcion, 
                                 StockActual = StockActual + @StockNuevo 
                                 WHERE Id = @Id";
                Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Nombre", nombre.ToUpper());
                cmd.Parameters.AddWithValue("@CategoriaId", categoriaId);
                cmd.Parameters.AddWithValue("@PrecioVenta", precioVenta);
                cmd.Parameters.AddWithValue("@StockMinimo", stockMinimo);
                cmd.Parameters.AddWithValue("@SeVendePorPeso", seVendePorPeso);
                cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrEmpty(descripcion) ? (object)DBNull.Value : descripcion);
                cmd.Parameters.AddWithValue("@StockNuevo", stockNuevo);
                cmd.ExecuteNonQuery();

                // Si ingresó stock nuevo, agregamos un lote nuevo a la lista (NO borramos los anteriores)
                if (stockNuevo > 0)
                {
                    string queryLote = "INSERT INTO Lote (ProductoId, FechaIngreso, FechaVencimiento, StockInicial, StockActual, Activo) VALUES (@Id, GETDATE(), @Venc, @Stock, @Stock, 1)";
                    Microsoft.Data.SqlClient.SqlCommand cmdLote = new Microsoft.Data.SqlClient.SqlCommand(queryLote, conexion);
                    cmdLote.Parameters.AddWithValue("@Id", id);
                    cmdLote.Parameters.AddWithValue("@Venc", (object)fechaVencimiento ?? DBNull.Value);
                    cmdLote.Parameters.AddWithValue("@Stock", stockNuevo);
                    cmdLote.ExecuteNonQuery();
                }
            }
        }

        public void EliminarProducto(int id)
        {
            string stringConexion = "Server=(localdb)\\MSSQLLocalDB; DataBase=PacuIberaDB; Integrated Security=True; TrustServerCertificate=True;";
           // string stringConexion = "Server=AGUS\\SQLEXPRESS; DataBase=PacuIberaDB; Integrated Security=True; TrustServerCertificate=True;";

            using (SqlConnection conexion = new SqlConnection(stringConexion))
            {
                string query = "UPDATE Producto SET Activo = 0 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@Id", id);
                conexion.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void RegistrarProducto(string nombre, int categoriaId, decimal precioVenta, decimal stockMinimo, bool seVendePorPeso, string descripcion, decimal stock, DateTime? fechaVencimiento)
        {
            string stringConexion = "Server=(localdb)\\MSSQLLocalDB; DataBase=PacuIberaDB; Integrated Security=True; TrustServerCertificate=True;";
            //string stringConexion = "Server=AGUS\\SQLEXPRESS; DataBase=PacuIberaDB; Integrated Security=True; TrustServerCertificate=True;";

            using (Microsoft.Data.SqlClient.SqlConnection conexion = new Microsoft.Data.SqlClient.SqlConnection(stringConexion))
            {
                conexion.Open();
                string query = @"INSERT INTO Producto (Nombre, CategoriaId, PrecioVenta, StockMinimo, SeVendePorPeso, Descripcion, StockActual, Activo) 
                                 OUTPUT INSERTED.Id 
                                 VALUES (@Nombre, @CategoriaId, @PrecioVenta, @StockMinimo, @SeVendePorPeso, @Descripcion, @Stock, 1)";
                Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@Nombre", nombre.ToUpper());
                cmd.Parameters.AddWithValue("@CategoriaId", categoriaId);
                cmd.Parameters.AddWithValue("@PrecioVenta", precioVenta);
                cmd.Parameters.AddWithValue("@StockMinimo", stockMinimo);
                cmd.Parameters.AddWithValue("@SeVendePorPeso", seVendePorPeso);
                cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrEmpty(descripcion) ? (object)DBNull.Value : descripcion);
                cmd.Parameters.AddWithValue("@Stock", stock);

                int nuevoId = (int)cmd.ExecuteScalar();

                // Si cargó stock inicial, creamos el lote
                if (stock > 0)
                {
                    string queryLote = "INSERT INTO Lote (ProductoId, FechaIngreso, FechaVencimiento, StockInicial, StockActual, Activo) VALUES (@Id, GETDATE(), @Venc, @Stock, @Stock, 1)";
                    Microsoft.Data.SqlClient.SqlCommand cmdLote = new Microsoft.Data.SqlClient.SqlCommand(queryLote, conexion);
                    cmdLote.Parameters.AddWithValue("@Id", nuevoId);
                    cmdLote.Parameters.AddWithValue("@Venc", (object)fechaVencimiento ?? DBNull.Value);
                    cmdLote.Parameters.AddWithValue("@Stock", stock);
                    cmdLote.ExecuteNonQuery();
                }
            }
        }
        public void ProcesarLotesVencidos()
        {
            string stringConexion = "Server=(localdb)\\MSSQLLocalDB; DataBase=PacuIberaDB; Integrated Security=True; TrustServerCertificate=True;";
           // string stringConexion = "Server=AGUS\\SQLEXPRESS; DataBase=PacuIberaDB; Integrated Security=True; TrustServerCertificate=True;";

            using (Microsoft.Data.SqlClient.SqlConnection conexion = new Microsoft.Data.SqlClient.SqlConnection(stringConexion))
            {
                conexion.Open();
                // 1. Resta al Producto el stock exacto de los lotes que se vencieron hoy o antes
                string queryUpdateProducto = @"
            UPDATE p
            SET p.StockActual = p.StockActual - l.StockActual
            FROM Producto p
            INNER JOIN Lote l ON p.Id = l.ProductoId
            WHERE l.Activo = 1 AND l.StockActual > 0 AND l.FechaVencimiento < CAST(GETDATE() AS DATE)";

                Microsoft.Data.SqlClient.SqlCommand cmdProd = new Microsoft.Data.SqlClient.SqlCommand(queryUpdateProducto, conexion);
                cmdProd.ExecuteNonQuery();

                // 2. Desactiva el lote para que ya no aparezca en las ventas
                string queryBajaLote = @"
            UPDATE Lote 
            SET Activo = 0 
            WHERE Activo = 1 AND StockActual > 0 AND FechaVencimiento < CAST(GETDATE() AS DATE)";

                Microsoft.Data.SqlClient.SqlCommand cmdLote = new Microsoft.Data.SqlClient.SqlCommand(queryBajaLote, conexion);
                cmdLote.ExecuteNonQuery();
            }
        }

        public void ActualizarLote(int loteId, int productoId, decimal nuevoStock, DateTime nuevaFecha)
        {
            string stringConexion = "Server=(localdb)\\MSSQLLocalDB; DataBase=PacuIberaDB; Integrated Security=True; TrustServerCertificate=True;";
            //string stringConexion = "Server=AGUS\\SQLEXPRESS; DataBase=PacuIberaDB; Integrated Security=True; TrustServerCertificate=True;";

            using (Microsoft.Data.SqlClient.SqlConnection conexion = new Microsoft.Data.SqlClient.SqlConnection(stringConexion))
            {
                conexion.Open();
                using (Microsoft.Data.SqlClient.SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        // 1. Obtener el stock anterior del lote para saber la diferencia
                        string queryAnterior = "SELECT StockActual FROM Lote WHERE Id = @LoteId";
                        Microsoft.Data.SqlClient.SqlCommand cmdAnterior = new Microsoft.Data.SqlClient.SqlCommand(queryAnterior, conexion, transaccion);
                        cmdAnterior.Parameters.AddWithValue("@LoteId", loteId);
                        decimal stockAnterior = Convert.ToDecimal(cmdAnterior.ExecuteScalar());

                        decimal diferencia = nuevoStock - stockAnterior;

                        // 2. Actualizar el Lote
                        string queryLote = "UPDATE Lote SET StockActual = @NuevoStock, FechaVencimiento = @NuevaFecha WHERE Id = @LoteId";
                        Microsoft.Data.SqlClient.SqlCommand cmdLote = new Microsoft.Data.SqlClient.SqlCommand(queryLote, conexion, transaccion);
                        cmdLote.Parameters.AddWithValue("@NuevoStock", nuevoStock);
                        cmdLote.Parameters.AddWithValue("@NuevaFecha", nuevaFecha);
                        cmdLote.Parameters.AddWithValue("@LoteId", loteId);
                        cmdLote.ExecuteNonQuery();

                        // 3. Ajustar el stock total del Producto
                        string queryProd = "UPDATE Producto SET StockActual = StockActual + @Diferencia WHERE Id = @ProductoId";
                        Microsoft.Data.SqlClient.SqlCommand cmdProd = new Microsoft.Data.SqlClient.SqlCommand(queryProd, conexion, transaccion);
                        cmdProd.Parameters.AddWithValue("@Diferencia", diferencia);
                        cmdProd.Parameters.AddWithValue("@ProductoId", productoId);
                        cmdProd.ExecuteNonQuery();

                        transaccion.Commit();
                    }
                    catch
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}