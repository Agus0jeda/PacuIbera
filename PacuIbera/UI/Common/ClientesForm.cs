using Negocio;
using PacuIbera.Datos;
using PacuIbera.Dominio;
using Datos;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private bool vistaCarteraActiva = true;
        private int? provinciaLocalidadesCargadas;

        private static readonly Regex NombrePermitido = new Regex(@"\A[\p{L} ]+\z", RegexOptions.Compiled);
        private static readonly Regex DireccionPermitida = new Regex(@"\A[\p{L}\p{N} .,#ºª°/\-()'’\r\n]*\z", RegexOptions.Compiled);

        public ClientesForm()
        {
            InitializeComponent();

            // El Designer ya conecta los botones, la provincia y el buscador.
            // Acá conectamos únicamente los eventos que no están en el Designer.
            Load += ClientesForm_Load;
            dgvClientes.DataBindingComplete += DgvClientes_DataBindingComplete;
            dgvClientes.CellFormatting += DgvClientes_CellFormatting;
            dgvClientes.CellClick += DgvClientes_CellClick;
            dgvClientes.SelectionChanged += DgvClientes_SelectionChanged;
            Resize += ClientesForm_Resize;

            cmbProvincia.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLocalidad.DropDownStyle = ComboBoxStyle.DropDownList;

            // Activar scroll en el panel izquierdo.
            if (nuevoCliente != null)
                nuevoCliente.AutoScroll = true;
        }

        private void ClientesForm_Load(object sender, EventArgs e)
        {
            ConfigurarPestanas();
            AjustarEncabezado();
            EstiloUI.AplicarEstiloGrid(dgvClientes);
            CargarCombos();
            CargarGrillaTabs("Cartera");

            EstiloUI.CrearBotonCerrar(this);
        }

        private void ConfigurarPestanas()
        {
            const int x = 20;
            const int y = 86;

            btnTabCartera = new Button
            {
                Text = "Cartera de Clientes",
                Location = new Point(x, y),
                Size = new Size(190, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = ColorTranslator.FromHtml("#EAEDED"),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnTabCartera.FlatAppearance.BorderSize = 0;
            btnTabCartera.Click += (s, ev) => CargarGrillaTabs("Cartera");

            btnTabTop = new Button
            {
                Text = "Top Mejores Clientes",
                Location = new Point(x + 198, y),
                Size = new Size(190, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnTabTop.FlatAppearance.BorderSize = 0;
            btnTabTop.Click += (s, ev) => CargarGrillaTabs("Top");

            panel1.Controls.Add(btnTabCartera);
            panel1.Controls.Add(btnTabTop);
            btnTabCartera.BringToFront();
            btnTabTop.BringToFront();
        }

        private void CargarGrillaTabs(string tipo)
        {
            try
            {
                bool esCartera = tipo == "Cartera";
                vistaCarteraActiva = esCartera;
                dgvClientes.DataSource = null;
                dgvClientes.Columns.Clear();
                dgvClientes.AutoGenerateColumns = true;

                btnTabCartera.BackColor = esCartera
                    ? ColorTranslator.FromHtml("#EAEDED")
                    : Color.White;
                btnTabTop.BackColor = esCartera
                    ? Color.White
                    : ColorTranslator.FromHtml("#EAEDED");

                dgvClientes.DataSource = esCartera
                    ? clienteDatos.ObtenerClientesCRM()
                    : clienteDatos.ObtenerTopClientes();

                ConfigurarColumnas(esCartera);

                if (dgvClientes.Columns.Contains("Id"))
                    dgvClientes.Columns["Id"].Visible = false;
                else if (dgvClientes.Columns.Count > 0)
                    dgvClientes.Columns[0].Visible = false;

                if (esCartera)
                {
                    if (dgvClientes.Columns.Contains("Nombre"))
                        dgvClientes.Columns["Nombre"].Visible = false;
                    if (dgvClientes.Columns.Contains("Apellido"))
                        dgvClientes.Columns["Apellido"].Visible = false;

                    if (dgvClientes.Columns.Contains("Cliente"))
                        dgvClientes.Columns["Cliente"].FillWeight = 150;
                    if (dgvClientes.Columns.Contains("Telefono"))
                        dgvClientes.Columns["Telefono"].FillWeight = 80;
                    if (dgvClientes.Columns.Contains("Direccion"))
                        dgvClientes.Columns["Direccion"].FillWeight = 120;
                    if (dgvClientes.Columns.Contains("Provincia"))
                        dgvClientes.Columns["Provincia"].FillWeight = 90;
                    if (dgvClientes.Columns.Contains("Localidad"))
                        dgvClientes.Columns["Localidad"].FillWeight = 100;
                }

                dgvClientes.ClearSelection();
                btnEliminar.Visible = false;
                AjustarBotonesFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar la grilla: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ConfigurarColumnas(bool esCartera)
        {
            dgvClientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvClientes.ScrollBars = ScrollBars.Both;
            dgvClientes.AllowUserToResizeColumns = true;
            dgvClientes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvClientes.MultiSelect = false;
            dgvClientes.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            var anchos = esCartera
                ? new System.Collections.Generic.Dictionary<string, int>
                {
                    ["Cliente"] = 210, ["Telefono"] = 125, ["Direccion"] = 230,
                    ["Provincia"] = 185, ["Localidad"] = 185, ["Estado"] = 95
                }
                : new System.Collections.Generic.Dictionary<string, int>
                {
                    ["Cliente"] = 240, ["TotalGastado"] = 145,
                    ["CantidadCompras"] = 145, ["UltimaCompra"] = 180
                };

            foreach (DataGridViewColumn columna in dgvClientes.Columns)
            {
                if (columna.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                {
                    columna.Visible = false;
                    continue;
                }

                columna.MinimumWidth = 80;
                columna.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                columna.Width = anchos.TryGetValue(columna.Name, out int ancho) ? ancho : 140;
                columna.Resizable = DataGridViewTriState.True;
            }
        }

        private void ClientesForm_Resize(object? sender, EventArgs e)
        {
            AjustarEncabezado();
            AjustarBotonesFormulario();
        }

        private void AjustarEncabezado()
        {
            if (panel1 == null || txtBuscar == null || btnCerrar == null)
                return;

            txtBuscar.Width = Math.Max(180, panel1.ClientSize.Width - txtBuscar.Left - 28);
            btnCerrar.Location = new Point(panel1.ClientSize.Width - btnCerrar.Width - 12, 8);
        }

        private void AjustarBotonesFormulario()
        {
            if (nuevoCliente == null || btnGuardar == null || btnCancelar == null || btnEliminar == null)
                return;

            int anchoInterior = Math.Max(250, nuevoCliente.ClientSize.Width - 40);
            int separacion = 8;
            if (btnEliminar.Visible)
            {
                int ancho = (anchoInterior - separacion * 2) / 3;
                btnGuardar.SetBounds(20, btnGuardar.Top, ancho, btnGuardar.Height);
                btnCancelar.SetBounds(20 + ancho + separacion, btnCancelar.Top, ancho, btnCancelar.Height);
                btnEliminar.SetBounds(20 + (ancho + separacion) * 2, btnEliminar.Top, ancho, btnEliminar.Height);
            }
            else
            {
                int ancho = (anchoInterior - separacion) / 2;
                btnGuardar.SetBounds(20, btnGuardar.Top, ancho, btnGuardar.Height);
                btnCancelar.SetBounds(20 + ancho + separacion, btnCancelar.Top, ancho, btnCancelar.Height);
            }
        }

        private void DgvClientes_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (dgvClientes.Columns[e.ColumnIndex].Name == "TotalGastado" && e.Value != null && e.Value != DBNull.Value)
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
            if (!dgvClientes.Columns.Contains("Estado"))
                return;

            foreach (DataGridViewRow fila in dgvClientes.Rows)
            {
                if (fila.IsNewRow)
                    continue;

                string estado = fila.Cells["Estado"].Value?.ToString();

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

                if (!int.TryParse(fila.Cells["Id"].Value?.ToString(), out int idSeleccionado))
                    return;

                idClienteSeleccionado = idSeleccionado;
                txtNombreCliente.Text = fila.Cells["Nombre"].Value?.ToString() ?? string.Empty;
                txtApellidoCliente.Text = fila.Cells["Apellido"].Value?.ToString() ?? string.Empty;
                txtTelefonoCliente.Text = fila.Cells["Telefono"].Value?.ToString() ?? string.Empty;
                txtDireccionCliente.Text = fila.Cells["Direccion"].Value?.ToString() ?? string.Empty;
                string? nombreProvincia = dgvClientes.Columns.Contains("Provincia")
                    ? fila.Cells["Provincia"].Value?.ToString()
                    : null;
                if (!string.IsNullOrWhiteSpace(nombreProvincia) && cmbProvincia.DataSource is DataTable provincias)
                {
                    DataRow? provincia = provincias.AsEnumerable().FirstOrDefault(r =>
                        string.Equals(r["Nombre"]?.ToString(), nombreProvincia, StringComparison.OrdinalIgnoreCase));
                    if (provincia != null)
                        cmbProvincia.SelectedValue = provincia["Id"];
                }

                string? nombreLocalidad = dgvClientes.Columns.Contains("Localidad")
                    ? fila.Cells["Localidad"].Value?.ToString()
                    : null;
                if (!string.IsNullOrWhiteSpace(nombreLocalidad) && cmbLocalidad.DataSource is DataTable localidades)
                {
                    DataRow? localidad = localidades.AsEnumerable().FirstOrDefault(r =>
                        string.Equals(r["Nombre"]?.ToString(), nombreLocalidad, StringComparison.OrdinalIgnoreCase));
                    if (localidad != null)
                        cmbLocalidad.SelectedValue = localidad["Id"];
                }

                btnGuardar.Text = "ACTUALIZAR";
                lblNuevoCliente.Text = "Datos del cliente · Editar";
                btnEliminar.Visible = vistaCarteraActiva;
                AjustarBotonesFormulario();
            }
        }

        private void DgvClientes_SelectionChanged(object? sender, EventArgs e)
        {
            bool hayFilaElegible = vistaCarteraActiva
                && dgvClientes.SelectedRows.Count > 0
                && dgvClientes.SelectedRows[0].Index >= 0;
            btnEliminar.Visible = hayFilaElegible;
            AjustarBotonesFormulario();
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
                if (!ValidarFormularioCliente(out string nombre, out string apellido, out string telefono, out string direccion,
                    out int provId, out int locId))
                    return;

                // Persistir nombres normalizados y sin espacios externos.
                txtNombreCliente.Text = nombre;
                txtApellidoCliente.Text = apellido;
                txtTelefonoCliente.Text = telefono;
                txtDireccionCliente.Text = direccion;

                if (idClienteSeleccionado == 0)
                {
                    Cliente nuevoCliente = new Cliente
                    {
                        Nombre = nombre,
                        Apellido = apellido,
                        Telefono = telefono,
                        Direccion = direccion,
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
                        nombre,
                        apellido,
                        telefono,
                        direccion,
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

        private bool ValidarFormularioCliente(out string nombre, out string apellido, out string telefono,
            out string direccion, out int provinciaId, out int localidadId)
        {
            RestablecerResaltado();
            nombre = txtNombreCliente.Text.Trim().Normalize(NormalizationForm.FormC);
            apellido = txtApellidoCliente.Text.Trim().Normalize(NormalizationForm.FormC);
            telefono = txtTelefonoCliente.Text.Trim();
            direccion = txtDireccionCliente.Text.Trim().Normalize(NormalizationForm.FormC);
            provinciaId = 0;
            localidadId = 0;

            if (nombre.Length < 2 || nombre.Length > 100 || !NombrePermitido.IsMatch(nombre))
                return RechazarCampo(txtNombreCliente, "Ingrese un nombre de 2 a 100 caracteres, usando solo letras y espacios.");

            if (apellido.Length < 2 || apellido.Length > 100 || !NombrePermitido.IsMatch(apellido))
                return RechazarCampo(txtApellidoCliente, "Ingrese un apellido de 2 a 100 caracteres, usando solo letras y espacios.");

            if (telefono.Length > 0)
            {
                int digitos = telefono.Count(c => c >= '0' && c <= '9');
                bool caracteresValidos = telefono.All(c => (c >= '0' && c <= '9') || c == ' ' || c == '+' || c == '(' || c == ')' || c == '-');
                if (!caracteresValidos || digitos < 7 || digitos > 15 || telefono.Length > 20)
                    return RechazarCampo(txtTelefonoCliente, "El teléfono es opcional. Si lo completa, use entre 7 y 15 dígitos, sin letras.");
            }

            // Cliente.Direccion es varchar(150) en el esquema instalado. Se limita
            // al tamaño real para evitar truncamientos o errores en SQL Server.
            if (direccion.Length > 150 || !DireccionPermitida.IsMatch(direccion))
                return RechazarCampo(txtDireccionCliente, direccion.Length > 150
                    ? "La dirección admite hasta 150 caracteres en la base actual."
                    : "La dirección contiene caracteres no permitidos.");

            if (cmbProvincia.SelectedIndex < 0 || !int.TryParse(cmbProvincia.SelectedValue?.ToString(), out provinciaId))
                return RechazarCampo(cmbProvincia, "Seleccione una provincia de la lista.");

            if (cmbLocalidad.SelectedIndex < 0 || !int.TryParse(cmbLocalidad.SelectedValue?.ToString(), out localidadId))
                return RechazarCampo(cmbLocalidad, "Seleccione una localidad de la lista.");

            if (!LocalidadPerteneceAProvincia(provinciaId, localidadId))
                return RechazarCampo(cmbLocalidad, "La localidad no corresponde a la provincia seleccionada. Vuelva a elegirla.");

            return true;
        }

        private bool LocalidadPerteneceAProvincia(int provinciaId, int localidadId)
        {
            if (provinciaLocalidadesCargadas != provinciaId || cmbLocalidad.DataSource is not DataTable localidades
                || !localidades.Columns.Contains("Id"))
                return false;

            return localidades.AsEnumerable().Any(fila =>
                fila["Id"] != DBNull.Value && Convert.ToInt32(fila["Id"]) == localidadId);
        }

        private bool RechazarCampo(Control campo, string mensaje)
        {
            campo.BackColor = Color.MistyRose;
            campo.Focus();
            MessageBox.Show(mensaje, "Revise los datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private void RestablecerResaltado()
        {
            txtNombreCliente.BackColor = SystemColors.Window;
            txtApellidoCliente.BackColor = SystemColors.Window;
            txtTelefonoCliente.BackColor = SystemColors.Window;
            txtDireccionCliente.BackColor = SystemColors.Window;
            cmbProvincia.BackColor = SystemColors.Window;
            cmbLocalidad.BackColor = SystemColors.Window;
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            idClienteSeleccionado = 0;
            btnGuardar.Text = "GUARDAR";
            lblNuevoCliente.Text = "Datos del cliente · Nuevo";
            btnEliminar.Visible = false;
            AjustarBotonesFormulario();

            txtNombreCliente.Clear();
            txtApellidoCliente.Clear();
            txtTelefonoCliente.Clear();
            txtDireccionCliente.Clear();

            if (dgvClientes.SelectedRows.Count > 0)
                dgvClientes.ClearSelection();
        }

        private void CargarCombos()
        {
            cmbProvincia.DisplayMember = "Nombre";
            cmbProvincia.ValueMember = "Id";
            cmbProvincia.DataSource = otrosDatos.ObtenerProvincias();
            cmbProvincia.SelectedIndex = -1;
            cmbLocalidad.DataSource = null;
            cmbLocalidad.Items.Clear();
            provinciaLocalidadesCargadas = null;
        }

        private void cmbProvincia_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbLocalidad.SelectedIndex = -1;
            cmbLocalidad.DataSource = null;
            cmbLocalidad.Items.Clear();
            provinciaLocalidadesCargadas = null;

            if (cmbProvincia.SelectedValue != null && int.TryParse(cmbProvincia.SelectedValue.ToString(), out int provinciaId))
            {
                try
                {
                    DataTable localidades = usuarioNegocio.ListarLocalidades(provinciaId);
                    cmbLocalidad.DisplayMember = "Nombre";
                    cmbLocalidad.ValueMember = "Id";
                    cmbLocalidad.DataSource = localidades;
                    cmbLocalidad.SelectedIndex = -1;
                    provinciaLocalidadesCargadas = provinciaId;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar localidades: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Eventos referenciados por ClientesForm.Designer.cs.
        private void label3_Click(object sender, EventArgs e)
        {
            // Sin acción: etiqueta decorativa.
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Sin acción: evento visual del campo de nombre.
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            FiltrarClientes(txtBuscar.Text.Trim());
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            DgvClientes_CellClick(sender, e);
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
