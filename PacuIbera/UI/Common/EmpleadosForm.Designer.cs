namespace PacuIbera.UI.Common
{
    partial class EmpleadosForm
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
            components = new System.ComponentModel.Container();
            panelAñadirEmpleado = new Panel();
            btnLimpiar = new Button();
            lblFechaNacimiento = new Label();
            dtFechaNacimiento = new DateTimePicker();
            gbEstado = new GroupBox();
            rbInactivo = new RadioButton();
            rbActivo = new RadioButton();
            txtClave = new TextBox();
            cmbRol = new ComboBox();
            cmbLocalidad = new ComboBox();
            cmbProvincia = new ComboBox();
            lblClave = new Label();
            lblRol = new Label();
            lblProvincia = new Label();
            btnEliminar = new Button();
            btnGuardar = new Button();
            lblNuevosEmpleados = new Label();
            lblApellido = new Label();
            lblDni = new Label();
            lblDireccion = new Label();
            lblEmail = new Label();
            lblTelefono = new Label();
            lblLocalidad = new Label();
            lblNombre = new Label();
            txtTelefono = new TextBox();
            txtEmail = new TextBox();
            txtDireccion = new TextBox();
            txtDni = new TextBox();
            txtApellido = new TextBox();
            txtNombre = new TextBox();
            lblBuscar = new Label();
            lblEmpleados = new Label();
            lblAñadir = new Label();
            panelEmpleados = new Panel();
            dgvEmpleados = new DataGridView();
            Id = new DataGridViewTextBoxColumn();
            Nombre = new DataGridViewTextBoxColumn();
            Apellido = new DataGridViewTextBoxColumn();
            DNI = new DataGridViewTextBoxColumn();
            Telefono = new DataGridViewTextBoxColumn();
            Email = new DataGridViewTextBoxColumn();
            Direccion = new DataGridViewTextBoxColumn();
            Localidad = new DataGridViewTextBoxColumn();
            Provincia = new DataGridViewTextBoxColumn();
            Rol = new DataGridViewTextBoxColumn();
            Clave = new DataGridViewTextBoxColumn();
            Estado = new DataGridViewTextBoxColumn();
            pnlSuperior = new Panel();
            txtFiltrarRol = new TextBox();
            txtFiltrarNombre = new TextBox();
            usuarioDatosBindingSource = new BindingSource(components);
            panelAñadirEmpleado.SuspendLayout();
            gbEstado.SuspendLayout();
            panelEmpleados.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvEmpleados).BeginInit();
            pnlSuperior.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)usuarioDatosBindingSource).BeginInit();
            SuspendLayout();
            // 
            // panelAñadirEmpleado
            // 
            panelAñadirEmpleado.BackColor = Color.WhiteSmoke;
            panelAñadirEmpleado.Controls.Add(btnLimpiar);
            panelAñadirEmpleado.Controls.Add(lblFechaNacimiento);
            panelAñadirEmpleado.Controls.Add(dtFechaNacimiento);
            panelAñadirEmpleado.Controls.Add(gbEstado);
            panelAñadirEmpleado.Controls.Add(txtClave);
            panelAñadirEmpleado.Controls.Add(cmbRol);
            panelAñadirEmpleado.Controls.Add(cmbLocalidad);
            panelAñadirEmpleado.Controls.Add(cmbProvincia);
            panelAñadirEmpleado.Controls.Add(lblClave);
            panelAñadirEmpleado.Controls.Add(lblRol);
            panelAñadirEmpleado.Controls.Add(lblProvincia);
            panelAñadirEmpleado.Controls.Add(btnEliminar);
            panelAñadirEmpleado.Controls.Add(btnGuardar);
            panelAñadirEmpleado.Controls.Add(lblNuevosEmpleados);
            panelAñadirEmpleado.Controls.Add(lblApellido);
            panelAñadirEmpleado.Controls.Add(lblDni);
            panelAñadirEmpleado.Controls.Add(lblDireccion);
            panelAñadirEmpleado.Controls.Add(lblEmail);
            panelAñadirEmpleado.Controls.Add(lblTelefono);
            panelAñadirEmpleado.Controls.Add(lblLocalidad);
            panelAñadirEmpleado.Controls.Add(lblNombre);
            panelAñadirEmpleado.Controls.Add(txtTelefono);
            panelAñadirEmpleado.Controls.Add(txtEmail);
            panelAñadirEmpleado.Controls.Add(txtDireccion);
            panelAñadirEmpleado.Controls.Add(txtDni);
            panelAñadirEmpleado.Controls.Add(txtApellido);
            panelAñadirEmpleado.Controls.Add(txtNombre);
            panelAñadirEmpleado.Dock = DockStyle.Left;
            panelAñadirEmpleado.Location = new Point(0, 0);
            panelAñadirEmpleado.Name = "panelAñadirEmpleado";
            panelAñadirEmpleado.Size = new Size(501, 533);
            panelAñadirEmpleado.TabIndex = 0;
            // 
            // btnLimpiar
            // 
            btnLimpiar.BackColor = Color.Silver;
            btnLimpiar.FlatAppearance.BorderSize = 0;
            btnLimpiar.FlatAppearance.MouseOverBackColor = Color.FromArgb(192, 255, 192);
            btnLimpiar.FlatStyle = FlatStyle.Flat;
            btnLimpiar.Font = new Font("Microsoft Sans Serif", 10F);
            btnLimpiar.Location = new Point(192, 465);
            btnLimpiar.Name = "btnLimpiar";
            btnLimpiar.Size = new Size(110, 40);
            btnLimpiar.TabIndex = 28;
            btnLimpiar.Text = "LIMPIAR";
            btnLimpiar.UseVisualStyleBackColor = false;
            btnLimpiar.Click += btnLimpiar_Click;
            // 
            // lblFechaNacimiento
            // 
            lblFechaNacimiento.AutoSize = true;
            lblFechaNacimiento.Font = new Font("Microsoft Sans Serif", 11.25F);
            lblFechaNacimiento.Location = new Point(264, 255);
            lblFechaNacimiento.Name = "lblFechaNacimiento";
            lblFechaNacimiento.Size = new Size(118, 24);
            lblFechaNacimiento.TabIndex = 27;
            lblFechaNacimiento.Text = "Fecha Nac. :";
            // 
            // dtFechaNacimiento
            // 
            dtFechaNacimiento.Font = new Font("Microsoft Sans Serif", 11.25F);
            dtFechaNacimiento.Format = DateTimePickerFormat.Short;
            dtFechaNacimiento.Location = new Point(350, 291);
            dtFechaNacimiento.Name = "dtFechaNacimiento";
            dtFechaNacimiento.Size = new Size(142, 29);
            dtFechaNacimiento.TabIndex = 26;
            // 
            // gbEstado
            // 
            gbEstado.Controls.Add(rbInactivo);
            gbEstado.Controls.Add(rbActivo);
            gbEstado.Location = new Point(334, 345);
            gbEstado.Name = "gbEstado";
            gbEstado.Size = new Size(158, 92);
            gbEstado.TabIndex = 25;
            gbEstado.TabStop = false;
            gbEstado.Text = "Estado :";
            // 
            // rbInactivo
            // 
            rbInactivo.AutoSize = true;
            rbInactivo.Font = new Font("Microsoft Sans Serif", 11.25F);
            rbInactivo.Location = new Point(56, 59);
            rbInactivo.Name = "rbInactivo";
            rbInactivo.Size = new Size(94, 28);
            rbInactivo.TabIndex = 1;
            rbInactivo.TabStop = true;
            rbInactivo.Text = "Inactivo";
            rbInactivo.UseVisualStyleBackColor = true;
            // 
            // rbActivo
            // 
            rbActivo.AutoSize = true;
            rbActivo.Font = new Font("Microsoft Sans Serif", 11.25F);
            rbActivo.Location = new Point(56, 27);
            rbActivo.Name = "rbActivo";
            rbActivo.Size = new Size(82, 28);
            rbActivo.TabIndex = 0;
            rbActivo.TabStop = true;
            rbActivo.Text = "Activo";
            rbActivo.UseVisualStyleBackColor = true;
            // 
            // txtClave
            // 
            txtClave.Location = new Point(91, 392);
            txtClave.Name = "txtClave";
            txtClave.Size = new Size(150, 27);
            txtClave.TabIndex = 24;
            // 
            // cmbRol
            // 
            cmbRol.ForeColor = SystemColors.WindowFrame;
            cmbRol.FormattingEnabled = true;
            cmbRol.Location = new Point(91, 345);
            cmbRol.Name = "cmbRol";
            cmbRol.Size = new Size(150, 28);
            cmbRol.TabIndex = 23;
            cmbRol.Text = "SELECCIONAR...";
            // 
            // cmbLocalidad
            // 
            cmbLocalidad.ForeColor = SystemColors.WindowFrame;
            cmbLocalidad.FormattingEnabled = true;
            cmbLocalidad.Location = new Point(342, 201);
            cmbLocalidad.Name = "cmbLocalidad";
            cmbLocalidad.Size = new Size(150, 28);
            cmbLocalidad.TabIndex = 22;
            cmbLocalidad.Text = "SELECCIONAR...";
            // 
            // cmbProvincia
            // 
            cmbProvincia.ForeColor = SystemColors.WindowFrame;
            cmbProvincia.FormattingEnabled = true;
            cmbProvincia.Location = new Point(342, 155);
            cmbProvincia.Name = "cmbProvincia";
            cmbProvincia.Size = new Size(150, 28);
            cmbProvincia.TabIndex = 21;
            cmbProvincia.Text = "SELECCIONAR...";
            cmbProvincia.SelectedIndexChanged += cmbProvincia_SelectedIndexChanged;
            // 
            // lblClave
            // 
            lblClave.AutoSize = true;
            lblClave.Font = new Font("Microsoft Sans Serif", 11.25F);
            lblClave.Location = new Point(24, 395);
            lblClave.Name = "lblClave";
            lblClave.Size = new Size(67, 24);
            lblClave.TabIndex = 20;
            lblClave.Text = "Clave :";
            // 
            // lblRol
            // 
            lblRol.AutoSize = true;
            lblRol.Font = new Font("Microsoft Sans Serif", 11.25F);
            lblRol.Location = new Point(42, 348);
            lblRol.Name = "lblRol";
            lblRol.Size = new Size(48, 24);
            lblRol.TabIndex = 18;
            lblRol.Text = "Rol :";
            // 
            // lblProvincia
            // 
            lblProvincia.AutoSize = true;
            lblProvincia.Font = new Font("Microsoft Sans Serif", 10F);
            lblProvincia.Location = new Point(248, 157);
            lblProvincia.Name = "lblProvincia";
            lblProvincia.Size = new Size(88, 20);
            lblProvincia.TabIndex = 17;
            lblProvincia.Text = "Provincia :";
            // 
            // btnEliminar
            // 
            btnEliminar.BackColor = Color.Red;
            btnEliminar.Cursor = Cursors.Hand;
            btnEliminar.FlatAppearance.BorderSize = 0;
            btnEliminar.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 192, 192);
            btnEliminar.FlatStyle = FlatStyle.Flat;
            btnEliminar.Font = new Font("Microsoft Sans Serif", 10F);
            btnEliminar.Location = new Point(362, 465);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new Size(110, 40);
            btnEliminar.TabIndex = 16;
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
            btnGuardar.Location = new Point(25, 465);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(110, 40);
            btnGuardar.TabIndex = 15;
            btnGuardar.Text = "GUARDAR";
            btnGuardar.UseVisualStyleBackColor = false;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // lblNuevosEmpleados
            // 
            lblNuevosEmpleados.AutoSize = true;
            lblNuevosEmpleados.Font = new Font("Microsoft Sans Serif", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblNuevosEmpleados.Location = new Point(145, 35);
            lblNuevosEmpleados.Name = "lblNuevosEmpleados";
            lblNuevosEmpleados.Size = new Size(224, 29);
            lblNuevosEmpleados.TabIndex = 14;
            lblNuevosEmpleados.Text = "Nuevos Empleados";
            // 
            // lblApellido
            // 
            lblApellido.AutoSize = true;
            lblApellido.Font = new Font("Microsoft Sans Serif", 11.25F);
            lblApellido.Location = new Point(8, 160);
            lblApellido.Name = "lblApellido";
            lblApellido.Size = new Size(89, 24);
            lblApellido.TabIndex = 13;
            lblApellido.Text = "Apellido :";
            // 
            // lblDni
            // 
            lblDni.AutoSize = true;
            lblDni.Font = new Font("Microsoft Sans Serif", 11.25F);
            lblDni.Location = new Point(38, 205);
            lblDni.Name = "lblDni";
            lblDni.Size = new Size(51, 24);
            lblDni.TabIndex = 12;
            lblDni.Text = "DNI :";
            // 
            // lblDireccion
            // 
            lblDireccion.AutoSize = true;
            lblDireccion.Font = new Font("Microsoft Sans Serif", 11.25F);
            lblDireccion.Location = new Point(246, 115);
            lblDireccion.Name = "lblDireccion";
            lblDireccion.Size = new Size(100, 24);
            lblDireccion.TabIndex = 11;
            lblDireccion.Text = "Direccion :";
            lblDireccion.Click += lblDireccion_Click;
            // 
            // lblEmail
            // 
            lblEmail.AutoSize = true;
            lblEmail.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblEmail.Location = new Point(25, 297);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(61, 20);
            lblEmail.TabIndex = 10;
            lblEmail.Text = "Email :";
            // 
            // lblTelefono
            // 
            lblTelefono.AutoSize = true;
            lblTelefono.Font = new Font("Microsoft Sans Serif", 10F);
            lblTelefono.Location = new Point(3, 252);
            lblTelefono.Name = "lblTelefono";
            lblTelefono.Size = new Size(83, 20);
            lblTelefono.TabIndex = 9;
            lblTelefono.Text = "Telefono :";
            // 
            // lblLocalidad
            // 
            lblLocalidad.AutoSize = true;
            lblLocalidad.Font = new Font("Microsoft Sans Serif", 10F);
            lblLocalidad.Location = new Point(245, 204);
            lblLocalidad.Name = "lblLocalidad";
            lblLocalidad.Size = new Size(91, 20);
            lblLocalidad.TabIndex = 8;
            lblLocalidad.Text = "Localidad :";
            // 
            // lblNombre
            // 
            lblNombre.AutoSize = true;
            lblNombre.Font = new Font("Microsoft Sans Serif", 11.25F);
            lblNombre.Location = new Point(8, 115);
            lblNombre.Name = "lblNombre";
            lblNombre.Size = new Size(89, 24);
            lblNombre.TabIndex = 7;
            lblNombre.Text = "Nombre :";
            // 
            // txtTelefono
            // 
            txtTelefono.Font = new Font("Microsoft Sans Serif", 10F);
            txtTelefono.Location = new Point(91, 249);
            txtTelefono.Name = "txtTelefono";
            txtTelefono.Size = new Size(150, 26);
            txtTelefono.TabIndex = 6;
            txtTelefono.KeyPress += txtTelefono_KeyPress;
            // 
            // txtEmail
            // 
            txtEmail.Font = new Font("Microsoft Sans Serif", 10F);
            txtEmail.Location = new Point(91, 293);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(150, 26);
            txtEmail.TabIndex = 5;
            // 
            // txtDireccion
            // 
            txtDireccion.Font = new Font("Microsoft Sans Serif", 10F);
            txtDireccion.Location = new Point(342, 115);
            txtDireccion.Name = "txtDireccion";
            txtDireccion.Size = new Size(150, 26);
            txtDireccion.TabIndex = 3;
            // 
            // txtDni
            // 
            txtDni.Font = new Font("Microsoft Sans Serif", 10F);
            txtDni.Location = new Point(91, 203);
            txtDni.Name = "txtDni";
            txtDni.Size = new Size(150, 26);
            txtDni.TabIndex = 2;
            txtDni.KeyPress += txtDni_KeyPress;
            // 
            // txtApellido
            // 
            txtApellido.Font = new Font("Microsoft Sans Serif", 10F);
            txtApellido.Location = new Point(91, 157);
            txtApellido.Name = "txtApellido";
            txtApellido.Size = new Size(150, 26);
            txtApellido.TabIndex = 1;
            // 
            // txtNombre
            // 
            txtNombre.Font = new Font("Microsoft Sans Serif", 10F);
            txtNombre.Location = new Point(91, 112);
            txtNombre.Name = "txtNombre";
            txtNombre.Size = new Size(150, 26);
            txtNombre.TabIndex = 0;
            // 
            // lblBuscar
            // 
            lblBuscar.AutoSize = true;
            lblBuscar.Font = new Font("Microsoft Sans Serif", 10F);
            lblBuscar.Location = new Point(19, 68);
            lblBuscar.Name = "lblBuscar";
            lblBuscar.Size = new Size(58, 20);
            lblBuscar.TabIndex = 17;
            lblBuscar.Text = "Filtrar:";
            // 
            // lblEmpleados
            // 
            lblEmpleados.AutoSize = true;
            lblEmpleados.Font = new Font("Microsoft Sans Serif", 11.25F);
            lblEmpleados.Location = new Point(135, 35);
            lblEmpleados.Name = "lblEmpleados";
            lblEmpleados.Size = new Size(127, 24);
            lblEmpleados.TabIndex = 16;
            lblEmpleados.Text = "EMPLEADOS";
            // 
            // lblAñadir
            // 
            lblAñadir.Location = new Point(0, 0);
            lblAñadir.Name = "lblAñadir";
            lblAñadir.Size = new Size(101, 23);
            lblAñadir.TabIndex = 21;
            // 
            // panelEmpleados
            // 
            panelEmpleados.BackColor = Color.WhiteSmoke;
            panelEmpleados.Controls.Add(dgvEmpleados);
            panelEmpleados.Controls.Add(lblAñadir);
            panelEmpleados.Controls.Add(pnlSuperior);
            panelEmpleados.Dock = DockStyle.Fill;
            panelEmpleados.Location = new Point(501, 0);
            panelEmpleados.Name = "panelEmpleados";
            panelEmpleados.Size = new Size(496, 533);
            panelEmpleados.TabIndex = 2;
            // 
            // dgvEmpleados
            // 
            dgvEmpleados.AllowUserToAddRows = false;
            dgvEmpleados.AllowUserToDeleteRows = false;
            dgvEmpleados.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvEmpleados.Columns.AddRange(new DataGridViewColumn[] { Id, Nombre, Apellido, DNI, Telefono, Email, Direccion, Localidad, Provincia, Rol, Clave, Estado });
            dgvEmpleados.Dock = DockStyle.Fill;
            dgvEmpleados.Location = new Point(0, 125);
            dgvEmpleados.Name = "dgvEmpleados";
            dgvEmpleados.ReadOnly = true;
            dgvEmpleados.RowHeadersWidth = 51;
            dgvEmpleados.Size = new Size(496, 408);
            dgvEmpleados.TabIndex = 19;
            dgvEmpleados.DataBindingComplete += dgvEmpleados_DataBindingComplete;
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
            // Nombre
            // 
            Nombre.DataPropertyName = "Nombre";
            Nombre.HeaderText = "Nombre";
            Nombre.MinimumWidth = 6;
            Nombre.Name = "Nombre";
            Nombre.ReadOnly = true;
            Nombre.Width = 125;
            // 
            // Apellido
            // 
            Apellido.DataPropertyName = "Apellido";
            Apellido.HeaderText = "Apellido";
            Apellido.MinimumWidth = 6;
            Apellido.Name = "Apellido";
            Apellido.ReadOnly = true;
            Apellido.Width = 125;
            // 
            // DNI
            // 
            DNI.DataPropertyName = "DNI";
            DNI.HeaderText = "DNI";
            DNI.MinimumWidth = 6;
            DNI.Name = "DNI";
            DNI.ReadOnly = true;
            DNI.Width = 125;
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
            // Provincia
            // 
            Provincia.DataPropertyName = "Provincia";
            Provincia.HeaderText = "Provincia";
            Provincia.MinimumWidth = 6;
            Provincia.Name = "Provincia";
            Provincia.ReadOnly = true;
            Provincia.Width = 125;
            // 
            // Rol
            // 
            Rol.DataPropertyName = "Rol";
            Rol.HeaderText = "Rol";
            Rol.MinimumWidth = 6;
            Rol.Name = "Rol";
            Rol.ReadOnly = true;
            Rol.Width = 125;
            // 
            // Clave
            // 
            Clave.DataPropertyName = "Clave";
            Clave.HeaderText = "Clave";
            Clave.MinimumWidth = 6;
            Clave.Name = "Clave";
            Clave.ReadOnly = true;
            Clave.Width = 125;
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
            // pnlSuperior
            // 
            pnlSuperior.Controls.Add(lblEmpleados);
            pnlSuperior.Controls.Add(txtFiltrarRol);
            pnlSuperior.Controls.Add(lblBuscar);
            pnlSuperior.Controls.Add(txtFiltrarNombre);
            pnlSuperior.Dock = DockStyle.Top;
            pnlSuperior.Location = new Point(0, 0);
            pnlSuperior.Name = "pnlSuperior";
            pnlSuperior.Size = new Size(496, 125);
            pnlSuperior.TabIndex = 22;
            // 
            // txtFiltrarRol
            // 
            txtFiltrarRol.CharacterCasing = CharacterCasing.Upper;
            txtFiltrarRol.Cursor = Cursors.Hand;
            txtFiltrarRol.ForeColor = SystemColors.WindowFrame;
            txtFiltrarRol.Location = new Point(274, 65);
            txtFiltrarRol.Name = "txtFiltrarRol";
            txtFiltrarRol.Size = new Size(159, 27);
            txtFiltrarRol.TabIndex = 20;
            txtFiltrarRol.Text = "ROL";
            txtFiltrarRol.TextChanged += txtFiltrarRol_TextChanged;
            txtFiltrarRol.Enter += txtFiltrarRol_Enter;
            txtFiltrarRol.Leave += txtFiltrarRol_Leave;
            // 
            // txtFiltrarNombre
            // 
            txtFiltrarNombre.CharacterCasing = CharacterCasing.Upper;
            txtFiltrarNombre.Cursor = Cursors.Hand;
            txtFiltrarNombre.ForeColor = SystemColors.WindowFrame;
            txtFiltrarNombre.Location = new Point(83, 65);
            txtFiltrarNombre.Name = "txtFiltrarNombre";
            txtFiltrarNombre.Size = new Size(185, 27);
            txtFiltrarNombre.TabIndex = 18;
            txtFiltrarNombre.Text = "NOMBRE O APELLIDO";
            txtFiltrarNombre.TextChanged += txtFiltrarNombre_TextChanged;
            txtFiltrarNombre.Enter += txtFiltrarNombre_Enter;
            txtFiltrarNombre.Leave += txtFiltrarNombre_Leave;
            // 
            // EmpleadosForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(997, 533);
            Controls.Add(panelEmpleados);
            Controls.Add(panelAñadirEmpleado);
            FormBorderStyle = FormBorderStyle.None;
            Name = "EmpleadosForm";
            Text = "EmpleadoFormulario";
            panelAñadirEmpleado.ResumeLayout(false);
            panelAñadirEmpleado.PerformLayout();
            gbEstado.ResumeLayout(false);
            gbEstado.PerformLayout();
            panelEmpleados.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvEmpleados).EndInit();
            pnlSuperior.ResumeLayout(false);
            pnlSuperior.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)usuarioDatosBindingSource).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelAñadirEmpleado;
        private Label lblTelefono;
        private Label lblLocalidad;
        private Label lblNombre;
        private TextBox txtTelefono;
        private TextBox txtEmail;
        private TextBox txtDireccion;
        private TextBox txtDni;
        private TextBox txtApellido;
        private TextBox txtNombre;
        private Panel panelEmpleados;
        private Label lblNuevosEmpleados;
        private Label lblApellido;
        private Label lblDni;
        private Label lblDireccion;
        private Label lblEmail;
        private Label lblBuscar;
        private Label lblEmpleados;
        private Label lblAñadir;
        private Button btnEliminar;
        private Button btnGuardar;
        private Label lblProvincia;
        private Label lblRol;
        private Label lblClave;
        private ComboBox cmbLocalidad;
        private ComboBox cmbProvincia;
        private GroupBox gbEstado;
        private RadioButton rbInactivo;
        private RadioButton rbActivo;
        private TextBox txtClave;
        private ComboBox cmbRol;
        private DateTimePicker dtFechaNacimiento;
        private Label lblFechaNacimiento;
        private TextBox txtFiltrarNombre;
        private DataGridView dgvEmpleados;
        private BindingSource usuarioDatosBindingSource;
        private TextBox txtFiltrarRol;
        private Panel pnlSuperior;
        private DataGridViewTextBoxColumn Id;
        private DataGridViewTextBoxColumn Nombre;
        private DataGridViewTextBoxColumn Apellido;
        private DataGridViewTextBoxColumn DNI;
        private DataGridViewTextBoxColumn Telefono;
        private DataGridViewTextBoxColumn Email;
        private DataGridViewTextBoxColumn Direccion;
        private DataGridViewTextBoxColumn Localidad;
        private DataGridViewTextBoxColumn Provincia;
        private DataGridViewTextBoxColumn Rol;
        private DataGridViewTextBoxColumn Clave;
        private DataGridViewTextBoxColumn Estado;
        private Button btnLimpiar;
    }
}