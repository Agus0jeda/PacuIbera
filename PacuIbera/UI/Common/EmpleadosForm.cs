using Datos;
using Microsoft.VisualBasic.Logging;
using Negocio;
using PacuIbera.Datos;
using PacuIbera.Dominio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public partial class EmpleadosForm : Form
    {
        private UsuarioDatos usuarioDatos = new UsuarioDatos();
        private OtrosDatos otrosDatos = new OtrosDatos();
        private UsuarioNegocio negocio = new UsuarioNegocio();
        private int idEmpleadoSeleccionado = 0;// 0 = Nuevo empleado, Mayor a 0 = Editando
        public EmpleadosForm()
        {
            InitializeComponent();
            this.Load += EmpleadosForm_Load;
            cmbProvincia.SelectedIndexChanged += cmbProvincia_SelectedIndexChanged;

        }

        private void EmpleadosForm_Load(object sender, EventArgs e)
        {
            if (SesionActiva.Rol == "Administrador")
            {
                btnEliminar.Visible = false;

                // Ocultamos la columna de claves en la tabla para el Administrador
                if (dgvEmpleados.Columns.Contains("Clave"))
                {
                    dgvEmpleados.Columns["Clave"].Visible = false;
                }
            }
            ConfigurarColumnasGrilla();
            CargarGrilla();
            CargarCombos();
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

            // Filtramos la vista de la tabla si el usuario es Administrador
            if (SesionActiva.Rol == "Administrador")
            {
                dtRoles.DefaultView.RowFilter = "Nombre = 'Vendedor'";
                cmbRol.DataSource = dtRoles.DefaultView;
            }
            else
            {
                // El Gerente tiene acceso a la tabla completa
                cmbRol.DataSource = dtRoles;
            }

            cmbRol.DisplayMember = "Nombre";
            cmbRol.ValueMember = "Id";

            cmbProvincia.DataSource = otrosDatos.ObtenerProvincias();
            cmbProvincia.DisplayMember = "Nombre";
            cmbProvincia.ValueMember = "Id";
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

        private void dgvEmpleados_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow fila = dgvEmpleados.Rows[e.RowIndex];
                idEmpleadoSeleccionado = Convert.ToInt32(fila.Cells["Id"].Value);

                txtNombre.Text = fila.Cells["Nombre"].Value.ToString();
                txtApellido.Text = fila.Cells["Apellido"].Value.ToString();
                txtDni.Text = fila.Cells["DNI"].Value.ToString();
                txtTelefono.Text = fila.Cells["Telefono"].Value.ToString();
                txtEmail.Text = fila.Cells["Email"].Value.ToString();
                txtDireccion.Text = fila.Cells["Direccion"].Value.ToString();
                cmbRol.Text = fila.Cells["Rol"].Value.ToString();
                cmbProvincia.Text = fila.Cells["Provincia"].Value.ToString();
                cmbLocalidad.Text = fila.Cells["Localidad"].Value.ToString();

                string estadoEmpleado = fila.Cells["Estado"].Value.ToString();
                bool estaActivo = (estadoEmpleado == "Activo");
                rbActivo.Checked = estaActivo;
                rbInactivo.Checked = !estaActivo;

                //  CONTROL DE ACCESOS PARA LA CLAVE Y EDICIÓN
                if (SesionActiva.Rol == "Administrador")
                {
                    txtClave.Text = "********";
                    txtClave.Enabled = false;
                    btnEliminar.Visible = false;

                    // Si el Administrador selecciona a alguien con un rango mayor o igual, bloqueamos todo
                    string rolSeleccionado = fila.Cells["Rol"].Value.ToString();
                    if (rolSeleccionado != "Vendedor")
                    {
                        btnGuardar.Enabled = false;
                        cmbRol.Enabled = false;
                    }
                    else
                    {
                        btnGuardar.Enabled = true;
                        cmbRol.Enabled = true;
                    }
                }
                else if (SesionActiva.Rol == "Gerente")
                {
                    if (fila.Cells["Clave"].Value != null)
                        txtClave.Text = fila.Cells["Clave"].Value.ToString();

                    txtClave.Enabled = true;
                    btnGuardar.Enabled = true;
                    btnEliminar.Visible = true;
                }

                panelAñadirEmpleado.Width = 500;
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                // VALIDACIÓN: Asegurarnos de que eligió algo antes de intentar guardar
                if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                    string.IsNullOrWhiteSpace(txtApellido.Text) ||
                    string.IsNullOrWhiteSpace(txtDni.Text))
                {
                    MessageBox.Show("Por favor, complete los campos obligatorios (Nombre, Apellido, DNI).", "Campos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return; // Cortamos la ejecución
                }
                if (!EsEmailValido(txtEmail.Text.Trim()))
                {
                    MessageBox.Show("Por favor, ingrese un correo electrónico válido.", "Email inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cmbProvincia.SelectedValue == null || cmbLocalidad.SelectedValue == null || cmbRol.SelectedValue == null)
                {
                    MessageBox.Show("Por favor, seleccione un Rol, una Provincia y una Localidad válidas.", "Campos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }


                Usuario usuarioCargado = new Usuario
                {
                    Id = idEmpleadoSeleccionado,
                    Nombre = txtNombre.Text.Trim(),
                    Apellido = txtApellido.Text.Trim(),
                    DNI = txtDni.Text.Trim(),
                    Telefono = txtTelefono.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Direccion = txtDireccion.Text.Trim(),
                    Rol = cmbRol.Text,
                    ClaveHash = txtClave.Text.Trim(),
                    Activo = rbActivo.Checked,
                    ProvinciaId = Convert.ToInt32(cmbProvincia.SelectedValue),
                    LocalidadId = Convert.ToInt32(cmbLocalidad.SelectedValue)
                };

                //  GUARDADO Y MENSAJE EN ORDEN CORRECTO
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
            catch (Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                // Atrapamos exclusivamente errores que vienen del motor de SQL Server
                switch (sqlEx.Number)
                {
                    case 2627: // Violación de UNIQUE KEY o PRIMARY KEY
                    case 2601: // Duplicado en índice único
                        MessageBox.Show("El DNI o el Correo Electrónico ingresado ya está registrado en otro usuario.", "Dato Duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;

                    case 547: // Violación de FOREIGN KEY
                        MessageBox.Show("No se puede guardar/eliminar porque este registro está vinculado a otras partes del sistema (Ej: tiene ventas asociadas).", "Conflicto de Datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;

                    case 53: // Servidor no encontrado
                    case 26: // Error de conexión de red
                        MessageBox.Show("No se pudo conectar a la base de datos. Verifique que SQL Server esté encendido.", "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;

                    default: // Cualquier otro error de SQL
                        MessageBox.Show("Ocurrió un error en la base de datos: " + sqlEx.Message, "Error Interno", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }
            }
            catch (Exception ex)
            {
                // Atrapamos errores generales de C# (ej: intentó convertir una letra a número)
                MessageBox.Show(ex.Message, "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Pequeño método para vaciar el panel después de guardar o al darle al botón +
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

            // --- NUEVO: REINICIO DE PERMISOS PARA USUARIO NUEVO ---
            txtClave.Enabled = true;
            btnGuardar.Enabled = true;
            btnEliminar.Visible = false;

            if (SesionActiva.Rol == "Administrador")
            {
                cmbRol.Enabled = true; // Para que pueda seleccionar el único rol disponible (Vendedor)
            }
        }

        private bool EsEmailValido(string email)
        {
            try
            {
                var correo = new System.Net.Mail.MailAddress(email);
                return correo.Address == email;
            }
            catch
            {
                return false;
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
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar localidades: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void txtDni_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Verificamos si la tecla presionada NO es un número y NO es la tecla de borrar (Backspace)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Le decimos a C# que "maneje" el evento ignorando la tecla
            }
        }

        private void txtTelefono_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
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

        private void txtFiltrarNombre_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }

        private void txtFiltrarRol_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }
        private void AplicarFiltros()
        {
            if (dgvEmpleados.DataSource is DataTable dt)
            {
                string filtroNombre = "";
                string filtroRol = "";

                // Verificamos el primer filtro ignorando su texto de ayuda
                if (!string.IsNullOrWhiteSpace(txtFiltrarNombre.Text) && txtFiltrarNombre.Text != "NOMBRE O APELLIDO")
                {
                    filtroNombre = $"(Nombre LIKE '%{txtFiltrarNombre.Text.Trim()}%' OR Apellido LIKE '%{txtFiltrarNombre.Text.Trim()}%')";
                }

                // Verificamos el segundo filtro ignorando su texto de ayuda
                if (!string.IsNullOrWhiteSpace(txtFiltrarRol.Text) && txtFiltrarRol.Text != "ROL")
                {
                    filtroRol = $"(Rol LIKE '%{txtFiltrarRol.Text.Trim()}%')";
                }

                // Combinamos los filtros
                string filtroFinal = "";
                if (filtroNombre != "" && filtroRol != "")
                    filtroFinal = $"{filtroNombre} AND {filtroRol}";
                else if (filtroNombre != "")
                    filtroFinal = filtroNombre;
                else if (filtroRol != "")
                    filtroFinal = filtroRol;

                // Aplicamos a la grilla
                dt.DefaultView.RowFilter = filtroFinal;
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            // Verificamos que haya seleccionado a alguien en la grilla
            if (idEmpleadoSeleccionado > 0)
            {
                DialogResult respuesta = MessageBox.Show($"¿Está seguro que desea dar de baja al empleado {txtNombre.Text} {txtApellido.Text}?",
                                                         "Confirmar Baja",
                                                         MessageBoxButtons.YesNo,
                                                         MessageBoxIcon.Question);

                if (respuesta == DialogResult.Yes)
                {
                    try
                    {
                        // ¡Llamamos directamente a tu método Eliminar pasándole el ID!
                        usuarioDatos.Eliminar(idEmpleadoSeleccionado);

                        MessageBox.Show("El empleado ha sido dado de baja correctamente.", "Baja exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);


                        CargarGrilla();
                        LimpiarCampos();
                    }
                    catch (Microsoft.Data.SqlClient.SqlException sqlEx)
                    {
                        // Atrapamos errores que vienen exclusivamente del motor de SQL Server
                        switch (sqlEx.Number)
                        {
                            case 547: // Violación de FOREIGN KEY (Por si en el futuro cambias a un DELETE real)
                                MessageBox.Show("No se puede eliminar porque este empleado tiene registros vinculados en el sistema (ej. ventas o aperturas de caja).", "Conflicto de Datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                break;

                            case 53: // Servidor no encontrado
                            case 26: // Error de conexión de red
                                MessageBox.Show("No se pudo conectar a la base de datos. Verifique que SQL Server esté encendido y accesible.", "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;

                            default: // Cualquier otro error de base de datos
                                MessageBox.Show("Ocurrió un error en la base de datos: " + sqlEx.Message, "Error Interno", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Atrapamos cualquier otro error general de C#
                        MessageBox.Show("Error general al dar de baja: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        // Eventos para el cuadro de Búsqueda por Nombre
        private void txtFiltrarNombre_Enter(object sender, EventArgs e)
        {
            if (txtFiltrarNombre.Text == "NOMBRE O APELLIDO")
            {
                txtFiltrarNombre.Text = "";
                txtFiltrarNombre.ForeColor = Color.Black;
            }
        }

        private void txtFiltrarNombre_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFiltrarNombre.Text))
            {
                txtFiltrarNombre.Text = "NOMBRE O APELLIDO";
                txtFiltrarNombre.ForeColor = Color.Gray;
            }
        }

        // Eventos para el cuadro de Búsqueda por Rol
        private void txtFiltrarRol_Enter(object sender, EventArgs e)
        {
            if (txtFiltrarRol.Text == "ROL")
            {
                txtFiltrarRol.Text = "";
                txtFiltrarRol.ForeColor = Color.Black;
            }
        }

        private void txtFiltrarRol_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFiltrarRol.Text))
            {
                txtFiltrarRol.Text = "ROL";
                txtFiltrarRol.ForeColor = Color.Gray;
            }
        }

        private void lblDireccion_Click(object sender, EventArgs e)
        {

        }
    }
}
