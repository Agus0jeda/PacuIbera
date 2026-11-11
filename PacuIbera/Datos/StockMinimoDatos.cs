using Datos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace PacuIbera.Datos
{
    public sealed class StockMinimoDatos : ConexionBD
    {
        public (string Nombre, bool PorPeso, decimal Minimo) ObtenerConfiguracion(int productoId)
        {
            using var conexion = ObtenerConexion();
            using var cmd = new SqlCommand(@"
                SELECT p.Nombre, p.SeVendePorPeso, ISNULL(m.StockMinimo, 0) AS Minimo
                FROM dbo.Producto p LEFT JOIN dbo.DepositoStockMinimo m ON m.ProductoId = p.Id
                WHERE p.Id = @ProductoId;", conexion);
            cmd.Parameters.Add("@ProductoId", SqlDbType.Int).Value = productoId;
            conexion.Open();
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) throw new InvalidOperationException("El producto seleccionado no existe.");
            return (reader.GetString(0), reader.GetBoolean(1), reader.GetDecimal(2));
        }
    }
}
