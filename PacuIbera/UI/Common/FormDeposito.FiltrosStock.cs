using Datos;
using System.Data;

namespace PacuIbera.UI.Common
{
    public partial class FormDeposito
    {
        private ComboBox cbCategoriaStock = null!;
        private ComboBox cbEstadoStock = null!;
        private ComboBox cbVencimientoStock = null!;
        private bool cargandoCategoriasStock;

        private void InicializarFiltrosStock()
        {
            panelHeader.Controls.Clear();
            panelHeader.Height = 146;
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 3, Padding = new Padding(10)
            };
            // Ancho compartido por Buscar, Categoria y Vencimientos; los combos conservan su alineacion.
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 85));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            lblFiltrar.Dock = DockStyle.Fill;
            lblFiltrar.TextAlign = ContentAlignment.MiddleLeft;
            txtFiltrar.Dock = DockStyle.Fill;
            txtFiltrar.Margin = new Padding(3, 6, 3, 3);
            var actualizar = new Button { Text = "Actualizar", Dock = DockStyle.Fill };
            EstiloUI.AplicarEstiloBotonAccion(actualizar, EstiloUI.TipoAccion.Positivo);
            actualizar.Click += (_, _) => CargarStock();
            layout.Controls.Add(lblFiltrar, 0, 0);
            layout.Controls.Add(txtFiltrar, 1, 0);
            layout.SetColumnSpan(txtFiltrar, 2);
            layout.Controls.Add(actualizar, 3, 0);
            cbCategoriaStock = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
            cbEstadoStock = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
            cbEstadoStock.Items.AddRange(new object[] {
                "Todos los estados", "SIN STOCK", "STOCK BAJO", "EN EL MÍNIMO", "STOCK SUFICIENTE"
            });
            cbEstadoStock.SelectedIndex = 0;
            cbCategoriaStock.SelectedIndexChanged += (_, _) => AplicarFiltrosStock(ProductoStockSeleccionado());
            cbEstadoStock.SelectedIndexChanged += (_, _) => AplicarFiltrosStock(ProductoStockSeleccionado());
            layout.Controls.Add(new Label { Text = "Categoría:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
            layout.Controls.Add(cbCategoriaStock, 1, 1);
            layout.Controls.Add(new Label { Text = "Estado:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 2, 1);
            layout.Controls.Add(cbEstadoStock, 3, 1);
            cbVencimientoStock = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
            cbVencimientoStock.Items.AddRange(new object[] { "Todos", "Próximos a vencer", "Con lotes vencidos", "Sin alerta" });
            cbVencimientoStock.SelectedIndex = 0;
            cbVencimientoStock.SelectedIndexChanged += (_, _) => AplicarFiltrosStock(ProductoStockSeleccionado());
            layout.Controls.Add(new Label { Text = "Vencimientos:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
            layout.Controls.Add(cbVencimientoStock, 1, 2);
            var umbral = new Label { Text = "Próximos: hoy a 30 días inclusive", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            layout.Controls.Add(umbral, 2, 2);
            layout.SetColumnSpan(umbral, 2);
            panelHeader.Controls.Add(layout);
        }

        private void CargarCategoriasStock()
        {
            int? seleccionada = cbCategoriaStock.SelectedIndex > 0 && cbCategoriaStock.SelectedItem is DataRowView fila
                ? Convert.ToInt32(fila["Id"]) : null;
            // Reutilizar exactamente la consulta de categorias del modulo Productos.
            DataTable categorias = new ProductoDatos().ObtenerCategorias();
            var todas = categorias.NewRow();
            todas["Id"] = -1; // Identificador solo visual; nunca se envia a SQL.
            todas["Nombre"] = "Todas las categorías";
            categorias.Rows.InsertAt(todas, 0);
            cargandoCategoriasStock = true;
            try
            {
                cbCategoriaStock.DisplayMember = "Nombre";
                cbCategoriaStock.ValueMember = "Id";
                cbCategoriaStock.DataSource = categorias;
                cbCategoriaStock.SelectedIndex = 0;
                if (seleccionada.HasValue)
                {
                    for (int i = 1; i < categorias.Rows.Count; i++)
                        if (Convert.ToInt32(categorias.Rows[i]["Id"]) == seleccionada.Value)
                            cbCategoriaStock.SelectedIndex = i;
                }
            }
            finally { cargandoCategoriasStock = false; }
        }

        private int? ProductoStockSeleccionado() => dgvStock.CurrentRow?.DataBoundItem is DataRowView fila
            ? Convert.ToInt32(fila["ProductoId"]) : null;

        private static string EscaparTextoStock(string texto) => texto.Replace("'", "''");

        private void AplicarFiltrosStock(int? seleccionado)
        {
            if (cargandoCategoriasStock || tablaStock == null) return;
            var filtros = new List<string>();
            string texto = txtFiltrar.Text.Trim();
            if (texto.Length > 0)
            {
                string literal = string.Concat(texto.Select(c => c switch
                {
                    '\'' => "''", '[' => "[[]", ']' => "[]]", '%' => "[%]", '*' => "[*]", _ => c.ToString()
                }));
                filtros.Add($"(ISNULL([Nombre], '') LIKE '%{literal}%' OR ISNULL([Descripcion], '') LIKE '%{literal}%')");
            }
            if (cbCategoriaStock.SelectedIndex > 0 && cbCategoriaStock.SelectedItem is DataRowView categoria)
                filtros.Add($"[Categoria] = '{EscaparTextoStock(Convert.ToString(categoria["Nombre"]) ?? "")}'");
            if (cbEstadoStock.SelectedIndex > 0)
                filtros.Add($"[EstadoStock] = '{EscaparTextoStock(cbEstadoStock.Text)}'");
            // Filtrar por cantidades: un producto mixto aparece en ambas alertas.
            if (cbVencimientoStock.SelectedIndex == 1) filtros.Add("CantidadProxima > 0");
            else if (cbVencimientoStock.SelectedIndex == 2) filtros.Add("CantidadVencida > 0");
            else if (cbVencimientoStock.SelectedIndex == 3) filtros.Add("CantidadProxima = 0 AND CantidadVencida = 0");
            stockBinding.Filter = string.Join(" AND ", filtros);
            if (seleccionado.HasValue)
            {
                int posicion = stockBinding.Find("ProductoId", seleccionado.Value);
                if (posicion >= 0)
                {
                    stockBinding.Position = posicion;
                    if (posicion < dgvStock.Rows.Count)
                    {
                        DataGridViewColumn? visible = dgvStock.Columns.Cast<DataGridViewColumn>()
                            .Where(c => c.Visible).OrderBy(c => c.DisplayIndex).FirstOrDefault();
                        if (visible != null) dgvStock.CurrentCell = dgvStock.Rows[posicion].Cells[visible.Index];
                    }
                }
            }
            btnVerLotes.Enabled = dgvStock.CurrentRow?.DataBoundItem is DataRowView;
        }
    }
}
