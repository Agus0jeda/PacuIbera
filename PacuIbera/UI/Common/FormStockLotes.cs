using System.Data;
using PacuIbera.Datos;

namespace PacuIbera.UI.Common
{
    public sealed class FormStockLotes : Form
    {
        public FormStockLotes(DataTable datos)
        {
            Text = "Stock por lote";
            Font = new Font("Segoe UI", 9F);
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1100, 650);
            MinimumSize = new Size(850, 500);
            BackColor = Color.White;
            string producto = Convert.ToString(datos.Rows[0]["Producto"]) ?? "";
            // La fila sin lote del LEFT JOIN permite identificar productos sin existencias.
            DataTable lotes = datos.Clone();
            foreach (DataRow fila in datos.Rows)
                if (!fila.IsNull("LoteId")) lotes.ImportRow(fila);
            decimal fisico = lotes.AsEnumerable().Sum(f => f.Field<decimal>("StockActual"));
            decimal utilizable = lotes.AsEnumerable().Sum(f => f.Field<decimal>("Utilizable"));
            var cabecera = new Label
            {
                Text = $"STOCK POR LOTE · {producto}", Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(45, 55, 65), ForeColor = Color.White,
                Font = new Font(Font, FontStyle.Bold), Padding = new Padding(14), AutoEllipsis = true
            };
            var totales = new Label
            {
                Text = $"Stock actual utilizable: {utilizable:0.###}   ·   Saldo físico registrado: {fisico:0.###}\nEl saldo físico incluye vencidos e inactivos. El utilizable suma solo lotes activos vigentes.",
                Dock = DockStyle.Fill, Padding = new Padding(12),
                BackColor = Color.FromArgb(250, 248, 243), ForeColor = Color.FromArgb(45, 55, 65)
            };
            var tabla = FormDetalleRecepcion.CrearTabla(lotes);
            tabla.Columns["Producto"].Visible = false;
            tabla.Columns["LoteId"].HeaderText = "Lote";
            tabla.Columns["StockInicial"].HeaderText = "Cantidad inicial";
            tabla.Columns["StockActual"].HeaderText = "Saldo del lote";
            tabla.Columns["Utilizable"].HeaderText = "Disponible utilizable";
            tabla.Columns["FechaIngreso"].HeaderText = "Fecha de ingreso";
            tabla.Columns["FechaVencimiento"].HeaderText = "Vencimiento";
            tabla.Columns["FechaIngreso"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            tabla.Columns["FechaIngreso"].DefaultCellStyle.NullValue = "Sin registro";
            tabla.Columns["FechaVencimiento"].DefaultCellStyle.Format = "dd/MM/yyyy";
            tabla.Columns["FechaVencimiento"].DefaultCellStyle.NullValue = "Sin vencimiento";
            tabla.Columns["Estado"].MinimumWidth = 155;
            // Conservar el orden por vencimiento de la consulta.
            foreach (DataGridViewColumn columna in tabla.Columns)
                columna.SortMode = DataGridViewColumnSortMode.NotSortable;
            tabla.CellFormatting += (_, e) =>
            {
                if (e.RowIndex < 0 || e.CellStyle == null) return;
                var fila = (DataRowView)tabla.Rows[e.RowIndex].DataBoundItem;
                string estado = Convert.ToString(fila["Estado"]) ?? "";
                Color fondo = estado == "Vencido" ? Color.MistyRose
                    : estado == "Próximo a vencer" ? Color.LightGoldenrodYellow : Color.White;
                e.CellStyle.BackColor = fondo;
                e.CellStyle.ForeColor = estado == "Vencido" ? Color.Maroon : Color.FromArgb(64, 64, 64);
                e.CellStyle.SelectionBackColor = estado == "Vencido" ? Color.Maroon : Color.FromArgb(45, 55, 65);
                e.CellStyle.SelectionForeColor = Color.White;
            };
            var nota = new Label
            {
                Text = lotes.Rows.Count == 0 ? "Este producto no tiene lotes registrados."
                    : $"Próximo a vencer: desde hoy hasta {StockLotesDatos.DiasProximoVencimiento} días inclusive. Se incluyen lotes agotados e inactivos; los inactivos no son utilizables.",
                Dock = DockStyle.Fill, Padding = new Padding(12, 8, 12, 0)
            };
            var pie = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(10), BackColor = Color.WhiteSmoke };
            var cerrar = new Button { Text = "Cerrar", AutoSize = true, DialogResult = DialogResult.Cancel };
            EstiloUI.AplicarEstiloBotonAccion(cerrar, EstiloUI.TipoAccion.Editar);
            pie.Controls.Add(cerrar);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            layout.Controls.Add(cabecera, 0, 0);
            layout.Controls.Add(totales, 0, 1);
            layout.Controls.Add(tabla, 0, 2);
            layout.Controls.Add(nota, 0, 3);
            layout.Controls.Add(pie, 0, 4);
            Controls.Add(layout);
            CancelButton = cerrar;
        }
    }
}
