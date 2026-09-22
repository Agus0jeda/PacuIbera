using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Datos;

namespace PacuIbera.UI.Common
{
    public partial class ReportesForm : Form
    {
        private Label lblKPIIngresos;
        private Chart chartMetodosPago;
        private Chart chartTopProductos;
        private DataGridView grillaStock;
        private DataGridView dgvResumenCajas;

        // Variables para los botones de las pestañas (Tabs)
        private Button btnTabStock;
        private Button btnTabVencimientos;

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

            // Título Principal
            Label lblTitulo = new Label { Text = "DASHBOARD GERENCIAL", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = ColorTranslator.FromHtml("#1C2833"), Dock = DockStyle.Top, Padding = new Padding(15, 15, 0, 10), AutoSize = true };
            this.Controls.Add(lblTitulo);

            // Grilla Maestra
            TableLayoutPanel tlpMain = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), ColumnCount = 2, RowCount = 2 };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));

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
            Label lblTituloKPI = new Label { Text = "INGRESOS DE HOY", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White, Location = new Point(15, 15), AutoSize = true };
            lblKPIIngresos = new Label { Text = "$0.00", Font = new Font("Segoe UI", 26, FontStyle.Bold), ForeColor = Color.White, Location = new Point(15, 40), AutoSize = true };
            pnlKPI.Controls.Add(lblTituloKPI);
            pnlKPI.Controls.Add(lblKPIIngresos);
            tlpIzquierda.Controls.Add(pnlKPI, 0, 0);

            // --- TARJETA 3: PRODUCTOS ---
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

            // --- TARJETA 2: PAGOS ---
            Panel pnlPagos = CrearTarjetaFill();
            Label lblTitPagos = new Label { Text = "Distribución de Ingresos (Hoy)", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.DimGray, Dock = DockStyle.Top, Padding = new Padding(10, 10, 0, 0), AutoSize = true };

            chartMetodosPago = new Chart { Dock = DockStyle.Fill, MinimumSize = new Size(10, 10) };
            chartMetodosPago.ChartAreas.Add(new ChartArea { BackColor = Color.White });
            chartMetodosPago.Legends.Add(new Legend { Font = new Font("Segoe UI", 10), Docking = Docking.Right });
            Series seriePagos = new Series("Pagos") { ChartType = SeriesChartType.Doughnut, Font = new Font("Segoe UI", 10, FontStyle.Bold), Label = "#PERCENT{P0}", LegendText = "#VALX: $#VALY" };
            chartMetodosPago.Series.Add(seriePagos);
            pnlPagos.Controls.Add(chartMetodosPago);
            pnlPagos.Controls.Add(lblTitPagos);
            tlpDerecha.Controls.Add(pnlPagos, 0, 0);

            // --- TARJETA 4: ALERTAS (AHORA CON PESTAÑAS) ---
            Panel pnlStock = CrearTarjetaFill();

            // Botonera de pestañas
            Panel pnlTabs = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.White };

            btnTabStock = new Button { Text = "📦 Stock Crítico", Dock = DockStyle.Left, Width = 150, FlatStyle = FlatStyle.Flat, BackColor = ColorTranslator.FromHtml("#EAEDED"), Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnTabStock.FlatAppearance.BorderSize = 0;
            btnTabStock.Click += (s, e) => CargarGrillaAlertas("Stock");

            btnTabVencimientos = new Button
            {
                Text = "⚠️ Vencimientos",
                Dock = DockStyle.Left,
                Width = 150,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = ColorTranslator.FromHtml("#E74C3C"), // Letra roja brillante
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnTabVencimientos.FlatAppearance.BorderSize = 0;
            btnTabVencimientos.Click += (s, e) => CargarGrillaAlertas("Vencimientos");

            pnlTabs.Controls.Add(btnTabVencimientos); // Agregamos invertido para que flote a la izquierda
            pnlTabs.Controls.Add(btnTabStock);

            grillaStock = FormatearGrilla(new DataGridView { Dock = DockStyle.Fill });
            grillaStock.CellFormatting += GrillaStock_CellFormatting; // Pinta de colores

            pnlStock.Controls.Add(grillaStock);
            pnlStock.Controls.Add(pnlTabs);
            tlpDerecha.Controls.Add(pnlStock, 0, 1);

            // --- TARJETA 5: CAJAS ---
            Panel pnlCajas = CrearTarjetaFill();
            Label lblTitCajas = new Label { Text = "Auditoría de Cajas (Turnos de Hoy)", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.DimGray, Dock = DockStyle.Top, Padding = new Padding(10, 10, 0, 10), AutoSize = true };
            dgvResumenCajas = FormatearGrilla(new DataGridView { Dock = DockStyle.Fill });
            dgvResumenCajas.CellFormatting += DgvResumenCajas_CellFormatting;
            pnlCajas.Controls.Add(dgvResumenCajas);
            pnlCajas.Controls.Add(lblTitCajas);

            tlpMain.Controls.Add(pnlCajas, 0, 1);
            tlpMain.SetColumnSpan(pnlCajas, 2);

            this.Controls.Add(tlpMain);
            tlpMain.BringToFront();

            this.ResumeLayout(false);
        }

        private Panel CrearTarjetaFill()
        {
            return new Panel
            {
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Margin = new Padding(8)
            };
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

        private void ReportesForm_Load(object sender, EventArgs e)
        {
            CargarDatosEnVivo();
            CargarGrillaAlertas("Stock"); // Por defecto arranca mostrando el stock
        }

        private void CargarDatosEnVivo()
        {
            try
            {
                ReportesDatos repDatos = new ReportesDatos();
                CajaDatos cajaDatos = new CajaDatos();

                // 1. KPI y Métodos de Pago
                DataTable dtPagos = repDatos.ObtenerMetodosPagoHoy();
                decimal ingresosTotales = 0;
                if (dtPagos.Rows.Count > 0)
                {
                    chartMetodosPago.DataSource = dtPagos;
                    chartMetodosPago.Series[0].XValueMember = "Nombre";
                    chartMetodosPago.Series[0].YValueMembers = "Total";
                    chartMetodosPago.DataBind();

                    foreach (DataRow row in dtPagos.Rows)
                    {
                        ingresosTotales += Convert.ToDecimal(row["Total"]);
                    }
                }
                lblKPIIngresos.Text = $"$ {ingresosTotales:N2}";

                // 2. Top Productos
                DataTable dtProd = repDatos.ObtenerTopProductos();
                if (dtProd.Rows.Count > 0)
                {
                    chartTopProductos.DataSource = dtProd;
                    chartTopProductos.Series[0].XValueMember = "Nombre";
                    chartTopProductos.Series[0].YValueMembers = "TotalVendido";
                    chartTopProductos.DataBind();
                }

                // 3. Resumen de Cajas
                dgvResumenCajas.DataSource = cajaDatos.ObtenerResumenCajas(DateTime.Now);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el Dashboard: " + ex.Message, "Error de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- LÓGICA DE PESTAÑAS Y COLORES ---
        private void CargarGrillaAlertas(string tipo)
        {
            try
            {
                ReportesDatos repDatos = new ReportesDatos();
                if (tipo == "Stock")
                {
                    btnTabStock.BackColor = ColorTranslator.FromHtml("#EAEDED"); // Gris clarito activo
                    btnTabVencimientos.BackColor = Color.White;
                    grillaStock.DataSource = repDatos.ObtenerStockCritico();
                }
                else
                {
                    btnTabVencimientos.BackColor = ColorTranslator.FromHtml("#FADBD8"); // Rojo clarito activo
                    btnTabStock.BackColor = Color.White;
                    grillaStock.DataSource = repDatos.ObtenerLotesVencidos();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la alerta: " + ex.Message);
            }
        }

        private void GrillaStock_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Si estamos en la pestaña de vencimientos, pintamos según el estado
            if (grillaStock.Columns[e.ColumnIndex].Name == "Estado" && e.Value != DBNull.Value)
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
        }

        private void DgvResumenCajas_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvResumenCajas.Columns[e.ColumnIndex].Name == "Diferencia" && e.Value != DBNull.Value)
            {
                decimal dif = Convert.ToDecimal(e.Value);
                if (dif < 0)
                {
                    dgvResumenCajas.Rows[e.RowIndex].DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#FADBD8");
                    dgvResumenCajas.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
                }
            }
        }
    }
}