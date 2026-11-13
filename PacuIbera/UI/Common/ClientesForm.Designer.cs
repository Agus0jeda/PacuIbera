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
            btnEliminar = new Button();
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
            nuevoCliente.Controls.Add(btnEliminar);
            nuevoCliente.Dock = DockStyle.Left;
            nuevoCliente.Font = new Font("Microsoft Sans Serif", 12F);
            nuevoCliente.Location = new Point(0, 0);
            nuevoCliente.Name = "nuevoCliente";
            nuevoCliente.Size = new Size(350, 590);
            nuevoCliente.TabIndex = 0;
            // 
            // cmbLocalidad
            // 
            cmbLocalidad.FormattingEnabled = true;
            cmbLocalidad.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbLocalidad.Location = new Point(20, 397);
            cmbLocalidad.Name = "cmbLocalidad";
            cmbLocalidad.Size = new Size(310, 33);
            cmbLocalidad.TabIndex = 24;
            // 
            // cmbProvincia
            // 
            cmbProvincia.FormattingEnabled = true;
            cmbProvincia.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbProvincia.Location = new Point(20, 337);
            cmbProvincia.Name = "cmbProvincia";
            cmbProvincia.Size = new Size(310, 33);
            cmbProvincia.TabIndex = 23;
            cmbProvincia.SelectedIndexChanged += cmbProvincia_SelectedIndexChanged;
            // 
            // lblTelefonoCliente
            // 
            lblTelefonoCliente.AutoSize = true;
            lblTelefonoCliente.Font = new Font("Microsoft Sans Serif", 9F);
            lblTelefonoCliente.Location = new Point(20, 184);
            lblTelefonoCliente.Name = "lblTelefonoCliente";
            lblTelefonoCliente.Size = new Size(74, 18);
            lblTelefonoCliente.TabIndex = 22;
            lblTelefonoCliente.Text = "Telefono :";
            // 
            // txtTelefonoCliente
            // 
            txtTelefonoCliente.CharacterCasing = CharacterCasing.Upper;
            txtTelefonoCliente.Font = new Font("Microsoft Sans Serif", 9F);
            txtTelefonoCliente.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTelefonoCliente.Location = new Point(20, 205);
            txtTelefonoCliente.MaxLength = 20;
            txtTelefonoCliente.Multiline = false;
            txtTelefonoCliente.Name = "txtTelefonoCliente";
            txtTelefonoCliente.Size = new Size(310, 27);
            txtTelefonoCliente.TabIndex = 21;
            // 
            // lblDireccionCliente
            // 
            lblDireccionCliente.AutoSize = true;
            lblDireccionCliente.Font = new Font("Microsoft Sans Serif", 9F);
            lblDireccionCliente.Location = new Point(20, 245);
            lblDireccionCliente.Name = "lblDireccionCliente";
            lblDireccionCliente.Size = new Size(79, 18);
            lblDireccionCliente.TabIndex = 18;
            lblDireccionCliente.Text = "Direccion :";
            // 
            // txtDireccionCliente
            // 
            txtDireccionCliente.CharacterCasing = CharacterCasing.Upper;
            txtDireccionCliente.Font = new Font("Microsoft Sans Serif", 9F);
            txtDireccionCliente.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDireccionCliente.Location = new Point(20, 266);
            txtDireccionCliente.MaxLength = 150;
            txtDireccionCliente.Multiline = true;
            txtDireccionCliente.Name = "txtDireccionCliente";
            txtDireccionCliente.Size = new Size(310, 27);
            txtDireccionCliente.TabIndex = 17;
            // 
            // lblLocCliente
            // 
            lblLocCliente.AutoSize = true;
            lblLocCliente.Font = new Font("Microsoft Sans Serif", 9F);
            lblLocCliente.Location = new Point(20, 376);
            lblLocCliente.Name = "lblLocCliente";
            lblLocCliente.Size = new Size(79, 18);
            lblLocCliente.TabIndex = 17;
            lblLocCliente.Text = "Localidad :";
            // 
            // lblProvCliente
            // 
            lblProvCliente.AutoSize = true;
            lblProvCliente.Font = new Font("Microsoft Sans Serif", 9F);
            lblProvCliente.Location = new Point(20, 316);
            lblProvCliente.Name = "lblProvCliente";
            lblProvCliente.Size = new Size(77, 18);
            lblProvCliente.TabIndex = 16;
            lblProvCliente.Text = "Provincia :";
            // 
            // lblApellidoCliente
            // 
            lblApellidoCliente.AutoSize = true;
            lblApellidoCliente.Font = new Font("Microsoft Sans Serif", 9F);
            lblApellidoCliente.Location = new Point(20, 124);
            lblApellidoCliente.Name = "lblApellidoCliente";
            lblApellidoCliente.Size = new Size(67, 18);
            lblApellidoCliente.TabIndex = 14;
            lblApellidoCliente.Text = "Apellido :";
            // 
            // lblNombreCliente
            // 
            lblNombreCliente.AutoSize = true;
            lblNombreCliente.Font = new Font("Microsoft Sans Serif", 9F);
            lblNombreCliente.Location = new Point(20, 64);
            lblNombreCliente.Name = "lblNombreCliente";
            lblNombreCliente.Size = new Size(70, 18);
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
            btnCancelar.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            btnCancelar.ForeColor = Color.WhiteSmoke;
            btnCancelar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnCancelar.Location = new Point(126, 480);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(98, 40);
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
            btnGuardar.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            btnGuardar.ForeColor = Color.WhiteSmoke;
            btnGuardar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnGuardar.Location = new Point(20, 480);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(98, 40);
            btnGuardar.TabIndex = 11;
            btnGuardar.Text = "GUARDAR";
            btnGuardar.UseVisualStyleBackColor = false;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // txtApellidoCliente
            // 
            txtApellidoCliente.CharacterCasing = CharacterCasing.Upper;
            txtApellidoCliente.Font = new Font("Microsoft Sans Serif", 9F);
            txtApellidoCliente.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtApellidoCliente.Location = new Point(20, 145);
            txtApellidoCliente.MaxLength = 100;
            txtApellidoCliente.Name = "txtApellidoCliente";
            txtApellidoCliente.Size = new Size(310, 27);
            txtApellidoCliente.TabIndex = 8;
            // 
            // txtNombreCliente
            // 
            txtNombreCliente.CharacterCasing = CharacterCasing.Upper;
            txtNombreCliente.Font = new Font("Microsoft Sans Serif", 9F);
            txtNombreCliente.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtNombreCliente.Location = new Point(20, 85);
            txtNombreCliente.MaxLength = 100;
            txtNombreCliente.Name = "txtNombreCliente";
            txtNombreCliente.Size = new Size(310, 27);
            txtNombreCliente.TabIndex = 6;
            txtNombreCliente.TextChanged += textBox1_TextChanged;
            // 
            // lblNuevoCliente
            // 
            lblNuevoCliente.AutoSize = true;
            lblNuevoCliente.Font = new Font("Microsoft Sans Serif", 12F);
            lblNuevoCliente.Location = new Point(20, 18);
            lblNuevoCliente.Name = "lblNuevoCliente";
            lblNuevoCliente.Size = new Size(135, 25);
            lblNuevoCliente.TabIndex = 0;
            lblNuevoCliente.Text = "Datos del cliente · Nuevo";
            // 
            // btnEliminar
            // 
            btnEliminar.BackColor = Color.Red;
            btnEliminar.Cursor = Cursors.Hand;
            btnEliminar.FlatAppearance.BorderSize = 0;
            btnEliminar.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 192, 192);
            btnEliminar.FlatStyle = FlatStyle.Flat;
            btnEliminar.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            btnEliminar.ForeColor = Color.WhiteSmoke;
            btnEliminar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnEliminar.Location = new Point(232, 480);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new Size(98, 40);
            btnEliminar.Visible = false;
            btnEliminar.TabIndex = 13;
            btnEliminar.Text = "ELIMINAR";
            btnEliminar.UseVisualStyleBackColor = false;
            btnEliminar.Click += btnEliminar_Click;
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
            panel1.Size = new Size(730, 126);
            panel1.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(20, 53);
            label1.Name = "label1";
            label1.Size = new Size(299, 20);
            label1.TabIndex = 11;
            label1.Text = "Buscar cliente:";
            // 
            // txtBuscar
            // 
            txtBuscar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtBuscar.Location = new Point(132, 48);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.Size = new Size(550, 30);
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
            btnCerrar.Location = new Point(613, 9);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new Size(23, 25);
            btnCerrar.TabIndex = 9;
            btnCerrar.Text = "X";
            btnCerrar.Click += btnCerrar_Click;
            // 
            // lblTituloCliente
            // 
            lblTituloCliente.AutoSize = true;
            lblTituloCliente.Font = new Font("Microsoft Sans Serif", 12F);
            lblTituloCliente.Location = new Point(20, 13);
            lblTituloCliente.Name = "lblTituloCliente";
            lblTituloCliente.Size = new Size(110, 25);
            lblTituloCliente.TabIndex = 0;
            lblTituloCliente.Text = "LISTADO DE CLIENTES";
            // 
            // dgvClientes
            // 
            dgvClientes.AutoGenerateColumns = false;
            dgvClientes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvClientes.Columns.AddRange(new DataGridViewColumn[] { id, Nombre, Column1, DNI, Provincia, Localidad, Rol, Estado });
            dgvClientes.DataSource = usuarioDatosBindingSource;
            dgvClientes.Dock = DockStyle.Fill;
            dgvClientes.Location = new Point(0, 0);
            dgvClientes.Name = "dgvClientes";
            dgvClientes.AllowUserToResizeColumns = true;
            dgvClientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvClientes.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvClientes.ScrollBars = ScrollBars.Both;
            dgvClientes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvClientes.MultiSelect = false;
            dgvClientes.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvClientes.RowHeadersWidth = 51;
            dgvClientes.Size = new Size(647, 464);
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
            lblSinResultados.Location = new Point(123, 85);
            lblSinResultados.Name = "lblSinResultados";
            lblSinResultados.Size = new Size(21, 20);
            lblSinResultados.TabIndex = 12;
            lblSinResultados.Text = "\"\"";
            lblSinResultados.Visible = false;
            // 
            // panel2
            // 
            panel2.Controls.Add(lblSinResultados);
            panel2.Controls.Add(dgvClientes);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(350, 69);
            panel2.Name = "panel2";
            panel2.Size = new Size(730, 524);
            panel2.TabIndex = 3;
            // 
            // ClientesForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1080, 590);
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
            ((System.ComponentModel.ISupportInitialize)dgvClientes).EndInit();
            ((System.ComponentModel.ISupportInitialize)usuarioDatosBindingSource).EndInit();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel nuevoCliente;
        private Button btnEliminar;
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
