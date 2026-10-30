using PacuIbera.Datos;
using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public class FormDetallePedido : Form
    {
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private int pedidoId;
        private PedidosDatos pedidosDatos = new PedidosDatos();
        private DataGridView dgvDetalle;
        private Label lblProveedor;
        private Label lblTitulo;
        private Panel panel1;
        private Panel panelHeader;
        private Panel panel2;
        private Button btnConfirmarRecepcion;

        public FormDetallePedido(int pedidoId)
        {
            this.pedidoId = pedidoId;
            InitializeComponent();
            Load += FormDetallePedido_Load;
            this.DoubleBuffered = true;
        }

        private void InitializeComponent()
        {
            dgvDetalle = new DataGridView();
            panelHeader = new Panel();
            lblProveedor = new Label();
            lblTitulo = new Label();
            btnConfirmarRecepcion = new Button();
            panel1 = new Panel();
            panel2 = new Panel();
            ((System.ComponentModel.ISupportInitialize)dgvDetalle).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // dgvDetalle
            // 
            dgvDetalle.ColumnHeadersHeight = 29;
            dgvDetalle.Dock = DockStyle.Fill;
            dgvDetalle.Location = new Point(0, 36);
            dgvDetalle.Name = "dgvDetalle";
            dgvDetalle.RowHeadersWidth = 51;
            dgvDetalle.Size = new Size(682, 367);
            dgvDetalle.TabIndex = 0;
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.WhiteSmoke;
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(682, 36);
            panelHeader.TabIndex = 4;
            panelHeader.MouseDown += PanelHeader_MouseDown;
            lblTitulo.AutoSize = true;
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Text = "Detalles del pedido";
            lblTitulo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblTitulo.ForeColor = System.Drawing.Color.FromArgb(45, 55, 65);
            lblTitulo.Location = new System.Drawing.Point(10, 8);
            lblTitulo.MouseDown += PanelHeader_MouseDown;
            panelHeader.Controls.Add(lblTitulo);
            // 
            // lblProveedor
            // 
            lblProveedor.Location = new Point(0, 0);
            lblProveedor.Name = "lblProveedor";
            lblProveedor.Size = new Size(100, 23);
            lblProveedor.TabIndex = 0;
            // 
            // btnConfirmarRecepcion
            // 
            btnConfirmarRecepcion.Location = new Point(112, 11);
            btnConfirmarRecepcion.Name = "btnConfirmarRecepcion";
            btnConfirmarRecepcion.Size = new Size(160, 36);
            btnConfirmarRecepcion.TabIndex = 2;
            btnConfirmarRecepcion.Text = "Confirmar Recepción";
            btnConfirmarRecepcion.Click += BtnConfirmarRecepcion_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnConfirmarRecepcion);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 353);
            panel1.Name = "panel1";
            panel1.Size = new Size(682, 50);
            panel1.TabIndex = 3;
            // 
            // panel2
            // 
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(682, 403);
            panel2.TabIndex = 5;
            // 
            // FormDetallePedido
            // 
            ClientSize = new Size(682, 403);
            Controls.Add(panel1);
            Controls.Add(dgvDetalle);
            Controls.Add(panelHeader);
            Controls.Add(panel2);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FormDetallePedido";
            Text = "Detalle de Pedido";
            ((System.ComponentModel.ISupportInitialize)dgvDetalle).EndInit();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        private void PanelHeader_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void FormDetallePedido_Load(object sender, EventArgs e)
        {
            EstiloUI.CrearBotonCerrar(this);
            EstiloUI.AplicarEstiloGrid(dgvDetalle);
            EstiloUI.AplicarEstiloBotonAccion(btnConfirmarRecepcion, EstiloUI.TipoAccion.Positivo);

            try
            {
                panel1.Controls.Clear();
                EstiloUI.AlinearBotonesInferiores(panel1, 6, btnConfirmarRecepcion);
            }
            catch { }

            CargarDetalle();
        }

        private void CargarDetalle()
        {
            try
            {
                DataTable tabla = pedidosDatos.ObtenerDetallePedido(pedidoId);
                dgvDetalle.DataSource = tabla;

                // Ocultar identificadores internos: se conservan en el DataTable para las operaciones.
                if (dgvDetalle.Columns.Contains("ProductoId"))
                    dgvDetalle.Columns["ProductoId"].Visible = false;
                if (dgvDetalle.Columns.Contains("DetalleId"))
                    dgvDetalle.Columns["DetalleId"].Visible = false;

                // El usuario ve los datos del producto y las cantidades, no las claves internas.
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar detalle: " + ex.Message);
            }
        }

        private void BtnConfirmarRecepcion_Click(object sender, EventArgs e)
        {
            if (dgvDetalle.CurrentRow == null) { MessageBox.Show("Seleccione un item del pedido"); return; }

            int detalleId = Convert.ToInt32(dgvDetalle.CurrentRow.Cells["DetalleId"].Value);
            int pendiente = Convert.ToInt32(dgvDetalle.CurrentRow.Cells["Pendiente"].Value);

            using (var form = new FormIngresoCantidad(pendiente))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    int cantidadIngresada = form.CantidadIngresada;
                    DateTime? fechaVto = form.FechaVencimiento;
                    try
                    {
                        pedidosDatos.RecepcionarItem(detalleId, cantidadIngresada, fechaVto);
                        MessageBox.Show("Recepción registrada correctamente");
                        CargarDetalle();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al recepcionar item: " + ex.Message);
                    }
                }
            }
        }
    }
}
