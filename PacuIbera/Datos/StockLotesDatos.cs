using Datos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace PacuIbera.Datos
{
    public sealed class StockLotesDatos : ConexionBD
    {
        // Umbral visual; no modifica las reglas de disponibilidad ni de salida.
        public const int DiasProximoVencimiento = 30;

        public DataTable ObtenerResumenVencimientos()
        {
            using var conexion = ObtenerConexion();
            using var cmd = new SqlCommand(@"
                SELECT ds.ProductoId,
                       MIN(CASE WHEN ds.FechaVencimiento >= hoy.Fecha
                                THEN ds.FechaVencimiento END) AS ProximoVencimiento,
                       SUM(CASE WHEN ds.FechaVencimiento < hoy.Fecha
                                THEN ds.StockActual ELSE 0 END) AS CantidadVencida,
                       SUM(CASE WHEN ds.FechaVencimiento >= hoy.Fecha
                                     AND ds.FechaVencimiento < DATEADD(day, @Dias + 1, hoy.Fecha)
                                THEN ds.StockActual ELSE 0 END) AS CantidadProxima
                FROM dbo.DepositoStock ds
                CROSS JOIN (SELECT CONVERT(date, SYSDATETIME()) AS Fecha) hoy
                WHERE ds.Activo = 1 AND ds.StockActual > 0 AND ds.FechaVencimiento IS NOT NULL
                GROUP BY ds.ProductoId;", conexion);
            cmd.Parameters.Add("@Dias", SqlDbType.Int).Value = DiasProximoVencimiento;
            conexion.Open();
            using var reader = cmd.ExecuteReader();
            var tabla = new DataTable();
            tabla.Load(reader);
            return tabla;
        }

        public DataTable ObtenerLotes(int productoId)
        {
            using var conexion = ObtenerConexion();
            using var cmd = new SqlCommand(@"
                SELECT p.Nombre AS Producto, ds.Id AS LoteId, ds.StockInicial,
                       ds.StockActual, ds.FechaIngreso, ds.FechaVencimiento, ds.Activo,
                       CASE WHEN ds.Activo = 1 AND (ds.FechaVencimiento IS NULL
                                 OR ds.FechaVencimiento >= CONVERT(date, SYSDATETIME()))
                            THEN ds.StockActual ELSE CONVERT(decimal(10,3), 0) END AS Utilizable,
                       CASE WHEN ds.StockActual <= 0 THEN N'Agotado'
                            WHEN ds.FechaVencimiento < CONVERT(date, SYSDATETIME()) THEN N'Vencido'
                            WHEN ds.FechaVencimiento < DATEADD(day, @Dias + 1, CONVERT(date, SYSDATETIME()))
                                 THEN N'Próximo a vencer'
                            ELSE N'Disponible' END AS Estado
                FROM dbo.Producto p
                LEFT JOIN dbo.DepositoStock ds ON ds.ProductoId = p.Id
                WHERE p.Id = @ProductoId
                ORDER BY CASE WHEN ds.FechaVencimiento IS NULL THEN 1 ELSE 0 END,
                         ds.FechaVencimiento, ds.Id;", conexion);
            cmd.Parameters.Add("@ProductoId", SqlDbType.Int).Value = productoId;
            cmd.Parameters.Add("@Dias", SqlDbType.Int).Value = DiasProximoVencimiento;
            conexion.Open();
            using var reader = cmd.ExecuteReader();
            var tabla = new DataTable();
            tabla.Load(reader);
            return tabla;
        }
    }
}
