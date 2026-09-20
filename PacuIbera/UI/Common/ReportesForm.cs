using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace PacuIbera.UI.Common
{
    public partial class ReportesForm : Form
    {
        private Chart? chartMetodosPago;
        private Chart? chartTopProductos;
        private Button? btnFinanzas;
        private Button? btnInventario;
        private Label? lblDetalles;

        public ReportesForm()
        {
            InitializeComponent();
            this.Load += ReportesForm_Load;
        }

        private void ReportesForm_Load(object? sender, EventArgs e)
        {
            ConfigurarInterfaz();
            CargarGraficoMetodosPago();
            CargarGraficoTopProductos();
            CargarAlertasStock();

            // Arrancamos mostrando por defecto la vista de Finanzas
            MostrarFinanzas(null, EventArgs.Empty);
        }

        private void ConfigurarInterfaz()
        {
            // 1. Botón Finanzas
            btnFinanzas = new Button();
            btnFinanzas.Text = "💰 Ver Finanzas";
            btnFinanzas.Size = new Size(150, 40);
            btnFinanzas.Location = new Point(20, 20);
            btnFinanzas.BackColor = Color.White;
            btnFinanzas.Cursor = Cursors.Hand;
            btnFinanzas.Click += MostrarFinanzas;
            this.Controls.Add(btnFinanzas);

            // 2. Botón Inventario
            btnInventario = new Button();
            btnInventario.Text = "📦 Ver Inventario";
            btnInventario.Size = new Size(150, 40);
            btnInventario.Location = new Point(180, 20);
            btnInventario.BackColor = Color.White;
            btnInventario.Cursor = Cursors.Hand;
            btnInventario.Click += MostrarInventario;
            this.Controls.Add(btnInventario);

            // 3. Etiqueta de detalles dinámicos
            lblDetalles = new Label();
            lblDetalles.Location = new Point(20, 75);
            lblDetalles.Size = new Size(800, 30);
            lblDetalles.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblDetalles.ForeColor = Color.DarkSlateGray;
            this.Controls.Add(lblDetalles);

            // 4. Gráfico de Pagos
            chartMetodosPago = new Chart();
            chartMetodosPago.Size = new Size(500, 350);
            chartMetodosPago.Location = new Point(20, 120);
            ChartArea areaPago = new ChartArea();
            chartMetodosPago.ChartAreas.Add(areaPago);

            // Agregar leyenda para entender los colores
            Legend leyenda = new Legend("Leyenda");
            chartMetodosPago.Legends.Add(leyenda);
            this.Controls.Add(chartMetodosPago);

            // 5. Gráfico de Productos
            chartTopProductos = new Chart();
            chartTopProductos.Size = new Size(400, 350);
            chartTopProductos.Location = new Point(20, 120);
            ChartArea areaProductos = new ChartArea();

            // Evitar que se corten los nombres de abajo
            areaProductos.AxisX.Interval = 1;
            areaProductos.AxisX.LabelStyle.Angle = -45;
            chartTopProductos.ChartAreas.Add(areaProductos);
            this.Controls.Add(chartTopProductos);

            // 6. Configurar la Grilla de Alertas (dgvAlertasStock ya existe en el diseñador)
            dgvAlertasStock.Location = new Point(440, 120);
            dgvAlertasStock.Size = new Size(350, 350);
            dgvAlertasStock.BackgroundColor = Color.White;
            dgvAlertasStock.RowHeadersVisible = false;
        }

        private void MostrarFinanzas(object? sender, EventArgs e)
        {
            // Cambiamos visibilidad
            chartMetodosPago!.Visible = true;
            chartTopProductos!.Visible = false;
            dgvAlertasStock.Visible = false;

            // Actualizamos la información
            lblDetalles!.Text = "Resumen del Día: Ingresos totales por $630,000. Método líder: Mercado Pago.";
            btnFinanzas!.BackColor = Color.LightGreen;
            btnInventario!.BackColor = Color.White;
        }

        private void MostrarInventario(object? sender, EventArgs e)
        {
            // Cambiamos visibilidad
            chartMetodosPago!.Visible = false;
            chartTopProductos!.Visible = true;
            dgvAlertasStock.Visible = true;

            // Actualizamos la información
            lblDetalles!.Text = "Estado de Inventario: 3 productos requieren reposición urgente.";
            btnInventario!.BackColor = Color.LightGreen;
            btnFinanzas!.BackColor = Color.White;
        }

        private void CargarGraficoMetodosPago()
        {
            chartMetodosPago!.Series.Clear();
            chartMetodosPago.Titles.Clear();

            Series serie = new Series("Pagos");
            serie.ChartType = SeriesChartType.Pie;

            serie.Points.AddXY("Mercado Pago", 350000);
            serie.Points.AddXY("Efectivo", 150000);
            serie.Points.AddXY("Débito", 85000);
            serie.Points.AddXY("Crédito", 45000);

            serie.IsValueShownAsLabel = true;
            chartMetodosPago.Series.Add(serie);
            chartMetodosPago.Titles.Add("Ingresos por Método de Pago");
        }

        private void CargarGraficoTopProductos()
        {
            chartTopProductos!.Series.Clear();
            chartTopProductos.Titles.Clear();

            Series serie = new Series("Ventas");
            serie.ChartType = SeriesChartType.Column; // Cambiado a vertical (Column)

            serie.Points.AddXY("Iniciador 20kg", 125);
            serie.Points.AddXY("Bomba 1HP", 45);
            serie.Points.AddXY("Red Captura", 38);
            serie.Points.AddXY("Filtro Biol.", 25);
            serie.Points.AddXY("Termómetro", 18);

            chartTopProductos.Series.Add(serie);
            chartTopProductos.Titles.Add("Top 5 Productos Más Vendidos");
        }

        private void CargarAlertasStock()
        {
            dgvAlertasStock.Columns.Clear();
            dgvAlertasStock.Columns.Add("Producto", "Producto");
            dgvAlertasStock.Columns.Add("Stock", "Stock");
            dgvAlertasStock.Columns.Add("Estado", "Estado");

            dgvAlertasStock.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvAlertasStock.AllowUserToAddRows = false;

            dgvAlertasStock.Rows.Add("Alimento Engorde 10kg", "5", "Crítico");
            dgvAlertasStock.Rows.Add("Oxigenador Solar", "2", "Crítico");
            dgvAlertasStock.Rows.Add("Kit Medidor pH", "8", "Bajo");
        }
    }
}