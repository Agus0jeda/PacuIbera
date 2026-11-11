using System.Data;

namespace PacuIbera.UI.Common
{
    // Consulta de valores historicos; no contiene acciones de escritura.
    public sealed class FormDetalleRecepcion : Form
    {
        public FormDetalleRecepcion(DataSet datos)
        {
            Text = "Detalle de recepción";
            Font = new Font("Segoe UI", 9F);
            Size = new Size(1000, 700);
            MinimumSize = new Size(780, 540);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            var fila = datos.Tables[0].Rows[0];
            var cabecera = new Label
            {
                Text = $"RECEPCIÓN N.º {fila["RecepcionId"]} · PEDIDO N.º {fila["PedidoId"]}\nProveedor: {fila["Proveedor"]}\nRecibió: {fila["Usuario"]} · {Convert.ToDateTime(fila["FechaRecepcion"]):dd/MM/yyyy HH:mm:ss}\nEstado: {fila["Estado"]}",
                Dock = DockStyle.Fill, Padding = new Padding(12),
                BackColor = Color.FromArgb(45, 55, 65), ForeColor = Color.White,
                Font = new Font(Font, FontStyle.Bold), AutoEllipsis = true
            };
            var productos = CrearTabla(datos.Tables[1]);
            var lotes = CrearTabla(datos.Tables[2]);
            productos.Columns["DetalleRecepcionId"].Visible = false;
            lotes.Columns["DetalleRecepcionId"].Visible = false;
            lotes.Columns["Vencimiento"].DefaultCellStyle.Format = "dd/MM/yyyy";
            lotes.Columns["Vencimiento"].DefaultCellStyle.NullValue = "Sin vencimiento";
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            layout.Controls.Add(cabecera, 0, 0);
            layout.Controls.Add(Titulo("PRODUCTOS · Cantidades registradas al recibir"), 0, 1);
            layout.Controls.Add(productos, 0, 2);
            layout.Controls.Add(Titulo(datos.Tables[2].Rows.Count == 0 ? "LOTES · Sin lotes registrados" : "LOTES REGISTRADOS POR PRODUCTO"), 0, 3);
            layout.Controls.Add(lotes, 0, 4);
            var pie = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(10), BackColor = Color.WhiteSmoke };
            var cerrar = new Button { Text = "Cerrar", AutoSize = true, DialogResult = DialogResult.Cancel };
            EstiloUI.AplicarEstiloBotonAccion(cerrar, EstiloUI.TipoAccion.Editar);
            pie.Controls.Add(cerrar);
            layout.Controls.Add(pie, 0, 5);
            Controls.Add(layout);
            CancelButton = cerrar;
        }

        private static Label Titulo(string texto) => new Label
        {
            Text = texto, Dock = DockStyle.Fill, Padding = new Padding(12, 6, 0, 0),
            ForeColor = Color.FromArgb(45, 55, 65)
        };

        internal static DataGridView CrearTabla(DataTable? datos = null)
        {
            var tabla = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                AllowUserToDeleteRows = false, MultiSelect = false, RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BindingContext = new BindingContext(), DataSource = datos,
                ScrollBars = ScrollBars.Both, BackgroundColor = Color.White
            };
            EstiloUI.AplicarEstiloGrid(tabla);
            foreach (DataGridViewColumn columna in tabla.Columns)
            {
                columna.MinimumWidth = columna.Name == "Producto" ? 230 : 110;
                if (columna.ValueType == typeof(decimal))
                {
                    columna.DefaultCellStyle.Format = "0.###";
                    columna.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
            return tabla;
        }
    }
}
