using PacuIbera.Datos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public class ProductosProveedorForm : Form
    {
        private readonly int proveedorId;
        private readonly string proveedorNombre;
        private readonly ProveedorDatos proveedorDatos = new ProveedorDatos();
        private readonly CheckedListBox productos = new CheckedListBox();
        private readonly Button btnGuardar = new Button();
        private readonly Button btnCancelar = new Button();

        public ProductosProveedorForm(int proveedorId, string proveedorNombre)
        {
            this.proveedorId = proveedorId;
            this.proveedorNombre = proveedorNombre;
            InicializarComponentes();
            Load += ProductosProveedorForm_Load;
        }

        private void InicializarComponentes()
        {
            Text = "Productos que ofrece";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(620, 480);
            MinimumSize = new Size(520, 380);
            BackColor = Color.White;

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.White,
                Padding = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));

            Panel encabezado = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 52,
                BackColor = Color.FromArgb(45, 55, 65),
                Margin = new Padding(0)
            };
            Label titulo = new Label
            {
                AutoSize = true,
                Location = new Point(16, 15),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Text = $"Productos que ofrece: {proveedorNombre}"
            };
            encabezado.Controls.Add(titulo);

            productos.Dock = DockStyle.Fill;
            productos.CheckOnClick = true;
            productos.DisplayMember = "Nombre";
            productos.ValueMember = "Id";
            productos.BorderStyle = BorderStyle.None;
            productos.Font = new Font("Segoe UI", 10F);
            productos.BackColor = Color.White;
            productos.IntegralHeight = false;
            productos.Margin = new Padding(12, 8, 12, 8);
            productos.HorizontalScrollbar = false;
            productos.ScrollAlwaysVisible = false;

            Panel pie = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.WhiteSmoke,
                Margin = new Padding(0)
            };
            btnCancelar.Text = "Cancelar";
            btnCancelar.Size = new Size(110, 36);
            btnCancelar.Location = new Point(16, 13);
            btnCancelar.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            btnCancelar.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
            btnGuardar.Text = "Guardar";
            btnGuardar.Size = new Size(110, 36);
            btnGuardar.Location = new Point(0, 13);
            btnGuardar.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnGuardar.BackColor = Color.LightGreen;
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;
            pie.Controls.Add(btnCancelar);
            pie.Controls.Add(btnGuardar);

            pie.Resize += (_, _) =>
            {
                btnGuardar.Location = new Point(pie.ClientSize.Width - btnGuardar.Width - 16, 13);
            };

            layout.Controls.Add(encabezado, 0, 0);
            layout.Controls.Add(productos, 0, 1);
            layout.Controls.Add(pie, 0, 2);
            Controls.Add(layout);
        }

        private void ProductosProveedorForm_Load(object sender, EventArgs e)
        {
            try
            {
                DataTable tabla = proveedorDatos.ObtenerProductosOfrecidos(proveedorId);
                foreach (DataRow fila in tabla.Rows)
                {
                    int indice = productos.Items.Add(fila.Table.DefaultView[ fila.Table.Rows.IndexOf(fila) ]);
                    productos.SetItemChecked(indice, Convert.ToBoolean(fila["Asociado"]));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los productos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                IEnumerable<int> seleccionados = productos.CheckedItems
                    .Cast<DataRowView>()
                    .Select(item => Convert.ToInt32(item["Id"]));

                proveedorDatos.GuardarProductosOfrecidos(proveedorId, seleccionados);
                MessageBox.Show("Productos ofrecidos guardados correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar los productos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
