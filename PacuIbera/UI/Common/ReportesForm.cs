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
        private DataGridView grillaStock; // Variable renombrada correctamente
        private DataGridView dgvResumenCajas;

        public ReportesForm()
        {
            ConfigurarInterfaz();
            this.Load += ReportesForm_Load;
        }

        private void ConfigurarInterfaz()
        {
            // 1. Clave para que se incruste en el PanelContenedor sin bordes feos
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = ColorTranslator.FromHtml("#F0F2F5");
            this.AutoScroll = true;

            // Título principal
            Label lblTitulo = new Label { Text = "DASHBOARD GERENCIAL", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = ColorTranslator.FromHtml("#1C2833"), Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblTitulo);

            // --- TARJETA 1: KPI INGRESOS ---
            Panel pnlKPI = CrearTarjeta(20, 70, 300, 110);
            pnlKPI.BackColor = ColorTranslator.FromHtml("#28B463");
            Label lblTituloKPI = new Label { Text = "INGRESOS DE HOY", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White, Location = new Point(15, 15), AutoSize = true };
            lblKPIIngresos = new Label { Text = "$0.00", Font = new Font("Segoe UI", 26, FontStyle.Bold), ForeColor = Color.White, Location = new Point(15, 45), AutoSize = true };
            pnlKPI.Controls.Add(lblTituloKPI);
            pnlKPI.Controls.Add(lblKPIIngresos);
            this.Controls.Add(pnlKPI);

            // --- TARJETA 2: MÉTODOS DE PAGO (Ahora con leyenda y formato) ---
            Panel pnlPagos = CrearTarjeta(340, 70, 500, 320);
            Label lblTitPagos = new Label { Text = "Distribución de Ingresos (Hoy)", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.DimGray, Location = new Point(15, 15), AutoSize = true };
            chartMetodosPago = new Chart { Location = new Point(15, 45), Size = new Size(460, 260) };
            chartMetodosPago.ChartAreas.Add(new ChartArea { BackColor = Color.White });

            // Agregamos la leyenda lateral
            Legend leyendaPagos = new Legend { Font = new Font("Segoe UI", 10), Docking = Docking.Right };
            chartMetodosPago.Legends.Add(leyendaPagos);

            Series seriePagos = new Series("Pagos") { ChartType = SeriesChartType.Doughnut };
            seriePagos.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            seriePagos.Label = "#PERCENT{P0}"; // Muestra el porcentaje adentro del gráfico (Ej: 100%)
            seriePagos.LegendText = "#VALX: $#VALY"; // Muestra "Efectivo: $25500" en la leyenda
            chartMetodosPago.Series.Add(seriePagos);

            pnlPagos.Controls.Add(lblTitPagos);
            pnlPagos.Controls.Add(chartMetodosPago);
            this.Controls.Add(pnlPagos);

            // --- TARJETA 3: TOP PRODUCTOS ---
            Panel pnlProductos = CrearTarjeta(20, 200, 300, 390);
            Label lblTitProd = new Label { Text = "Top 5 Productos (Global)", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.DimGray, Location = new Point(15, 15), AutoSize = true };
            chartTopProductos = new Chart { Location = new Point(10, 45), Size = new Size(280, 330) };
            ChartArea areaProd = new ChartArea { BackColor = Color.White };
            areaProd.AxisX.MajorGrid.Enabled = false;
            areaProd.AxisY.MajorGrid.LineColor = Color.LightGray;
            areaProd.AxisX.LabelStyle.Angle = -45;
            chartTopProductos.ChartAreas.Add(areaProd);

            Series serieProd = new Series("Ventas") { ChartType = SeriesChartType.Column, Color = ColorTranslator.FromHtml("#3498DB") };
            serieProd.IsValueShownAsLabel = true;
            serieProd["PixelPointWidth"] = "45"; // Hace que la barra no ocupe todo el ancho
            chartTopProductos.Series.Add(serieProd);

            pnlProductos.Controls.Add(lblTitProd);
            pnlProductos.Controls.Add(chartTopProductos);
            this.Controls.Add(pnlProductos);

            // --- TARJETA 4: ALERTAS DE STOCK ---
            Panel pnlStock = CrearTarjeta(340, 410, 500, 180);
            Label lblTitStock = new Label { Text = "Alertas de Stock Crítico", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = ColorTranslator.FromHtml("#E74C3C"), Location = new Point(15, 15), AutoSize = true };
            grillaStock = FormatearGrilla(new DataGridView { Location = new Point(15, 50), Size = new Size(470, 115) });
            pnlStock.Controls.Add(lblTitStock);
            pnlStock.Controls.Add(grillaStock);
            this.Controls.Add(pnlStock);

            // --- TARJETA 5: CONTROL DE CAJAS ---
            Panel pnlCajas = CrearTarjeta(20, 610, 820, 200);
            Label lblTitCajas = new Label { Text = "Auditoría de Cajas (Turnos de Hoy)", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.DimGray, Location = new Point(15, 15), AutoSize = true };
            dgvResumenCajas = FormatearGrilla(new DataGridView { Location = new Point(15, 50), Size = new Size(790, 135) });
            dgvResumenCajas.CellFormatting += DgvResumenCajas_CellFormatting;
            pnlCajas.Controls.Add(lblTitCajas);
            pnlCajas.Controls.Add(dgvResumenCajas);
            this.Controls.Add(pnlCajas);
        }

        private Panel CrearTarjeta(int x, int y, int ancho, int alto)
        {
            return new Panel { BackColor = Color.White, Location = new Point(x, y), Size = new Size(ancho, alto) };
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

                // 3. Alertas de Stock conectado a la variable correcta
                grillaStock.DataSource = repDatos.ObtenerStockCritico();

                // 4. Resumen de Cajas
                dgvResumenCajas.DataSource = cajaDatos.ObtenerResumenCajas(DateTime.Now);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el Dashboard: " + ex.Message, "Error de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
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