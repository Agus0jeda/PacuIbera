using System;
using System.Data;
using System.Drawing;
using System.Globalization;
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

            var distribucion = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(18),
                BackColor = BackColor
            };
            distribucion.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            distribucion.RowStyles.Add(new RowStyle(SizeType.Percent, 44));
            distribucion.RowStyles.Add(new RowStyle(SizeType.Percent, 56));

            var seccionTendencia = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Margin = new Padding(0, 0, 0, 12),
                BackColor = BackColor
            };
            seccionTendencia.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            seccionTendencia.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            seccionTendencia.RowStyles.Add(new RowStyle(SizeType.Absolute, 27));
            seccionTendencia.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            Label lblTitulo = new Label
            {
                Text = "ANÁLISIS ESTRATÉGICO",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };

            // --- 1. Gráfico Curva de Ventas ---
            Label lblCurva = new Label
            {
                Text = "📈 Tendencia de Ventas (Últimos 30 días)",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };
            chartCurvaVentas = new Chart { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0) };
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
            seccionTendencia.Controls.Add(lblTitulo, 0, 0);
            seccionTendencia.Controls.Add(lblCurva, 0, 1);
            seccionTendencia.Controls.Add(chartCurvaVentas, 0, 2);

            // --- 2. Consulta de mercadería vencida ---
            var seccionVencimientos = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Margin = new Padding(0),
                BackColor = BackColor
            };
            seccionVencimientos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            seccionVencimientos.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            seccionVencimientos.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            seccionVencimientos.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            Label lblMerma = new Label
            {
                Text = "Mercadería vencida y valor estimado",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#8E3B35"),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };
            Label lblAclaracionMerma = new Label
            {
                Text = "Valor estimado a precio de venta. No representa mercadería descartada ni una pérdida contable confirmada",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = ColorTranslator.FromHtml("#555555"),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false,
                Padding = new Padding(0, 2, 0, 2)
            };
            dgvMermas = new DataGridView
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                BackgroundColor = Color.White,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                ScrollBars = ScrollBars.Both,
                MultiSelect = false
            };
            dgvMermas.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#FADBD8");
            dgvMermas.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvMermas.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvMermas.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            dgvMermas.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#F5D5D1");
            dgvMermas.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvMermas.DataBindingComplete += (_, _) => ConfigurarColumnasMermas();
            dgvMermas.CellFormatting += DgvMermas_CellFormatting;

            seccionVencimientos.Controls.Add(lblMerma, 0, 0);
            seccionVencimientos.Controls.Add(lblAclaracionMerma, 0, 1);
            seccionVencimientos.Controls.Add(dgvMermas, 0, 2);

            distribucion.Controls.Add(seccionTendencia, 0, 0);
            distribucion.Controls.Add(seccionVencimientos, 0, 1);
            this.Controls.Add(distribucion);
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
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Value == null || e.Value == DBNull.Value)
                return;

            var columna = dgvMermas.Columns[e.ColumnIndex];
            string nombre = string.IsNullOrEmpty(columna.DataPropertyName) ? columna.Name : columna.DataPropertyName;

            if (nombre == "FechaVencimiento" && e.Value is DateTime fecha)
            {
                e.Value = fecha.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("es-AR"));
                e.FormattingApplied = true;
            }
            else if (nombre is "Cantidad vencida" or "Unidades Tiradas")
            {
                decimal cantidad = Convert.ToDecimal(e.Value, CultureInfo.InvariantCulture);
                if (dgvMermas.Columns.Contains("SeVendePorPeso"))
                {
                    bool porPeso = Convert.ToBoolean(dgvMermas.Rows[e.RowIndex].Cells["SeVendePorPeso"].Value ?? false);
                    e.Value = porPeso
                        ? cantidad.ToString("0.000 'kg'", CultureInfo.GetCultureInfo("es-AR"))
                        : cantidad.ToString("0 'un.'", CultureInfo.GetCultureInfo("es-AR"));
                }
                else
                {
                    // Compatibilidad visual con la versión instalada antes del script:
                    // no se inventa la unidad si el procedimiento aún no devuelve el tipo de venta.
                    e.Value = cantidad.ToString("0.###", CultureInfo.GetCultureInfo("es-AR"));
                }
                e.FormattingApplied = true;
                columna.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            else if (nombre is "Valor estimado" or "Dinero Perdido")
            {
                e.Value = Convert.ToDecimal(e.Value, CultureInfo.InvariantCulture)
                    .ToString("C2", CultureInfo.GetCultureInfo("es-AR"));
                e.FormattingApplied = true;
                columna.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            dgvMermas.Rows[e.RowIndex].DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#FDEDEC");
            dgvMermas.Rows[e.RowIndex].DefaultCellStyle.ForeColor = ColorTranslator.FromHtml("#6E3430");
        }

        private void ConfigurarColumnasMermas()
        {
            foreach (DataGridViewColumn columna in dgvMermas.Columns)
            {
                string nombre = string.IsNullOrEmpty(columna.DataPropertyName) ? columna.Name : columna.DataPropertyName;
                switch (nombre)
                {
                    case "Producto":
                        columna.HeaderText = "Producto";
                        columna.MinimumWidth = 145;
                        columna.FillWeight = 145;
                        break;
                    case "Lote":
                        columna.HeaderText = "N.º de lote";
                        columna.MinimumWidth = 78;
                        columna.FillWeight = 75;
                        columna.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        break;
                    case "FechaVencimiento":
                        columna.HeaderText = "Vencimiento";
                        columna.MinimumWidth = 105;
                        columna.FillWeight = 100;
                        columna.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        break;
                    case "Cantidad vencida":
                    case "Unidades Tiradas":
                        columna.HeaderText = "Cantidad vencida";
                        columna.MinimumWidth = 155;
                        columna.FillWeight = 145;
                        columna.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        break;
                    case "Valor estimado":
                    case "Dinero Perdido":
                        columna.HeaderText = "Valor estimado";
                        columna.MinimumWidth = 170;
                        columna.FillWeight = 155;
                        columna.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        break;
                    case "SeVendePorPeso":
                        columna.Visible = false;
                        break;
                }
            }
        }
    }
}
