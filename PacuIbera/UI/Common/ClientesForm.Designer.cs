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
            lblEmailCliente = new Label();
            txtEmailCliente = new TextBox();
            lblDireccionCliente = new Label();
            txtDireccionCliente = new TextBox();
            lblLocCliente = new Label();
            lblProvCliente = new Label();
            lblDniCliente = new Label();
            lblApellidoCliente = new Label();
            lblNombreCliente = new Label();
            btnCancelar = new Button();
            btnGuardar = new Button();
            txtApellidoCliente = new TextBox();
            txtDniCliente = new TextBox();
            txtNombreCliente = new TextBox();
            lblNuevoCliente = new Label();
            panel1 = new Panel();
            label1 = new Label();
            txtBuscar = new TextBox();
            btnCerrar = new Label();
            lblTituloCliente = new Label();
            dataGridView1 = new DataGridView();
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
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
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
            nuevoCliente.Controls.Add(lblEmailCliente);
            nuevoCliente.Controls.Add(txtEmailCliente);
            nuevoCliente.Controls.Add(lblDireccionCliente);
            nuevoCliente.Controls.Add(txtDireccionCliente);
            nuevoCliente.Controls.Add(lblLocCliente);
            nuevoCliente.Controls.Add(lblProvCliente);
            nuevoCliente.Controls.Add(lblDniCliente);
            nuevoCliente.Controls.Add(lblApellidoCliente);
            nuevoCliente.Controls.Add(lblNombreCliente);
            nuevoCliente.Controls.Add(btnCancelar);
            nuevoCliente.Controls.Add(btnGuardar);
            nuevoCliente.Controls.Add(txtApellidoCliente);
            nuevoCliente.Controls.Add(txtDniCliente);
            nuevoCliente.Controls.Add(txtNombreCliente);
            nuevoCliente.Controls.Add(lblNuevoCliente);
            nuevoCliente.Dock = DockStyle.Left;
            nuevoCliente.Font = new Font("SansSerif", 12F);
            nuevoCliente.Location = new Point(0, 0);
            nuevoCliente.Name = "nuevoCliente";
            nuevoCliente.Size = new Size(350, 534);
            nuevoCliente.TabIndex = 0;
            // 
            // cmbLocalidad
            // 
            cmbLocalidad.FormattingEnabled = true;
            cmbLocalidad.Location = new Point(99, 371);
            cmbLocalidad.Name = "cmbLocalidad";
            cmbLocalidad.Size = new Size(222, 31);
            cmbLocalidad.TabIndex = 24;
            // 
            // cmbProvincia
            // 
            cmbProvincia.FormattingEnabled = true;
            cmbProvincia.Location = new Point(99, 319);
            cmbProvincia.Name = "cmbProvincia";
            cmbProvincia.Size = new Size(222, 31);
            cmbProvincia.TabIndex = 23;
            // 
            // lblTelefonoCliente
            // 
            lblTelefonoCliente.AutoSize = true;
            lblTelefonoCliente.Font = new Font("SansSerif", 9F);
            lblTelefonoCliente.Location = new Point(13, 201);
            lblTelefonoCliente.Name = "lblTelefonoCliente";
            lblTelefonoCliente.Size = new Size(72, 17);
            lblTelefonoCliente.TabIndex = 22;
            lblTelefonoCliente.Text = "Telefono :";
            // 
            // txtTelefonoCliente
            // 
            txtTelefonoCliente.CharacterCasing = CharacterCasing.Upper;
            txtTelefonoCliente.Font = new Font("Microsoft Sans Serif", 9F);
            txtTelefonoCliente.Location = new Point(99, 197);
            txtTelefonoCliente.MaxLength = 100;
            txtTelefonoCliente.Multiline = true;
            txtTelefonoCliente.Name = "txtTelefonoCliente";
            txtTelefonoCliente.Size = new Size(222, 27);
            txtTelefonoCliente.TabIndex = 21;
            // 
            // lblEmailCliente
            // 
            lblEmailCliente.AutoSize = true;
            lblEmailCliente.Font = new Font("SansSerif", 9F);
            lblEmailCliente.Location = new Point(32, 247);
            lblEmailCliente.Name = "lblEmailCliente";
            lblEmailCliente.Size = new Size(52, 17);
            lblEmailCliente.TabIndex = 20;
            lblEmailCliente.Text = "Email :";
            // 
            // txtEmailCliente
            // 
            txtEmailCliente.CharacterCasing = CharacterCasing.Upper;
            txtEmailCliente.Font = new Font("Microsoft Sans Serif", 9F);
            txtEmailCliente.Location = new Point(99, 243);
            txtEmailCliente.MaxLength = 100;
            txtEmailCliente.Multiline = true;
            txtEmailCliente.Name = "txtEmailCliente";
            txtEmailCliente.Size = new Size(222, 27);
            txtEmailCliente.TabIndex = 19;
            // 
            // lblDireccionCliente
            // 
            lblDireccionCliente.AutoSize = true;
            lblDireccionCliente.Font = new Font("SansSerif", 9F);
            lblDireccionCliente.Location = new Point(7, 285);
            lblDireccionCliente.Name = "lblDireccionCliente";
            lblDireccionCliente.Size = new Size(78, 17);
            lblDireccionCliente.TabIndex = 18;
            lblDireccionCliente.Text = "Direccion :";
            // 
            // txtDireccionCliente
            // 
            txtDireccionCliente.CharacterCasing = CharacterCasing.Upper;
            txtDireccionCliente.Font = new Font("Microsoft Sans Serif", 9F);
            txtDireccionCliente.Location = new Point(99, 281);
            txtDireccionCliente.MaxLength = 100;
            txtDireccionCliente.Multiline = true;
            txtDireccionCliente.Name = "txtDireccionCliente";
            txtDireccionCliente.Size = new Size(222, 27);
            txtDireccionCliente.TabIndex = 17;
            // 
            // lblLocCliente
            // 
            lblLocCliente.AutoSize = true;
            lblLocCliente.Font = new Font("SansSerif", 9F);
            lblLocCliente.Location = new Point(6, 379);
            lblLocCliente.Name = "lblLocCliente";
            lblLocCliente.Size = new Size(78, 17);
            lblLocCliente.TabIndex = 17;
            lblLocCliente.Text = "Localidad :";
            // 
            // lblProvCliente
            // 
            lblProvCliente.AutoSize = true;
            lblProvCliente.Font = new Font("SansSerif", 9F);
            lblProvCliente.Location = new Point(7, 327);
            lblProvCliente.Name = "lblProvCliente";
            lblProvCliente.Size = new Size(77, 17);
            lblProvCliente.TabIndex = 16;
            lblProvCliente.Text = "Provincia :";
            // 
            // lblDniCliente
            // 
            lblDniCliente.AutoSize = true;
            lblDniCliente.Font = new Font("SansSerif", 9F);
            lblDniCliente.Location = new Point(4, 162);
            lblDniCliente.Name = "lblDniCliente";
            lblDniCliente.Size = new Size(81, 17);
            lblDniCliente.TabIndex = 15;
            lblDniCliente.Text = "DNI/CUIT :";
            // 
            // lblApellidoCliente
            // 
            lblApellidoCliente.AutoSize = true;
            lblApellidoCliente.Font = new Font("SansSerif", 9F);
            lblApellidoCliente.Location = new Point(17, 118);
            lblApellidoCliente.Name = "lblApellidoCliente";
            lblApellidoCliente.Size = new Size(67, 17);
            lblApellidoCliente.TabIndex = 14;
            lblApellidoCliente.Text = "Apellido :";
            // 
            // lblNombreCliente
            // 
            lblNombreCliente.AutoSize = true;
            lblNombreCliente.Font = new Font("SansSerif", 9F);
            lblNombreCliente.Location = new Point(16, 74);
            lblNombreCliente.Name = "lblNombreCliente";
            lblNombreCliente.Size = new Size(68, 17);
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
            btnCancelar.Location = new Point(190, 462);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(110, 40);
            btnCancelar.TabIndex = 12;
            btnCancelar.Text = "CANCELAR";
            btnCancelar.UseVisualStyleBackColor = false;
            // 
            // btnGuardar
            // 
            btnGuardar.BackColor = Color.LightGreen;
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.FlatAppearance.MouseOverBackColor = Color.FromArgb(192, 255, 192);
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.Font = new Font("Microsoft Sans Serif", 9F);
            btnGuardar.Location = new Point(34, 462);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(110, 40);
            btnGuardar.TabIndex = 11;
            btnGuardar.Text = "GUARDAR";
            btnGuardar.UseVisualStyleBackColor = false;
            // 
            // txtApellidoCliente
            // 
            txtApellidoCliente.CharacterCasing = CharacterCasing.Upper;
            txtApellidoCliente.Font = new Font("Microsoft Sans Serif", 9F);
            txtApellidoCliente.Location = new Point(99, 114);
            txtApellidoCliente.MaxLength = 100;
            txtApellidoCliente.Name = "txtApellidoCliente";
            txtApellidoCliente.Size = new Size(222, 24);
            txtApellidoCliente.TabIndex = 8;
            // 
            // txtDniCliente
            // 
            txtDniCliente.CharacterCasing = CharacterCasing.Upper;
            txtDniCliente.Font = new Font("Microsoft Sans Serif", 9F);
            txtDniCliente.Location = new Point(99, 158);
            txtDniCliente.MaxLength = 100;
            txtDniCliente.Name = "txtDniCliente";
            txtDniCliente.Size = new Size(222, 24);
            txtDniCliente.TabIndex = 7;
            // 
            // txtNombreCliente
            // 
            txtNombreCliente.CharacterCasing = CharacterCasing.Upper;
            txtNombreCliente.Font = new Font("Microsoft Sans Serif", 9F);
            txtNombreCliente.Location = new Point(99, 70);
            txtNombreCliente.MaxLength = 100;
            txtNombreCliente.Name = "txtNombreCliente";
            txtNombreCliente.Size = new Size(222, 24);
            txtNombreCliente.TabIndex = 6;
            txtNombreCliente.TextChanged += textBox1_TextChanged;
            // 
            // lblNuevoCliente
            // 
            lblNuevoCliente.AutoSize = true;
            lblNuevoCliente.Font = new Font("SansSerif", 12F);
            lblNuevoCliente.Location = new Point(99, 20);
            lblNuevoCliente.Name = "lblNuevoCliente";
            lblNuevoCliente.Size = new Size(134, 23);
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
            panel1.Location = new Point(350, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(647, 70);
            panel1.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("SansSerif", 8.999999F, FontStyle.Regular, GraphicsUnit.Point, 2);
            label1.Location = new Point(6, 46);
            label1.Name = "label1";
            label1.Size = new Size(259, 17);
            label1.TabIndex = 11;
            label1.Text = "Filtrar (Nombre/ Apellido /DNI_CUIT):  ";
            // 
            // txtBuscar
            // 
            txtBuscar.Location = new Point(271, 41);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.Size = new Size(235, 27);
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
            btnCerrar.Location = new Point(612, 9);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new Size(23, 25);
            btnCerrar.TabIndex = 9;
            btnCerrar.Text = "X";
            btnCerrar.Click += btnCerrar_Click;
            // 
            // lblTituloCliente
            // 
            lblTituloCliente.AutoSize = true;
            lblTituloCliente.Font = new Font("SansSerif", 12F);
            lblTituloCliente.Location = new Point(256, 9);
            lblTituloCliente.Name = "lblTituloCliente";
            lblTituloCliente.Size = new Size(106, 23);
            lblTituloCliente.TabIndex = 0;
            lblTituloCliente.Text = "CLIENTES";
            // 
            // dataGridView1
            // 
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { id, Nombre, Column1, DNI, Provincia, Localidad, Rol, Estado });
            dataGridView1.DataSource = usuarioDatosBindingSource;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Location = new Point(0, 0);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.Size = new Size(647, 464);
            dataGridView1.TabIndex = 2;
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
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
            // usuarioDatosBindingSource
            // 
            usuarioDatosBindingSource.DataSource = typeof(Datos.UsuarioDatos);
            // 
            // lblSinResultados
            // 
            lblSinResultados.AutoSize = true;
            lblSinResultados.Location = new Point(124, 86);
            lblSinResultados.Name = "lblSinResultados";
            lblSinResultados.Size = new Size(21, 20);
            lblSinResultados.TabIndex = 12;
            lblSinResultados.Text = "\"\"";
            lblSinResultados.Visible = false;
            // 
            // panel2
            // 
            panel2.Controls.Add(lblSinResultados);
            panel2.Controls.Add(dataGridView1);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(350, 70);
            panel2.Name = "panel2";
            panel2.Size = new Size(647, 464);
            panel2.TabIndex = 3;
            // 
            // ClientesForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(997, 534);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Controls.Add(nuevoCliente);
            FormBorderStyle = FormBorderStyle.None;
            Name = "ClientesForm";
            Text = "ClientesForm";
            nuevoCliente.ResumeLayout(false);
            nuevoCliente.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
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
        private TextBox txtDniCliente;
        private TextBox txtNombreCliente;
        private Label lblNuevoCliente;
        private Button btnCancelar;
        private Button btnGuardar;
        private DataGridView dataGridView1;
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
        private Label lblDniCliente;
        private Label lblApellidoCliente;
        private Label lblNombreCliente;
        private Label btnCerrar;
        private Label lblDireccionCliente;
        private TextBox txtDireccionCliente;
        private Label lblTelefonoCliente;
        private TextBox txtTelefonoCliente;
        private Label lblEmailCliente;
        private TextBox txtEmailCliente;
        private ComboBox cmbLocalidad;
        private ComboBox cmbProvincia;
        private TextBox txtBuscar;
        private Label label1;
        private Label lblSinResultados;
        private Panel panel2;
    }
}