using Datos;
using System;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public class SeleccionarProductoForm : Form
    {
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private static extern void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private static extern void SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);
        private Panel panelTitulo;
        private Label lblTitulo;
        private Button btnCerrar;
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
            panelTitulo = new Panel();
            lblTitulo = new Label();
            btnCerrar = new Button();
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

            panelTitulo.BackColor = Color.FromArgb(45, 55, 65);
            panelTitulo.Dock = DockStyle.Top;
            panelTitulo.Height = 42;
            panelTitulo.Dock = DockStyle.None;
            panelTitulo.Location = new Point(0, 0);
            panelTitulo.Size = new Size(650, 42);
            panelTitulo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelTitulo.MouseDown += PanelTitulo_MouseDown;
            lblTitulo.Text = "AGREGAR PRODUCTO AL PEDIDO";
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTitulo.AutoSize = true;
            lblTitulo.Location = new Point(14, 11);
            lblTitulo.MouseDown += PanelTitulo_MouseDown;
            btnCerrar.Text = "X";
            btnCerrar.Size = new Size(38, 30);
            btnCerrar.Location = new Point(606, 6);
            btnCerrar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCerrar.FlatStyle = FlatStyle.Flat;
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.BackColor = Color.FromArgb(45, 55, 65);
            btnCerrar.ForeColor = Color.White;
            btnCerrar.Click += BtnCancelar_Click;
            panelTitulo.Controls.Add(lblTitulo);
            panelTitulo.Controls.Add(btnCerrar);

            dgvProductos.Dock = DockStyle.None;
            dgvProductos.Location = new Point(0, 42);
            dgvProductos.Size = new Size(650, 288);
            dgvProductos.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvProductos.Name = "dgvProductos";
            dgvProductos.AllowUserToAddRows = false;
            dgvProductos.ReadOnly = true;
            dgvProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProductos.MultiSelect = false;
            dgvProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvProductos.BackgroundColor = Color.White;
            dgvProductos.RowHeadersVisible = false;

            panel1.Dock = DockStyle.None;
            panel1.Location = new Point(0, 330);
            panel1.Size = new Size(650, 100);
            panel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.BackColor = Color.WhiteSmoke;

            Label lblCantidad = new Label();
            lblCantidad.Text = "Cantidad:";
            lblCantidad.AutoSize = true;
            lblCantidad.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCantidad.Location = new Point(18, 15);
            nudCantidad.Location = new Point(95, 11);
            nudCantidad.Size = new Size(110, 27);
            nudCantidad.Minimum = 1;
            nudCantidad.Maximum = 100000;
            nudCantidad.Value = 1;

            Label lblPrecio = new Label();
            lblPrecio.Text = "Precio de costo:";
            lblPrecio.AutoSize = true;
            lblPrecio.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPrecio.Location = new Point(230, 15);
            nudPrecio.Location = new Point(340, 11);
            nudPrecio.Size = new Size(130, 27);
            nudPrecio.Maximum = 100000000;
            nudPrecio.DecimalPlaces = 2;
            nudPrecio.ThousandsSeparator = true;

            btnAceptar.Text = "Agregar al pedido";
            btnAceptar.Location = new Point(480, 55);
            btnAceptar.Size = new Size(140, 32);
            btnAceptar.Click += BtnAceptar_Click;
            btnCancelar.Text = "Cancelar";
            btnCancelar.Location = new Point(18, 55);
            btnCancelar.Size = new Size(100, 32);
            btnCancelar.Click += BtnCancelar_Click;
            panel1.Controls.AddRange(new Control[] { lblCantidad, nudCantidad, lblPrecio, nudPrecio, btnAceptar, btnCancelar });

            ClientSize = new Size(650, 430);
            Controls.Add(dgvProductos);
            Controls.Add(panel1);
            Controls.Add(panelTitulo);
            panelTitulo.BringToFront();
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Name = "SeleccionarProductoForm";
            Text = "Agregar producto al pedido";
            MinimumSize = new Size(560, 360);
            ((System.ComponentModel.ISupportInitialize)dgvProductos).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudCantidad).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudPrecio).EndInit();
            ResumeLayout(true);
        }

        private void PanelTitulo_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(Handle, 0x112, 0xf012, 0);
        }

        private void SeleccionarProductoForm_Load(object sender, EventArgs e)
        {
            try
            {
                var prodDatos = new ProductoDatos();
                var tabla = prodDatos.ObtenerProductos();
                dgvProductos.DataSource = tabla;
                if (dgvProductos.Columns.Contains("Id"))
                {
                    dgvProductos.Columns["Id"].HeaderText = "ID";
                    dgvProductos.Columns["Id"].Visible = false;
                }
                if (dgvProductos.Columns.Contains("Nombre"))
                    dgvProductos.Columns["Nombre"].HeaderText = "Producto";
                if (dgvProductos.Columns.Contains("Categoria"))
                    dgvProductos.Columns["Categoria"].HeaderText = "Categoría";
                EstiloUI.AplicarEstiloGrid(dgvProductos);
                dgvProductos.ColumnHeadersVisible = true;
                dgvProductos.EnableHeadersVisualStyles = false;
                dgvProductos.ColumnHeadersHeight = 36;
                dgvProductos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 55, 65);
                dgvProductos.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dgvProductos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                dgvProductos.RowHeadersVisible = false;
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
