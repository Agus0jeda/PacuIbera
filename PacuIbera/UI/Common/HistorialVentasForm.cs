using Datos;
using Microsoft.Data.SqlClient;
using PacuIbera.Datos;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Printing;

namespace PacuIbera.UI.Common // Cambiá esto si tu namespace de UI es distinto
{
    public partial class HistorialVentasForm : Form
    {
        private DataGridView dgvHistorial;
        private Button btnAnular;
        private VentaDatos ventaDatos = new VentaDatos();

        private TextBox txtBuscador;

        public HistorialVentasForm()
        {
            InitializeComponent();

            // Integración transparente para el menú lateral
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

            // 1. Panel Superior
            Panel pnlCabecera = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = Color.White };
            Label lblTitulo = new Label { Text = "Historial y Anulaciones", Font = new Font("Segoe UI", 18, FontStyle.Bold), Location = new Point(20, 25), AutoSize = true };


            // Buscador Universal
            Label lblBuscar = new Label { Text = "🔍 Buscar:", Font = new Font("Segoe UI", 11, FontStyle.Bold), Location = new Point(20, 75), AutoSize = true };

            // Lo ponemos pegadito al texto "Buscar:" y lo hacemos un poco más largo
            txtBuscador = new TextBox { Font = new Font("Segoe UI", 12), Location = new Point(105, 72), Width = 400 };
            // Conectamos el evento para que busque a medida que se escribe
            txtBuscador.TextChanged += TxtBuscador_TextChanged;

            pnlCabecera.Controls.Add(lblBuscar);
            pnlCabecera.Controls.Add(txtBuscador);
            btnAnular = new Button { Text = "❌ Anular Venta", Font = new Font("Segoe UI", 11, FontStyle.Bold), BackColor = Color.LightCoral, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(180, 45), Cursor = Cursors.Hand };
            btnAnular.Location = new Point(pnlCabecera.Width - 200, 18);
            btnAnular.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAnular.FlatAppearance.BorderSize = 0;
            btnAnular.Click += BtnAnular_Click;

            pnlCabecera.Controls.Add(lblTitulo);
            pnlCabecera.Controls.Add(btnAnular);

            // 2. Panel Central y Grilla
            Panel pnlCentro = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            dgvHistorial = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false, Font = new Font("Segoe UI", 12) };
            dgvHistorial.CellContentClick += dgvHistorial_CellContentClick;
            dgvHistorial.RowTemplate.Height = 40;
            dgvHistorial.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            dgvHistorial.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvHistorial.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);

            // Evento para pintar las filas anuladas
            dgvHistorial.DataBindingComplete += DgvHistorial_DataBindingComplete;

            pnlCentro.Controls.Add(dgvHistorial);

            this.Controls.Add(pnlCentro);
            this.Controls.Add(pnlCabecera);
        }

        private void TxtBuscador_TextChanged(object sender, EventArgs e)
        {
            if (dgvHistorial.DataSource is DataTable dt)
            {
                string texto = txtBuscador.Text.Trim().Replace("'", ""); // Limpiamos el texto por seguridad

                if (string.IsNullOrEmpty(texto))
                {
                    dt.DefaultView.RowFilter = ""; // Muestra todo si está vacío
                }
                else
                {
                    // Agregamos la conversión del Total a texto para poder filtrarlo
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
                dgvHistorial.DataSource = ventaDatos.ObtenerHistorialVentas();

                // 1. Centrar TODAS las columnas y cabeceras
                foreach (DataGridViewColumn col in dgvHistorial.Columns)
                {
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }

                // 2. Arreglar el formato del Total (Adiós XDR)
                if (dgvHistorial.Columns.Contains("Total"))
                {
                    dgvHistorial.Columns["Total"].DefaultCellStyle.Format = "$ #,##0.00";
                }

                // 3. Agregar el botón de Ticket (si no existe aún)
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el historial: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvHistorial_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Pinta visualmente las ventas muertas para no confundir al cajero
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
                MessageBox.Show("Esta venta ya fue anulada previamente. El stock ya fue devuelto.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult res = MessageBox.Show($"¿Está seguro de que desea ANULAR la Venta N° {idVenta}?\n\nAl confirmar:\n• Se marcará el ticket como anulado.\n• Se devolverá el stock exacto a la pescadería.\n\nEsta acción no se puede deshacer.",
                                               "Confirmar Anulación Crítica", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (res == DialogResult.Yes)
            {
                try
                {
                    ventaDatos.AnularVenta(idVenta);
                    MessageBox.Show("Venta anulada con éxito. El stock ha sido devuelto a los lotes originales.", "Operación Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarHistorial(); // Refrescamos la grilla para ver la fila gris
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error crítico al intentar anular: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvHistorial_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvHistorial.Columns[e.ColumnIndex].Name == "ColumnaTicket")
            {
                // 1. Extraemos los datos de cabecera directamente de la fila seleccionada
                int idVenta = Convert.ToInt32(dgvHistorial.Rows[e.RowIndex].Cells["Nro Venta"].Value);
                string fecha = dgvHistorial.Rows[e.RowIndex].Cells["Fecha"].Value.ToString();
                string cliente = dgvHistorial.Rows[e.RowIndex].Cells["Cliente"].Value.ToString();
                string cajero = dgvHistorial.Rows[e.RowIndex].Cells["Cajero"].Value.ToString();

                // Limpiamos el signo pesos del string para poder convertirlo a decimal
                string totalString = dgvHistorial.Rows[e.RowIndex].Cells["Total"].Value.ToString().Replace("$", "").Trim();
                decimal total = Convert.ToDecimal(totalString);

                // 2. Buscamos el detalle de los pescados en la base de datos
                DataTable detalle = ventaDatos.ObtenerDetalleVenta(idVenta);

                // 3. Mandamos a dibujar el ticket
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