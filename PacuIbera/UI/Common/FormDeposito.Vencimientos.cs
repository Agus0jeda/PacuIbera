using System.Data;
using PacuIbera.Datos;

namespace PacuIbera.UI.Common
{
    public partial class FormDeposito
    {
        private static void AgregarResumenVencimientos(DataTable stock)
        {
            DataTable resumen = new StockLotesDatos().ObtenerResumenVencimientos();
            var porProducto = resumen.AsEnumerable().ToDictionary(f => f.Field<int>("ProductoId"));
            stock.Columns.Add("ProximoVencimiento", typeof(DateTime));
            stock.Columns.Add("EstadoVencimiento", typeof(string));
            stock.Columns.Add("CantidadVencida", typeof(decimal));
            stock.Columns.Add("CantidadProxima", typeof(decimal));
            foreach (DataRow producto in stock.Rows)
            {
                porProducto.TryGetValue(producto.Field<int>("ProductoId"), out DataRow? lotes);
                decimal vencida = lotes?.Field<decimal>("CantidadVencida") ?? 0;
                decimal proxima = lotes?.Field<decimal>("CantidadProxima") ?? 0;
                producto["ProximoVencimiento"] = lotes?["ProximoVencimiento"] ?? DBNull.Value;
                producto["CantidadVencida"] = vencida;
                producto["CantidadProxima"] = proxima;
                producto["EstadoVencimiento"] = vencida > 0 ? "VENCIDO" : proxima > 0 ? "PRÓXIMO A VENCER" : "SIN ALERTA";
            }
        }
    }
}
