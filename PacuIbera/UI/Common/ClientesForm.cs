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

        private OtrosDatos otrosDatos = new OtrosDatos();
        private UsuarioNegocio usuarioNegocio = new UsuarioNegocio();

        private Button btnTabCartera;
        private Button btnTabTop;

        // --- NUEVO: Control de edición ---
        private int idClienteSeleccionado = 0;

        public ClientesForm()
        {
            InitializeComponent();
            AsignarEventos();
            InicializarScrollPanel();
        }

        private void AsignarEventos()
        {
            this.Load += ClientesForm_Load;
            btnGuardar.Click += btnGuardar_Click;
            btnCancelar.Click += btnCancelar_Click;
            cmbProvincia.SelectedIndexChanged += cmbProvincia_SelectedIndexChanged;
            dgvClientes.DataBindingComplete += DgvClientes_DataBindingComplete;
            dgvClientes.CellFormatting += DgvClientes_CellFormatting;
            dgvClientes.CellClick += DgvClientes_CellClick;
            txtBuscar.TextChanged += txtBuscar_TextChanged;
        }

        private void InicializarScrollPanel()
        {
            try
            {
                if (nuevoCliente != null)
                {
                    nuevoCliente.AutoScroll = true;
                }
            }
            catch { }
        }

        private void ClientesForm_Load(object sender, EventArgs e)
        {
            ConfigurarPestanas();
            CargarCombos();
            CargarGrillaTabs("Cartera");
            EstilizarGrilla();

            // Aplicar EstiloUI al btnCerrar y al dgv
            EstiloUI.CrearBotonCerrar(this);
            EstiloUI.AplicarEstiloGrid(dgvClientes);

            dgvClientes.CellFormatting += DgvClientes_CellFormatting;
            dgvClientes.CellClick += DgvClientes_CellClick;
        }


        private void ConfigurarPestanas()
        {
            int x = dgvClientes.Location.X;
            int y = dgvClientes.Location.Y - 40;

            btnTabCartera = CrearBotonTab("👥 Cartera de Clientes", new Point(x, y), 200, () => CargarGrillaTabs("Cartera"));
            btnTabTop = CrearBotonTab("🏆 Top Mejores Clientes", new Point(x + 205, y), 210, () => CargarGrillaTabs("Top"));

            btnTabTop = new Button { Text = "🏆 Top Mejores Clientes", Location = new Point(x + 205, y), Size = new Size(210, 40), FlatStyle = FlatStyle.Flat, BackColor = Color.White, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnTabTop.FlatAppearance.BorderSize = 0;
            btnTabTop.Click += (s, ev) => CargarGrillaTabs("Top");

            this.Controls.Add(btnTabCartera);
            this.Controls.Add(btnTabTop);
            btnTabCartera.BringToFront();
            btnTabTop.BringToFront();
        }

        private Button CrearBotonTab(string texto, Point ubicacion, int ancho, Action accionClick)
        {
            var btn = new Button
            {
                Text = texto,
                Location = ubicacion,
                Size = new Size(ancho, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btn.FlatAppearance.BorderSize = 0;
            // Aquí envolvemos la Action para que cumpla con el evento Click
            btn.Click += (s, e) => accionClick();
            return btn;
        }

        private void CargarGrillaTabs(string tipo)
        {
            try
            {
                dgvClientes.Columns.Clear();
                dgvClientes.AutoGenerateColumns = true;

                bool esCartera = (tipo == "Cartera");

                btnTabCartera.BackColor = esCartera ? ColorTranslator.FromHtml("#EAEDED") : Color.White;
                btnTabTop.BackColor = esCartera ? Color.White : ColorTranslator.FromHtml("#EAEDED");

                if (esCartera)
                {
                    btnTabCartera.BackColor = ColorTranslator.FromHtml("#EAEDED");
                    btnTabTop.BackColor = Color.White;
                    dgvClientes.DataSource = clienteDatos.ObtenerClientesCRM();

                    // Ocultamos las columnas separadas
                    if (dgvClientes.Columns.Contains("Nombre")) dgvClientes.Columns["Nombre"].Visible = false;
                    if (dgvClientes.Columns.Contains("Apellido")) dgvClientes.Columns["Apellido"].Visible = false;

                    // Ocultamos el ID
                    try
                    {
                        if (dgvClientes.Columns.Contains("Id")) dgvClientes.Columns["Id"].Visible = false;
                        else if (dgvClientes.Columns.Count > 0) dgvClientes.Columns[0].Visible = false;
                    }

                OcultarIdColumna();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la grilla: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

                    // Ocultar ID en Top tambien
                    try
                    {
                        if (dgvClientes.Columns.Contains("Id")) dgvClientes.Columns["Id"].Visible = false;
                        else if (dgvClientes.Columns.Count > 0) dgvClientes.Columns[0].Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                if (dgvClientes.Columns.Contains("Id"))
                    dgvClientes.Columns["Id"].Visible = false;
                else if (dgvClientes.Columns.Count > 0)
                    dgvClientes.Columns[0].Visible = false;
            }
            catch { }
        }

        private void DgvClientes_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgvClientes.Columns[e.ColumnIndex].Name == "TotalGastado" && e.Value != DBNull.Value && e.Value != null)
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

        private void DgvClientes_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (!dgvClientes.Columns.Contains("Estado")) return;

            foreach (DataGridViewRow fila in dgvClientes.Rows)
            {
                if (fila.Cells["Estado"].Value != null)
                {
                    string estado = fila.Cells["Estado"].Value.ToString();
                    if (estado == "Inactivo")
                    {
                        fila.DefaultCellStyle.BackColor = Color.LightCoral;
                        fila.DefaultCellStyle.ForeColor = Color.White;
                    }
                    else
                    {
                        fila.DefaultCellStyle.BackColor = Color.White;
                        fila.DefaultCellStyle.ForeColor = Color.FromArgb(64, 64, 64);
                    }
                }
            dgvClientes.ClearSelection();
        }

        private void DgvClientes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Solo permitimos editar si estamos en la pestaña "Cartera" (no en el Top)
            if (e.RowIndex >= 0 && btnTabCartera.BackColor != Color.White)
            {
                DataGridViewRow fila = dgvClientes.Rows[e.RowIndex];

                idClienteSeleccionado = Convert.ToInt32(fila.Cells["Id"].Value ?? 0);
                txtNombreCliente.Text = fila.Cells["Nombre"]?.Value?.ToString() ?? string.Empty;
                txtApellidoCliente.Text = fila.Cells["Apellido"]?.Value?.ToString() ?? string.Empty;
                txtTelefonoCliente.Text = fila.Cells["Telefono"]?.Value?.ToString() ?? string.Empty;
                txtDireccionCliente.Text = fila.Cells["Direccion"]?.Value?.ToString() ?? string.Empty;

                if (dgvClientes.Columns.Contains("Provincia") && fila.Cells["Provincia"].Value != null)
                    cmbProvincia.Text = fila.Cells["Provincia"].Value.ToString();

                if (dgvClientes.Columns.Contains("Localidad") && fila.Cells["Localidad"].Value != null)
                    cmbLocalidad.Text = fila.Cells["Localidad"].Value.ToString();

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


        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (ValidarCamposVacios())
                {
                    MessageBox.Show("Por favor, complete todos los campos y seleccione Provincia y Localidad.", "Datos Incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int provId = Convert.ToInt32(cmbProvincia.SelectedValue);
                int locId = Convert.ToInt32(cmbLocalidad.SelectedValue);

                if (idClienteSeleccionado == 0)
                {
                    Cliente nuevoClienteObj = new Cliente
                    {
                        Nombre = txtNombreCliente.Text.Trim(),
                        Apellido = txtApellidoCliente.Text.Trim(),
                        Telefono = txtTelefonoCliente.Text.Trim(),
                        Direccion = txtDireccionCliente.Text.Trim(),
                        ProvinciaId = provId,
                        LocalidadId = locId,
                        DNI_CUIT = "",
                        Email = ""
                    };
                    negocio.Registrar(nuevoClienteObj);
                    MessageBox.Show("¡Cliente registrado con éxito!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    clienteDatos.ModificarClienteCRM(
                        idClienteSeleccionado,
                        txtNombreCliente.Text.Trim(),
                        txtApellidoCliente.Text.Trim(),
                        txtTelefonoCliente.Text.Trim(),
                        txtDireccionCliente.Text.Trim(),
                        provId,
                        locId
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

        private bool ValidarCamposVacios()
        {
            return string.IsNullOrWhiteSpace(txtNombreCliente.Text) ||
                   string.IsNullOrWhiteSpace(txtApellidoCliente.Text) ||
                   string.IsNullOrWhiteSpace(txtTelefonoCliente.Text) ||
                   string.IsNullOrWhiteSpace(txtDireccionCliente.Text) ||
                   cmbProvincia.SelectedIndex == -1 ||
                   cmbLocalidad.SelectedIndex == -1;
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
            // Dejo el método para retrocompatibilidad pero delego la mayor parte a EstiloUI
            EstiloUI.AplicarEstiloGrid(dgvClientes);
        }

        private void btnCerrar_Click(object sender, EventArgs e) { this.Close(); }
        private void txtBuscar_TextChanged(object sender, EventArgs e) { FiltrarClientes(txtBuscar.Text.Trim()); }
        private void label3_Click(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void nuevoCliente_Paint(object sender, PaintEventArgs e) { }

        private void CargarCombos()
        {
            cmbProvincia.DataSource = otrosDatos.ObtenerProvincias();
            cmbProvincia.DisplayMember = "Nombre";
            cmbProvincia.ValueMember = "Id";
            cmbProvincia.SelectedIndex = -1;
        }

        private void cmbProvincia_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Cuando el usuario elige una provincia, cargamos sus localidades
            if (cmbProvincia.SelectedValue != null && int.TryParse(cmbProvincia.SelectedValue.ToString(), out int provinciaId))
            {
                try
                {
                    cmbLocalidad.DataSource = usuarioNegocio.ListarLocalidades(provinciaId);
                    cmbLocalidad.DisplayMember = "Nombre";
                    cmbLocalidad.ValueMember = "Id";
                    cmbLocalidad.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar localidades: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvClientes.CurrentRow == null) return;
                int idSel = Convert.ToInt32(dgvClientes.CurrentRow.Cells[0].Value);
                // Lógica de eliminación (invocar clienteDatos u otro)
                var confirm = MessageBox.Show("¿Confirma la eliminación del cliente?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirm == DialogResult.Yes)
                {
                    var clienteDatos = new PacuIbera.Datos.ClienteDatos();
                    clienteDatos.EliminarCliente(idSel);
                    MessageBox.Show("Cliente eliminado", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarGrillaTabs("Cartera");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar cliente: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            FiltrarClientes(txtBuscar.Text.Trim());
        }
    }
}