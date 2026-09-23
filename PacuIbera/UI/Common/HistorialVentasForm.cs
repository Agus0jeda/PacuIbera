using Datos;
using Microsoft.Data.SqlClient;
using PacuIbera.Datos;
using PacuIbera.Dominio;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Printing;

namespace PacuIbera.UI.Common
{
    public partial class HistorialVentasForm : Form
    {
        private DataGridView dgvHistorial;
        private Button btnAnular;
        private VentaDatos ventaDatos = new VentaDatos();
        private TextBox txtBuscador;

        // --- CONTROLES NUEVOS ---
        private DateTimePicker dtpDesde;
        private DateTimePicker dtpHasta;
        private Label lblEfectivo;
        private Label lblTarjeta;
        private Label lblTransferencia;
        private Label lblTotal;

        public HistorialVentasForm()
        {
            InitializeComponent();
            this.Text = "Historial de Ventas";
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;

            CrearInterfazGrafica();
            this.Load += HistorialVentasForm_Load;
        }

        private void HistorialVentasForm_Load(object sender, EventArgs e)
        {
            CargarHistorial();
        }

        private void CrearInterfazGrafica()
        {
            this.Controls.Clear();

            // Hacemos el panel más alto para que entren las fechas y los totales
            Panel pnlCabecera = new Panel { Dock = DockStyle.Top, Height = 145, BackColor = Color.White };
            Label lblTitulo = new Label { Text = "Historial de Ventas y Caja", Font = new Font("Segoe UI", 18, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true };

            Label lblBuscar = new Label { Text = "🔍 Buscar:", Font = new Font("Segoe UI", 11, FontStyle.Bold), Location = new Point(20, 65), AutoSize = true };
            txtBuscador = new TextBox { Font = new Font("Segoe UI", 12), Location = new Point(105, 62), Width = 350 };
            txtBuscador.TextChanged += TxtBuscador_TextChanged;

            // --- CALENDARIOS DE FILTRO ---
            Label lblDesde = new Label { Text = "Desde:", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(20, 105), AutoSize = true };
            dtpDesde = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(75, 102), Width = 100, Value = DateTime.Today };

            Label lblHasta = new Label { Text = "Hasta:", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(190, 105), AutoSize = true };
            dtpHasta = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(245, 102), Width = 100, Value = DateTime.Today };

            Button btnFiltrar = new Button { Text = "Filtrar Fecha", Location = new Point(355, 100), Size = new Size(100, 28), BackColor = Color.LightSkyBlue, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btnFiltrar.FlatAppearance.BorderSize = 0;
            btnFiltrar.Click += (s, e) => CargarHistorial();

            // --- PANEL DE TOTALES ---
            Panel pnlTotales = new Panel { Location = new Point(480, 62), Size = new Size(500, 66), BackColor = ColorTranslator.FromHtml("#F4F6F6") };
            lblEfectivo = new Label { Text = "Efectivo: $0.00", Location = new Point(10, 10), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.DarkGreen };
            lblTarjeta = new Label { Text = "Tarjeta: $0.00", Location = new Point(10, 35), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.DarkBlue };
            lblTransferencia = new Label { Text = "Transf: $0.00", Location = new Point(160, 10), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.DarkOrange };
            lblTotal = new Label { Text = "TOTAL: $0.00", Location = new Point(310, 20), AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold) };

            pnlTotales.Controls.Add(lblEfectivo);
            pnlTotales.Controls.Add(lblTarjeta);
            pnlTotales.Controls.Add(lblTransferencia);
            pnlTotales.Controls.Add(lblTotal);

            btnAnular = new Button { Text = "❌ Anular Venta", Font = new Font("Segoe UI", 11, FontStyle.Bold), BackColor = Color.LightCoral, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(180, 45), Cursor = Cursors.Hand };
            btnAnular.Location = new Point(pnlCabecera.Width - 200, 15);
            btnAnular.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAnular.FlatAppearance.BorderSize = 0;
            btnAnular.Click += BtnAnular_Click;

            pnlCabecera.Controls.Add(lblTitulo);
            pnlCabecera.Controls.Add(lblBuscar);
            pnlCabecera.Controls.Add(txtBuscador);
            pnlCabecera.Controls.Add(lblDesde);
            pnlCabecera.Controls.Add(dtpDesde);
            pnlCabecera.Controls.Add(lblHasta);
            pnlCabecera.Controls.Add(dtpHasta);
            pnlCabecera.Controls.Add(btnFiltrar);
            pnlCabecera.Controls.Add(pnlTotales);
            pnlCabecera.Controls.Add(btnAnular);

            Panel pnlCentro = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            dgvHistorial = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false, Font = new Font("Segoe UI", 12) };
            dgvHistorial.CellContentClick += dgvHistorial_CellContentClick;
            dgvHistorial.RowTemplate.Height = 40;
            dgvHistorial.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            dgvHistorial.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvHistorial.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgvHistorial.DataBindingComplete += DgvHistorial_DataBindingComplete;

            pnlCentro.Controls.Add(dgvHistorial);
            this.Controls.Add(pnlCentro);
            this.Controls.Add(pnlCabecera);
        }

        private void TxtBuscador_TextChanged(object sender, EventArgs e)
        {
            if (dgvHistorial.DataSource is DataTable dt)
            {
                string texto = txtBuscador.Text.Trim().Replace("'", "");
                if (string.IsNullOrEmpty(texto)) { dt.DefaultView.RowFilter = ""; }
                else
                {
                    dt.DefaultView.RowFilter = $"(Cliente LIKE '%{texto}%') " +
                                               $"OR (Convert([Nro Venta], 'System.String') LIKE '%{texto}%') " +
                                               $"OR (Convert(Fecha, 'System.String') LIKE '%{texto}%') " +
                                               $"OR (Convert(Total, 'System.String') LIKE '%{texto}%')";
                }
            }
        }

        private void CargarHistorial()
        {
            try
            {
                // Traemos las fechas seleccionadas
                DateTime desde = dtpDesde.Value.Date;
                DateTime hasta = dtpHasta.Value.Date;

                // 1. CARGAMOS LA TABLA
                dgvHistorial.DataSource = ventaDatos.ObtenerHistorialVentas(SesionActiva.Rol, SesionActiva.IdUsuario, desde, hasta);

                foreach (DataGridViewColumn col in dgvHistorial.Columns)
                {
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }

                if (dgvHistorial.Columns.Contains("Total")) dgvHistorial.Columns["Total"].DefaultCellStyle.Format = "$ #,##0.00";

                if (!dgvHistorial.Columns.Contains("ColumnaTicket"))
                {
                    DataGridViewButtonColumn btnTicket = new DataGridViewButtonColumn();
                    btnTicket.Name = "ColumnaTicket";
                    btnTicket.HeaderText = "Acción";
                    btnTicket.Text = "🎫 Ticket";
                    btnTicket.UseColumnTextForButtonValue = true;
                    btnTicket.FlatStyle = FlatStyle.Flat;
                    dgvHistorial.Columns.Add(btnTicket);
                }

                // 2. ACTUALIZAMOS LOS TOTALES DE CAJA
                DataTable dtTotales = ventaDatos.ObtenerTotalesVentas(SesionActiva.Rol, SesionActiva.IdUsuario, desde, hasta);
                if (dtTotales.Rows.Count > 0)
                {
                    DataRow row = dtTotales.Rows[0];
                    lblEfectivo.Text = $"Efectivo: ${Convert.ToDecimal(row["TotalEfectivo"]):N2}";
                    lblTarjeta.Text = $"Tarjeta: ${Convert.ToDecimal(row["TotalTarjeta"]):N2}";
                    lblTransferencia.Text = $"Transf: ${Convert.ToDecimal(row["TotalTransferencia"]):N2}";
                    lblTotal.Text = $"TOTAL: ${Convert.ToDecimal(row["TotalGeneral"]):N2}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el historial: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvHistorial_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in dgvHistorial.Rows)
            {
                if (row.Cells["Estado"].Value.ToString() == "Anulada")
                {
                    row.DefaultCellStyle.BackColor = Color.LightGray;
                    row.DefaultCellStyle.ForeColor = Color.DimGray;
                    row.DefaultCellStyle.SelectionBackColor = Color.LightGray;
                    row.DefaultCellStyle.SelectionForeColor = Color.DimGray;
                    row.DefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Strikeout);
                }
            }
        }

        private void BtnAnular_Click(object sender, EventArgs e)
        {
            if (dgvHistorial.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una venta de la lista para anular.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idVenta = Convert.ToInt32(dgvHistorial.CurrentRow.Cells["Nro Venta"].Value);
            string estado = dgvHistorial.CurrentRow.Cells["Estado"].Value.ToString();

            if (estado == "Anulada")
            {
                MessageBox.Show("Esta venta ya fue anulada previamente.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult res = MessageBox.Show($"¿Está seguro de que desea ANULAR la Venta N° {idVenta}?\n\nAl confirmar:\n• Se marcará el ticket como anulado.\n• Se devolverá el stock exacto a la pescadería.\n\nEsta acción no se puede deshacer.",
                                               "Confirmar Anulación Crítica", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (res == DialogResult.Yes)
            {
                string motivo = PedirMotivoAnulacion();

                if (string.IsNullOrWhiteSpace(motivo))
                {
                    MessageBox.Show("La anulación fue cancelada porque es obligatorio ingresar un motivo para el Administrador.", "Operación Cancelada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    ventaDatos.AnularVentaConMotivo(idVenta, SesionActiva.IdUsuario, motivo);
                    MessageBox.Show("Venta anulada con éxito.", "Operación Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarHistorial();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error crítico al intentar anular: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string PedirMotivoAnulacion()
        {
            Form prompt = new Form() { Width = 450, Height = 250, FormBorderStyle = FormBorderStyle.FixedDialog, Text = "Auditoría: Motivo de Anulación", StartPosition = FormStartPosition.CenterScreen, MaximizeBox = false, MinimizeBox = false, BackColor = Color.White };
            Label textLabel = new Label() { Left = 20, Top = 20, Width = 390, Text = "Por favor, explique detalladamente el motivo de la anulación.", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = false, Height = 40 };
            TextBox inputBox = new TextBox() { Left = 20, Top = 70, Width = 390, Height = 70, Multiline = true, Font = new Font("Segoe UI", 10) };
            Button confirmation = new Button() { Text = "Confirmar Anulación", Left = 230, Width = 180, Top = 160, DialogResult = DialogResult.OK, BackColor = Color.LightCoral, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            confirmation.FlatAppearance.BorderSize = 0;
            Button cancel = new Button() { Text = "Cancelar", Left = 20, Width = 100, Top = 160, DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9), Cursor = Cursors.Hand };

            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(inputBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);
            prompt.AcceptButton = confirmation;
            prompt.CancelButton = cancel;

            return prompt.ShowDialog() == DialogResult.OK ? inputBox.Text.Trim() : "";
        }

        private void dgvHistorial_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvHistorial.Columns[e.ColumnIndex].Name == "ColumnaTicket")
            {
                int idVenta = Convert.ToInt32(dgvHistorial.Rows[e.RowIndex].Cells["Nro Venta"].Value);
                string fecha = dgvHistorial.Rows[e.RowIndex].Cells["Fecha"].Value?.ToString() ?? "";
                string cliente = dgvHistorial.Rows[e.RowIndex].Cells["Cliente"].Value?.ToString() ?? "";
                string cajero = dgvHistorial.Rows[e.RowIndex].Cells["Cajero"].Value?.ToString() ?? "";
                string totalString = dgvHistorial.Rows[e.RowIndex].Cells["Total"].Value?.ToString().Replace("$", "").Trim() ?? "0";
                decimal total = Convert.ToDecimal(totalString);

                DataTable detalle = ventaDatos.ObtenerDetalleVenta(idVenta);
                ReimprimirTicket(idVenta, fecha, cliente, cajero, total, detalle);
            }
        }

        private void ReimprimirTicket(int idVenta, string fecha, string cliente, string cajero, decimal total, DataTable detalle)
        {
            PrintDocument pd = new PrintDocument();
            pd.DefaultPageSettings.PaperSize = new PaperSize("Ticket80mm", 314, 1000);

            pd.PrintPage += (sender, e) =>
            {
                Graphics g = e.Graphics;
                Font fontTitulo = new Font("Courier New", 12, FontStyle.Bold);
                Font fontNormal = new Font("Courier New", 10);

                int y = 20; int x = 10;
                g.DrawString("PESCADERÍA PACÚ IBERÁ", fontTitulo, Brushes.Black, x + 20, y); y += 20;
                g.DrawString("** REIMPRESIÓN **", fontNormal, Brushes.Black, x + 60, y); y += 30;
                g.DrawString("Fecha original: " + fecha, fontNormal, Brushes.Black, x, y); y += 20;
                g.DrawString("Ticket N°: " + idVenta, fontNormal, Brushes.Black, x, y); y += 20;
                g.DrawString("Cajero: " + cajero, fontNormal, Brushes.Black, x, y); y += 20;

                string clienteTicket = cliente.Length > 20 ? cliente.Substring(0, 20) : cliente;
                g.DrawString("Cliente: " + clienteTicket, fontNormal, Brushes.Black, x, y); y += 30;
                g.DrawString("----------------------------------------", fontNormal, Brushes.Black, x, y); y += 15;
                g.DrawString("CANT   DESCRIPCION            SUBTOTAL", fontNormal, Brushes.Black, x, y); y += 15;
                g.DrawString("----------------------------------------", fontNormal, Brushes.Black, x, y); y += 20;

                foreach (DataRow row in detalle.Rows)
                {
                    string cant = Convert.ToDecimal(row["Cantidad"]).ToString("0.00");
                    string nombre = row["Nombre"].ToString();
                    if (nombre.Length > 18) nombre = nombre.Substring(0, 18);
                    string subtotal = Convert.ToDecimal(row["Subtotal"]).ToString("0.00");
                    g.DrawString($"{cant,-6} {nombre,-19} ${subtotal,8}", fontNormal, Brushes.Black, x, y); y += 20;
                }

                y += 10;
                g.DrawString("----------------------------------------", fontNormal, Brushes.Black, x, y); y += 20;
                g.DrawString("TOTAL:  $" + total.ToString("0.00"), fontTitulo, Brushes.Black, x, y);
            };

            try
            {
                PrintPreviewDialog vistaPrevia = new PrintPreviewDialog();
                vistaPrevia.Document = pd;
                vistaPrevia.WindowState = FormWindowState.Maximized;
                vistaPrevia.PrintPreviewControl.Zoom = 1.5;
                vistaPrevia.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al previsualizar la reimpresión: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}