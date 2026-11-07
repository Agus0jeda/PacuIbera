using PacuIbera.Datos;
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public class FormDetallePedido : Form
    {
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private static extern void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private static extern void SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        private readonly int pedidoId;
        private readonly PedidosDatos pedidosDatos = new PedidosDatos();
        private readonly NumberFormatInfo formatoMonedaArgentina = CultureInfo.GetCultureInfo("es-AR").NumberFormat;
        private DataGridView dgvDetalle;
        private Label lblTitulo;
        private Label lblNumero;
        private Label lblProveedor;
        private Label lblUsuario;
        private Label lblFecha;
        private Label lblEstado;
        private Label lblTotal;
        private Button btnCerrar;
        private Panel panelHeader;
        private Panel panelDatos;
        private Panel panelPie;

        public FormDetallePedido(int pedidoId)
        {
            this.pedidoId = pedidoId;
            InitializeComponent();
            Load += FormDetallePedido_Load;
            DoubleBuffered = true;
        }

        private void InitializeComponent()
        {
            panelHeader = new Panel();
            lblTitulo = new Label();
            btnCerrar = new Button();
            panelDatos = new Panel();
            panelPie = new Panel();
            dgvDetalle = new DataGridView();
            lblNumero = CrearEtiquetaDato();
            lblProveedor = CrearEtiquetaDato();
            lblUsuario = CrearEtiquetaDato();
            lblFecha = CrearEtiquetaDato();
            lblEstado = CrearEtiquetaDato();
            lblTotal = CrearEtiquetaDato();

            SuspendLayout();
            panelHeader.BackColor = Color.FromArgb(45, 55, 65);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Height = 52;
            panelHeader.MouseDown += PanelHeader_MouseDown;
            lblTitulo.Text = "DETALLE DE ORDEN DE COMPRA";
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(18, 15);
            lblTitulo.MouseDown += PanelHeader_MouseDown;
            panelHeader.Controls.Add(lblTitulo);
            btnCerrar.Text = "×";
            btnCerrar.Size = new Size(42, 34);
            btnCerrar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCerrar.Location = new Point(738, 9);
            btnCerrar.FlatStyle = FlatStyle.Flat;
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.BackColor = panelHeader.BackColor;
            btnCerrar.ForeColor = Color.White;
            btnCerrar.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnCerrar.Click += (_, _) => Close();
            panelHeader.Controls.Add(btnCerrar);
            panelHeader.Resize += (_, _) => btnCerrar.Location = new Point(panelHeader.ClientSize.Width - btnCerrar.Width - 10, 9);

            panelDatos.Dock = DockStyle.Top;
            panelDatos.Height = 96;
            panelDatos.BackColor = Color.FromArgb(250, 248, 243);
            panelDatos.Padding = new Padding(18, 12, 18, 8);
            panelDatos.Controls.AddRange(new Control[] { lblNumero, lblProveedor, lblUsuario, lblFecha, lblEstado });
            ConfigurarEtiqueta(lblNumero, "Pedido:", 18, 14);
            ConfigurarEtiqueta(lblProveedor, "Proveedor:", 210, 14);
            ConfigurarEtiqueta(lblUsuario, "Solicitante:", 18, 52);
            ConfigurarEtiqueta(lblFecha, "Fecha y hora:", 360, 52);
            ConfigurarEtiqueta(lblEstado, "Estado:", 600, 14);

            dgvDetalle.Dock = DockStyle.Fill;
            dgvDetalle.ReadOnly = true;
            dgvDetalle.AllowUserToAddRows = false;
            dgvDetalle.AllowUserToDeleteRows = false;
            dgvDetalle.MultiSelect = false;
            dgvDetalle.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDetalle.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDetalle.BackgroundColor = Color.White;
            dgvDetalle.BorderStyle = BorderStyle.None;
            dgvDetalle.RowHeadersVisible = false;
            dgvDetalle.AllowUserToResizeRows = false;

            panelPie.Dock = DockStyle.Bottom;
            panelPie.Height = 62;
            panelPie.BackColor = Color.FromArgb(250, 248, 243);
            lblTotal.Text = "Total estimado: $0,00";
            lblTotal.AutoSize = true;
            lblTotal.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTotal.ForeColor = Color.FromArgb(45, 55, 65);
            lblTotal.Location = new Point(18, 18);
            panelPie.Controls.Add(lblTotal);
            Button botonCerrar = CrearBotonCerrar();
            panelPie.Controls.Add(botonCerrar);
            panelPie.Resize += (_, _) => botonCerrar.Location = new Point(panelPie.ClientSize.Width - botonCerrar.Width - 18, 12);

            ClientSize = new Size(820, 540);
            MinimumSize = new Size(700, 440);
            Controls.Add(dgvDetalle);
            Controls.Add(panelPie);
            Controls.Add(panelDatos);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Text = "Detalle de orden de compra";
            ResumeLayout(false);
        }

        private static Label CrearEtiquetaDato()
        {
            return new Label { AutoSize = true, Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(45, 55, 65) };
        }

        private static void ConfigurarEtiqueta(Label label, string titulo, int x, int y)
        {
            label.Text = titulo;
            label.Location = new Point(x, y);
        }

        private Button CrearBotonCerrar()
        {
            Button boton = new Button { Text = "Cerrar", Size = new Size(110, 36), BackColor = Color.FromArgb(45, 55, 65), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            boton.FlatAppearance.BorderSize = 0;
            boton.Click += (_, _) => Close();
            return boton;
        }

        private void PanelHeader_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(Handle, 0x112, 0xf012, 0);
        }

        private void FormDetallePedido_Load(object sender, EventArgs e)
        {
            CargarCabecera();
            CargarDetalle();
        }

        private void CargarCabecera()
        {
            try
            {
                DataTable cabecera = pedidosDatos.ObtenerCabeceraPedido(pedidoId);
                if (cabecera.Rows.Count == 0)
                {
                    MessageBox.Show("No se encontró la orden de compra.", "Detalle", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Close();
                    return;
                }
                DataRow fila = cabecera.Rows[0];
                lblNumero.Text = $"Pedido: N.º {fila["Id"]}";
                lblProveedor.Text = $"Proveedor: {fila["Proveedor"]}";
                lblUsuario.Text = $"Solicitante: {fila["UsuarioSolicitante"]}";
                lblFecha.Text = $"Fecha y hora: {Convert.ToDateTime(fila["FechaPedido"]):dd/MM/yyyy HH:mm}";
                lblEstado.Text = $"Estado: {fila["Estado"]}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la cabecera: " + ex.Message, "Detalle", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarDetalle()
        {
            try
            {
                DataTable tabla = pedidosDatos.ObtenerDetallePedido(pedidoId);
                dgvDetalle.DataSource = tabla;
                ConfigurarColumnas();
                decimal total = tabla.AsEnumerable().Sum(fila => fila.Field<decimal>("Cantidad") * fila.Field<decimal>("PrecioCosto"));
                lblTotal.Text = $"Total estimado: {total.ToString("C2", formatoMonedaArgentina)}";
                EstiloUI.AplicarEstiloGrid(dgvDetalle);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el detalle: " + ex.Message, "Detalle", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurarColumnas()
        {
            foreach (DataGridViewColumn columna in dgvDetalle.Columns)
                columna.Visible = false;
            ConfigurarColumna("Producto", "Producto", true);
            ConfigurarColumna("Cantidad", "Cantidad", true);
            ConfigurarColumna("PrecioCosto", "Precio de costo", true);
            if (!dgvDetalle.Columns.Contains("Subtotal"))
                dgvDetalle.Columns.Add("Subtotal", "Subtotal");
            foreach (DataGridViewRow fila in dgvDetalle.Rows)
            {
                if (!fila.IsNewRow)
                    fila.Cells["Subtotal"].Value = Convert.ToDecimal(fila.Cells["Cantidad"].Value) * Convert.ToDecimal(fila.Cells["PrecioCosto"].Value);
            }
            ConfigurarColumna("Subtotal", "Subtotal", true);
            dgvDetalle.Columns["Cantidad"].DefaultCellStyle.Format = "0.###";
            dgvDetalle.Columns["PrecioCosto"].DefaultCellStyle.Format = "C2";
            dgvDetalle.Columns["PrecioCosto"].DefaultCellStyle.FormatProvider = formatoMonedaArgentina;
            dgvDetalle.Columns["Subtotal"].DefaultCellStyle.Format = "C2";
            dgvDetalle.Columns["Subtotal"].DefaultCellStyle.FormatProvider = formatoMonedaArgentina;
        }

        private void ConfigurarColumna(string nombre, string encabezado, bool visible)
        {
            if (dgvDetalle.Columns.Contains(nombre))
            {
                dgvDetalle.Columns[nombre].Visible = visible;
                dgvDetalle.Columns[nombre].HeaderText = encabezado;
            }
        }
    }
}
