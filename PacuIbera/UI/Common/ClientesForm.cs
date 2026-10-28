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
        private int idClienteSeleccionado = 0;

        public ClientesForm()
        {
            InitializeComponent();
            this.Load += ClientesForm_Load;
            btnGuardar.Click += btnGuardar_Click;
            btnCancelar.Click += btnCancelar_Click;
            btnEliminar.Click += btnEliminar_Click;
            cmbProvincia.SelectedIndexChanged += cmbProvincia_SelectedIndexChanged;
            dgvClientes.DataBindingComplete += DgvClientes_DataBindingComplete;

            // Activar scroll en el panel izquierdo
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

            EstiloUI.CrearBotonCerrar(this);
            EstiloUI.AplicarEstiloGrid(dgvClientes);
        }

        private void ConfigurarPestanas()
        {
            int x = dgvClientes.Location.X;
            int y = dgvClientes.Location.Y - 40;

            btnTabCartera = new Button { Text = "👥 Cartera de Clientes", Location = new Point(x, y), Size = new Size(200, 40), FlatStyle = FlatStyle.Flat, BackColor = ColorTranslator.FromHtml("#EAEDED"), Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnTabCartera.FlatAppearance.BorderSize = 0;
            btnTabCartera.Click += (s, ev) => CargarGrillaTabs("Cartera");

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
                    dgvClientes.DataSource = clienteDatos.ObtenerClientesCRM();

                    if (dgvClientes.Columns.Contains("Nombre")) dgvClientes.Columns["Nombre"].Visible = false;
                    if (dgvClientes.Columns.Contains("Apellido")) dgvClientes.Columns["Apellido"].Visible = false;

                    // Ocultamos el ID
                    try
                    {
                        if (dgvClientes.Columns.Contains("Id")) dgvClientes.Columns["Id"].Visible = false;
                        else if (dgvClientes.Columns.Count > 0) dgvClientes.Columns[0].Visible = false;
                    }

                    // Asignamos pesos para que se estiren de manera proporcional
                    if (dgvClientes.Columns.Contains("Cliente")) dgvClientes.Columns["Cliente"].FillWeight = 150;
                    if (dgvClientes.Columns.Contains("Telefono")) dgvClientes.Columns["Telefono"].FillWeight = 80;
                    if (dgvClientes.Columns.Contains("Direccion")) dgvClientes.Columns["Direccion"].FillWeight = 120;
                    if (dgvClientes.Columns.Contains("Provincia")) dgvClientes.Columns["Provincia"].FillWeight = 90;
                    if (dgvClientes.Columns.Contains("Localidad")) dgvClientes.Columns["Localidad"].FillWeight = 100;
                }
                else
                {
                    btnTabTop.BackColor = ColorTranslator.FromHtml("#EAEDED");
                    btnTabCartera.BackColor = Color.White;
                    dgvClientes.DataSource = clienteDatos.ObtenerTopClientes();

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
            if (e.RowIndex >= 0 && btnTabCartera.BackColor != Color.White)
            {
                DataGridViewRow fila = dgvClientes.Rows[e.RowIndex];

                idClienteSeleccionado = Convert.ToInt32(fila.Cells["Id"].Value);
                txtNombreCliente.Text = fila.Cells["Nombre"].Value.ToString();
                txtApellidoCliente.Text = fila.Cells["Apellido"].Value.ToString();
                txtTelefonoCliente.Text = fila.Cells["Telefono"].Value.ToString();
                txtDireccionCliente.Text = fila.Cells["Direccion"].Value.ToString();
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
                // 1. VALIDACIÓN: Ningún campo o combo puede estar vacío
                if (string.IsNullOrWhiteSpace(txtNombreCliente.Text) ||
                    string.IsNullOrWhiteSpace(txtApellidoCliente.Text) ||
                    string.IsNullOrWhiteSpace(txtTelefonoCliente.Text) ||
                    string.IsNullOrWhiteSpace(txtDireccionCliente.Text) ||
                    cmbProvincia.SelectedIndex == -1 ||
                    cmbLocalidad.SelectedIndex == -1)
                {
                    MessageBox.Show("Por favor, complete todos los campos y seleccione Provincia y Localidad.", "Datos Incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int provId = Convert.ToInt32(cmbProvincia.SelectedValue);
                int locId = Convert.ToInt32(cmbLocalidad.SelectedValue);

                if (idClienteSeleccionado == 0)
                {
                    Cliente nuevoCliente = new Cliente
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
                    negocio.Registrar(nuevoCliente);
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

        private void CargarCombos()
        {
            cmbProvincia.DataSource = otrosDatos.ObtenerProvincias();
            cmbProvincia.DisplayMember = "Nombre";
            cmbProvincia.ValueMember = "Id";
            cmbProvincia.SelectedIndex = -1;
        }

        private void cmbProvincia_SelectedIndexChanged(object sender, EventArgs e)
        {
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

                if (dgvClientes.Columns.Contains("Estado") &&
                    dgvClientes.CurrentRow.Cells["Estado"].Value?.ToString() == "Inactivo")
                {
                    MessageBox.Show("Este cliente ya se encuentra dado de baja.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int idSel = Convert.ToInt32(dgvClientes.CurrentRow.Cells["Id"].Value);
                var confirm = MessageBox.Show("¿Confirma la eliminación del cliente?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    clienteDatos.EliminarCliente(idSel);
                    MessageBox.Show("Cliente eliminado", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LimpiarFormulario();
                    CargarGrillaTabs("Cartera");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar cliente: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}