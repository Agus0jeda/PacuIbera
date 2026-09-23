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

        private Button btnTabCartera;
        private Button btnTabTop;

        // --- NUEVO: Control de edición ---
        private int idClienteSeleccionado = 0;

        public ClientesForm()
        {
            InitializeComponent();
            this.Load += ClientesForm_Load;
            btnGuardar.Click += btnGuardar_Click;
            btnCancelar.Click += btnCancelar_Click; // Conectamos el botón de Limpiar
        }

        private void ClientesForm_Load(object sender, EventArgs e)
        {
            ConfigurarPestanas();
            CargarGrillaTabs("Cartera");
            EstilizarGrilla();

            dgvClientes.CellFormatting += DgvClientes_CellFormatting;
            dgvClientes.CellClick += DgvClientes_CellClick; // Conectamos el clic en la tabla
        }

        private void ConfigurarPestanas()
        {
            int x = dgvClientes.Location.X;
            int y = dgvClientes.Location.Y - 40;

            btnTabCartera = new Button { Text = "👥 Cartera de Clientes", Location = new Point(x, y), Size = new Size(200, 40), FlatStyle = FlatStyle.Flat, BackColor = ColorTranslator.FromHtml("#EAEDED"), Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnTabCartera.FlatAppearance.BorderSize = 0;
            btnTabCartera.Click += (s, ev) => CargarGrillaTabs("Cartera");

            btnTabTop = new Button { Text = "🏆 Top Mejores Clientes", Location = new Point(x + 205, y), Size = new Size(210, 40), FlatStyle = FlatStyle.Flat, BackColor = Color.White, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
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

                    // Ocultamos las columnas separadas para que el usuario solo vea la unificada "Cliente"
                    if (dgvClientes.Columns.Contains("Nombre")) dgvClientes.Columns["Nombre"].Visible = false;
                    if (dgvClientes.Columns.Contains("Apellido")) dgvClientes.Columns["Apellido"].Visible = false;
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

        // --- NUEVO: Clic para Editar ---
        private void DgvClientes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Solo permitimos editar si estamos en la pestaña "Cartera" (no en el Top)
            if (e.RowIndex >= 0 && btnTabCartera.BackColor != Color.White)
            {
                DataGridViewRow fila = dgvClientes.Rows[e.RowIndex];

                idClienteSeleccionado = Convert.ToInt32(fila.Cells["Id"].Value);
                txtNombreCliente.Text = fila.Cells["Nombre"].Value.ToString();
                txtApellidoCliente.Text = fila.Cells["Apellido"].Value.ToString();
                txtTelefonoCliente.Text = fila.Cells["Telefono"].Value.ToString();
                txtDireccionCliente.Text = fila.Cells["Direccion"].Value.ToString();

                btnGuardar.Text = "ACTUALIZAR";
            }
        }

        public void FiltrarClientes(string filtro)
        {
            if (dgvClientes.DataSource is DataTable dt)
            {
                dt.DefaultView.RowFilter = $"Cliente LIKE '%{filtro}%' OR Telefono LIKE '%{filtro}%'";
            }
        }

        // --- NUEVO: Validación Extrema y Guardado Dual ---
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                // VALIDACIÓN OBLIGATORIA DE TODOS LOS CAMPOS
                if (string.IsNullOrWhiteSpace(txtNombreCliente.Text) ||
                    string.IsNullOrWhiteSpace(txtApellidoCliente.Text) ||
                    string.IsNullOrWhiteSpace(txtTelefonoCliente.Text) ||
                    string.IsNullOrWhiteSpace(txtDireccionCliente.Text))
                {
                    MessageBox.Show("Por favor, complete TODOS los campos (Nombre, Apellido, Teléfono y Dirección) antes de continuar.", "Datos Incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (idClienteSeleccionado == 0)
                {
                    // MODO: NUEVO CLIENTE
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
                }
                else
                {
                    // MODO: ACTUALIZAR CLIENTE EXISTENTE
                    clienteDatos.ModificarClienteCRM(
                        idClienteSeleccionado,
                        txtNombreCliente.Text,
                        txtApellidoCliente.Text,
                        txtTelefonoCliente.Text,
                        txtDireccionCliente.Text
                    );
                    MessageBox.Show("¡Cliente actualizado con éxito!", "Actualización", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                LimpiarFormulario();
                CargarGrillaTabs("Cartera");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en la operación: " + ex.Message, "Atención", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- NUEVO: Botón de Limpieza ---
        private void btnCancelar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            idClienteSeleccionado = 0;
            btnGuardar.Text = "GUARDAR";

            txtNombreCliente.Clear();
            txtApellidoCliente.Clear();
            txtTelefonoCliente.Clear();
            txtDireccionCliente.Clear();

            if (dgvClientes.SelectedRows.Count > 0)
                dgvClientes.ClearSelection();
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

        private void btnCerrar_Click(object sender, EventArgs e) { this.Close(); }
        private void txtBuscar_TextChanged(object sender, EventArgs e) { FiltrarClientes(txtBuscar.Text.Trim()); }
        private void label3_Click(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void nuevoCliente_Paint(object sender, PaintEventArgs e) { }
    }
}