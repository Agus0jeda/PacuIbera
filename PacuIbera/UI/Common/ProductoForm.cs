using Datos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public partial class ProductoForm : Form
    {
        private ProductoDatos productoDatos = new ProductoDatos();

        public ProductoForm()
        {
            InitializeComponent();
            this.Load += ProductoForm_Load;
        }

        private void ProductoForm_Load(object sender, EventArgs e)
        {
            CargarCategorias();
        }

        private void CargarCategorias()
        {
            try
            {
                DataTable dtCategorias = productoDatos.ObtenerCategorias();
                cmbCategoria.DataSource = dtCategorias;
                cmbCategoria.DisplayMember = "Nombre";
                cmbCategoria.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar categorías: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

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

                if (nupPrecio.Value <= 0)
                {
                    MessageBox.Show("El precio del producto debe ser mayor a $0.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string nombre = textBox1.Text;
                string descripcion = textBox4.Text;
                decimal precio = nupPrecio.Value;
                int categoriaId = Convert.ToInt32(cmbCategoria.SelectedValue);
                decimal stockMinimo = nupStockMin.Value;
                bool seVendePorPeso = chkPorPeso.Checked;

                productoDatos.RegistrarProducto(nombre, categoriaId, precio, stockMinimo, seVendePorPeso, descripcion);

                MessageBox.Show("¡Producto guardado exitosamente!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                textBox1.Clear();
                textBox4.Clear();
                nupPrecio.Value = 0;
                nupStockMin.Value = 0;
                chkPorPeso.Checked = false;
                if (cmbCategoria.Items.Count > 0) cmbCategoria.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        // Eventos vacíos del diseñador
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void btnGuardar_Click(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void button2_Click(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }
        private void panel2_Paint(object sender, PaintEventArgs e) { }
        private void btnCancelar_Click(object sender, EventArgs e) { }
        private void numericUpDown1_ValueChanged(object sender, EventArgs e) { }
        private void panelNuevoProducto_Paint(object sender, PaintEventArgs e) { }
    }
}