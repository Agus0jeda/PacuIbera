using Datos;
using PacuIbera.Datos;
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
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
        private TextBox txtBuscar;
        private Label lblSubtotal;
        private readonly int proveedorId;
        private readonly ProveedorDatos proveedorDatos = new ProveedorDatos();
        private DataTable productosDisponibles;
        private static readonly NumberFormatInfo FormatoMonedaArgentina = CultureInfo.GetCultureInfo("es-AR").NumberFormat;

        public int ProductoId { get; private set; }
        public string ProductoNombre { get; private set; }
        public decimal Cantidad { get; private set; }
        public bool SeVendePorPeso { get; private set; }
        public decimal PrecioCosto { get; private set; }

        public SeleccionarProductoForm(int proveedorId)
        {
            this.proveedorId = proveedorId;
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
            txtBuscar = new TextBox();
            lblSubtotal = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvProductos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudCantidad).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudPrecio).BeginInit();
            SuspendLayout();

            panelTitulo.BackColor = Color.FromArgb(45, 55, 65);
            panelTitulo.Dock = DockStyle.Fill;
            panelTitulo.MouseDown += PanelTitulo_MouseDown;
            lblTitulo.Text = "AGREGAR PRODUCTO AL PEDIDO";
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTitulo.AutoSize = true;
            lblTitulo.Location = new Point(14, 11);
            lblTitulo.MouseDown += PanelTitulo_MouseDown;
            btnCerrar.Text = "X";
            btnCerrar.Size = new Size(38, 30);
            btnCerrar.Location = new Point(panelTitulo.ClientSize.Width - btnCerrar.Width - 6, 6);
            btnCerrar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCerrar.FlatStyle = FlatStyle.Flat;
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.BackColor = Color.FromArgb(45, 55, 65);
            btnCerrar.ForeColor = Color.White;
            btnCerrar.Click += BtnCancelar_Click;
            panelTitulo.Controls.Add(lblTitulo);
            panelTitulo.Controls.Add(btnCerrar);
            panelTitulo.Resize += (_, _) =>
            {
                btnCerrar.Location = new Point(panelTitulo.ClientSize.Width - btnCerrar.Width - 6, 6);
            };

            dgvProductos.Dock = DockStyle.Fill;
            dgvProductos.Name = "dgvProductos";
            dgvProductos.AllowUserToAddRows = false;
            dgvProductos.ReadOnly = true;
            dgvProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProductos.MultiSelect = false;
            dgvProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvProductos.BackgroundColor = Color.White;
            dgvProductos.RowHeadersVisible = false;
            dgvProductos.SelectionChanged += DgvProductos_SelectionChanged;

            panel1.Dock = DockStyle.Fill;
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
            nudCantidad.ValueChanged += ControlesCosto_ValueChanged;
            nudPrecio.ValueChanged += ControlesCosto_ValueChanged;

            btnAceptar.Text = "Agregar al pedido";
            btnAceptar.Location = new Point(0, 55);
            btnAceptar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAceptar.Size = new Size(140, 32);
            btnAceptar.Click += BtnAceptar_Click;
            btnCancelar.Text = "Cancelar";
            btnCancelar.Location = new Point(18, 55);
            btnCancelar.Size = new Size(100, 32);
            btnCancelar.Click += BtnCancelar_Click;
            lblSubtotal.AutoSize = true;
            lblSubtotal.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblSubtotal.Location = new Point(18, 57);
            lblSubtotal.Text = "Subtotal estimado: $0,00";
            panel1.Controls.AddRange(new Control[] { lblCantidad, nudCantidad, lblPrecio, nudPrecio, lblSubtotal, btnAceptar, btnCancelar });

            panel1.Resize += (_, _) =>
            {
                btnAceptar.Location = new Point(panel1.ClientSize.Width - btnAceptar.Width - 18, 55);
            };

            Panel panelBusqueda = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(12, 8, 12, 8) };
            Label lblBuscar = new Label { Text = "Buscar producto:", AutoSize = true, Location = new Point(12, 13), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtBuscar.Dock = DockStyle.Right;
            txtBuscar.Width = 380;
            txtBuscar.PlaceholderText = "Nombre del producto";
            txtBuscar.TextChanged += TxtBuscar_TextChanged;
            panelBusqueda.Controls.Add(txtBuscar);
            panelBusqueda.Controls.Add(lblBuscar);

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.White
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            layout.Controls.Add(panelTitulo, 0, 0);
            layout.Controls.Add(panelBusqueda, 0, 1);
            layout.Controls.Add(dgvProductos, 0, 2);
            layout.Controls.Add(panel1, 0, 3);

            ClientSize = new Size(700, 500);
            Controls.Add(layout);
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Name = "SeleccionarProductoForm";
            Text = "Agregar producto al pedido";
            MinimumSize = new Size(620, 420);
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
                productosDisponibles = proveedorDatos.ObtenerProductosOfrecidosActivos(proveedorId);
                if (productosDisponibles.Rows.Count == 0)
                {
                    MessageBox.Show("El proveedor seleccionado no tiene productos asociados.", "Sin productos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }

                dgvProductos.DataSource = productosDisponibles;
                if (dgvProductos.Columns.Contains("Id"))
                {
                    dgvProductos.Columns["Id"].HeaderText = "ID";
                    dgvProductos.Columns["Id"].Visible = false;
                }
                if (dgvProductos.Columns.Contains("Nombre"))
                    dgvProductos.Columns["Nombre"].HeaderText = "Producto";
                if (dgvProductos.Columns.Contains("Categoria"))
                    dgvProductos.Columns["Categoria"].HeaderText = "Categoría";
                if (dgvProductos.Columns.Contains("SeVendePorPeso"))
                {
                    dgvProductos.Columns.Add("Unidad", "Unidad");
                    foreach (DataGridViewRow fila in dgvProductos.Rows)
                    {
                        if (!fila.IsNewRow)
                        {
                            fila.Cells["Unidad"].Value = Convert.ToBoolean(fila.Cells["SeVendePorPeso"].Value)
                                ? "kg"
                                : "unidad";
                        }
                    }
                }

                foreach (DataGridViewColumn columna in dgvProductos.Columns)
                {
                    columna.Visible = columna.Name == "Nombre" ||
                                      columna.Name == "Categoria" ||
                                      columna.Name == "Unidad";
                }
                dgvProductos.Columns["Nombre"].HeaderText = "Producto";
                dgvProductos.Columns["Nombre"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvProductos.Columns["Categoria"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvProductos.Columns["Unidad"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvProductos.Columns["Unidad"].Width = 100;
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

        private void TxtBuscar_TextChanged(object sender, EventArgs e)
        {
            if (dgvProductos.DataSource is DataTable tabla)
            {
                string texto = txtBuscar.Text.Trim().Replace("'", "''");
                tabla.DefaultView.RowFilter = string.IsNullOrWhiteSpace(texto)
                    ? string.Empty
                    : $"Nombre LIKE '%{texto}%'";
            }
        }

        private void ControlesCosto_ValueChanged(object sender, EventArgs e)
        {
            lblSubtotal.Text = $"Subtotal estimado: {(nudCantidad.Value * nudPrecio.Value).ToString("C2", FormatoMonedaArgentina)}";
        }

        private void BtnAceptar_Click(object sender, EventArgs e)
        {
            if (dgvProductos.CurrentRow == null) { MessageBox.Show("Seleccione un producto"); return; }
            ProductoId = Convert.ToInt32(dgvProductos.CurrentRow.Cells["Id"].Value);
            SeVendePorPeso = Convert.ToBoolean(dgvProductos.CurrentRow.Cells["SeVendePorPeso"].Value);

            if (productosDisponibles == null || !productosDisponibles.AsEnumerable()
                .Any(fila => fila.Field<int>("Id") == ProductoId))
            {
                MessageBox.Show("El producto seleccionado no pertenece al proveedor actual.", "Producto no válido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // ProductoDatos devuelve columna 'Nombre'
            ProductoNombre = dgvProductos.CurrentRow.Cells["Nombre"].Value.ToString();
            Cantidad = nudCantidad.Value;
            PrecioCosto = nudPrecio.Value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void DgvProductos_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProductos.CurrentRow == null || !dgvProductos.Columns.Contains("SeVendePorPeso"))
                return;

            bool seVendePorPeso = Convert.ToBoolean(dgvProductos.CurrentRow.Cells["SeVendePorPeso"].Value);
            decimal cantidadActual = nudCantidad.Value;
            nudCantidad.DecimalPlaces = seVendePorPeso ? 3 : 0;
            nudCantidad.Increment = seVendePorPeso ? 0.001M : 1M;
            nudCantidad.Value = seVendePorPeso
                ? Math.Min(nudCantidad.Maximum, cantidadActual)
                : Math.Min(nudCantidad.Maximum, Math.Max(nudCantidad.Minimum, decimal.Truncate(cantidadActual)));
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
