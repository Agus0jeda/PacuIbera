using Datos;
using System;
using System.Data;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public partial class ProductoForm : Form
    {
        private ProductoDatos productoDatos = new ProductoDatos();
        private DataTable dtProductosGlobal; // Guarda los productos para filtrar rápido
        private int idProductoSeleccionado = 0; // Para saber si editamos o guardamos nuevo

        public ProductoForm()
        {
            InitializeComponent();
            this.Load += ProductoForm_Load;

            // Conectamos los filtros y los clics de la lista
            bucarNombreProducto.TextChanged += Filtros_Changed;
            buscarCatProd.SelectedIndexChanged += Filtros_Changed;
            listView1.SelectedIndexChanged += listView1_SelectedIndexChanged;

            // Actualizamos la frase del efecto fantasma
            bucarNombreProducto.Enter += (s, e) => { if (bucarNombreProducto.Text == "TODOS LOS PRODUCTOS") bucarNombreProducto.Text = ""; };
            bucarNombreProducto.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(bucarNombreProducto.Text)) bucarNombreProducto.Text = "TODOS LOS PRODUCTOS"; };
        }

        private void ProductoForm_Load(object sender, EventArgs e)
        {
            btnEliminar.Visible = false; // Lo ocultamos hasta que seleccione uno
            CargarCategorias();
            CargarListaProductos();
        }

        private void CargarCategorias()
        {
            try
            {
                DataTable dtCategorias = productoDatos.ObtenerCategorias();

                // Llenamos el ComboBox de crear/editar
                cmbCategoria.DataSource = dtCategorias;
                cmbCategoria.DisplayMember = "Nombre";
                cmbCategoria.ValueMember = "Id";

                // Llenamos el ComboBox del Filtro de arriba agregando la opción "TODAS"
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
                listView1.Columns.Add("Stock Min", 80);
                listView1.Columns.Add("X Peso", 60);
                listView1.Columns.Add("Descripción", 180);

                dtProductosGlobal = productoDatos.ObtenerProductos();

                // Vaciamos la lista y agregamos la opción "TODOS" al principio
                bucarNombreProducto.Items.Clear();
                bucarNombreProducto.Items.Add("TODOS LOS PRODUCTOS");

                foreach (DataRow row in dtProductosGlobal.Rows)
                {
                    bucarNombreProducto.Items.Add(row["Nombre"].ToString());
                }

                bucarNombreProducto.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                bucarNombreProducto.AutoCompleteSource = AutoCompleteSource.ListItems;

                // Que arranque mostrando "TODOS LOS PRODUCTOS"
                bucarNombreProducto.SelectedIndex = 0;

                AplicarFiltros();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la lista: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- SISTEMA DE FILTROS ---
        private void Filtros_Changed(object sender, EventArgs e)
        {
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            if (dtProductosGlobal == null) return;

            string filtro = "1=1"; // Trae todo por defecto

            // Filtro por Nombre
            if (!string.IsNullOrWhiteSpace(bucarNombreProducto.Text) &&
                bucarNombreProducto.Text != "TODOS LOS PRODUCTOS" &&
                bucarNombreProducto.Text != "Nombre del Producto") // Dejamos el viejo por las dudas
            {
                filtro += $" AND Nombre LIKE '%{bucarNombreProducto.Text}%'";
            }

            // Filtro por Categoría
            if (buscarCatProd.SelectedIndex > 0) // Si no es "TODAS"
            {
                filtro += $" AND Categoria = '{buscarCatProd.Text}'";
            }

            // Aplicamos el filtro a la memoria
            DataView dv = dtProductosGlobal.DefaultView;
            dv.RowFilter = filtro;

            // Dibujamos las filas filtradas
            listView1.Items.Clear();
            foreach (DataRowView rowView in dv)
            {
                DataRow row = rowView.Row;
                ListViewItem item = new ListViewItem(row["Id"].ToString());
                item.SubItems.Add(row["Nombre"].ToString());
                item.SubItems.Add(row["Categoria"].ToString());
                item.SubItems.Add(row["PrecioVenta"].ToString());
                item.SubItems.Add(row["StockMinimo"].ToString());
                item.SubItems.Add(Convert.ToBoolean(row["SeVendePorPeso"]) ? "Sí" : "No");
                item.SubItems.Add(row["Descripcion"].ToString());

                listView1.Items.Add(item);
            }
        }

        // --- AL HACER CLIC EN LA GRILLA ---
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem item = listView1.SelectedItems[0];
                idProductoSeleccionado = Convert.ToInt32(item.Text); // Guardamos el ID

                textBox1.Text = item.SubItems[1].Text; // Nombre
                cmbCategoria.Text = item.SubItems[2].Text; // Categoria
                nupPrecio.Value = Convert.ToDecimal(item.SubItems[3].Text);
                nupStockMin.Value = Convert.ToDecimal(item.SubItems[4].Text);
                chkPorPeso.Checked = item.SubItems[5].Text == "Sí";
                textBox4.Text = item.SubItems[6].Text; // Descripción

                GUARDAR.Text = "ACTUALIZAR"; // Cambiamos el texto del botón
                btnEliminar.Visible = true;  // Mostramos el botón Eliminar
            }
        }

        // --- BOTÓN GUARDAR (NUEVO O ACTUALIZAR) ---
        private void GUARDAR_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textBox1.Text))
                {
                    MessageBox.Show("El nombre del producto es obligatorio.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cmbCategoria.SelectedValue == null)
                {
                    MessageBox.Show("Debe seleccionar una categoría.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string nombre = textBox1.Text;
                string descripcion = textBox4.Text;
                decimal precio = nupPrecio.Value;
                int categoriaId = Convert.ToInt32(cmbCategoria.SelectedValue);
                decimal stockMinimo = nupStockMin.Value;
                bool seVendePorPeso = chkPorPeso.Checked;

                if (idProductoSeleccionado == 0)
                {
                    // Es un producto nuevo
                    productoDatos.RegistrarProducto(nombre, categoriaId, precio, stockMinimo, seVendePorPeso, descripcion);
                    MessageBox.Show("¡Producto guardado exitosamente!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Estamos editando un producto existente
                    productoDatos.ModificarProducto(idProductoSeleccionado, nombre, categoriaId, precio, stockMinimo, seVendePorPeso, descripcion);
                    MessageBox.Show("¡Producto actualizado exitosamente!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                btnCancelarCliente_Click(null, null); // Reutilizamos el botón cancelar para limpiar todo
                CargarListaProductos(); // Recargamos la grilla
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- BOTÓN ELIMINAR (BAJA LÓGICA) ---
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

                        btnCancelarCliente_Click(null, null); // Limpiar cajas
                        CargarListaProductos(); // Recargar grilla (ya no va a aparecer)
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al eliminar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // --- BOTÓN CANCELAR (LIMPIAR) ---
        private void btnCancelarCliente_Click(object sender, EventArgs e)
        {
            idProductoSeleccionado = 0; // Reseteamos el ID
            GUARDAR.Text = "GUARDAR"; // Volvemos el botón a la normalidad
            btnEliminar.Visible = false; // Ocultamos el botón eliminar

            // Vaciamos campos
            textBox1.Clear();
            textBox4.Clear();
            nupPrecio.Value = 0;
            nupStockMin.Value = 0;
            chkPorPeso.Checked = false;
            if (cmbCategoria.Items.Count > 0) cmbCategoria.SelectedIndex = 0;

            // Sacamos la selección de la grilla
            if (listView1.SelectedItems.Count > 0) listView1.SelectedItems[0].Selected = false;
        }

        // --- BOTÓN CREAR NUEVA CATEGORÍA ---
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
                catch (Exception ex)
                {
                    MessageBox.Show("Error al guardar categoría: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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


            // Solo mostramos el cartel si la caja tiene algo escrito
            if (!string.IsNullOrWhiteSpace(textBox4.Text))
            {
                MessageBox.Show(textBox4.Text, "Descripción Completa", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }

        }

        // --- MOSTRAR DESCRIPCIÓN AL HACER DOBLE CLIC EN LA TABLA ---
        

        private void listView1_DoubleClick_1(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                
                string descripcion = listView1.SelectedItems[0].SubItems[6].Text;

                if (!string.IsNullOrWhiteSpace(descripcion))
                {
                    MessageBox.Show(descripcion, "Descripción del Producto", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}