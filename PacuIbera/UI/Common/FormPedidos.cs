using PacuIbera.Datos;
using PacuIbera.Dominio;
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
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
        private Label lblTitulo;
        private PedidosDatos pedidosDatos = new PedidosDatos();
        private TextBox txtBuscar;
        private ComboBox cbEstado;
        private Panel panelFiltros;
        private DataTable pedidosTabla;
        private readonly NumberFormatInfo formatoMonedaArgentina = CultureInfo.GetCultureInfo("es-AR").NumberFormat;

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
            lblTitulo = new Label();
            panelFiltros = new Panel();
            txtBuscar = new TextBox();
            cbEstado = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)dgvPedidos).BeginInit();
            SuspendLayout();

            panelHeader.Dock = DockStyle.Top;
            panelHeader.Height = 48;
            panelHeader.BackColor = Color.FromArgb(45, 55, 65);
            panelHeader.MouseDown += PanelHeader_MouseDown;
            lblTitulo.AutoSize = true;
            lblTitulo.Text = "PEDIDOS A PROVEEDORES";
            lblTitulo.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(165, 14);
            lblTitulo.MouseDown += PanelHeader_MouseDown;
            panelHeader.Controls.Add(lblTitulo);

            btnNuevoPedido.Location = new Point(12, 8);
            btnNuevoPedido.Name = "btnNuevoPedido";
            btnNuevoPedido.Text = "Nuevo pedido";
            btnNuevoPedido.Size = new Size(140, 32);
            btnNuevoPedido.Click += BtnNuevoPedido_Click;
            panelHeader.Controls.Add(btnNuevoPedido);

            panelFiltros.Dock = DockStyle.Top;
            panelFiltros.Height = 58;
            panelFiltros.BackColor = Color.FromArgb(250, 248, 243);
            panelFiltros.Padding = new Padding(14, 12, 14, 10);
            Label lblBuscar = new Label { Text = "Buscar:", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(14, 19) };
            txtBuscar.PlaceholderText = "Proveedor o N.º de pedido";
            txtBuscar.Location = new Point(68, 14);
            txtBuscar.Size = new Size(280, 27);
            txtBuscar.TextChanged += Filtros_TextChanged;
            Label lblEstado = new Label { Text = "Estado:", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(380, 19) };
            cbEstado.DropDownStyle = ComboBoxStyle.DropDownList;
            cbEstado.Items.AddRange(new object[] { "Todos", "Pendientes", "Finalizados" });
            cbEstado.SelectedIndex = 0;
            cbEstado.Location = new Point(435, 14);
            cbEstado.Size = new Size(150, 27);
            cbEstado.SelectedIndexChanged += Filtros_TextChanged;
            panelFiltros.Controls.AddRange(new Control[] { lblBuscar, txtBuscar, lblEstado, cbEstado });

            dgvPedidos.Dock = DockStyle.Fill;
            dgvPedidos.Name = "dgvPedidos";
            dgvPedidos.ReadOnly = true;
            dgvPedidos.AllowUserToAddRows = false;
            dgvPedidos.AllowUserToDeleteRows = false;
            dgvPedidos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPedidos.MultiSelect = false;
            dgvPedidos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPedidos.BackgroundColor = Color.FromArgb(233, 237, 241);
            dgvPedidos.DefaultCellStyle.BackColor = Color.FromArgb(233, 237, 241);
            dgvPedidos.RowHeadersVisible = false;
            dgvPedidos.CellFormatting += DgvPedidos_CellFormatting;

            ClientSize = new Size(850, 520);
            MinimumSize = new Size(700, 420);
            Controls.Add(dgvPedidos);
            Controls.Add(panelFiltros);
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

            bool esAdministrador = SesionActiva.Rol == "Administrador" ||
                                   SesionActiva.Rol == "SuperAdministrador";
            btnNuevoPedido.Visible = esAdministrador;

            CargarPedidos();
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
                pedidosTabla = pedidosDatos.ObtenerPedidos();
                dgvPedidos.DataSource = pedidosTabla;
                ConfigurarColumnas();
                AplicarFiltros();
                dgvPedidos.CellClick -= DgvPedidos_CellClick;
                dgvPedidos.CellClick += DgvPedidos_CellClick;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando pedidos: " + ex.Message);
            }
        }

        private void ConfigurarColumnas()
        {
            foreach (DataGridViewColumn columna in dgvPedidos.Columns)
                columna.Visible = false;

            ConfigurarColumna("Id", "N.º pedido", true);
            ConfigurarColumna("Proveedor", "Proveedor", true);
            ConfigurarColumna("FechaPedido", "Fecha y hora", true);
            ConfigurarColumna("Estado", "Estado", true);
            ConfigurarColumna("TotalEstimado", "Total estimado", true);

            if (!dgvPedidos.Columns.Contains("Ver"))
            {
                DataGridViewButtonColumn boton = new DataGridViewButtonColumn { Name = "Ver", HeaderText = "Ver detalle", Text = "Ver", UseColumnTextForButtonValue = true, Width = 100 };
                boton.DefaultCellStyle.BackColor = Color.FromArgb(100, 116, 139);
                boton.DefaultCellStyle.ForeColor = Color.White;
                boton.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvPedidos.Columns.Add(boton);
            }

            ConfigurarColumna("Ver", "Ver detalle", true);
            dgvPedidos.Columns["FechaPedido"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            dgvPedidos.Columns["TotalEstimado"].DefaultCellStyle.Format = "C2";
            dgvPedidos.Columns["TotalEstimado"].DefaultCellStyle.FormatProvider = formatoMonedaArgentina;
            dgvPedidos.Columns["Id"].FillWeight = 90;
            dgvPedidos.Columns["Id"].MinimumWidth = 90;
            dgvPedidos.Columns["Proveedor"].FillWeight = 150;
            dgvPedidos.Columns["FechaPedido"].FillWeight = 115;
            dgvPedidos.Columns["Estado"].FillWeight = 100;
            dgvPedidos.Columns["TotalEstimado"].FillWeight = 110;
            dgvPedidos.Columns["Ver"].FillWeight = 75;
        }

        private void ConfigurarColumna(string nombre, string encabezado, bool visible)
        {
            if (!dgvPedidos.Columns.Contains(nombre)) return;
            dgvPedidos.Columns[nombre].Visible = visible;
            dgvPedidos.Columns[nombre].HeaderText = encabezado;
        }

        private void Filtros_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            if (pedidosTabla == null) return;
            string texto = txtBuscar.Text.Trim().Replace("'", "''");
            string filtroBusqueda = string.IsNullOrWhiteSpace(texto)
                ? string.Empty
                : $"(CONVERT(Id, 'System.String') LIKE '%{texto}%' OR Proveedor LIKE '%{texto}%')";
            string filtroEstado = cbEstado.SelectedItem?.ToString() switch
            {
                "Pendientes" => "Estado = 'Pendiente'",
                "Finalizados" => "Estado <> 'Pendiente'",
                _ => string.Empty
            };
            pedidosTabla.DefaultView.RowFilter = string.Join(" AND ", new[] { filtroBusqueda, filtroEstado }.Where(filtro => !string.IsNullOrEmpty(filtro)));
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
            if (SesionActiva.Rol != "Administrador" &&
                SesionActiva.Rol != "SuperAdministrador")
            {
                MessageBox.Show("Solo los administradores pueden solicitar pedidos.",
                    "Acceso restringido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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
