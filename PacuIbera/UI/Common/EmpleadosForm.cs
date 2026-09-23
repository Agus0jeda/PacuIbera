using Datos;
using Negocio;
using PacuIbera.Datos;
using PacuIbera.Dominio;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public partial class EmpleadosForm : Form
    {
        private UsuarioDatos usuarioDatos = new UsuarioDatos();
        private OtrosDatos otrosDatos = new OtrosDatos();
        private UsuarioNegocio negocio = new UsuarioNegocio();
        private int idEmpleadoSeleccionado = 0;

        public EmpleadosForm()
        {
            InitializeComponent();
            this.Load += EmpleadosForm_Load;
            cmbProvincia.SelectedIndexChanged += cmbProvincia_SelectedIndexChanged;

            // Conectamos el clic rápido de la grilla
            dgvEmpleados.CellClick += dgvEmpleados_CellClick;
        }

        private void EmpleadosForm_Load(object sender, EventArgs e)
        {
            if (SesionActiva.Rol == "Administrador")
            {
                btnEliminar.Visible = false;
                if (dgvEmpleados.Columns.Contains("Clave"))
                {
                    dgvEmpleados.Columns["Clave"].Visible = false;
                }
            }

            ConfigurarColumnasGrilla();
            CargarCombos(); // IMPORTANTE: Cargar combos antes que la grilla
            CargarGrilla();
        }

        private void ConfigurarColumnasGrilla()
        {
            dgvEmpleados.AutoGenerateColumns = false;
            if (dgvEmpleados.Columns["Nombre"] != null) dgvEmpleados.Columns["Nombre"].DataPropertyName = "Nombre";
            if (dgvEmpleados.Columns["Apellido"] != null) dgvEmpleados.Columns["Apellido"].DataPropertyName = "Apellido";
            if (dgvEmpleados.Columns["DNI"] != null) dgvEmpleados.Columns["DNI"].DataPropertyName = "DNI";
            if (dgvEmpleados.Columns["Telefono"] != null) dgvEmpleados.Columns["Telefono"].DataPropertyName = "Telefono";
            if (dgvEmpleados.Columns["Rol"] != null) dgvEmpleados.Columns["Rol"].DataPropertyName = "Rol";
            if (dgvEmpleados.Columns["Estado"] != null) dgvEmpleados.Columns["Estado"].DataPropertyName = "Estado";
            if (dgvEmpleados.Columns["Clave"] != null) dgvEmpleados.Columns["Clave"].DataPropertyName = "Clave";
        }

        private void CargarCombos()
        {
            DataTable dtRoles = otrosDatos.ObtenerRoles();

            if (SesionActiva.Rol == "Administrador")
            {
                dtRoles.DefaultView.RowFilter = "Nombre = 'Vendedor'";
                cmbRol.DataSource = dtRoles.DefaultView;
            }
            else
            {
                cmbRol.DataSource = dtRoles;
            }

            cmbRol.DisplayMember = "Nombre";
            cmbRol.ValueMember = "Id";

            cmbProvincia.DataSource = otrosDatos.ObtenerProvincias();
            cmbProvincia.DisplayMember = "Nombre";
            cmbProvincia.ValueMember = "Id";
            cmbProvincia.SelectedIndex = -1;
        }

        private void CargarGrilla()
        {
            DataTable dt = usuarioDatos.ObtenerTodos();
            dgvEmpleados.DataSource = dt;

            if (dgvEmpleados.Columns.Contains("Id"))
            {
                dgvEmpleados.Columns["Id"].Visible = false;
            }
        }

        // --- NUEVO: CLIC RÁPIDO PARA EDITAR ---
        private void dgvEmpleados_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow fila = dgvEmpleados.Rows[e.RowIndex];
                idEmpleadoSeleccionado = Convert.ToInt32(fila.Cells["Id"].Value);

                txtNombre.Text = fila.Cells["Nombre"].Value?.ToString();
                txtApellido.Text = fila.Cells["Apellido"].Value?.ToString();
                txtDni.Text = fila.Cells["DNI"].Value?.ToString();
                txtTelefono.Text = fila.Cells["Telefono"].Value?.ToString();
                txtEmail.Text = fila.Cells["Email"].Value?.ToString();
                txtDireccion.Text = fila.Cells["Direccion"].Value?.ToString();

                // Llenar combos automáticamente por su texto visible
                cmbProvincia.SelectedIndex = cmbProvincia.FindStringExact(fila.Cells["Provincia"].Value?.ToString());
                cmbLocalidad.SelectedIndex = cmbLocalidad.FindStringExact(fila.Cells["Localidad"].Value?.ToString());

                string rolSeleccionado = fila.Cells["Rol"].Value?.ToString() ?? "";

                // Si el Administrador está mirando, el combo Rol solo tiene "Vendedor". 
                // Si selecciona un Gerente o Admin, el combo queda vacío visualmente para no causar errores.
                if (SesionActiva.Rol == "Administrador" && rolSeleccionado != "Vendedor")
                {
                    cmbRol.SelectedIndex = -1;
                }
                else
                {
                    cmbRol.SelectedIndex = cmbRol.FindStringExact(rolSeleccionado);
                }

                bool estaActivo = (fila.Cells["Estado"].Value?.ToString() == "Activo");
                rbActivo.Checked = estaActivo;
                rbInactivo.Checked = !estaActivo;

                // REGLAS DE SEGURIDAD AL HACER CLIC
                if (SesionActiva.Rol == "Administrador")
                {
                    txtClave.Text = "********";
                    txtClave.Enabled = false;

                    // Si intenta editar a un Gerente o a otro Administrador, le bloqueamos Guardar
                    if (rolSeleccionado == "Gerente" || rolSeleccionado == "Administrador")
                    {
                        btnGuardar.Enabled = false;
                        cmbRol.Enabled = false;
                        MessageBox.Show("No tiene permisos para modificar el perfil de este rango.", "Acceso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                    else
                    {
                        btnGuardar.Enabled = true;
                        cmbRol.Enabled = true;
                    }
                }
                else if (SesionActiva.Rol == "Gerente")
                {
                    txtClave.Text = fila.Cells["Clave"].Value?.ToString();
                    txtClave.Enabled = true;
                    btnGuardar.Enabled = true;
                    btnEliminar.Visible = true;
                }
            }
        }

        // Mantenemos tu código de DoubleClick por si acaso (aunque ahora se usa un solo clic)
        private void dgvEmpleados_CellContentClick(object sender, DataGridViewCellEventArgs e) { }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtApellido.Text) || string.IsNullOrWhiteSpace(txtDni.Text) || string.IsNullOrWhiteSpace(txtClave.Text))
                {
                    MessageBox.Show("Por favor, complete los campos obligatorios (Nombre, Apellido, DNI, Clave).", "Campos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cmbRol.SelectedValue == null)
                {
                    MessageBox.Show("Debe seleccionar un Rol válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // AHORA MANDAMOS EL ID REAL DEL ROL (Value) EN LUGAR DEL TEXTO
                Usuario usuarioCargado = new Usuario
                {
                    Id = idEmpleadoSeleccionado,
                    Nombre = txtNombre.Text,
                    Apellido = txtApellido.Text,
                    DNI = txtDni.Text,
                    Telefono = txtTelefono.Text,
                    Email = txtEmail.Text,
                    Direccion = txtDireccion.Text,
                    Rol = cmbRol.SelectedValue.ToString(), // <-- EL TRUCO ESTÁ ACÁ
                    ClaveHash = txtClave.Text,
                    Activo = rbActivo.Checked,
                    ProvinciaId = Convert.ToInt32(cmbProvincia.SelectedValue),
                    LocalidadId = Convert.ToInt32(cmbLocalidad.SelectedValue)
                };

                if (idEmpleadoSeleccionado == 0)
                {
                    usuarioDatos.Insertar(usuarioCargado);
                    MessageBox.Show("Empleado registrado con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    usuarioDatos.Actualizar(usuarioCargado);
                    MessageBox.Show("Datos actualizados correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                CargarGrilla();
                LimpiarCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarCampos()
        {
            idEmpleadoSeleccionado = 0;
            txtNombre.Clear();
            txtApellido.Clear();
            txtDni.Clear();
            txtTelefono.Clear();
            txtEmail.Clear();
            txtDireccion.Clear();
            txtClave.Clear();

            cmbRol.SelectedIndex = -1;
            cmbProvincia.SelectedIndex = -1;
            cmbLocalidad.DataSource = null;
            rbActivo.Checked = true;

            txtClave.Enabled = true;
            btnGuardar.Enabled = true;
            btnEliminar.Visible = false;

            if (SesionActiva.Rol == "Administrador")
            {
                cmbRol.Enabled = true;
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (idEmpleadoSeleccionado > 0)
            {
                DialogResult respuesta = MessageBox.Show($"¿Está seguro que desea dar de baja al empleado {txtNombre.Text} {txtApellido.Text}?", "Confirmar Baja", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (respuesta == DialogResult.Yes)
                {
                    try
                    {
                        usuarioDatos.Eliminar(idEmpleadoSeleccionado);
                        MessageBox.Show("El empleado ha sido dado de baja.", "Baja exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CargarGrilla();
                        LimpiarCampos();
                    }
                    catch (Exception ex) { MessageBox.Show("Error al dar de baja: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
            }
        }

        private void cmbProvincia_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProvincia.SelectedValue != null && int.TryParse(cmbProvincia.SelectedValue.ToString(), out int provinciaId))
            {
                try
                {
                    cmbLocalidad.DataSource = negocio.ListarLocalidades(provinciaId);
                    cmbLocalidad.DisplayMember = "Nombre";
                    cmbLocalidad.ValueMember = "Id";
                    cmbLocalidad.SelectedIndex = -1;
                }
                catch (Exception ex) { MessageBox.Show("Error al cargar localidades: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void dgvEmpleados_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow fila in dgvEmpleados.Rows)
            {
                if (fila.Cells["Estado"].Value != null)
                {
                    string estadoEmpleado = fila.Cells["Estado"].Value.ToString();
                    if (estadoEmpleado == "Inactivo")
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
            }
            dgvEmpleados.ClearSelection();
        }

        private void AplicarFiltros()
        {
            if (dgvEmpleados.DataSource is DataTable dt)
            {
                string filtroNombre = "";
                string filtroRol = "";

                if (!string.IsNullOrWhiteSpace(txtFiltrarNombre.Text) && txtFiltrarNombre.Text != "NOMBRE O APELLIDO")
                    filtroNombre = $"(Nombre LIKE '%{txtFiltrarNombre.Text.Trim()}%' OR Apellido LIKE '%{txtFiltrarNombre.Text.Trim()}%')";

                if (!string.IsNullOrWhiteSpace(txtFiltrarRol.Text) && txtFiltrarRol.Text != "ROL")
                    filtroRol = $"(Rol LIKE '%{txtFiltrarRol.Text.Trim()}%')";

                string filtroFinal = "";
                if (filtroNombre != "" && filtroRol != "") filtroFinal = $"{filtroNombre} AND {filtroRol}";
                else if (filtroNombre != "") filtroFinal = filtroNombre;
                else if (filtroRol != "") filtroFinal = filtroRol;

                dt.DefaultView.RowFilter = filtroFinal;
            }
        }

        private void txtFiltrarNombre_TextChanged(object sender, EventArgs e) { AplicarFiltros(); }
        private void txtFiltrarRol_TextChanged(object sender, EventArgs e) { AplicarFiltros(); }

        private void txtFiltrarNombre_Enter(object sender, EventArgs e) { if (txtFiltrarNombre.Text == "NOMBRE O APELLIDO") { txtFiltrarNombre.Text = ""; txtFiltrarNombre.ForeColor = Color.Black; } }
        private void txtFiltrarNombre_Leave(object sender, EventArgs e) { if (string.IsNullOrWhiteSpace(txtFiltrarNombre.Text)) { txtFiltrarNombre.Text = "NOMBRE O APELLIDO"; txtFiltrarNombre.ForeColor = Color.Gray; } }
        private void txtFiltrarRol_Enter(object sender, EventArgs e) { if (txtFiltrarRol.Text == "ROL") { txtFiltrarRol.Text = ""; txtFiltrarRol.ForeColor = Color.Black; } }
        private void txtFiltrarRol_Leave(object sender, EventArgs e) { if (string.IsNullOrWhiteSpace(txtFiltrarRol.Text)) { txtFiltrarRol.Text = "ROL"; txtFiltrarRol.ForeColor = Color.Gray; } }

        private void txtDni_KeyPress(object sender, KeyPressEventArgs e) { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; }
        private void txtTelefono_KeyPress(object sender, KeyPressEventArgs e) { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; }
        private void lblDireccion_Click(object sender, EventArgs e) { }
    }
}