using Negocio;
using PacuIbera.Dominio;
using System;
using System.Data;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PacuIbera.UI.Common
{
    public partial class ClientesForm : Form
    {
        private ClienteNegocio negocio = new ClienteNegocio();

        public ClientesForm()
        {
            InitializeComponent();

            this.Load += ClientesForm_Load;
            cmbProvincia.SelectedIndexChanged += cmbProvincia_SelectedIndexChanged;
            btnGuardar.Click += btnGuardar_Click;
        }

        private void ClientesForm_Load(object sender, EventArgs e)
        {
            CargarProvincias();
            CargarGrillaClientes();
            ConfigurarColumnasGrilla();
            EstilizarGrilla();
        }

        private void CargarProvincias()
        {
            try
            {
                cmbProvincia.DataSource = negocio.ListarProvincias();
                cmbProvincia.DisplayMember = "Nombre";
                cmbProvincia.ValueMember = "Id";
                cmbProvincia.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar provincias: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarGrillaClientes()
        {
            try
            {
                dataGridView1.DataSource = negocio.ListarClientes();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la lista de clientes: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurarColumnasGrilla()
        {
            // Vinculamos los DataPropertyName de tus columnas con las columnas de la consulta SQL
            // Asegúrate de que los nombres de las columnas en tu diseñador coincidan (id, Nombre, Column1, etc.)
            if (dataGridView1.Columns["id"] != null) dataGridView1.Columns["id"].DataPropertyName = "Id";
            if (dataGridView1.Columns["Nombre"] != null) dataGridView1.Columns["Nombre"].DataPropertyName = "Nombre";
            if (dataGridView1.Columns["Column1"] != null) dataGridView1.Columns["Column1"].DataPropertyName = "Apellido"; // Asumiendo Column1 es Apellido
            if (dataGridView1.Columns["DNI"] != null) dataGridView1.Columns["DNI"].DataPropertyName = "DNI_CUIT";
            if (dataGridView1.Columns["Provincia"] != null) dataGridView1.Columns["Provincia"].DataPropertyName = "Provincia";
            if (dataGridView1.Columns["Localidad"] != null) dataGridView1.Columns["Localidad"].DataPropertyName = "Localidad";
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
            try
            {
                if (cmbProvincia.SelectedValue == null || cmbLocalidad.SelectedValue == null)
                {
                    MessageBox.Show("Por favor, seleccione una Provincia y una Localidad válidas.", "Campos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Cliente nuevoCliente = new Cliente
                {
                    Nombre = txtNombreCliente.Text.Trim(),
                    Apellido = txtApellidoCliente.Text.Trim(),
                    DNI_CUIT = txtDniCliente.Text.Trim(),
                    Telefono = txtTelefonoCliente.Text.Trim(),
                    Email = txtEmailCliente.Text.Trim(),
                    Direccion = txtDireccionCliente.Text.Trim(),
                    ProvinciaId = Convert.ToInt32(cmbProvincia.SelectedValue),
                    LocalidadId = Convert.ToInt32(cmbLocalidad.SelectedValue)
                };

                negocio.Registrar(nuevoCliente);

                MessageBox.Show("¡Cliente registrado con éxito!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarFormulario();

                // Recargamos la grilla para que aparezca el nuevo cliente inmediatamente
                CargarGrillaClientes();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el cliente: " + ex.Message, "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void EstilizarGrilla()
        {
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridView1.GridColor = Color.FromArgb(230, 230, 230);
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.RowHeadersVisible = false;

            // Encabezado en Verde (acorde al botón Guardar)
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(46, 204, 113); // Verde claro
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dataGridView1.ColumnHeadersHeight = 35;

            // Filas y selección en un tono verde más elegante
            dataGridView1.DefaultCellStyle.BackColor = Color.White;
            dataGridView1.DefaultCellStyle.ForeColor = Color.FromArgb(64, 64, 64);
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(39, 174, 96); // Verde de selección
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridView1.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            dataGridView1.RowTemplate.Height = 30;
        }

           public void FiltrarClientes(string filtro)
        {
            if (dataGridView1.DataSource is DataTable dt)
            {
                // Aplicamos el filtro por Nombre, Apellido o DNI/CUIT
                dt.DefaultView.RowFilter = $"Nombre LIKE '%{filtro}%' OR Apellido LIKE '%{filtro}%' OR DNI_CUIT LIKE '%{filtro}%'";

                // Evaluamos si la búsqueda no arroja resultados
                if (dt.DefaultView.Count == 0 && !string.IsNullOrWhiteSpace(filtro))
                {
                    lblSinResultados.Text = "⚠️ No se encontró ningún cliente con ese criterio.";
                    lblSinResultados.Visible = true; // Mostramos el aviso
                }
                else
                {
                    lblSinResultados.Visible = false; // Ocultamos el aviso si hay resultados o se borró el texto
                }
            }
        }

        private void LimpiarFormulario()
        {
            txtNombreCliente.Clear();
            txtApellidoCliente.Clear();
            txtDniCliente.Clear();
            txtTelefonoCliente.Clear();
            txtEmailCliente.Clear();
            txtDireccionCliente.Clear();
            cmbProvincia.SelectedIndex = -1;
            cmbLocalidad.DataSource = null;
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label3_Click(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            FiltrarClientes(txtBuscar.Text.Trim());
        }
    }
}