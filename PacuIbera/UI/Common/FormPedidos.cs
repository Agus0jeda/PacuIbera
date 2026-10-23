using PacuIbera.Datos;
using PacuIbera.Dominio;
using System;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public class FormPedidos : Form
    {
        // DllImports para permitir mover ventanas sin borde
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private DataGridView dgvPedidos;
        private Button btnNuevoPedido;
        private Panel panelHeader;
        private PedidosDatos pedidosDatos = new PedidosDatos();

        public FormPedidos()
        {
            InitializeComponent();
            Load += FormPedidos_Load;
            this.DoubleBuffered = true;
        }

        private void InitializeComponent()
        {
            dgvPedidos = new DataGridView();
            btnNuevoPedido = new Button();
            panelHeader = new Panel();
            ((System.ComponentModel.ISupportInitialize)dgvPedidos).BeginInit();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Height = 36;
            panelHeader.BackColor = Color.WhiteSmoke;
            panelHeader.MouseDown += PanelHeader_MouseDown;
            // 
            // dgvPedidos
            // 
            dgvPedidos.ColumnHeadersHeight = 29;
            dgvPedidos.Dock = DockStyle.Fill;
            dgvPedidos.Location = new Point(0, panelHeader.Height);
            dgvPedidos.Name = "dgvPedidos";
            dgvPedidos.RowHeadersWidth = 51;
            dgvPedidos.Size = new Size(782, 453 - panelHeader.Height);
            dgvPedidos.TabIndex = 0;
            dgvPedidos.CellFormatting += DgvPedidos_CellFormatting;
            // 
            // btnNuevoPedido
            // 
            btnNuevoPedido.Location = new Point(10, 6);
            btnNuevoPedido.Name = "btnNuevoPedido";
            btnNuevoPedido.Size = new Size(120, 30);
            btnNuevoPedido.TabIndex = 1;
            btnNuevoPedido.Click += BtnNuevoPedido_Click;
            
            panelHeader.Controls.Add(btnNuevoPedido);
            // 
            // FormPedidos
            // 
            ClientSize = new Size(782, 453);
            Controls.Add(dgvPedidos);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FormPedidos";
            Text = "Pedidos a Proveedores";
            ((System.ComponentModel.ISupportInitialize)dgvPedidos).EndInit();
            ResumeLayout(false);
        }

        private void PanelHeader_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void FormPedidos_Load(object sender, EventArgs e)
        {
            // Aplicar estilos UI estandarizados
            EstiloUI.CrearBotonCerrar(this);
            EstiloUI.AplicarEstiloGrid(dgvPedidos);

            // Estilizar botón Nuevo
            EstiloUI.AplicarEstiloBotonAccion(btnNuevoPedido, EstiloUI.TipoAccion.Positivo);

            CargarPedidos();

            // Alinear boton inferior bajo la grilla
            try { EstiloUI.AlinearBotonesInferiores(this, this.ClientSize.Height - 60, btnNuevoPedido); } catch { }
        }

        private void DgvPedidos_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                if (dgvPedidos.Columns[e.ColumnIndex].Name == "Estado" && e.Value != null)
                {
                    string estado = e.Value.ToString();
                    DataGridViewRow row = dgvPedidos.Rows[e.RowIndex];
                    switch (estado)
                    {
                        case "Completado":
                            row.DefaultCellStyle.BackColor = Color.FromArgb(209, 250, 229);
                            row.DefaultCellStyle.ForeColor = Color.FromArgb(4, 120, 87); // verde oscuro
                            break;
                        case "Pendiente":
                            row.DefaultCellStyle.BackColor = Color.FromArgb(254, 243, 199);
                            row.DefaultCellStyle.ForeColor = Color.FromArgb(133, 94, 66); // café/ámbar
                            break;
                        case "Parcialmente Recibido":
                            row.DefaultCellStyle.BackColor = Color.FromArgb(219, 234, 254);
                            row.DefaultCellStyle.ForeColor = Color.FromArgb(30, 64, 175); // azul oscuro
                            break;
                        default:
                            // dejar estilo por defecto
                            break;
                    }
                }
            }
            catch { /* no bloquear la UI por formateo */ }
        }

        private void CargarPedidos()
        {
            try
            {
                DataTable tabla = pedidosDatos.ObtenerPedidos();
                dgvPedidos.DataSource = tabla;

                // Ocultar columnas identificadoras si existen para evitar mostrarlas
                try
                {
                    if (dgvPedidos.Columns.Contains("Id")) dgvPedidos.Columns["Id"].Visible = false;
                    if (dgvPedidos.Columns.Contains("ProductoId")) dgvPedidos.Columns["ProductoId"].Visible = false;
                }
                catch { /* no bloquear la carga por visibilidad */ }

                // Asegurarse de nombres que devuelve el SP

                // Agregar columna ver si no existe
                if (!dgvPedidos.Columns.Contains("Ver"))
                {
                    DataGridViewButtonColumn btnVer = new DataGridViewButtonColumn();
                    btnVer.Name = "Ver";
                    btnVer.Text = "Ver";
                    btnVer.UseColumnTextForButtonValue = true;
                    btnVer.Width = 80;
                    dgvPedidos.Columns.Add(btnVer);

                    // Aplicar estilo a la columna de botones
                    btnVer.DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#64748B");
                    btnVer.DefaultCellStyle.ForeColor = Color.White;
                    btnVer.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }

                dgvPedidos.CellClick -= DgvPedidos_CellClick;
                dgvPedidos.CellClick += DgvPedidos_CellClick;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando pedidos: " + ex.Message);
            }
        }


        private void DgvPedidos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (dgvPedidos.Columns[e.ColumnIndex].Name != "Ver")
                return;

            if (!dgvPedidos.Columns.Contains("Id"))
            {
                MessageBox.Show(
                    "La consulta de pedidos no devuelve una columna llamada Id."
                );
                return;
            }

            object valorId = dgvPedidos.Rows[e.RowIndex].Cells["Id"].Value;

            if (valorId == null || valorId == DBNull.Value)
            {
                MessageBox.Show("El ID del pedido está vacío.");
                return;
            }

            if (!int.TryParse(valorId.ToString(), out int pedidoId))
            {
                MessageBox.Show("El ID del pedido no es un número entero.");
                return;
            }

            using (FormDetallePedido detalle = new FormDetallePedido(pedidoId))
            {
                detalle.ShowDialog();
            }

            CargarPedidos();
        }


        private void BtnNuevoPedido_Click(object sender, EventArgs e)
        {
            using (NuevoPedidoForm nuevo = new NuevoPedidoForm())
            {
                if (nuevo.ShowDialog() == DialogResult.OK)
                {
                    CargarPedidos();
                }
            }
        }
    }
}
