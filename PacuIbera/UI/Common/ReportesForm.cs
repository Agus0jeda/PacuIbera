using Datos; // Unificamos la referencia a la capa de datos
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using PacuIbera.Datos;

namespace PacuIbera.UI.Common
{
    public partial class ReportesForm : Form
    {
        private ReportesDatos reportesDatos = new ReportesDatos();
        private CajaDatos cajaDatos = new CajaDatos();

        private Label lblKPIIngresos;
        private Chart chartMetodosPago;
        private Chart chartTopProductos;
        private DataGridView grillaStock;
        private DataGridView dgvResumenCajas;
        private DataGridView dgvDetalleCierre;
        private Label lblIdentificacionCaja;
        private Label lblDatosCierre;
        private DataTable? datosResumenCajas;
        private static readonly CultureInfo CulturaArgentina = CultureInfo.GetCultureInfo("es-AR");

        // Filtros de fecha
        private DateTimePicker dtpDesde;
        private DateTimePicker dtpHasta;

        // Pestañas
        private Button btnTabStock;
        private Button btnTabVencimientos;
        private Button btnTabVendedores;
        private Button btnTabAuditoria;

        private string tabActiva = "Stock";

        public ReportesForm()
        {
            ConfigurarInterfaz();
            this.Load += ReportesForm_Load;
        }

        private void ConfigurarInterfaz()
        {
            this.SuspendLayout();
            this.Size = new Size(1024, 768);
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = ColorTranslator.FromHtml("#F0F2F5");

            // --- CABECERA CON FILTROS ---
            Panel pnlCabecera = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = ColorTranslator.FromHtml("#F0F2F5") };
            Label lblTitulo = new Label { Text = "DASHBOARD GERENCIAL", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = ColorTranslator.FromHtml("#1C2833"), Location = new Point(15, 15), AutoSize = true };

            Label lblDesde = new Label { Text = "Desde:", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(400, 25), AutoSize = true };
            dtpDesde = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(460, 23), Width = 110 };

            Label lblHasta = new Label { Text = "Hasta:", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(590, 25), AutoSize = true };
            dtpHasta = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(650, 23), Width = 110 };

            Button btnFiltrar = new Button { Text = "Filtrar Fechas", Location = new Point(780, 21), Size = new Size(110, 28), BackColor = ColorTranslator.FromHtml("#3498DB"), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            btnFiltrar.FlatAppearance.BorderSize = 0;
            btnFiltrar.Click += (s, e) => CargarEstadisticas();

            pnlCabecera.Controls.Add(lblTitulo);
            pnlCabecera.Controls.Add(lblDesde);
            pnlCabecera.Controls.Add(dtpDesde);
            pnlCabecera.Controls.Add(lblHasta);
            pnlCabecera.Controls.Add(dtpHasta);
            pnlCabecera.Controls.Add(btnFiltrar);
            this.Controls.Add(pnlCabecera);

            // --- GRILLA MAESTRA ---
            TableLayoutPanel tlpMain = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), ColumnCount = 2, RowCount = 2 };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));

            TableLayoutPanel tlpIzquierda = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = new Padding(0) };
            tlpIzquierda.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
            tlpIzquierda.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            TableLayoutPanel tlpDerecha = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = new Padding(0) };
            tlpDerecha.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));
            tlpDerecha.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));

            tlpMain.Controls.Add(tlpIzquierda, 0, 0);
            tlpMain.Controls.Add(tlpDerecha, 1, 0);

            // --- TARJETA 1: KPI ---
            Panel pnlKPI = CrearTarjetaFill();
            pnlKPI.BackColor = ColorTranslator.FromHtml("#28B463");
            Label lblTituloKPI = new Label { Text = "INGRESOS TOTALES (Período Seleccionado)", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White, Location = new Point(15, 15), AutoSize = true };
            lblKPIIngresos = new Label { Text = "$0.00", Font = new Font("Segoe UI", 26, FontStyle.Bold), ForeColor = Color.White, Location = new Point(15, 40), AutoSize = true };
            pnlKPI.Controls.Add(lblTituloKPI);
            pnlKPI.Controls.Add(lblKPIIngresos);
            tlpIzquierda.Controls.Add(pnlKPI, 0, 0);

            // --- TARJETA 2: PRODUCTOS ---
            Panel pnlProductos = CrearTarjetaFill();
            Label lblTitProd = new Label { Text = "Top 5 Productos", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.DimGray, Dock = DockStyle.Top, Padding = new Padding(10, 10, 0, 0), AutoSize = true };
            chartTopProductos = new Chart { Dock = DockStyle.Fill, MinimumSize = new Size(10, 10) };
            chartTopProductos.ChartAreas.Add(new ChartArea { BackColor = Color.White });
            chartTopProductos.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chartTopProductos.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
            Series serieProd = new Series("Ventas") { ChartType = SeriesChartType.Column, Color = ColorTranslator.FromHtml("#3498DB"), IsValueShownAsLabel = true };
            serieProd["PixelPointWidth"] = "40";
            chartTopProductos.Series.Add(serieProd);
            pnlProductos.Controls.Add(chartTopProductos);
            pnlProductos.Controls.Add(lblTitProd);
            tlpIzquierda.Controls.Add(pnlProductos, 0, 1);

            // --- TARJETA 3: PAGOS ---
            Panel pnlPagos = CrearTarjetaFill();
            Label lblTitPagos = new Label { Text = "Distribución de Ingresos", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.DimGray, Dock = DockStyle.Top, Padding = new Padding(10, 10, 0, 0), AutoSize = true };
            chartMetodosPago = new Chart { Dock = DockStyle.Fill, MinimumSize = new Size(10, 10) };
            chartMetodosPago.ChartAreas.Add(new ChartArea { BackColor = Color.White });
            chartMetodosPago.Legends.Add(new Legend { Font = new Font("Segoe UI", 10), Docking = Docking.Right });

            Series seriePagos = new Series("Pagos") { ChartType = SeriesChartType.Doughnut, Font = new Font("Segoe UI", 10, FontStyle.Bold), Label = "#PERCENT{P0}", LegendText = "#VALX: $#VALY{N2}" };
            chartMetodosPago.Series.Add(seriePagos);

            pnlPagos.Controls.Add(chartMetodosPago);
            pnlPagos.Controls.Add(lblTitPagos);
            tlpDerecha.Controls.Add(pnlPagos, 0, 0);

            // --- TARJETA 4: ALERTAS Y RANKING ---
            Panel pnlStock = CrearTarjetaFill();
            Panel pnlTabs = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.White };

            btnTabStock = new Button { Text = "📦 Stock Crítico", Dock = DockStyle.Left, Width = 130, FlatStyle = FlatStyle.Flat, BackColor = ColorTranslator.FromHtml("#EAEDED"), Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btnTabStock.FlatAppearance.BorderSize = 0;
            btnTabStock.Click += (s, e) => CargarGrillaAlertas("Stock");

            btnTabVencimientos = new Button { Text = "⚠️ Vencimientos", Dock = DockStyle.Left, Width = 140, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = ColorTranslator.FromHtml("#E74C3C"), Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btnTabVencimientos.FlatAppearance.BorderSize = 0;
            btnTabVencimientos.Click += (s, e) => CargarGrillaAlertas("Vencimientos");

            btnTabVendedores = new Button { Text = "🏆 Vendedores", Dock = DockStyle.Left, Width = 130, FlatStyle = FlatStyle.Flat, BackColor = Color.White, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btnTabVendedores.FlatAppearance.BorderSize = 0;
            btnTabVendedores.Click += (s, e) => CargarGrillaAlertas("Vendedores");

            btnTabAuditoria = new Button { Text = "🚨 Anulaciones", Dock = DockStyle.Left, Width = 140, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Color.DarkRed, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btnTabAuditoria.FlatAppearance.BorderSize = 0;
            btnTabAuditoria.Click += (s, e) => CargarGrillaAlertas("Auditoria");

            pnlTabs.Controls.Add(btnTabAuditoria);
            pnlTabs.Controls.Add(btnTabVendedores);
            pnlTabs.Controls.Add(btnTabVencimientos);
            pnlTabs.Controls.Add(btnTabStock);

            grillaStock = FormatearGrilla(new DataGridView { Dock = DockStyle.Fill });
            grillaStock.CellFormatting += GrillaStock_CellFormatting;
            grillaStock.CellDoubleClick += GrillaStock_CellDoubleClick;
            grillaStock.CellContentClick += grillaStock_CellContentClick; // Conectamos el clic del ticket

            pnlStock.Controls.Add(grillaStock);
            pnlStock.Controls.Add(pnlTabs);
            tlpDerecha.Controls.Add(pnlStock, 0, 1);

            // --- TARJETA 5: CAJAS ---
            Panel pnlCajas = CrearTarjetaFill();
            Label lblTitCajas = new Label { Text = "Auditoría de Cajas (Turnos cerrados)", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.DimGray, Dock = DockStyle.Top, Height = 34, Padding = new Padding(10, 8, 0, 0) };
            var seccionesCajas = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Size = new Size(900, 320),
                Orientation = Orientation.Horizontal,
                SplitterDistance = 130,
                Panel1MinSize = 100,
                Panel2MinSize = 150,
                FixedPanel = FixedPanel.Panel1,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White
            };
            var panelListadoCajas = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            var lblListadoCajas = new Label { Text = "LISTADO RESUMIDO · Seleccioná un cierre para ver el detalle", Dock = DockStyle.Top, Height = 25, Padding = new Padding(10, 5, 0, 0), Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = ColorTranslator.FromHtml("#34495E") };
            dgvResumenCajas = FormatearGrilla(new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                ScrollBars = ScrollBars.Both,
                ColumnHeadersHeight = 38,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            });
            dgvResumenCajas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvResumenCajas.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvResumenCajas.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#D5EBDD");
            dgvResumenCajas.DefaultCellStyle.SelectionForeColor = Color.FromArgb(35, 65, 48);
            dgvResumenCajas.CellFormatting += DgvResumenCajas_CellFormatting;
            dgvResumenCajas.SelectionChanged += DgvResumenCajas_SelectionChanged;
            panelListadoCajas.Controls.Add(dgvResumenCajas);
            panelListadoCajas.Controls.Add(lblListadoCajas);
            seccionesCajas.Panel1.Controls.Add(panelListadoCajas);

            var panelDetalleCierre = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(0, 0, 0, 2) };
            lblIdentificacionCaja = new Label
            {
                Text = "Detalle del cierre seleccionado",
                Dock = DockStyle.Top,
                Height = 25,
                Padding = new Padding(10, 4, 0, 0),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#34495E")
            };
            lblDatosCierre = new Label
            {
                Text = "Seleccioná un cierre del listado superior.",
                Dock = DockStyle.Top,
                Height = 25,
                Padding = new Padding(10, 3, 0, 0),
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(80, 80, 80),
                AutoEllipsis = true
            };
            dgvDetalleCierre = FormatearGrilla(new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                ScrollBars = ScrollBars.Vertical,
                ColumnHeadersHeight = 32
            });
            dgvDetalleCierre.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvDetalleCierre.Columns.Add(new DataGridViewTextBoxColumn { Name = "Concepto", HeaderText = "Concepto", Width = 245, MinimumWidth = 180, ReadOnly = true, SortMode = DataGridViewColumnSortMode.NotSortable });
            dgvDetalleCierre.Columns.Add(new DataGridViewTextBoxColumn { Name = "Valor", HeaderText = "Importe / observación", MinimumWidth = 220, ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, SortMode = DataGridViewColumnSortMode.NotSortable });
            dgvDetalleCierre.Columns["Valor"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvDetalleCierre.RowTemplate.Height = 26;
            dgvDetalleCierre.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#EAEDED");
            dgvDetalleCierre.DefaultCellStyle.SelectionForeColor = Color.FromArgb(40, 50, 45);
            panelDetalleCierre.Controls.Add(dgvDetalleCierre);
            panelDetalleCierre.Controls.Add(lblDatosCierre);
            panelDetalleCierre.Controls.Add(lblIdentificacionCaja);
            seccionesCajas.Panel2.Controls.Add(panelDetalleCierre);

            pnlCajas.Controls.Add(seccionesCajas);
            pnlCajas.Controls.Add(lblTitCajas);

            tlpMain.Controls.Add(pnlCajas, 0, 1);
            tlpMain.SetColumnSpan(pnlCajas, 2);

            this.Controls.Add(tlpMain);
            tlpMain.BringToFront();
            this.ResumeLayout(false);
        }

        private void ReportesForm_Load(object sender, EventArgs e)
        {
            dtpDesde.Value = DateTime.Today.AddDays(-30);
            dtpHasta.Value = DateTime.Today;
            CargarEstadisticas();
            CargarGrillaAlertas("Stock");
        }

        private void CargarEstadisticas()
        {
            try
            {
                DateTime desde = dtpDesde.Value.Date;
                DateTime hasta = dtpHasta.Value.Date;

                DataTable dtResumen = reportesDatos.ObtenerResumenGlobal(desde, hasta);
                if (dtResumen.Rows.Count > 0 && dtResumen.Rows[0]["TotalRecaudado"] != DBNull.Value)
                {
                    decimal total = Convert.ToDecimal(dtResumen.Rows[0]["TotalRecaudado"]);
                    lblKPIIngresos.Text = $"$ {total:N2}";
                }

                DataTable dtPagos = reportesDatos.ObtenerMetodosPago(desde, hasta);
                chartMetodosPago.DataSource = dtPagos;
                if (dtPagos.Rows.Count > 0)
                {
                    chartMetodosPago.Series[0].XValueMember = "Nombre";
                    chartMetodosPago.Series[0].YValueMembers = "Total";
                    chartMetodosPago.DataBind();
                }
                else { chartMetodosPago.Series[0].Points.Clear(); }

                DataTable dtProd = reportesDatos.ObtenerTopProductos(desde, hasta);
                chartTopProductos.DataSource = dtProd;
                if (dtProd.Rows.Count > 0)
                {
                    chartTopProductos.Series[0].XValueMember = "Nombre";
                    chartTopProductos.Series[0].YValueMembers = "TotalVendido";
                    chartTopProductos.DataBind();
                }
                else { chartTopProductos.Series[0].Points.Clear(); }

                CargarGrillaAlertas(tabActiva);
                datosResumenCajas = cajaDatos.ObtenerHistorialCierres(desde, hasta);
                ConfigurarListadoCajas(datosResumenCajas);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el Dashboard: " + ex.Message, "Error de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarGrillaAlertas(string tipo)
        {
            try
            {
                tabActiva = tipo;
                btnTabStock.BackColor = Color.White;
                btnTabVencimientos.BackColor = Color.White;
                btnTabVendedores.BackColor = Color.White;
                btnTabAuditoria.BackColor = Color.White;

                if (grillaStock.Columns.Contains("ColumnaTicket"))
                {
                    grillaStock.Columns.Remove("ColumnaTicket");
                }

                if (tipo == "Stock")
                {
                    btnTabStock.BackColor = ColorTranslator.FromHtml("#EAEDED");
                    grillaStock.DataSource = reportesDatos.ObtenerStockCritico();
                }
                else if (tipo == "Vencimientos")
                {
                    btnTabVencimientos.BackColor = ColorTranslator.FromHtml("#FADBD8");
                    grillaStock.DataSource = reportesDatos.ObtenerLotesVencidos();
                }
                else if (tipo == "Vendedores")
                {
                    btnTabVendedores.BackColor = ColorTranslator.FromHtml("#EAEDED");
                    grillaStock.DataSource = reportesDatos.ObtenerRankingVendedores(dtpDesde.Value.Date, dtpHasta.Value.Date);
                }
                else if (tipo == "Auditoria")
                {
                    btnTabAuditoria.BackColor = ColorTranslator.FromHtml("#FADBD8");
                    grillaStock.DataSource = reportesDatos.ObtenerAuditoriaAnulaciones(dtpDesde.Value.Date, dtpHasta.Value.Date);

                    DataGridViewButtonColumn btnTicket = new DataGridViewButtonColumn();
                    btnTicket.Name = "ColumnaTicket";
                    btnTicket.HeaderText = "Acción";
                    btnTicket.Text = "🎫 Ver Ticket";
                    btnTicket.UseColumnTextForButtonValue = true;
                    btnTicket.FlatStyle = FlatStyle.Flat;
                    grillaStock.Columns.Add(btnTicket);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la tabla: " + ex.Message);
            }
        }

        private void GrillaStock_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && tabActiva == "Auditoria")
            {
                string motivo = grillaStock.Rows[e.RowIndex].Cells["Motivo"].Value.ToString();
                string vendedor = grillaStock.Rows[e.RowIndex].Cells["Vendedor que Anuló"].Value.ToString();

                MessageBox.Show($"Motivo de anulación:\n\n\"{motivo}\"", $"Anulado por {vendedor}", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void grillaStock_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (tabActiva == "Auditoria" && e.RowIndex >= 0 && grillaStock.Columns[e.ColumnIndex].Name == "ColumnaTicket")
            {
                try
                {
                    int idVenta = Convert.ToInt32(grillaStock.Rows[e.RowIndex].Cells["Nro Venta"].Value);
                    string vendedor = grillaStock.Rows[e.RowIndex].Cells["Vendedor que Anuló"].Value.ToString();

                    // Aseguramos que la fecha se procese correctamente
                    string fecha = Convert.ToDateTime(grillaStock.Rows[e.RowIndex].Cells["Fecha"].Value).ToString("dd/MM/yyyy HH:mm");

                    // EXTRAEMOS EL VALOR PURO SIN IMPORTAR EL SIGNO PESOS O FORMATO VISUAL
                    decimal total = Convert.ToDecimal(grillaStock.Rows[e.RowIndex].Cells["Total"].Value);

                    VentaDatos vDatos = new VentaDatos();
                    DataTable detalle = vDatos.ObtenerDetalleVenta(idVenta);

                    ReimprimirTicketAuditoria(idVenta, fecha, "Anulado", vendedor, total, detalle);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al intentar abrir el ticket: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ReimprimirTicketAuditoria(int idVenta, string fecha, string estado, string vendedor, decimal total, DataTable detalle)
        {
            PrintDocument pd = new PrintDocument();
            pd.DefaultPageSettings.PaperSize = new PaperSize("Ticket80mm", 314, 1000);

            pd.PrintPage += (sender, ev) =>
            {
                Graphics g = ev.Graphics;
                Font fontTitulo = new Font("Courier New", 12, FontStyle.Bold);
                Font fontNormal = new Font("Courier New", 10);

                int y = 20; int x = 10;
                g.DrawString("PESCADERÍA PACÚ IBERÁ", fontTitulo, Brushes.Black, x + 20, y); y += 20;
                g.DrawString("*** TICKET ANULADO ***", fontNormal, Brushes.DarkRed, x + 40, y); y += 30;
                g.DrawString("Fecha original: " + fecha, fontNormal, Brushes.Black, x, y); y += 20;
                g.DrawString("Ticket N°: " + idVenta, fontNormal, Brushes.Black, x, y); y += 20;
                g.DrawString("Vendedor: " + vendedor, fontNormal, Brushes.Black, x, y); y += 30;
                g.DrawString("----------------------------------------", fontNormal, Brushes.Black, x, y); y += 15;
                g.DrawString("CANT   DESCRIPCION            SUBTOTAL", fontNormal, Brushes.Black, x, y); y += 15;
                g.DrawString("----------------------------------------", fontNormal, Brushes.Black, x, y); y += 20;

                if (detalle != null)
                {
                    foreach (DataRow row in detalle.Rows)
                    {
                        string cant = Convert.ToDecimal(row["Cantidad"]).ToString("0.00");
                        string nombre = row["Nombre"].ToString();
                        if (nombre.Length > 18) nombre = nombre.Substring(0, 18);
                        string subtotal = Convert.ToDecimal(row["Subtotal"]).ToString("0.00");
                        g.DrawString($"{cant,-6} {nombre,-19} ${subtotal,8}", fontNormal, Brushes.Black, x, y); y += 20;
                    }
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
                MessageBox.Show("Error al previsualizar el ticket: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GrillaStock_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (tabActiva == "Vencimientos" && grillaStock.Columns[e.ColumnIndex].Name == "Estado" && e.Value != DBNull.Value)
            {
                string estado = e.Value.ToString();
                if (estado == "Vencido")
                {
                    grillaStock.Rows[e.RowIndex].DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#FADBD8");
                    grillaStock.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
                }
                else if (estado == "Pronto")
                {
                    grillaStock.Rows[e.RowIndex].DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#FCF3CF");
                    grillaStock.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.DarkGoldenrod;
                }
            }

            // Damos formato visual de dinero sin alterar el valor real de la base de datos
            if (tabActiva == "Vendedores" && grillaStock.Columns[e.ColumnIndex].Name == "Total Recaudado" && e.Value != DBNull.Value)
            {
                e.Value = string.Format("${0:N2}", Convert.ToDecimal(e.Value));
                e.FormattingApplied = true;
            }
            if (tabActiva == "Auditoria" && grillaStock.Columns[e.ColumnIndex].Name == "Total" && e.Value != DBNull.Value)
            {
                e.Value = string.Format("${0:N2}", Convert.ToDecimal(e.Value));
                e.FormattingApplied = true;
            }
        }

        private void DgvResumenCajas_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            string columna = dgvResumenCajas.Columns[e.ColumnIndex].Name;
            if (e.Value == null || e.Value == DBNull.Value)
            {
                if (columna == "MontoEsperado") e.Value = "No calculable";
                else if (columna == "EfectivoDeclarado") e.Value = "Sin cierre";
                else if (columna == "Diferencia") e.Value = "No disponible";
                else return;
                e.FormattingApplied = true;
                return;
            }
            if (columna is "MontoEsperado" or "EfectivoDeclarado" or "Diferencia")
            {
                decimal importe = Convert.ToDecimal(e.Value, CultureInfo.InvariantCulture);
                e.Value = importe.ToString("C2", CulturaArgentina);
                e.FormattingApplied = true;
                if (columna == "Diferencia")
                {
                    var celda = dgvResumenCajas.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    celda.Style.BackColor = importe < 0 ? ColorTranslator.FromHtml("#FCE8E6")
                        : importe > 0 ? ColorTranslator.FromHtml("#E8F5E9") : ColorTranslator.FromHtml("#F1F8E9");
                    celda.Style.ForeColor = importe < 0 ? Color.DarkRed
                        : importe > 0 ? Color.DarkGreen : Color.DarkSlateGray;
                }
            }
            else if (columna is "Apertura" or "Cierre" && e.Value is DateTime fecha)
            {
                e.Value = fecha.ToString("dd/MM/yyyy HH:mm", CulturaArgentina);
                e.FormattingApplied = true;
            }
        }

        private void ConfigurarListadoCajas(DataTable datos)
        {
            if (!datos.Columns.Contains("Cajero") || !datos.Columns.Contains("Apertura"))
            {
                dgvResumenCajas.DataSource = datos;
                lblIdentificacionCaja.Text = "El resultado SQL no incluye los datos necesarios para identificar el cierre.";
                dgvDetalleCierre.Rows.Clear();
                return;
            }

            bool tieneNumeroCaja = datos.Columns.Contains("CajaId");
            string columnaIdentificacion = tieneNumeroCaja ? "CajaId" : "IdentificacionCaja";
            if (!tieneNumeroCaja && !datos.Columns.Contains("IdentificacionCaja"))
                datos.Columns.Add("IdentificacionCaja", typeof(string));
            foreach (DataRow fila in datos.Rows)
            {
                if (!tieneNumeroCaja)
                {
                    string cajero = fila["Cajero"] == DBNull.Value ? "Cajero sin dato" : Convert.ToString(fila["Cajero"]) ?? "Cajero sin dato";
                    string apertura = fila["Apertura"] == DBNull.Value ? "Apertura sin dato" : Convert.ToDateTime(fila["Apertura"]).ToString("dd/MM/yyyy HH:mm", CulturaArgentina);
                    fila["IdentificacionCaja"] = $"{cajero} · {apertura}";
                }
            }

            dgvResumenCajas.DataSource = datos;
            foreach (DataGridViewColumn columna in dgvResumenCajas.Columns) columna.Visible = false;
            MostrarColumnaResumen(datos, columnaIdentificacion, tieneNumeroCaja ? "N.º de caja" : "Identificación", tieneNumeroCaja ? 95 : 220);
            MostrarColumnaResumen(datos, "Cajero", "Cajero", 135);
            MostrarColumnaResumen(datos, "Apertura", "Apertura", 140);
            MostrarColumnaResumen(datos, "Cierre", "Cierre", 140);
            MostrarColumnaResumen(datos, "MontoEsperado", "Esperado", 110);
            MostrarColumnaResumen(datos, "EfectivoDeclarado", "Contado", 110);
            MostrarColumnaResumen(datos, "Diferencia", "Diferencia", 110);
            MostrarColumnaResumen(datos, "Estado", "Estado", 100);
            dgvResumenCajas.ClearSelection();
            dgvDetalleCierre.Rows.Clear();
            lblIdentificacionCaja.Text = tieneNumeroCaja
                ? "Detalle del cierre seleccionado"
                : "Detalle del cierre seleccionado · identificación por cajero y apertura";
            lblDatosCierre.Text = dgvResumenCajas.Rows.Count == 0
                ? "No hay cierres para el período seleccionado."
                : "Seleccioná un cierre del listado superior.";
            if (dgvResumenCajas.Rows.Count > 0) dgvResumenCajas.Rows[0].Selected = true;
        }

        private void MostrarColumnaResumen(DataTable datos, string nombre, string titulo, int ancho)
        {
            if (!datos.Columns.Contains(nombre) || !dgvResumenCajas.Columns.Contains(nombre)) return;
            var columna = dgvResumenCajas.Columns[nombre];
            columna.Visible = true;
            columna.HeaderText = titulo;
            columna.Width = ancho;
            columna.MinimumWidth = Math.Min(ancho, 75);
            columna.SortMode = DataGridViewColumnSortMode.Automatic;
        }

        private void DgvResumenCajas_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvResumenCajas.CurrentRow?.DataBoundItem is not DataRowView fila) return;
            MostrarDetalleCierre(fila.Row);
        }

        private void MostrarDetalleCierre(DataRow fila)
        {
            dgvDetalleCierre.Rows.Clear();
            if (datosResumenCajas == null) return;
            string cajero = ValorTexto(fila, "Cajero", "Sin dato");
            string apertura = ValorFecha(fila, "Apertura");
            string numeroCaja = fila.Table.Columns.Contains("CajaId") ? $"Caja {ValorTexto(fila, "CajaId", "Sin dato")} - " : string.Empty;
            lblDatosCierre.Text = $"{numeroCaja}{cajero} - Apertura {apertura}";

            AgregarDetalle("Fondo inicial", ValorDinero(fila, "FondoCaja"));
            AgregarDetalle("Ventas del sistema", ValorDinero(fila, "VentasSistema"));
            AgregarDetalle("Cobros en efectivo retenidos", ValorDinero(fila, "CobrosEfectivo"));
            AgregarDetalle("Cobros por tarjeta", ValorDinero(fila, "CobrosTarjeta"));
            AgregarDetalle("Cobros por transferencia", ValorDinero(fila, "CobrosTransferencia"));
            AgregarDetalle("Ingresos adicionales", ValorDinero(fila, "IngresosCaja"));
            AgregarDetalle("Retiros", ValorDinero(fila, "RetirosCaja"));
            AgregarDetalle("Efectivo esperado", ValorDinero(fila, "MontoEsperado"));
            AgregarDetalle("Efectivo contado", ValorDinero(fila, "EfectivoDeclarado"));
            AgregarDetalle("Diferencia", ValorDinero(fila, "Diferencia"));
            AgregarDetalle("Estado", ValorTexto(fila, "Estado", "Sin dato"));
            AgregarDetalle("Observaciones / bloqueo", ValorTexto(fila, "ObservacionEfectivo", "Sin observaciones"));
            dgvDetalleCierre.ClearSelection();
        }

        private string ValorTexto(DataRow fila, string columna, string siFalta) =>
            fila.Table.Columns.Contains(columna) && fila[columna] != DBNull.Value
                ? Convert.ToString(fila[columna]) ?? siFalta : siFalta;

        private string ValorFecha(DataRow fila, string columna) =>
            fila.Table.Columns.Contains(columna) && fila[columna] != DBNull.Value
                ? Convert.ToDateTime(fila[columna]).ToString("dd/MM/yyyy HH:mm", CulturaArgentina) : "Sin dato";

        private string ValorDinero(DataRow fila, string columna)
        {
            if (!fila.Table.Columns.Contains(columna) || fila[columna] == DBNull.Value)
                return columna == "MontoEsperado" ? "No calculable" : columna == "EfectivoDeclarado" ? "Sin cierre" : "No disponible";
            return Convert.ToDecimal(fila[columna], CultureInfo.InvariantCulture).ToString("C2", CulturaArgentina);
        }

        private void AgregarDetalle(string concepto, string valor) => dgvDetalleCierre.Rows.Add(concepto, valor);

        private Panel CrearTarjetaFill()
        {
            return new Panel { BackColor = Color.White, Dock = DockStyle.Fill, Margin = new Padding(8) };
        }

        private DataGridView FormatearGrilla(DataGridView dgv)
        {
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.GridColor = Color.WhiteSmoke;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#EAEDED");
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            return dgv;
        }
    }
}
