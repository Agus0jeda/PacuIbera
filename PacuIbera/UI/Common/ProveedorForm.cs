using PacuIbera.Datos;
using PacuIbera.Dominio;
using Negocio;
using System;
using System.Data;
using System.Drawing;          // Agregado para los colores
using System.Windows.Forms;    // Agregado para los botones, grillas y mensajes

namespace PacuIbera.UI.Common
{
    public partial class ProveedorForm : Form
    {
        
        private ProveedorDatos proveedorDatos = new ProveedorDatos();
        private OtrosDatos otrosDatos = new OtrosDatos();
        private UsuarioNegocio negocio = new UsuarioNegocio();
        private bool formateandoCuit = false;
        private int idProveedorSeleccionado = 0;
        public ProveedorForm()
        {
            InitializeComponent();

            // Conectamos el evento Load (te faltaba esto para que arranque con formato)
            this.Load += ProveedorForm_Load;
        }

        private void ProveedorForm_Load(object sender, EventArgs e)
        {
            ConfigurarColumnasGrilla();
            DarFormatoGrilla();
            CargarCombos();
            CargarGrilla();
        }

        private void txtBuscarProveedor_TextChanged(object sender, EventArgs e)
        {
            if (dgvProveedores.DataSource is DataTable dt)
            {
                string texto = txtBuscarProveedor.Text.Trim();

                if (string.IsNullOrWhiteSpace(texto) || texto == "BUSCAR POR RAZÓN SOCIAL O CUIT")
                {
                    dt.DefaultView.RowFilter = "";
                }
                else
                {
                    dt.DefaultView.RowFilter = $"RazonSocial LIKE '%{texto}%' OR CUIT LIKE '%{texto}%'";
                }
            }
        }
        private void txtBuscarProveedor_Enter(object sender, EventArgs e)
        {
            if (txtBuscarProveedor.Text == "BUSCAR POR RAZÓN SOCIAL O CUIT")
            {
                txtBuscarProveedor.Text = "";
                txtBuscarProveedor.ForeColor = Color.Black;
            }
        }

        private void txtBuscarProveedor_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBuscarProveedor.Text))
            {
                txtBuscarProveedor.Text = "BUSCAR POR RAZÓN SOCIAL O CUIT";
                txtBuscarProveedor.ForeColor = Color.Gray;
            }
        }
        private void CargarGrilla()
        {
            dgvProveedores.DataSource = proveedorDatos.ObtenerTodos();

            if (dgvProveedores.Columns.Contains("Id"))
                dgvProveedores.Columns["Id"].Visible = false;

            if (dgvProveedores.Columns.Contains("Activo"))
                dgvProveedores.Columns["Activo"].Visible = false;

            if (dgvProveedores.Columns.Contains("RazonSocial"))
                dgvProveedores.Columns["RazonSocial"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dgvProveedores.ClearSelection();
        }

        private void ConfigurarColumnasGrilla()
        {
            dgvProveedores.AutoGenerateColumns = false;

            if (dgvProveedores.Columns["RazonSocial"] != null) dgvProveedores.Columns["RazonSocial"].DataPropertyName = "RazonSocial";
            if (dgvProveedores.Columns["CUIT"] != null) dgvProveedores.Columns["CUIT"].DataPropertyName = "CUIT";
            if (dgvProveedores.Columns["Telefono"] != null) dgvProveedores.Columns["Telefono"].DataPropertyName = "Telefono";
            if (dgvProveedores.Columns["Email"] != null) dgvProveedores.Columns["Email"].DataPropertyName = "Email";
            if (dgvProveedores.Columns["Direccion"] != null) dgvProveedores.Columns["Direccion"].DataPropertyName = "Direccion";
            if (dgvProveedores.Columns["Provincia"] != null) dgvProveedores.Columns["Provincia"].DataPropertyName = "Provincia";
            if (dgvProveedores.Columns["Localidad"] != null) dgvProveedores.Columns["Localidad"].DataPropertyName = "Localidad";
            if (dgvProveedores.Columns["Estado"] != null) dgvProveedores.Columns["Estado"].DataPropertyName = "Estado";
        }

        private void DarFormatoGrilla()
        {
            dgvProveedores.AllowUserToAddRows = false;
            dgvProveedores.AllowUserToDeleteRows = false;
            dgvProveedores.AllowUserToResizeRows = false;
            dgvProveedores.ReadOnly = true;
            dgvProveedores.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProveedores.MultiSelect = false;
            dgvProveedores.RowHeadersVisible = false;

            dgvProveedores.BackgroundColor = Color.White;
            dgvProveedores.BorderStyle = BorderStyle.None;
            dgvProveedores.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvProveedores.GridColor = Color.LightGray;

            dgvProveedores.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgvProveedores.DefaultCellStyle.ForeColor = Color.FromArgb(64, 64, 64);
            dgvProveedores.DefaultCellStyle.SelectionBackColor = Color.DarkSlateGray;
            dgvProveedores.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvProveedores.RowTemplate.Height = 35;

            dgvProveedores.EnableHeadersVisualStyles = false;
            dgvProveedores.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvProveedores.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);
            dgvProveedores.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProveedores.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvProveedores.ColumnHeadersHeight = 40;
            dgvProveedores.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        }

        private void LimpiarCampos()
        {
            idProveedorSeleccionado = 0;

            txtRazonSocial.Clear();
            txtCuit.Clear();
            txtTelefono.Clear();
            txtEmail.Clear();
            txtDireccion.Clear();

            cmbProvincia.SelectedIndex = -1;
            cmbLocalidad.DataSource = null;

            rbActivo.Checked = true;
            rbInactivo.Checked = false;

            txtBuscarProveedor.Text = "BUSCAR POR RAZÓN SOCIAL O CUIT";
            txtBuscarProveedor.ForeColor = Color.Gray;

            if (dgvProveedores.DataSource is DataTable dt)
            {
                dt.DefaultView.RowFilter = "";
            }
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

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRazonSocial.Text) || string.IsNullOrWhiteSpace(txtCuit.Text))
            {
                MessageBox.Show("La Razón Social y el CUIT son campos obligatorios.", "Faltan datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !EsEmailValido(txtEmail.Text.Trim()))
            {
                MessageBox.Show("Ingrese un correo electrónico válido.", "Email inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Proveedor prov = new Proveedor
                {
                    Id = idProveedorSeleccionado,
                    RazonSocial = txtRazonSocial.Text.Trim(),
                    CUIT = txtCuit.Text.Trim(),
                    Telefono = txtTelefono.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Direccion = txtDireccion.Text.Trim(),
                    ProvinciaId = Convert.ToInt32(cmbProvincia.SelectedValue),
                    LocalidadId = Convert.ToInt32(cmbLocalidad.SelectedValue),
                    Activo = rbActivo.Checked
                };

                if (idProveedorSeleccionado == 0)
                    proveedorDatos.Insertar(prov);
                else
                    proveedorDatos.Actualizar(prov);

                MessageBox.Show("Proveedor guardado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarGrilla();
                LimpiarCampos();
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                {
                    MessageBox.Show("Ya existe un proveedor registrado con ese número de CUIT.", "CUIT Duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Error de base de datos: " + sqlEx.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (idProveedorSeleccionado > 0)
            {
                DialogResult respuesta = MessageBox.Show($"¿Está seguro que desea dar de baja al proveedor '{txtRazonSocial.Text}'?",
                                                         "Confirmar Baja",
                                                         MessageBoxButtons.YesNo,
                                                         MessageBoxIcon.Question);

                if (respuesta == DialogResult.Yes)
                {
                    try
                    {
                        proveedorDatos.Eliminar(idProveedorSeleccionado);
                        MessageBox.Show("El proveedor ha sido dado de baja correctamente.", "Baja exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CargarGrilla();
                        LimpiarCampos();
                    }
                    catch (Microsoft.Data.SqlClient.SqlException sqlEx)
                    {
                        switch (sqlEx.Number)
                        {
                            case 547:
                                MessageBox.Show("No se puede eliminar porque este proveedor tiene compras o ingresos vinculados en el sistema.", "Conflicto de Datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                break;
                            case 53:
                            case 26:
                                MessageBox.Show("No se pudo conectar a la base de datos. Verifique que SQL Server esté encendido y accesible.", "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            default:
                                MessageBox.Show("Ocurrió un error en la base de datos: " + sqlEx.Message, "Error Interno", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error general al dar de baja: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Por favor, seleccione un proveedor de la lista para dar de baja.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void txtCuit_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Permitir teclas de control (como Borrar / Backspace)
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // Solo permitir números
            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            // Opcional: limitar la longitud máxima a 11 dígitos reales (sin contar los guiones si los escriben aparte, o 13 con guiones)
            TextBox txt = sender as TextBox;
            if (txt.Text.Replace("-", "").Length >= 11 && !txt.SelectedText.Any())
            {
                e.Handled = true; // Ya llegó al límite de los 11 números de un CUIT
            }
        }
        private void txtCuit_TextChanged(object sender, EventArgs e)
        {
            if (formateandoCuit) return;

            formateandoCuit = true;
            int cursorPosition = txtCuit.SelectionStart;

            // Removemos guiones previos para limpiar
            string digits = txtCuit.Text.Replace("-", "");

            if (digits.Length > 11)
            {
                digits = digits.Substring(0, 11);
            }

            // Reconstruimos el formato XX-XXXXXXXX-X
            string formatted = "";
            if (digits.Length > 2)
            {
                formatted = digits.Substring(0, 2) + "-";
                if (digits.Length > 10)
                {
                    formatted += digits.Substring(2, 8) + "-";
                    formatted += digits.Substring(10);
                }
                else
                {
                    formatted += digits.Substring(2);
                }
            }
            else
            {
                formatted = digits;
            }

            txtCuit.Text = formatted;

            // Devolvemos el cursor al final para que no salte al tipear
            txtCuit.SelectionStart = txtCuit.Text.Length;
            formateandoCuit = false;
        }

        private void txtTelefono_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void dgvProveedores_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow fila = dgvProveedores.Rows[e.RowIndex];
                DataRowView dataRow = (DataRowView)fila.DataBoundItem;

                idProveedorSeleccionado = Convert.ToInt32(dataRow["Id"]);

                txtRazonSocial.Text = dataRow["RazonSocial"].ToString();
                txtCuit.Text = dataRow["CUIT"].ToString();
                txtTelefono.Text = dataRow["Telefono"].ToString();
                txtEmail.Text = dataRow["Email"].ToString();
                txtDireccion.Text = dataRow["Direccion"].ToString();
                cmbProvincia.Text = dataRow["Provincia"].ToString();
                cmbLocalidad.Text = dataRow["Localidad"].ToString();

                string estadoProveedor = dataRow["Estado"].ToString();
                bool estaActivo = (estadoProveedor == "Activo");
                rbActivo.Checked = estaActivo;
                rbInactivo.Checked = !estaActivo;
            }
        }

        private void dgvProveedores_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow fila in dgvProveedores.Rows)
            {
                if (fila.Cells["Estado"].Value != null)
                {
                    string estadoProveedor = fila.Cells["Estado"].Value.ToString();

                    if (estadoProveedor == "Inactivo")
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
            dgvProveedores.ClearSelection();
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

        private void lblGestionProveedor_Click(object sender, EventArgs e)
        {
            // Evento vacío generado por error en el diseñador visual, dejar así.
        }
    }
}