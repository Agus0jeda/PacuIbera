using PacuIbera.Datos;
using PacuIbera.Dominio;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public class NuevoPedidoForm : Form
    {
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
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // cbProveedores
            // 
            cbProveedores.ForeColor = SystemColors.WindowFrame;
            cbProveedores.Location = new Point(403, 26);
            cbProveedores.Name = "cbProveedores";
            cbProveedores.Size = new Size(206, 28);
            cbProveedores.TabIndex = 2;
            cbProveedores.Text = "Proveedores....";
            // 
            // dgvItems
            // 
            dgvItems.ColumnHeadersHeight = 29;
            dgvItems.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2, dataGridViewTextBoxColumn3, dataGridViewTextBoxColumn4 });
            dgvItems.Dock = DockStyle.Fill;
            dgvItems.Location = new Point(0, 0);
            dgvItems.Name = "dgvItems";
            dgvItems.RowHeadersWidth = 51;
            dgvItems.Size = new Size(782, 373);
            dgvItems.TabIndex = 0;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.HeaderText = "ProductoId";
            dataGridViewTextBoxColumn1.MinimumWidth = 6;
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            dataGridViewTextBoxColumn1.Width = 125;
            // 
            // dataGridViewTextBoxColumn2
            // 
            dataGridViewTextBoxColumn2.HeaderText = "Producto";
            dataGridViewTextBoxColumn2.MinimumWidth = 6;
            dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            dataGridViewTextBoxColumn2.Width = 125;
            // 
            // dataGridViewTextBoxColumn3
            // 
            dataGridViewTextBoxColumn3.HeaderText = "Cantidad";
            dataGridViewTextBoxColumn3.MinimumWidth = 6;
            dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            dataGridViewTextBoxColumn3.Width = 125;
            // 
            // dataGridViewTextBoxColumn4
            // 
            dataGridViewTextBoxColumn4.HeaderText = "Precio Costo";
            dataGridViewTextBoxColumn4.MinimumWidth = 6;
            dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            dataGridViewTextBoxColumn4.Width = 125;
            // 
            // btnAgregarProducto
            // 
            btnAgregarProducto.Location = new Point(188, 26);
            btnAgregarProducto.Name = "btnAgregarProducto";
            btnAgregarProducto.Size = new Size(147, 32);
            btnAgregarProducto.TabIndex = 1;
            btnAgregarProducto.Text = "Agregar Producto";
            btnAgregarProducto.Click += BtnAgregarProducto_Click;
            // 
            // btnGuardarPedido
            // 
            btnGuardarPedido.Location = new Point(22, 26);
            btnGuardarPedido.Name = "btnGuardarPedido";
            btnGuardarPedido.Size = new Size(147, 34);
            btnGuardarPedido.TabIndex = 3;
            btnGuardarPedido.Text = "Guardar Pedido";
            btnGuardarPedido.Click += BtnGuardarPedido_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnGuardarPedido);
            panel1.Controls.Add(cbProveedores);
            panel1.Controls.Add(btnAgregarProducto);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(782, 80);
            panel1.TabIndex = 4;
            // 
            // panel2
            // 
            panel2.Controls.Add(dgvItems);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(0, 80);
            panel2.Name = "panel2";
            panel2.Size = new Size(782, 373);
            panel2.TabIndex = 5;
            // 
            // NuevoPedidoForm
            // 
            ClientSize = new Size(782, 453);
            Controls.Add(panel2);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "NuevoPedidoForm";
            Text = "Nuevo Pedido";
            ((System.ComponentModel.ISupportInitialize)dgvItems).EndInit();
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        private void NuevoPedidoForm_Load(object sender, EventArgs e)
        {
            CargarProveedores();

            // Aplicar estilos UI
            EstiloUI.CrearBotonCerrar(this);
            EstiloUI.AplicarEstiloGrid(dgvItems);
            EstiloUI.AplicarEstiloBotonAccion(btnAgregarProducto, EstiloUI.TipoAccion.Positivo);
            EstiloUI.AplicarEstiloBotonAccion(btnGuardarPedido, EstiloUI.TipoAccion.Positivo);

            // Alinear botones inferiores (Guardar) y el Agregar arriba a la derecha del combo
            try
            {
                // posicionar btnAgregarProducto cerca del combo
                btnAgregarProducto.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                btnAgregarProducto.Location = new Point(this.ClientSize.Width - btnAgregarProducto.Width - 20, cbProveedores.Bottom + 4);

                // Alinear el boton guardar centrado en el fondo
                EstiloUI.AlinearBotonesInferiores(this, this.ClientSize.Height - 60, btnGuardarPedido);
            }
            catch { }
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
