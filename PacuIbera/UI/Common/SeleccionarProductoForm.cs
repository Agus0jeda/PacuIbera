using Datos;
using System;
using System.Data;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public class SeleccionarProductoForm : Form
    {
        private DataGridView dgvProductos;
        private NumericUpDown nudCantidad;
        private NumericUpDown nudPrecio;
        private Button btnAceptar;
        private Panel panel1;
        private Button btnCancelar;

        public int ProductoId { get; private set; }
        public string ProductoNombre { get; private set; }
        public int Cantidad { get; private set; }
        public decimal PrecioCosto { get; private set; }

        public SeleccionarProductoForm()
        {
            InitializeComponent();
            Load += SeleccionarProductoForm_Load;
        }

        private void InitializeComponent()
        {
            dgvProductos = new DataGridView();
            nudCantidad = new NumericUpDown();
            nudPrecio = new NumericUpDown();
            btnAceptar = new Button();
            btnCancelar = new Button();
            panel1 = new Panel();
            ((System.ComponentModel.ISupportInitialize)dgvProductos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudCantidad).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudPrecio).BeginInit();
            SuspendLayout();
            // 
            // dgvProductos
            // 
            dgvProductos.ColumnHeadersHeight = 29;
            dgvProductos.Dock = DockStyle.Fill;
            dgvProductos.Location = new Point(0, 0);
            dgvProductos.Name = "dgvProductos";
            dgvProductos.RowHeadersWidth = 51;
            dgvProductos.Size = new Size(582, 353);
            dgvProductos.TabIndex = 0;
            // 
            // nudCantidad
            // 
            nudCantidad.Location = new Point(0, 0);
            nudCantidad.Name = "nudCantidad";
            nudCantidad.Size = new Size(120, 27);
            nudCantidad.TabIndex = 1;
            // 
            // nudPrecio
            // 
            nudPrecio.Location = new Point(0, 0);
            nudPrecio.Name = "nudPrecio";
            nudPrecio.Size = new Size(120, 27);
            nudPrecio.TabIndex = 2;
            // 
            // btnAceptar
            // 
            btnAceptar.Location = new Point(0, 0);
            btnAceptar.Name = "btnAceptar";
            btnAceptar.Size = new Size(75, 23);
            btnAceptar.TabIndex = 3;
            btnAceptar.Click += BtnAceptar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.Location = new Point(0, 0);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(75, 23);
            btnCancelar.TabIndex = 4;
            btnCancelar.Click += BtnCancelar_Click;
            // 
            // panel1
            // 
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(582, 353);
            panel1.TabIndex = 5;
            // 
            // SeleccionarProductoForm
            // 
            ClientSize = new Size(582, 353);
            Controls.Add(dgvProductos);
            Controls.Add(nudCantidad);
            Controls.Add(nudPrecio);
            Controls.Add(btnAceptar);
            Controls.Add(btnCancelar);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "SeleccionarProductoForm";
            Text = "Seleccionar Producto";
            ((System.ComponentModel.ISupportInitialize)dgvProductos).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudCantidad).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudPrecio).EndInit();
            ResumeLayout(false);
        }

        private void SeleccionarProductoForm_Load(object sender, EventArgs e)
        {
            try
            {
                var prodDatos = new ProductoDatos();
                var tabla = prodDatos.ObtenerProductos();
                dgvProductos.DataSource = tabla;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando productos: " + ex.Message);
            }
        }

        private void BtnAceptar_Click(object sender, EventArgs e)
        {
            if (dgvProductos.CurrentRow == null) { MessageBox.Show("Seleccione un producto"); return; }
            ProductoId = Convert.ToInt32(dgvProductos.CurrentRow.Cells["Id"].Value);
            // ProductoDatos devuelve columna 'Nombre'
            ProductoNombre = dgvProductos.CurrentRow.Cells["Nombre"].Value.ToString();
            Cantidad = (int)nudCantidad.Value;
            PrecioCosto = nudPrecio.Value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
