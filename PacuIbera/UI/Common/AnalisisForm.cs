using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Datos;

namespace PacuIbera.UI.Common
{
    public partial class AnalisisForm : Form
    {
        private ReportesDatos repDatos = new ReportesDatos();
        private Chart chartCurvaVentas;
        private DataGridView dgvMermas;

        public AnalisisForm()
        {
            ConfigurarInterfaz();
            this.Load += AnalisisForm_Load;
        }

        private void ConfigurarInterfaz()
        {
            this.BackColor = ColorTranslator.FromHtml("#F0F2F5");
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;

            Label lblTitulo = new Label { Text = "ANÁLISIS ESTRATÉGICO", Font = new Font("Segoe UI", 18, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblTitulo);

            // --- 1. Gráfico Curva de Ventas ---
            Label lblCurva = new Label { Text = "📈 Tendencia de Ventas (Últimos 30 días)", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(20, 70), AutoSize = true };
            chartCurvaVentas = new Chart { Location = new Point(20, 100), Size = new Size(950, 300), BackColor = Color.White };
            ChartArea ca = new ChartArea();
            ca.AxisX.MajorGrid.LineColor = Color.WhiteSmoke;
            ca.AxisY.MajorGrid.LineColor = Color.LightGray;
            chartCurvaVentas.ChartAreas.Add(ca);

            Series serieCurva = new Series("Recaudacion")
            {
                ChartType = SeriesChartType.SplineArea, // Gráfico de área suavizada
                Color = Color.FromArgb(100, 52, 152, 219),
                BorderColor = ColorTranslator.FromHtml("#2980B9"),
                BorderWidth = 3,
                IsValueShownAsLabel = true
            };
            chartCurvaVentas.Series.Add(serieCurva);
            this.Controls.Add(lblCurva);
            this.Controls.Add(chartCurvaVentas);

            // --- 2. Grilla de Mermas (Pérdidas) ---
            Label lblMerma = new Label { Text = "🗑️ Pérdida Económica por Vencimientos", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.DarkRed, Location = new Point(20, 420), AutoSize = true };
            dgvMermas = new DataGridView
            {
                Location = new Point(20, 450),
                Size = new Size(950, 250),
                BackgroundColor = Color.White,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                BorderStyle = BorderStyle.None
            };
            dgvMermas.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#FADBD8");
            dgvMermas.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvMermas.CellFormatting += DgvMermas_CellFormatting;

            this.Controls.Add(lblMerma);
            this.Controls.Add(dgvMermas);
        }

        private void AnalisisForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Calculamos los últimos 30 días automáticamente
                DateTime desde = DateTime.Today.AddDays(-30);
                DateTime hasta = DateTime.Today;

                DataTable dtCurva = repDatos.ObtenerCurvaVentas(desde, hasta);
                chartCurvaVentas.DataSource = dtCurva;
                if (dtCurva.Rows.Count > 0)
                {
                    chartCurvaVentas.Series[0].XValueMember = "Dia";
                    chartCurvaVentas.Series[0].YValueMembers = "Recaudacion";
                    chartCurvaVentas.DataBind();
                }

                dgvMermas.DataSource = repDatos.ObtenerPerdidaVencimientos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el análisis: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvMermas_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvMermas.Columns[e.ColumnIndex].Name == "Dinero Perdido" && e.Value != DBNull.Value)
            {
                e.Value = string.Format("${0:N2}", Convert.ToDecimal(e.Value));
                e.FormattingApplied = true;
                // Pintamos toda la fila de rojo suave para alertar la pérdida
                dgvMermas.Rows[e.RowIndex].DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#FDEDEC");
                dgvMermas.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
            }
        }
    }
}