namespace PacuIbera.UI.Common
{
    partial class ProveedorForm
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
        private void InitializeComponent()
        {
            pnlDatosProv = new Panel();
            lblLocalidad = new Label();
            lblProvincia = new Label();
            lblDireccion = new Label();
            lblEmail = new Label();
            lblTelefono = new Label();
            lblCuit = new Label();
            lblRazonSocial = new Label();
            btnLimpiar = new Button();
            btnEliminar = new Button();
            btnGuardar = new Button();
            gbEstados = new GroupBox();
            rbInactivo = new RadioButton();
            rbActivo = new RadioButton();
            cmbLocalidad = new ComboBox();
            cmbProvincia = new ComboBox();
            txtDireccion = new TextBox();
            txtEmail = new TextBox();
            txtTelefono = new TextBox();
            txtCuit = new TextBox();
            txtRazonSocial = new TextBox();
            lblGestionProveedor = new Label();
            txtBuscarProveedor = new TextBox();
            dgvProveedores = new DataGridView();
            Id = new DataGridViewTextBoxColumn();
            RazonSocial = new DataGridViewTextBoxColumn();
            CUIT = new DataGridViewTextBoxColumn();
            Telefono = new DataGridViewTextBoxColumn();
            Email = new DataGridViewTextBoxColumn();
            Direccion = new DataGridViewTextBoxColumn();
            Localidad = new DataGridViewTextBoxColumn();
            Estado = new DataGridViewTextBoxColumn();
            lblFiltrar = new Label();
            pnlFiltro = new Panel();
            pnlDatosProv.SuspendLayout();
            gbEstados.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvProveedores).BeginInit();
            pnlFiltro.SuspendLayout();
            SuspendLayout();
            // 
            // pnlDatosProv
            // 
            pnlDatosProv.BackColor = Color.WhiteSmoke;
            pnlDatosProv.Controls.Add(lblLocalidad);
            pnlDatosProv.Controls.Add(lblProvincia);
            pnlDatosProv.Controls.Add(lblDireccion);
            pnlDatosProv.Controls.Add(lblEmail);
            pnlDatosProv.Controls.Add(lblTelefono);
            pnlDatosProv.Controls.Add(lblCuit);
            pnlDatosProv.Controls.Add(lblRazonSocial);
            pnlDatosProv.Controls.Add(btnLimpiar);
            pnlDatosProv.Controls.Add(btnEliminar);
            pnlDatosProv.Controls.Add(btnGuardar);
            pnlDatosProv.Controls.Add(gbEstados);
            pnlDatosProv.Controls.Add(cmbLocalidad);
            pnlDatosProv.Controls.Add(cmbProvincia);
            pnlDatosProv.Controls.Add(txtDireccion);
            pnlDatosProv.Controls.Add(txtEmail);
            pnlDatosProv.Controls.Add(txtTelefono);
            pnlDatosProv.Controls.Add(txtCuit);
            pnlDatosProv.Controls.Add(txtRazonSocial);
            pnlDatosProv.Dock = DockStyle.Left;
            pnlDatosProv.Location = new Point(0, 70);
            pnlDatosProv.Name = "pnlDatosProv";
            pnlDatosProv.Size = new Size(501, 380);
            pnlDatosProv.TabIndex = 0;
            // 
            // lblLocalidad
            // 
            lblLocalidad.Location = new Point(255, 62);
            lblLocalidad.Name = "lblLocalidad";
            lblLocalidad.Size = new Size(84, 23);
            lblLocalidad.TabIndex = 0;
            lblLocalidad.Text = "Localidad:";
            // 
            // lblProvincia
            // 
            lblProvincia.AutoSize = true;
            lblProvincia.Location = new Point(267, 17);
            lblProvincia.Name = "lblProvincia";
            lblProvincia.Size = new Size(72, 20);
            lblProvincia.TabIndex = 26;
            lblProvincia.Text = "Provincia:";
            // 
            // lblDireccion
            // 
            lblDireccion.AutoSize = true;
            lblDireccion.Location = new Point(26, 198);
            lblDireccion.Name = "lblDireccion";
            lblDireccion.Size = new Size(72, 20);
            lblDireccion.TabIndex = 24;
            lblDireccion.Text = "Direccion";
            // 
            // lblEmail
            // 
            lblEmail.AutoSize = true;
            lblEmail.Location = new Point(49, 150);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(49, 20);
            lblEmail.TabIndex = 23;
            lblEmail.Text = "Email:";
            // 
            // lblTelefono
            // 
            lblTelefono.AutoSize = true;
            lblTelefono.Location = new Point(28, 104);
            lblTelefono.Name = "lblTelefono";
            lblTelefono.Size = new Size(70, 20);
            lblTelefono.TabIndex = 22;
            lblTelefono.Text = "Telefono:";
            // 
            // lblCuit
            // 
            lblCuit.AutoSize = true;
            lblCuit.Location = new Point(55, 61);
            lblCuit.Name = "lblCuit";
            lblCuit.Size = new Size(43, 20);
            lblCuit.TabIndex = 21;
            lblCuit.Text = "CUIT:";
            // 
            // lblRazonSocial
            // 
            lblRazonSocial.AutoSize = true;
            lblRazonSocial.Location = new Point(1, 19);
            lblRazonSocial.Name = "lblRazonSocial";
            lblRazonSocial.Size = new Size(97, 20);
            lblRazonSocial.TabIndex = 20;
            lblRazonSocial.Text = "Razón Social:";
            // 
            // btnLimpiar
            // 
            btnLimpiar.BackColor = Color.Silver;
            btnLimpiar.FlatAppearance.BorderSize = 0;
            btnLimpiar.FlatAppearance.MouseOverBackColor = Color.FromArgb(192, 255, 192);
            btnLimpiar.FlatStyle = FlatStyle.Flat;
            btnLimpiar.Font = new Font("Microsoft Sans Serif", 10F);
            btnLimpiar.Location = new Point(188, 323);
            btnLimpiar.Name = "btnLimpiar";
            btnLimpiar.Size = new Size(110, 40);
            btnLimpiar.TabIndex = 19;
            btnLimpiar.Text = "LIMPIAR";
            btnLimpiar.UseVisualStyleBackColor = false;
            btnLimpiar.Click += btnLimpiar_Click;
            // 
            // btnEliminar
            // 
            btnEliminar.BackColor = Color.Red;
            btnEliminar.Cursor = Cursors.Hand;
            btnEliminar.FlatAppearance.BorderSize = 0;
            btnEliminar.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 192, 192);
            btnEliminar.FlatStyle = FlatStyle.Flat;
            btnEliminar.Font = new Font("Microsoft Sans Serif", 10F);
            btnEliminar.Location = new Point(353, 323);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new Size(110, 40);
            btnEliminar.TabIndex = 18;
            btnEliminar.Text = "ELIMINAR";
            btnEliminar.UseVisualStyleBackColor = false;
            btnEliminar.Click += btnEliminar_Click;
            // 
            // btnGuardar
            // 
            btnGuardar.BackColor = Color.LightGreen;
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.FlatAppearance.MouseOverBackColor = Color.FromArgb(192, 255, 192);
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.Font = new Font("Microsoft Sans Serif", 10F);
            btnGuardar.Location = new Point(29, 323);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(110, 40);
            btnGuardar.TabIndex = 17;
            btnGuardar.Text = "GUARDAR";
            btnGuardar.UseVisualStyleBackColor = false;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // gbEstados
            // 
            gbEstados.Controls.Add(rbInactivo);
            gbEstados.Controls.Add(rbActivo);
            gbEstados.Location = new Point(289, 133);
            gbEstados.Name = "gbEstados";
            gbEstados.Size = new Size(174, 104);
            gbEstados.TabIndex = 8;
            gbEstados.TabStop = false;
            gbEstados.Text = "Estado:";
            // 
            // rbInactivo
            // 
            rbInactivo.AutoSize = true;
            rbInactivo.Location = new Point(72, 65);
            rbInactivo.Name = "rbInactivo";
            rbInactivo.Size = new Size(82, 24);
            rbInactivo.TabIndex = 1;
            rbInactivo.TabStop = true;
            rbInactivo.Text = "Inactivo";
            rbInactivo.UseVisualStyleBackColor = true;
            // 
            // rbActivo
            // 
            rbActivo.AutoSize = true;
            rbActivo.Location = new Point(72, 26);
            rbActivo.Name = "rbActivo";
            rbActivo.Size = new Size(72, 24);
            rbActivo.TabIndex = 0;
            rbActivo.TabStop = true;
            rbActivo.Text = "Activo";
            rbActivo.UseVisualStyleBackColor = true;
            // 
            // cmbLocalidad
            // 
            cmbLocalidad.FormattingEnabled = true;
            cmbLocalidad.Location = new Point(345, 59);
            cmbLocalidad.Name = "cmbLocalidad";
            cmbLocalidad.Size = new Size(150, 28);
            cmbLocalidad.TabIndex = 7;
            // 
            // cmbProvincia
            // 
            cmbProvincia.FormattingEnabled = true;
            cmbProvincia.Location = new Point(345, 16);
            cmbProvincia.Name = "cmbProvincia";
            cmbProvincia.Size = new Size(150, 28);
            cmbProvincia.TabIndex = 6;
            cmbProvincia.SelectedIndexChanged += cmbProvincia_SelectedIndexChanged;
            // 
            // txtDireccion
            // 
            txtDireccion.Location = new Point(104, 195);
            txtDireccion.Name = "txtDireccion";
            txtDireccion.Size = new Size(150, 27);
            txtDireccion.TabIndex = 4;
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(104, 147);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(150, 27);
            txtEmail.TabIndex = 3;
            // 
            // txtTelefono
            // 
            txtTelefono.Location = new Point(104, 101);
            txtTelefono.Name = "txtTelefono";
            txtTelefono.Size = new Size(150, 27);
            txtTelefono.TabIndex = 2;
            txtTelefono.KeyPress += txtTelefono_KeyPress;
            // 
            // txtCuit
            // 
            txtCuit.Location = new Point(104, 58);
            txtCuit.Name = "txtCuit";
            txtCuit.Size = new Size(150, 27);
            txtCuit.TabIndex = 1;
            txtCuit.TextChanged += txtCuit_TextChanged;
            txtCuit.KeyPress += txtCuit_KeyPress;
            // 
            // txtRazonSocial
            // 
            txtRazonSocial.Location = new Point(104, 16);
            txtRazonSocial.Name = "txtRazonSocial";
            txtRazonSocial.Size = new Size(150, 27);
            txtRazonSocial.TabIndex = 0;
            // 
            // lblGestionProveedor
            // 
            lblGestionProveedor.AutoSize = true;
            lblGestionProveedor.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblGestionProveedor.Location = new Point(186, 17);
            lblGestionProveedor.Name = "lblGestionProveedor";
            lblGestionProveedor.Size = new Size(250, 32);
            lblGestionProveedor.TabIndex = 25;
            lblGestionProveedor.Text = "Gestionar Proveedor";
            lblGestionProveedor.Click += lblGestionProveedor_Click;
            // 
            // txtBuscarProveedor
            // 
            txtBuscarProveedor.Location = new Point(656, 13);
            txtBuscarProveedor.Name = "txtBuscarProveedor";
            txtBuscarProveedor.Size = new Size(332, 27);
            txtBuscarProveedor.TabIndex = 1;
            txtBuscarProveedor.TextChanged += txtBuscarProveedor_TextChanged;
            txtBuscarProveedor.Enter += txtBuscarProveedor_Enter;
            txtBuscarProveedor.Leave += txtBuscarProveedor_Leave;
            // 
            // dgvProveedores
            // 
            dgvProveedores.AllowUserToAddRows = false;
            dgvProveedores.AllowUserToDeleteRows = false;
            dgvProveedores.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvProveedores.Columns.AddRange(new DataGridViewColumn[] { Id, RazonSocial, CUIT, Telefono, Email, Direccion, Localidad, Estado });
            dgvProveedores.Dock = DockStyle.Fill;
            dgvProveedores.Location = new Point(501, 70);
            dgvProveedores.Name = "dgvProveedores";
            dgvProveedores.ReadOnly = true;
            dgvProveedores.RowHeadersWidth = 51;
            dgvProveedores.Size = new Size(499, 380);
            dgvProveedores.TabIndex = 2;
            dgvProveedores.CellDoubleClick += dgvProveedores_CellDoubleClick;
            dgvProveedores.DataBindingComplete += dgvProveedores_DataBindingComplete;
            // 
            // Id
            // 
            Id.DataPropertyName = "Id";
            Id.HeaderText = "Id";
            Id.MinimumWidth = 6;
            Id.Name = "Id";
            Id.ReadOnly = true;
            Id.Width = 125;
            // 
            // RazonSocial
            // 
            RazonSocial.DataPropertyName = "RazonSocial";
            RazonSocial.HeaderText = "RazonSocial";
            RazonSocial.MinimumWidth = 6;
            RazonSocial.Name = "RazonSocial";
            RazonSocial.ReadOnly = true;
            RazonSocial.Width = 125;
            // 
            // CUIT
            // 
            CUIT.DataPropertyName = "CUIT";
            CUIT.HeaderText = "CUIT";
            CUIT.MinimumWidth = 6;
            CUIT.Name = "CUIT";
            CUIT.ReadOnly = true;
            CUIT.Width = 125;
            // 
            // Telefono
            // 
            Telefono.DataPropertyName = "Telefono";
            Telefono.HeaderText = "Telefono";
            Telefono.MinimumWidth = 6;
            Telefono.Name = "Telefono";
            Telefono.ReadOnly = true;
            Telefono.Width = 125;
            // 
            // Email
            // 
            Email.DataPropertyName = "Email";
            Email.HeaderText = "Email";
            Email.MinimumWidth = 6;
            Email.Name = "Email";
            Email.ReadOnly = true;
            Email.Width = 125;
            // 
            // Direccion
            // 
            Direccion.DataPropertyName = "Direccion";
            Direccion.HeaderText = "Direccion";
            Direccion.MinimumWidth = 6;
            Direccion.Name = "Direccion";
            Direccion.ReadOnly = true;
            Direccion.Width = 125;
            // 
            // Localidad
            // 
            Localidad.DataPropertyName = "Localidad";
            Localidad.HeaderText = "Localidad";
            Localidad.MinimumWidth = 6;
            Localidad.Name = "Localidad";
            Localidad.ReadOnly = true;
            Localidad.Width = 125;
            // 
            // Estado
            // 
            Estado.DataPropertyName = "Estado";
            Estado.HeaderText = "Estado";
            Estado.MinimumWidth = 6;
            Estado.Name = "Estado";
            Estado.ReadOnly = true;
            Estado.Width = 125;
            // 
            // lblFiltrar
            // 
            lblFiltrar.Location = new Point(587, 16);
            lblFiltrar.Name = "lblFiltrar";
            lblFiltrar.Size = new Size(63, 33);
            lblFiltrar.TabIndex = 3;
            lblFiltrar.Text = "Filtrar:";
            // 
            // pnlFiltro
            // 
            pnlFiltro.Controls.Add(txtBuscarProveedor);
            pnlFiltro.Controls.Add(lblFiltrar);
            pnlFiltro.Controls.Add(lblGestionProveedor);
            pnlFiltro.Dock = DockStyle.Top;
            pnlFiltro.Location = new Point(0, 0);
            pnlFiltro.Name = "pnlFiltro";
            pnlFiltro.Size = new Size(1000, 70);
            pnlFiltro.TabIndex = 3;
            // 
            // ProveedorForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 450);
            Controls.Add(dgvProveedores);
            Controls.Add(pnlDatosProv);
            Controls.Add(pnlFiltro);
            FormBorderStyle = FormBorderStyle.None;
            Name = "ProveedorForm";
            Text = "ProveedorForm";
            Load += ProveedorForm_Load;
            pnlDatosProv.ResumeLayout(false);
            pnlDatosProv.PerformLayout();
            gbEstados.ResumeLayout(false);
            gbEstados.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvProveedores).EndInit();
            pnlFiltro.ResumeLayout(false);
            pnlFiltro.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlDatosProv;
        private ComboBox cmbLocalidad;
        private ComboBox cmbProvincia;
        private TextBox txtDireccion;
        private TextBox txtEmail;
        private TextBox txtTelefono;
        private TextBox txtCuit;
        private TextBox txtRazonSocial;
        private GroupBox gbEstados;
        private RadioButton rbInactivo;
        private RadioButton rbActivo;
        private TextBox txtBuscarProveedor;
        private Button btnLimpiar;
        private Button btnEliminar;
        private Button btnGuardar;
        private DataGridView dgvProveedores;
        private Label lblProvincia;
        private Label lblGestionProveedor;
        private Label lblDireccion;
        private Label lblEmail;
        private Label lblTelefono;
        private Label lblCuit;
        private Label lblRazonSocial;
        private Label lblLocalidad;
        private Label lblFiltrar;
        private Panel pnlFiltro;
        private DataGridViewTextBoxColumn Id;
        private DataGridViewTextBoxColumn RazonSocial;
        private DataGridViewTextBoxColumn CUIT;
        private DataGridViewTextBoxColumn Telefono;
        private DataGridViewTextBoxColumn Email;
        private DataGridViewTextBoxColumn Direccion;
        private DataGridViewTextBoxColumn Localidad;
        private DataGridViewTextBoxColumn Estado;
    }
}