using Negocio;

namespace PacuIbera.UI.Common
{
    partial class ClientesForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        /// 


        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            nuevoCliente = new Panel();
            cmbLocalidad = new ComboBox();
            cmbProvincia = new ComboBox();
            lblTelefonoCliente = new Label();
            txtTelefonoCliente = new TextBox();
            lblDireccionCliente = new Label();
            txtDireccionCliente = new TextBox();
            lblLocCliente = new Label();
            lblProvCliente = new Label();
            lblApellidoCliente = new Label();
            lblNombreCliente = new Label();
            btnCancelar = new Button();
            btnGuardar = new Button();
            txtApellidoCliente = new TextBox();
            txtNombreCliente = new TextBox();
            lblNuevoCliente = new Label();
            panel1 = new Panel();
            label1 = new Label();
            txtBuscar = new TextBox();
            btnCerrar = new Label();
            lblTituloCliente = new Label();
            dgvClientes = new DataGridView();
            id = new DataGridViewTextBoxColumn();
            Nombre = new DataGridViewTextBoxColumn();
            Column1 = new DataGridViewTextBoxColumn();
            DNI = new DataGridViewTextBoxColumn();
            Provincia = new DataGridViewTextBoxColumn();
            Localidad = new DataGridViewTextBoxColumn();
            Rol = new DataGridViewTextBoxColumn();
            Estado = new DataGridViewTextBoxColumn();
            usuarioDatosBindingSource = new BindingSource(components);
            lblSinResultados = new Label();
            panel2 = new Panel();
            nuevoCliente.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvClientes).BeginInit();
            ((System.ComponentModel.ISupportInitialize)usuarioDatosBindingSource).BeginInit();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // nuevoCliente
            // 
            nuevoCliente.BackColor = Color.WhiteSmoke;
            nuevoCliente.Controls.Add(cmbLocalidad);
            nuevoCliente.Controls.Add(cmbProvincia);
            nuevoCliente.Controls.Add(lblTelefonoCliente);
            nuevoCliente.Controls.Add(txtTelefonoCliente);
            nuevoCliente.Controls.Add(lblDireccionCliente);
            nuevoCliente.Controls.Add(txtDireccionCliente);
            nuevoCliente.Controls.Add(lblLocCliente);
            nuevoCliente.Controls.Add(lblProvCliente);
            nuevoCliente.Controls.Add(lblApellidoCliente);
            nuevoCliente.Controls.Add(lblNombreCliente);
            nuevoCliente.Controls.Add(btnCancelar);
            nuevoCliente.Controls.Add(btnGuardar);
            nuevoCliente.Controls.Add(txtApellidoCliente);
            nuevoCliente.Controls.Add(txtNombreCliente);
            nuevoCliente.Controls.Add(lblNuevoCliente);
            nuevoCliente.Dock = DockStyle.Left;
            nuevoCliente.Font = new Font("Microsoft Sans Serif", 12F);
            nuevoCliente.Location = new Point(0, 0);
            nuevoCliente.Margin = new Padding(3, 2, 3, 2);
            nuevoCliente.Name = "nuevoCliente";
            nuevoCliente.Size = new Size(306, 400);
            nuevoCliente.TabIndex = 0;
            nuevoCliente.Paint += nuevoCliente_Paint;
            // 
            // cmbLocalidad
            // 
            cmbLocalidad.FormattingEnabled = true;
            cmbLocalidad.Location = new Point(87, 236);
            cmbLocalidad.Margin = new Padding(3, 2, 3, 2);
            cmbLocalidad.Name = "cmbLocalidad";
            cmbLocalidad.Size = new Size(195, 28);
            cmbLocalidad.TabIndex = 24;
            // 
            // cmbProvincia
            // 
            cmbProvincia.FormattingEnabled = true;
            cmbProvincia.Location = new Point(87, 199);
            cmbProvincia.Margin = new Padding(3, 2, 3, 2);
            cmbProvincia.Name = "cmbProvincia";
            cmbProvincia.Size = new Size(195, 28);
            cmbProvincia.TabIndex = 23;
           
            // 
            // lblTelefonoCliente
            // 
            lblTelefonoCliente.AutoSize = true;
            lblTelefonoCliente.Font = new Font("Microsoft Sans Serif", 9F);
            lblTelefonoCliente.Location = new Point(15, 131);
            lblTelefonoCliente.Name = "lblTelefonoCliente";
            lblTelefonoCliente.Size = new Size(61, 15);
            lblTelefonoCliente.TabIndex = 22;
            lblTelefonoCliente.Text = "Telefono :";
            // 
            // txtTelefonoCliente
            // 
            txtTelefonoCliente.CharacterCasing = CharacterCasing.Upper;
            txtTelefonoCliente.Font = new Font("Microsoft Sans Serif", 9F);
            txtTelefonoCliente.Location = new Point(87, 125);
            txtTelefonoCliente.Margin = new Padding(3, 2, 3, 2);
            txtTelefonoCliente.MaxLength = 100;
            txtTelefonoCliente.Multiline = true;
            txtTelefonoCliente.Name = "txtTelefonoCliente";
            txtTelefonoCliente.Size = new Size(195, 21);
            txtTelefonoCliente.TabIndex = 21;
            // 
            // lblDireccionCliente
            // 
            lblDireccionCliente.AutoSize = true;
            lblDireccionCliente.Font = new Font("Microsoft Sans Serif", 9F);
            lblDireccionCliente.Location = new Point(15, 168);
            lblDireccionCliente.Name = "lblDireccionCliente";
            lblDireccionCliente.Size = new Size(65, 15);
            lblDireccionCliente.TabIndex = 18;
            lblDireccionCliente.Text = "Direccion :";
            // 
            // txtDireccionCliente
            // 
            txtDireccionCliente.CharacterCasing = CharacterCasing.Upper;
            txtDireccionCliente.Font = new Font("Microsoft Sans Serif", 9F);
            txtDireccionCliente.Location = new Point(87, 162);
            txtDireccionCliente.Margin = new Padding(3, 2, 3, 2);
            txtDireccionCliente.MaxLength = 100;
            txtDireccionCliente.Multiline = true;
            txtDireccionCliente.Name = "txtDireccionCliente";
            txtDireccionCliente.Size = new Size(195, 21);
            txtDireccionCliente.TabIndex = 17;
            // 
            // lblLocCliente
            // 
            lblLocCliente.AutoSize = true;
            lblLocCliente.Font = new Font("Microsoft Sans Serif", 9F);
            lblLocCliente.Location = new Point(15, 249);
            lblLocCliente.Name = "lblLocCliente";
            lblLocCliente.Size = new Size(67, 15);
            lblLocCliente.TabIndex = 17;
            lblLocCliente.Text = "Localidad :";
            // 
            // lblProvCliente
            // 
            lblProvCliente.AutoSize = true;
            lblProvCliente.Font = new Font("Microsoft Sans Serif", 9F);
            lblProvCliente.Location = new Point(15, 212);
            lblProvCliente.Name = "lblProvCliente";
            lblProvCliente.Size = new Size(63, 15);
            lblProvCliente.TabIndex = 16;
            lblProvCliente.Text = "Provincia :";
            // 
            // lblApellidoCliente
            // 
            lblApellidoCliente.AutoSize = true;
            lblApellidoCliente.Font = new Font("Microsoft Sans Serif", 9F);
            lblApellidoCliente.Location = new Point(15, 92);
            lblApellidoCliente.Name = "lblApellidoCliente";
            lblApellidoCliente.Size = new Size(57, 15);
            lblApellidoCliente.TabIndex = 14;
            lblApellidoCliente.Text = "Apellido :";
            // 
            // lblNombreCliente
            // 
            lblNombreCliente.AutoSize = true;
            lblNombreCliente.Font = new Font("Microsoft Sans Serif", 9F);
            lblNombreCliente.Location = new Point(15, 58);
            lblNombreCliente.Name = "lblNombreCliente";
            lblNombreCliente.Size = new Size(58, 15);
            lblNombreCliente.TabIndex = 13;
            lblNombreCliente.Text = "Nombre :";
            lblNombreCliente.Click += label3_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.BackColor = Color.Red;
            btnCancelar.Cursor = Cursors.Hand;
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 192, 192);
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.Font = new Font("Microsoft Sans Serif", 9F);
            btnCancelar.Location = new Point(166, 346);
            btnCancelar.Margin = new Padding(3, 2, 3, 2);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(96, 30);
            btnCancelar.TabIndex = 12;
            btnCancelar.Text = "CANCELAR";
            btnCancelar.UseVisualStyleBackColor = false;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // btnGuardar
            // 
            btnGuardar.BackColor = Color.LightGreen;
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.FlatAppearance.MouseOverBackColor = Color.FromArgb(192, 255, 192);
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.Font = new Font("Microsoft Sans Serif", 9F);
            btnGuardar.Location = new Point(30, 346);
            btnGuardar.Margin = new Padding(3, 2, 3, 2);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(96, 30);
            btnGuardar.TabIndex = 11;
            btnGuardar.Text = "GUARDAR";
            btnGuardar.UseVisualStyleBackColor = false;
            // 
            // txtApellidoCliente
            // 
            txtApellidoCliente.CharacterCasing = CharacterCasing.Upper;
            txtApellidoCliente.Font = new Font("Microsoft Sans Serif", 9F);
            txtApellidoCliente.Location = new Point(87, 86);
            txtApellidoCliente.Margin = new Padding(3, 2, 3, 2);
            txtApellidoCliente.MaxLength = 100;
            txtApellidoCliente.Name = "txtApellidoCliente";
            txtApellidoCliente.Size = new Size(195, 21);
            txtApellidoCliente.TabIndex = 8;
            // 
            // txtNombreCliente
            // 
            txtNombreCliente.CharacterCasing = CharacterCasing.Upper;
            txtNombreCliente.Font = new Font("Microsoft Sans Serif", 9F);
            txtNombreCliente.Location = new Point(87, 52);
            txtNombreCliente.Margin = new Padding(3, 2, 3, 2);
            txtNombreCliente.MaxLength = 100;
            txtNombreCliente.Name = "txtNombreCliente";
            txtNombreCliente.Size = new Size(195, 21);
            txtNombreCliente.TabIndex = 6;
            txtNombreCliente.TextChanged += textBox1_TextChanged;
            // 
            // lblNuevoCliente
            // 
            lblNuevoCliente.AutoSize = true;
            lblNuevoCliente.Font = new Font("Microsoft Sans Serif", 12F);
            lblNuevoCliente.Location = new Point(87, 15);
            lblNuevoCliente.Name = "lblNuevoCliente";
            lblNuevoCliente.Size = new Size(107, 20);
            lblNuevoCliente.TabIndex = 0;
            lblNuevoCliente.Text = "Nuevo Cliente";
            // 
            // panel1
            // 
            panel1.Controls.Add(label1);
            panel1.Controls.Add(txtBuscar);
            panel1.Controls.Add(btnCerrar);
            panel1.Controls.Add(lblTituloCliente);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(306, 0);
            panel1.Margin = new Padding(3, 2, 3, 2);
            panel1.Name = "panel1";
            panel1.Size = new Size(566, 52);
            panel1.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(5, 34);
            label1.Name = "label1";
            label1.Size = new Size(234, 16);
            label1.TabIndex = 11;
            label1.Text = "Filtrar (Nombre/ Apellido /DNI_CUIT):  ";
            // 
            // txtBuscar
            // 
            txtBuscar.Location = new Point(237, 31);
            txtBuscar.Margin = new Padding(3, 2, 3, 2);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.Size = new Size(206, 23);
            txtBuscar.TabIndex = 10;
            txtBuscar.TextChanged += txtBuscar_TextChanged;
            // 
            // btnCerrar
            // 
            btnCerrar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCerrar.AutoSize = true;
            btnCerrar.Cursor = Cursors.Hand;
            btnCerrar.Font = new Font("Segoe UI", 11F);
            btnCerrar.ForeColor = SystemColors.ControlDarkDark;
            btnCerrar.Location = new Point(536, 7);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new Size(18, 20);
            btnCerrar.TabIndex = 9;
            btnCerrar.Text = "X";
            btnCerrar.Click += btnCerrar_Click;
            // 
            // lblTituloCliente
            // 
            lblTituloCliente.AutoSize = true;
            lblTituloCliente.Font = new Font("Microsoft Sans Serif", 12F);
            lblTituloCliente.Location = new Point(224, 7);
            lblTituloCliente.Name = "lblTituloCliente";
            lblTituloCliente.Size = new Size(87, 20);
            lblTituloCliente.TabIndex = 0;
            lblTituloCliente.Text = "CLIENTES";
            // 
            // dgvClientes
            // 
            dgvClientes.AutoGenerateColumns = false;
            dgvClientes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvClientes.Columns.AddRange(new DataGridViewColumn[] { id, Nombre, Column1, DNI, Provincia, Localidad, Rol, Estado });
            dgvClientes.DataSource = usuarioDatosBindingSource;
            dgvClientes.Dock = DockStyle.Fill;
            dgvClientes.Location = new Point(0, 0);
            dgvClientes.Margin = new Padding(3, 2, 3, 2);
            dgvClientes.Name = "dgvClientes";
            dgvClientes.RowHeadersWidth = 51;
            dgvClientes.Size = new Size(566, 348);
            dgvClientes.TabIndex = 2;
            dgvClientes.CellContentClick += dataGridView1_CellContentClick;
            // 
            // id
            // 
            id.HeaderText = "ID";
            id.MinimumWidth = 6;
            id.Name = "id";
            id.Width = 125;
            // 
            // Nombre
            // 
            Nombre.HeaderText = "Nombre";
            Nombre.MinimumWidth = 6;
            Nombre.Name = "Nombre";
            Nombre.Width = 125;
            // 
            // Column1
            // 
            Column1.HeaderText = "Apellido";
            Column1.MinimumWidth = 6;
            Column1.Name = "Column1";
            Column1.Width = 125;
            // 
            // DNI
            // 
            DNI.HeaderText = "DNI";
            DNI.MinimumWidth = 6;
            DNI.Name = "DNI";
            DNI.Width = 125;
            // 
            // Provincia
            // 
            Provincia.HeaderText = "Provincia";
            Provincia.MinimumWidth = 6;
            Provincia.Name = "Provincia";
            Provincia.Width = 125;
            // 
            // Localidad
            // 
            Localidad.HeaderText = "Localidad";
            Localidad.MinimumWidth = 6;
            Localidad.Name = "Localidad";
            Localidad.Width = 125;
            // 
            // Rol
            // 
            Rol.HeaderText = "Rol";
            Rol.MinimumWidth = 6;
            Rol.Name = "Rol";
            Rol.Width = 125;
            // 
            // Estado
            // 
            Estado.HeaderText = "Estado";
            Estado.MinimumWidth = 6;
            Estado.Name = "Estado";
            Estado.Width = 125;
            // 
            // lblSinResultados
            // 
            lblSinResultados.AutoSize = true;
            lblSinResultados.Location = new Point(108, 64);
            lblSinResultados.Name = "lblSinResultados";
            lblSinResultados.Size = new Size(17, 15);
            lblSinResultados.TabIndex = 12;
            lblSinResultados.Text = "\"\"";
            lblSinResultados.Visible = false;
            // 
            // panel2
            // 
            panel2.Controls.Add(lblSinResultados);
            panel2.Controls.Add(dgvClientes);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(306, 52);
            panel2.Margin = new Padding(3, 2, 3, 2);
            panel2.Name = "panel2";
            panel2.Size = new Size(566, 348);
            panel2.TabIndex = 3;
            // 
            // ClientesForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(872, 400);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Controls.Add(nuevoCliente);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(3, 2, 3, 2);
            Name = "ClientesForm";
            Text = "ClientesForm";
            nuevoCliente.ResumeLayout(false);
            nuevoCliente.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvClientes).EndInit();
            ((System.ComponentModel.ISupportInitialize)usuarioDatosBindingSource).EndInit();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel nuevoCliente;
        private Panel panel1;
        private Label lblTituloCliente;
        private TextBox txtApellidoCliente;
        private TextBox txtNombreCliente;
        private Label lblNuevoCliente;
        private Button btnCancelar;
        private Button btnGuardar;
        private DataGridView dgvClientes;
        private DataGridViewTextBoxColumn id;
        private DataGridViewTextBoxColumn Nombre;
        private DataGridViewTextBoxColumn Column1;
        private DataGridViewTextBoxColumn DNI;
        private DataGridViewTextBoxColumn Provincia;
        private DataGridViewTextBoxColumn Localidad;
        private DataGridViewTextBoxColumn Rol;
        private DataGridViewTextBoxColumn Estado;
        private BindingSource usuarioDatosBindingSource;
        private Label lblLocCliente;
        private Label lblProvCliente;
        private Label lblApellidoCliente;
        private Label lblNombreCliente;
        private Label btnCerrar;
        private Label lblDireccionCliente;
        private TextBox txtDireccionCliente;
        private Label lblTelefonoCliente;
        private TextBox txtTelefonoCliente;
        private ComboBox cmbLocalidad;
        private ComboBox cmbProvincia;
        private TextBox txtBuscar;
        private Label label1;
        private Label lblSinResultados;
        private Panel panel2;
    }
}