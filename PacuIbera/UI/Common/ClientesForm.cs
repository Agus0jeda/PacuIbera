using Negocio;
using PacuIbera.Datos;
using PacuIbera.Dominio;
using Datos;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public partial class ClientesForm : Form
    {
        private ClienteNegocio negocio = new ClienteNegocio();
        private ClienteDatos clienteDatos = new ClienteDatos();

        // Variables para las pestañas
        private Button btnTabCartera;
        private Button btnTabTop;

        public ClientesForm()
        {
            InitializeComponent();
            this.Load += ClientesForm_Load;
            btnGuardar.Click += btnGuardar_Click;
        }

        private void ClientesForm_Load(object sender, EventArgs e)
        {
            ConfigurarPestanas();
            CargarGrillaTabs("Cartera");
            EstilizarGrilla();
            dgvClientes.CellFormatting += DgvClientes_CellFormatting;
        }

        // --- PESTAÑAS DINÁMICAS ---
        private void ConfigurarPestanas()
        {
            int x = dgvClientes.Location.X;
            int y = dgvClientes.Location.Y - 40;

            btnTabCartera = new Button
            {
                Text = "👥 Cartera de Clientes",
                Location = new Point(x, y),
                Size = new Size(200, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = ColorTranslator.FromHtml("#EAEDED"),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnTabCartera.FlatAppearance.BorderSize = 0;
            btnTabCartera.Click += (s, ev) => CargarGrillaTabs("Cartera");

            btnTabTop = new Button
            {
                Text = "🏆 Top Mejores Clientes",
                Location = new Point(x + 205, y),
                Size = new Size(210, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnTabTop.FlatAppearance.BorderSize = 0;
            btnTabTop.Click += (s, ev) => CargarGrillaTabs("Top");

            this.Controls.Add(btnTabCartera);
            this.Controls.Add(btnTabTop);
            btnTabCartera.BringToFront();
            btnTabTop.BringToFront();
        }

        private void CargarGrillaTabs(string tipo)
        {
            try
            {
                dgvClientes.Columns.Clear();
                dgvClientes.AutoGenerateColumns = true;

                if (tipo == "Cartera")
                {
                    btnTabCartera.BackColor = ColorTranslator.FromHtml("#EAEDED");
                    btnTabTop.BackColor = Color.White;
                    dgvClientes.DataSource = clienteDatos.ObtenerClientesCRM();
                }
                else
                {
                    btnTabTop.BackColor = ColorTranslator.FromHtml("#EAEDED");
                    btnTabCartera.BackColor = Color.White;
                    dgvClientes.DataSource = clienteDatos.ObtenerTopClientes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la grilla: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvClientes_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvClientes.Columns[e.ColumnIndex].Name == "TotalGastado" && e.Value != DBNull.Value)
            {
                e.Value = string.Format("${0:N2}", e.Value);
                e.FormattingApplied = true;
            }

            if (dgvClientes.Columns[e.ColumnIndex].Name == "Cliente" && e.RowIndex < 3 && btnTabTop.BackColor != Color.White)
            {
                dgvClientes.Rows[e.RowIndex].DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#FEF9E7");
                dgvClientes.Rows[e.RowIndex].DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            }
        }

        // --- BUSCADOR INTELIGENTE UNIFICADO ---
        public void FiltrarClientes(string filtro)
        {
            if (dgvClientes.DataSource is DataTable dt)
            {
                dt.DefaultView.RowFilter = $"Cliente LIKE '%{filtro}%' OR Telefono LIKE '%{filtro}%'";

                if (dt.DefaultView.Count == 0 && !string.IsNullOrWhiteSpace(filtro))
                {
                    // Asumimos que tenés un label llamado lblSinResultados. Si te da error, borrá el texto de abajo.
                    // lblSinResultados.Text = "⚠️ No se encontró ningún cliente con ese criterio.";
                    // lblSinResultados.Visible = true;
                }
                else
                {
                    // lblSinResultados.Visible = false;
                }
            }
        }

        // --- GUARDADO ULTRALIMPIO ---
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNombreCliente.Text))
                {
                    MessageBox.Show("El nombre del cliente es obligatorio.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Cliente nuevoCliente = new Cliente
                {
                    Nombre = txtNombreCliente.Text.Trim(),
                    Apellido = txtApellidoCliente.Text.Trim(),
                    Telefono = txtTelefonoCliente.Text.Trim(),
                    Direccion = txtDireccionCliente.Text.Trim(),

                    ProvinciaId = 1,
                    LocalidadId = 1,
                    DNI_CUIT = "",
                    Email = ""
                };

                negocio.Registrar(nuevoCliente);

                MessageBox.Show("¡Cliente registrado con éxito!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarFormulario();
                CargarGrillaTabs("Cartera");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el cliente: " + ex.Message, "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void EstilizarGrilla()
        {
            dgvClientes.BackgroundColor = Color.White;
            dgvClientes.BorderStyle = BorderStyle.None;
            dgvClientes.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvClientes.GridColor = Color.FromArgb(230, 230, 230);
            dgvClientes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvClientes.MultiSelect = false;
            dgvClientes.RowHeadersVisible = false;
            dgvClientes.ReadOnly = true;
            dgvClientes.AllowUserToAddRows = false;
            dgvClientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvClientes.EnableHeadersVisualStyles = false;
            dgvClientes.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvClientes.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(46, 204, 113);
            dgvClientes.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvClientes.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvClientes.ColumnHeadersHeight = 35;

            dgvClientes.DefaultCellStyle.BackColor = Color.White;
            dgvClientes.DefaultCellStyle.ForeColor = Color.FromArgb(64, 64, 64);
            dgvClientes.DefaultCellStyle.SelectionBackColor = Color.FromArgb(39, 174, 96);
            dgvClientes.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvClientes.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            dgvClientes.RowTemplate.Height = 30;
        }

        private void LimpiarFormulario()
        {
            txtNombreCliente.Clear();
            txtApellidoCliente.Clear();
            txtTelefonoCliente.Clear();
            txtDireccionCliente.Clear();
        }

        private void btnCerrar_Click(object sender, EventArgs e) { this.Close(); }
        private void txtBuscar_TextChanged(object sender, EventArgs e) { FiltrarClientes(txtBuscar.Text.Trim()); }
        private void label3_Click(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void btnCancelar_Click(object sender, EventArgs e) { }
        private void nuevoCliente_Paint(object sender, PaintEventArgs e) { }
    }
}