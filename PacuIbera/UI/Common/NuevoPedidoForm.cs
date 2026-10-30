using PacuIbera.Datos;
using PacuIbera.Dominio;
using System;
using System.Data;
using System.Linq;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public class NuevoPedidoForm : Form
    {
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private static extern void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private static extern void SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);
        private Panel panelTitulo;
        private Label lblTitulo;
        private Button btnCerrar;
        private ComboBox cbProveedores;
        private DataGridView dgvItems;
        private Button btnAgregarProducto;
        private Button btnGuardarPedido;
        private ProveedorDatos proveedorDatos = new ProveedorDatos();
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private Panel panel1;
        private Panel panel2;
        private PedidosDatos pedidosDatos = new PedidosDatos();

        public NuevoPedidoForm()
        {
            InitializeComponent();
            Load += NuevoPedidoForm_Load;
        }

        private void InitializeComponent()
        {
            panelTitulo = new Panel();
            lblTitulo = new Label();
            btnCerrar = new Button();
            cbProveedores = new ComboBox();
            dgvItems = new DataGridView();
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn4 = new DataGridViewTextBoxColumn();
            btnAgregarProducto = new Button();
            btnGuardarPedido = new Button();
            panel1 = new Panel();
            panel2 = new Panel();
            ((System.ComponentModel.ISupportInitialize)dgvItems).BeginInit();
            SuspendLayout();

            panelTitulo.BackColor = Color.FromArgb(45, 55, 65);
            panelTitulo.Dock = DockStyle.Top;
            panelTitulo.Height = 42;
            panelTitulo.Dock = DockStyle.None;
            panelTitulo.Location = new Point(0, 0);
            panelTitulo.Size = new Size(782, 42);
            panelTitulo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelTitulo.MouseDown += PanelTitulo_MouseDown;
            lblTitulo.Text = "CREAR NUEVO PEDIDO";
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTitulo.AutoSize = true;
            lblTitulo.Location = new Point(14, 11);
            lblTitulo.MouseDown += PanelTitulo_MouseDown;
            btnCerrar.Text = "X";
            btnCerrar.Size = new Size(38, 30);
            btnCerrar.Location = new Point(738, 6);
            btnCerrar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCerrar.FlatStyle = FlatStyle.Flat;
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.BackColor = Color.FromArgb(45, 55, 65);
            btnCerrar.ForeColor = Color.White;
            btnCerrar.Click += (s, e) => Close();
            panelTitulo.Controls.Add(lblTitulo);
            panelTitulo.Controls.Add(btnCerrar);

            cbProveedores.DropDownStyle = ComboBoxStyle.DropDownList;
            cbProveedores.Location = new Point(105, 21);
            cbProveedores.Size = new Size(260, 28);
            cbProveedores.Name = "cbProveedores";
            btnAgregarProducto.Text = "Agregar producto";
            btnAgregarProducto.Location = new Point(390, 17);
            btnAgregarProducto.Size = new Size(175, 38);
            btnAgregarProducto.Click += BtnAgregarProducto_Click;

            btnGuardarPedido.Text = "Guardar pedido";
            btnGuardarPedido.Size = new Size(155, 36);
            btnGuardarPedido.Click += BtnGuardarPedido_Click;

            panel1.Dock = DockStyle.None;
            panel1.Location = new Point(0, 42);
            panel1.Size = new Size(782, 70);
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel1.BackColor = Color.WhiteSmoke;
            Label lblProveedor = new Label();
            lblProveedor.Text = "Proveedor:";
            lblProveedor.AutoSize = true;
            lblProveedor.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblProveedor.Location = new Point(18, 23);
            panel1.Controls.Add(lblProveedor);
            panel1.Controls.Add(cbProveedores);
            panel1.Controls.Add(btnAgregarProducto);

            dgvItems.Dock = DockStyle.Fill;
            dgvItems.Name = "dgvItems";
            dgvItems.AllowUserToAddRows = false;
            dgvItems.AllowUserToDeleteRows = false;
            dgvItems.ReadOnly = true;
            dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvItems.BackgroundColor = Color.White;
            dgvItems.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2, dataGridViewTextBoxColumn3, dataGridViewTextBoxColumn4 });
            dataGridViewTextBoxColumn1.Name = "ProductoId";
            dataGridViewTextBoxColumn1.HeaderText = "ID interno";
            dataGridViewTextBoxColumn1.Visible = false;
            dataGridViewTextBoxColumn2.Name = "Producto";
            dataGridViewTextBoxColumn2.HeaderText = "Producto";
            dataGridViewTextBoxColumn3.Name = "Cantidad";
            dataGridViewTextBoxColumn3.HeaderText = "Cantidad";
            dataGridViewTextBoxColumn4.Name = "PrecioCosto";
            dataGridViewTextBoxColumn4.HeaderText = "Precio de costo";

            panel2.Dock = DockStyle.None;
            panel2.Location = new Point(0, 112);
            panel2.Size = new Size(782, 326);
            panel2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel2.Controls.Add(dgvItems);

            Panel panelInferior = new Panel();
            panelInferior.Dock = DockStyle.None;
            panelInferior.Location = new Point(0, 438);
            panelInferior.Size = new Size(782, 62);
            panelInferior.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelInferior.BackColor = Color.WhiteSmoke;
            btnGuardarPedido.Location = new Point(607, 12);
            btnGuardarPedido.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            panelInferior.Controls.Add(btnGuardarPedido);

            ClientSize = new Size(782, 500);
            Controls.Add(panel2);
            Controls.Add(panelInferior);
            Controls.Add(panel1);
            Controls.Add(panelTitulo);
            panelTitulo.BringToFront();
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Name = "NuevoPedidoForm";
            Text = "Crear nuevo pedido";
            MinimumSize = new Size(650, 420);
            ((System.ComponentModel.ISupportInitialize)dgvItems).EndInit();
            ResumeLayout(true);
        }

        private void PanelTitulo_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(Handle, 0x112, 0xf012, 0);
        }

        private void NuevoPedidoForm_Load(object sender, EventArgs e)
        {
            CargarProveedores();

            // Aplicar estilos UI
            EstiloUI.AplicarEstiloGrid(dgvItems);
            EstiloUI.AplicarEstiloBotonAccion(btnAgregarProducto, EstiloUI.TipoAccion.Positivo);
            EstiloUI.AplicarEstiloBotonAccion(btnGuardarPedido, EstiloUI.TipoAccion.Positivo);
            dgvItems.ColumnHeadersVisible = true;
            dgvItems.EnableHeadersVisualStyles = false;
            dgvItems.ColumnHeadersHeight = 36;
            dgvItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 55, 65);
            dgvItems.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvItems.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvItems.RowHeadersVisible = false;


        }

        private void CargarProveedores()
        {
            try
            {
                var tabla = proveedorDatos.ObtenerTodos();
                cbProveedores.DataSource = tabla;
                cbProveedores.DisplayMember = "RazonSocial";
                cbProveedores.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando proveedores: " + ex.Message);
            }
        }

        private void BtnAgregarProducto_Click(object sender, EventArgs e)
        {
            using (var prodForm = new SeleccionarProductoForm())
            {
                if (prodForm.ShowDialog() == DialogResult.OK)
                {
                    // agrega una fila
                    dgvItems.Rows.Add(prodForm.ProductoId, prodForm.ProductoNombre, prodForm.Cantidad, prodForm.PrecioCosto);
                }
            }
        }

        private void BtnGuardarPedido_Click(object sender, EventArgs e)
        {
            if (cbProveedores.SelectedValue == null) { MessageBox.Show("Seleccione un proveedor"); return; }
            if (dgvItems.Rows.Count == 0) { MessageBox.Show("Agregue al menos un producto"); return; }

            int proveedorId = Convert.ToInt32(cbProveedores.SelectedValue);
            int usuarioId = SesionActiva.IdUsuario;
            try
            {
                int nuevoPedidoId = pedidosDatos.CrearPedido(proveedorId, usuarioId, DateTime.Now);
                foreach (DataGridViewRow row in dgvItems.Rows)
                {
                    int productoId = Convert.ToInt32(row.Cells["ProductoId"].Value);
                    int cantidad = Convert.ToInt32(row.Cells["Cantidad"].Value);
                    decimal precio = Convert.ToDecimal(row.Cells["PrecioCosto"].Value);
                    pedidosDatos.AgregarItemPedido(nuevoPedidoId, productoId, cantidad, precio);
                }
                MessageBox.Show("Pedido creado correctamente");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al crear pedido: " + ex.Message);
            }
        }
    }
}
