using Datos;
using PacuIbera.Dominio;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public partial class ProductoForm : Form
    {
        private ProductoDatos productoDatos = new ProductoDatos();
        private DataTable dtProductosGlobal;
        private int idProductoSeleccionado = 0;
        private decimal precioOriginal = 0;

        private Panel panelLotes;
        private DataGridView dgvLotes;
        private TextBox txtStockLote;
        private DateTimePicker dtpVencimientoLote;
        private Button btnActualizarLote;
        private Button btnCerrarPanel;

        public ProductoForm()
        {
            InitializeComponent();
            this.Load += ProductoForm_Load;

            CrearPanelFlotantePorCodigo();

            bucarNombreProducto.TextChanged += Filtros_Changed;
            buscarCatProd.SelectedIndexChanged += Filtros_Changed;
            listView1.SelectedIndexChanged += listView1_SelectedIndexChanged;
            btnEliminar.Click += btnEliminar_Click;
            cmbNombreProducto.SelectedIndexChanged += cmbNombreProducto_SelectedIndexChanged;

            bucarNombreProducto.Enter += (s, e) => { if (bucarNombreProducto.Text == "TODOS LOS PRODUCTOS") bucarNombreProducto.Text = ""; };
            bucarNombreProducto.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(bucarNombreProducto.Text)) bucarNombreProducto.Text = "TODOS LOS PRODUCTOS"; };

            this.VisibleChanged += (sender, e) =>
            {
                if (this.Visible == true)
                {
                    CargarListaProductos();
                }
            };
        }

        private void ProductoForm_Load(object sender, EventArgs e)
        {
            string rolActual = SesionActiva.Rol != null ? SesionActiva.Rol.Trim().ToLower() : "";
            if (rolActual == "vendedor" || rolActual == "gerente")
            {
                BloquearParaVendedor(); // Esta función ya oculta todo el panel izquierdo
            }
        }

        private void BloquearParaVendedor()
        {
            // En lugar de ocultar cajita por cajita, apagamos el panel entero.
            // Esto hace que desaparezcan también las palabras "Nombre:", "Precio:", etc.
            if (panelNuevoProducto != null)
            {
                panelNuevoProducto.Visible = false;
            }

            GUARDAR.Visible = false;
            btnEliminar.Visible = false;
        }

        

        private void CargarCategorias()
        {
            try
            {
                DataTable dtCategorias = productoDatos.ObtenerCategorias();
                cmbCategoria.DataSource = dtCategorias;
                cmbCategoria.DisplayMember = "Nombre";
                cmbCategoria.ValueMember = "Id";

                DataTable dtFiltro = dtCategorias.Copy();
                DataRow filaTodas = dtFiltro.NewRow();
                filaTodas["Id"] = 0;
                filaTodas["Nombre"] = "TODAS LAS CATEGORÍAS";
                dtFiltro.Rows.InsertAt(filaTodas, 0);

                buscarCatProd.DataSource = dtFiltro;
                buscarCatProd.DisplayMember = "Nombre";
                buscarCatProd.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar categorías: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarListaProductos()
        {
            productoDatos.ProcesarLotesVencidos();
            try
            {
                listView1.Clear();
                listView1.View = View.Details;
                listView1.GridLines = true;
                listView1.FullRowSelect = true;

                listView1.Columns.Add("ID", 40);
                listView1.Columns.Add("Nombre", 180);
                listView1.Columns.Add("Categoría", 120);
                listView1.Columns.Add("Precio", 80);
                listView1.Columns.Add("Stock", 60);
                listView1.Columns.Add("Stock Min", 80);
                listView1.Columns.Add("X Peso", 60);
                listView1.Columns.Add("Próx. Venc.", 90);
                listView1.Columns.Add("Descripción", 180);

                dtProductosGlobal = productoDatos.ObtenerProductos();
                bucarNombreProducto.Items.Clear();
                bucarNombreProducto.Items.Add("TODOS LOS PRODUCTOS");
                cmbNombreProducto.Items.Clear();

                foreach (DataRow row in dtProductosGlobal.Rows)
                {
                    string nombreProd = row["Nombre"].ToString();
                    bucarNombreProducto.Items.Add(nombreProd);
                    cmbNombreProducto.Items.Add(nombreProd);
                }

                bucarNombreProducto.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                bucarNombreProducto.AutoCompleteSource = AutoCompleteSource.ListItems;
                bucarNombreProducto.SelectedIndex = 0;

                cmbNombreProducto.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                cmbNombreProducto.AutoCompleteSource = AutoCompleteSource.ListItems;

                AplicarFiltros();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la lista: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            ActualizarPanelAlertas();
        }

        private void ActualizarPanelAlertas()
        {
            listaStockBajo.Items.Clear();
            listaVencimientos.Items.Clear();
            if (dtProductosGlobal == null) return;

            foreach (DataRow row in dtProductosGlobal.Rows)
            {
                string nombre = row["Nombre"].ToString();
                decimal stockActual = Convert.ToDecimal(row["StockActual"]);
                decimal stockMin = Convert.ToDecimal(row["StockMinimo"]);

                if (stockActual <= stockMin)
                {
                    listaStockBajo.Items.Add($"⚠️ {nombre} - Quedan: {stockActual} (Mín: {stockMin})");
                }

                int lotesPorVencer = row["LotesPorVencer"] != DBNull.Value ? Convert.ToInt32(row["LotesPorVencer"]) : 0;
                if (lotesPorVencer > 0)
                {
                    string proxVenc = row["ProximoVencimiento"] != DBNull.Value ? Convert.ToDateTime(row["ProximoVencimiento"]).ToString("dd/MM/yyyy") : "N/A";
                    listaVencimientos.Items.Add($"⏳ {nombre} - {lotesPorVencer} lote(s) vence(n) ({proxVenc})");
                }
            }
        }

        private void Filtros_Changed(object sender, EventArgs e) { AplicarFiltros(); }

        private void AplicarFiltros()
        {
            if (dtProductosGlobal == null) return;
            string filtro = "1=1";

            if (!string.IsNullOrWhiteSpace(bucarNombreProducto.Text) &&
                bucarNombreProducto.Text != "TODOS LOS PRODUCTOS" &&
                bucarNombreProducto.Text != "Nombre del Producto")
            {
                filtro += $" AND Nombre LIKE '%{bucarNombreProducto.Text}%'";
            }

            if (buscarCatProd.SelectedIndex > 0)
            {
                filtro += $" AND Categoria = '{buscarCatProd.Text}'";
            }

            DataView dv = dtProductosGlobal.DefaultView;
            dv.RowFilter = filtro;

            listView1.Items.Clear();
            foreach (DataRowView rowView in dv)
            {
                DataRow row = rowView.Row;
                ListViewItem item = new ListViewItem(row["Id"].ToString());
                item.SubItems.Add(row["Nombre"].ToString());
                item.SubItems.Add(row["Categoria"].ToString());
                item.SubItems.Add(Convert.ToDecimal(row["PrecioVenta"]).ToString("N2"));
                item.SubItems.Add(Convert.ToDecimal(row["StockActual"]).ToString("N2"));
                item.SubItems.Add(Convert.ToDecimal(row["StockMinimo"]).ToString("N2"));
                item.SubItems.Add(Convert.ToBoolean(row["SeVendePorPeso"]) ? "Sí" : "No");

                if (row["ProximoVencimiento"] != DBNull.Value)
                {
                    item.SubItems.Add(Convert.ToDateTime(row["ProximoVencimiento"]).ToString("dd/MM/yyyy"));
                }
                else { item.SubItems.Add(""); }

                item.SubItems.Add(row["Descripcion"].ToString());

                decimal stockActual = Convert.ToDecimal(row["StockActual"]);
                decimal stockMinimo = Convert.ToDecimal(row["StockMinimo"]);
                int lotesPorVencer = Convert.ToInt32(row["LotesPorVencer"]);

                if (stockActual <= stockMinimo) { item.BackColor = System.Drawing.Color.LightCoral; }
                else if (lotesPorVencer > 0) { item.BackColor = System.Drawing.Color.LightYellow; }

                listView1.Items.Add(item);
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem item = listView1.SelectedItems[0];
                idProductoSeleccionado = Convert.ToInt32(item.Text);

                cmbNombreProducto.Text = item.SubItems[1].Text;
                cmbCategoria.Text = item.SubItems[2].Text;
                nupPrecio.Value = Convert.ToDecimal(item.SubItems[3].Text);
                nupStockMin.Value = Convert.ToDecimal(item.SubItems[5].Text);
                chkPorPeso.Checked = item.SubItems[6].Text == "Sí";

                string fechaGrilla = item.SubItems[7].Text;
                if (DateTime.TryParse(fechaGrilla, out DateTime fechaValida))
                {
                    chkVencimiento.Checked = true;
                    dtpVencimiento.Value = fechaValida;
                }
                else { chkVencimiento.Checked = false; }

                textBox4.Text = item.SubItems[8].Text;

                if (SesionActiva.Rol != "Vendedor")
                {
                    GUARDAR.Text = "ACTUALIZAR";
                    btnEliminar.Visible = true;
                }
            }
        }

        private void GUARDAR_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cmbNombreProducto.Text))
                {
                    MessageBox.Show("El nombre del producto es obligatorio.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cmbCategoria.SelectedValue == null || cmbCategoria.SelectedIndex == -1)
                {
                    MessageBox.Show("Debe seleccionar una categoría válida.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (nupPrecio.Value <= 0)
                {
                    MessageBox.Show("El precio de venta debe ser mayor a cero.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                decimal stockNuevo = 0;
                DateTime? fechaVenc = null;

                string nombre = cmbNombreProducto.Text;
                string descripcion = textBox4.Text;
                decimal precioActual = nupPrecio.Value;
                int categoriaId = Convert.ToInt32(cmbCategoria.SelectedValue);
                decimal stockMinimo = nupStockMin.Value;
                bool seVendePorPeso = chkPorPeso.Checked;

                if (idProductoSeleccionado > 0 && precioActual != precioOriginal)
                {
                    DialogResult resp = MessageBox.Show($"El precio cambió de ${precioOriginal} a ${precioActual}.\n\n¿Desea actualizar el precio en el sistema?", "Cambio de Precio", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (resp == DialogResult.No) precioActual = precioOriginal;
                }

                if (idProductoSeleccionado == 0)
                {
                    productoDatos.RegistrarProducto(nombre, categoriaId, precioActual, stockMinimo, seVendePorPeso, descripcion, stockNuevo, fechaVenc);
                    MessageBox.Show("¡Producto guardado exitosamente!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    productoDatos.ModificarProducto(idProductoSeleccionado, nombre, categoriaId, precioActual, stockMinimo, seVendePorPeso, descripcion, stockNuevo, fechaVenc);
                    MessageBox.Show("¡Producto actualizado exitosamente!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                btnCancelarCliente_Click(null, null);
                CargarListaProductos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (idProductoSeleccionado > 0)
            {
                DialogResult respuesta = MessageBox.Show("¿Está seguro que desea dar de baja este producto?", "Confirmar Baja", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (respuesta == DialogResult.Yes)
                {
                    try
                    {
                        productoDatos.EliminarProducto(idProductoSeleccionado);
                        MessageBox.Show("Producto eliminado correctamente.", "Baja Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        btnCancelarCliente_Click(null, null);
                        CargarListaProductos();
                    }
                    catch (Exception ex) { MessageBox.Show("Error al eliminar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
            }
        }

        private void btnCancelarCliente_Click(object sender, EventArgs e)
        {
            idProductoSeleccionado = 0;
            GUARDAR.Text = "GUARDAR";
            btnEliminar.Visible = false;

            cmbNombreProducto.Text = "";
            textBox4.Clear();
            nupPrecio.Value = 0;
            nupStockMin.Value = 0;
            chkPorPeso.Checked = false;
            if (cmbCategoria.Items.Count > 0) cmbCategoria.SelectedIndex = 0;

            if (listView1.SelectedItems.Count > 0) listView1.SelectedItems[0].Selected = false;
        }

        private void btnNuevaCat_Click(object sender, EventArgs e)
        {
            string nuevaCategoria = PromptParametro("Ingrese el nombre de la nueva categoría:", "Nueva Categoría");
            if (!string.IsNullOrWhiteSpace(nuevaCategoria))
            {
                try
                {
                    int nuevoId = productoDatos.RegistrarCategoria(nuevaCategoria.ToUpper());
                    MessageBox.Show("Categoría agregada con éxito.", "Excelente", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarCategorias();
                    cmbCategoria.SelectedValue = nuevoId;
                }
                catch (Exception ex) { MessageBox.Show("Error al guardar categoría: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        public static string PromptParametro(string texto, string titulo)
        {
            Form prompt = new Form() { Width = 350, Height = 150, FormBorderStyle = FormBorderStyle.FixedDialog, Text = titulo, StartPosition = FormStartPosition.CenterScreen };
            Label textLabel = new Label() { Left = 20, Top = 20, Text = texto, AutoSize = true };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 300 };
            Button confirmation = new Button() { Text = "Aceptar", Left = 220, Width = 100, Top = 80, DialogResult = DialogResult.OK };
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.AcceptButton = confirmation;
            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }

        private void btnCerrar_Click(object sender, EventArgs e) { this.Close(); }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void btnGuardar_Click(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void button2_Click(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }
        private void panel2_Paint(object sender, PaintEventArgs e) { }
        private void numericUpDown1_ValueChanged(object sender, EventArgs e) { }
        private void panelNuevoProducto_Paint(object sender, PaintEventArgs e) { }

        private void textBox4_DoubleClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox4.Text))
            {
                MessageBox.Show(textBox4.Text, "Descripción Completa", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void listView1_DoubleClick_1(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                Point mousePos = listView1.PointToClient(Control.MousePosition);
                ListViewHitTestInfo hit = listView1.HitTest(mousePos);
                int columnaClickeada = hit.Item.SubItems.IndexOf(hit.SubItem);

                int prodId = Convert.ToInt32(listView1.SelectedItems[0].Text);
                string prodNombre = listView1.SelectedItems[0].SubItems[1].Text;

                if (columnaClickeada == 7)
                {
                    // BLOQUEO: El Vendedor NO puede ver ni tocar el stock de lotes
                    if (SesionActiva.Rol == "Vendedor")
                    {
                        MessageBox.Show("Solo el Administrador y Gerencia pueden gestionar los lotes de stock.", "Acceso Restringido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DataTable dtLotes = productoDatos.ObtenerLotesPorProducto(prodId);
                    if (dtLotes.Rows.Count == 0)
                    {
                        MessageBox.Show("No hay lotes con stock para este producto.", "Lotes de " + prodNombre, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    dgvLotes.DataSource = dtLotes;
                    if (dgvLotes.Columns.Contains("Quedan"))
                    {
                        dgvLotes.Columns["Quedan"].DefaultCellStyle.Format = "N2";
                        dgvLotes.Columns["Quedan"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }

                    panelLotes.Tag = prodId;
                    panelLotes.Visible = true;
                    panelLotes.BringToFront();
                }
                else if (columnaClickeada == 8)
                {
                    string desc = hit.SubItem.Text;
                    if (!string.IsNullOrWhiteSpace(desc))
                        MessageBox.Show(desc, "Descripción", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void cmbNombreProducto_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbNombreProducto.SelectedIndex >= 0 && dtProductosGlobal != null)
            {
                string nombreElegido = cmbNombreProducto.Text;
                DataRow[] filas = dtProductosGlobal.Select($"Nombre = '{nombreElegido}'");
                if (filas.Length > 0)
                {
                    DataRow row = filas[0];
                    idProductoSeleccionado = Convert.ToInt32(row["Id"]);
                    cmbCategoria.Text = row["Categoria"].ToString();
                    precioOriginal = Convert.ToDecimal(row["PrecioVenta"]);
                    nupPrecio.Value = precioOriginal;
                    nupStockMin.Value = Convert.ToDecimal(row["StockMinimo"]);
                    chkPorPeso.Checked = Convert.ToBoolean(row["SeVendePorPeso"]);
                    textBox4.Text = row["Descripcion"].ToString();

                    if (SesionActiva.Rol != "Vendedor")
                    {
                        GUARDAR.Text = "ACTUALIZAR";
                        btnEliminar.Visible = true;
                    }
                }
            }
        }

        private void chkVencimiento_CheckedChanged(object sender, EventArgs e) { }
        private void label10_Click(object sender, EventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e) { }

        private void CrearPanelFlotantePorCodigo()
        {
            panelLotes = new Panel();
            panelLotes.Size = new Size(480, 350);
            panelLotes.Location = new Point(250, 100);
            panelLotes.BackColor = Color.WhiteSmoke;
            panelLotes.BorderStyle = BorderStyle.FixedSingle;
            panelLotes.Visible = false;

            Label lblTitulo = new Label();
            lblTitulo.Text = "GESTIÓN DE LOTES DEL PRODUCTO";
            lblTitulo.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblTitulo.Location = new Point(10, 10);
            lblTitulo.AutoSize = true;

            btnCerrarPanel = new Button();
            btnCerrarPanel.Text = "X";
            btnCerrarPanel.BackColor = Color.DarkRed;
            btnCerrarPanel.ForeColor = Color.White;
            btnCerrarPanel.Size = new Size(30, 30);
            btnCerrarPanel.Location = new Point(440, 5);
            btnCerrarPanel.Click += (s, e) => { panelLotes.Visible = false; };

            dgvLotes = new DataGridView();
            dgvLotes.Location = new Point(10, 50);
            dgvLotes.Size = new Size(450, 180);
            dgvLotes.AllowUserToAddRows = false;
            dgvLotes.ReadOnly = true;
            dgvLotes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLotes.BackgroundColor = Color.White;

            dgvLotes.CellClick += (s, ev) =>
            {
                if (dgvLotes.CurrentRow != null)
                {
                    txtStockLote.Text = dgvLotes.CurrentRow.Cells["Quedan"].Value.ToString();
                    dtpVencimientoLote.Value = Convert.ToDateTime(dgvLotes.CurrentRow.Cells["Vencimiento"].Value);
                }
            };

            Label lblStock = new Label() { Text = "Stock a dejar:", Location = new Point(10, 250), AutoSize = true };
            txtStockLote = new TextBox() { Location = new Point(10, 270), Size = new Size(100, 25) };

            Label lblFecha = new Label() { Text = "Nueva Fecha:", Location = new Point(130, 250), AutoSize = true };
            dtpVencimientoLote = new DateTimePicker() { Format = DateTimePickerFormat.Short, Location = new Point(130, 270), Size = new Size(120, 25) };

            btnActualizarLote = new Button();
            btnActualizarLote.Text = "ACTUALIZAR LOTE";
            btnActualizarLote.BackColor = ColorTranslator.FromHtml("#88E788");
            btnActualizarLote.Location = new Point(280, 255);
            btnActualizarLote.Size = new Size(180, 40);

            btnActualizarLote.Click += (s, ev) =>
            {
                if (dgvLotes.CurrentRow == null)
                {
                    MessageBox.Show("Seleccione un lote de la tablita primero.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string inputStock = txtStockLote.Text.Trim();
                inputStock = inputStock.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                                       .Replace(",", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);

                if (!decimal.TryParse(inputStock, out decimal nuevoStock) || nuevoStock < 0)
                {
                    MessageBox.Show("La cantidad de stock no es válida. Use números correctos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int loteId = Convert.ToInt32(dgvLotes.CurrentRow.Cells["Lote N°"].Value);
                int prodId = Convert.ToInt32(panelLotes.Tag);
                DateTime nuevaFecha = dtpVencimientoLote.Value;

                try
                {
                    productoDatos.ActualizarLote(loteId, prodId, nuevoStock, nuevaFecha);
                    MessageBox.Show("¡Lote actualizado con éxito!", "Excelente", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dgvLotes.DataSource = productoDatos.ObtenerLotesPorProducto(prodId);
                    CargarListaProductos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al actualizar el lote: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            panelLotes.Controls.Add(lblTitulo);
            panelLotes.Controls.Add(btnCerrarPanel);
            panelLotes.Controls.Add(dgvLotes);
            panelLotes.Controls.Add(lblStock);
            panelLotes.Controls.Add(txtStockLote);
            panelLotes.Controls.Add(lblFecha);
            panelLotes.Controls.Add(dtpVencimientoLote);
            panelLotes.Controls.Add(btnActualizarLote);

            this.Controls.Add(panelLotes);
            panelLotes.BringToFront();
        }
    }
}