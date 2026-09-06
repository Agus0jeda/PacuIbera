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
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            nuevoCliente = new Panel();
            lblLocCliente = new Label();
            lblProvCliente = new Label();
            lblDniCliente = new Label();
            lblApellidoCliente = new Label();
            lblNombreCliente = new Label();
            btnCancelarCliente = new Button();
            GUARDAR = new Button();
            txtProvCliente = new TextBox();
            txtLocCliente = new TextBox();
            txtApellidoCliente = new TextBox();
            txtDniCliente = new TextBox();
            txtNombreCliente = new TextBox();
            lblNuevoCliente = new Label();
            panel1 = new Panel();
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
            lblDireccionCliente = new Label();
            txtDireccionCliente = new TextBox();
            nuevoCliente.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)usuarioDatosBindingSource).BeginInit();
            SuspendLayout();
            // 
            // nuevoCliente
            // 
            nuevoCliente.BackColor = Color.WhiteSmoke;
            nuevoCliente.Controls.Add(lblDireccionCliente);
            nuevoCliente.Controls.Add(txtDireccionCliente);
            nuevoCliente.Controls.Add(lblLocCliente);
            nuevoCliente.Controls.Add(lblProvCliente);
            nuevoCliente.Controls.Add(lblDniCliente);
            nuevoCliente.Controls.Add(lblApellidoCliente);
            nuevoCliente.Controls.Add(lblNombreCliente);
            nuevoCliente.Controls.Add(btnCancelarCliente);
            nuevoCliente.Controls.Add(GUARDAR);
            nuevoCliente.Controls.Add(txtProvCliente);
            nuevoCliente.Controls.Add(txtLocCliente);
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
            nuevoCliente.Paint += this.nuevoCliente_Paint;
            // 
            // lblLocCliente
            // 
            lblLocCliente.AutoSize = true;
            lblLocCliente.Font = new Font("SansSerif", 9F);
            lblLocCliente.Location = new Point(22, 385);
            lblLocCliente.Name = "lblLocCliente";
            lblLocCliente.Size = new Size(78, 17);
            lblLocCliente.TabIndex = 17;
            lblLocCliente.Text = "Localidad :";
            // 
            // lblProvCliente
            // 
            lblProvCliente.AutoSize = true;
            lblProvCliente.Font = new Font("SansSerif", 9F);
            lblProvCliente.Location = new Point(22, 333);
            lblProvCliente.Name = "lblProvCliente";
            lblProvCliente.Size = new Size(77, 17);
            lblProvCliente.TabIndex = 16;
            lblProvCliente.Text = "Provincia :";
            lblProvCliente.Click += this.lblProvCliente_Click;
            // 
            // lblDniCliente
            // 
            lblDniCliente.AutoSize = true;
            lblDniCliente.Font = new Font("SansSerif", 9F);
            lblDniCliente.Location = new Point(22, 245);
            lblDniCliente.Name = "lblDniCliente";
            lblDniCliente.Size = new Size(42, 17);
            lblDniCliente.TabIndex = 15;
            lblDniCliente.Text = "DNI :";
            // 
            // lblApellidoCliente
            // 
            lblApellidoCliente.AutoSize = true;
            lblApellidoCliente.Font = new Font("SansSerif", 9F);
            lblApellidoCliente.Location = new Point(22, 202);
            lblApellidoCliente.Name = "lblApellidoCliente";
            lblApellidoCliente.Size = new Size(67, 17);
            lblApellidoCliente.TabIndex = 14;
            lblApellidoCliente.Text = "Apellido :";
            // 
            // lblNombreCliente
            // 
            lblNombreCliente.AutoSize = true;
            lblNombreCliente.Font = new Font("SansSerif", 9F);
            lblNombreCliente.Location = new Point(22, 154);
            lblNombreCliente.Name = "lblNombreCliente";
            lblNombreCliente.Size = new Size(68, 17);
            lblNombreCliente.TabIndex = 13;
            lblNombreCliente.Text = "Nombre :";
            lblNombreCliente.Click += label3_Click;
            // 
            // btnCancelarCliente
            // 
            btnCancelarCliente.BackColor = Color.Red;
            btnCancelarCliente.Cursor = Cursors.Hand;
            btnCancelarCliente.FlatAppearance.BorderSize = 0;
            btnCancelarCliente.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 192, 192);
            btnCancelarCliente.FlatStyle = FlatStyle.Flat;
            btnCancelarCliente.Font = new Font("Microsoft Sans Serif", 9F);
            btnCancelarCliente.Location = new Point(211, 462);
            btnCancelarCliente.Name = "btnCancelarCliente";
            btnCancelarCliente.Size = new Size(110, 40);
            btnCancelarCliente.TabIndex = 12;
            btnCancelarCliente.Text = "CANCELAR";
            btnCancelarCliente.UseVisualStyleBackColor = false;
            // 
            // GUARDAR
            // 
            GUARDAR.BackColor = Color.LightGreen;
            GUARDAR.FlatAppearance.BorderSize = 0;
            GUARDAR.FlatAppearance.MouseOverBackColor = Color.FromArgb(192, 255, 192);
            GUARDAR.FlatStyle = FlatStyle.Flat;
            GUARDAR.Font = new Font("Microsoft Sans Serif", 9F);
            GUARDAR.Location = new Point(22, 462);
            GUARDAR.Name = "GUARDAR";
            GUARDAR.Size = new Size(110, 40);
            GUARDAR.TabIndex = 11;
            GUARDAR.Text = "GUARDAR";
            GUARDAR.UseVisualStyleBackColor = false;
            // 
            // txtProvCliente
            // 
            txtProvCliente.CharacterCasing = CharacterCasing.Upper;
            txtProvCliente.Font = new Font("Microsoft Sans Serif", 9F);
            txtProvCliente.Location = new Point(99, 329);
            txtProvCliente.MaxLength = 100;
            txtProvCliente.Name = "txtProvCliente";
            txtProvCliente.Size = new Size(222, 24);
            txtProvCliente.TabIndex = 10;
            txtProvCliente.TextChanged += this.txtProvCliente_TextChanged;
            // 
            // txtLocCliente
            // 
            txtLocCliente.CharacterCasing = CharacterCasing.Upper;
            txtLocCliente.Font = new Font("Microsoft Sans Serif", 9F);
            txtLocCliente.Location = new Point(99, 381);
            txtLocCliente.Name = "txtLocCliente";
            txtLocCliente.Size = new Size(222, 24);
            txtLocCliente.TabIndex = 9;
            // 
            // txtApellidoCliente
            // 
            txtApellidoCliente.CharacterCasing = CharacterCasing.Upper;
            txtApellidoCliente.Font = new Font("Microsoft Sans Serif", 9F);
            txtApellidoCliente.Location = new Point(99, 198);
            txtApellidoCliente.MaxLength = 100;
            txtApellidoCliente.Name = "txtApellidoCliente";
            txtApellidoCliente.Size = new Size(222, 24);
            txtApellidoCliente.TabIndex = 8;
            // 
            // txtDniCliente
            // 
            txtDniCliente.CharacterCasing = CharacterCasing.Upper;
            txtDniCliente.Font = new Font("Microsoft Sans Serif", 9F);
            txtDniCliente.Location = new Point(99, 241);
            txtDniCliente.MaxLength = 100;
            txtDniCliente.Name = "txtDniCliente";
            txtDniCliente.Size = new Size(222, 24);
            txtDniCliente.TabIndex = 7;
            // 
            // txtNombreCliente
            // 
            txtNombreCliente.CharacterCasing = CharacterCasing.Upper;
            txtNombreCliente.Font = new Font("Microsoft Sans Serif", 9F);
            txtNombreCliente.Location = new Point(99, 150);
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
            lblNuevoCliente.Location = new Point(99, 70);
            lblNuevoCliente.Name = "lblNuevoCliente";
            lblNuevoCliente.Size = new Size(134, 23);
            lblNuevoCliente.TabIndex = 0;
            lblNuevoCliente.Text = "Nuevo Cliente";
            // 
            // panel1
            // 
            panel1.Controls.Add(btnCerrar);
            panel1.Controls.Add(lblTituloCliente);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(350, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(647, 70);
            panel1.TabIndex = 1;
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
            lblTituloCliente.Location = new Point(257, 20);
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
            dataGridView1.Location = new Point(350, 70);
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
            // lblDireccionCliente
            // 
            lblDireccionCliente.AutoSize = true;
            lblDireccionCliente.Font = new Font("SansSerif", 9F);
            lblDireccionCliente.Location = new Point(22, 291);
            lblDireccionCliente.Name = "lblDireccionCliente";
            lblDireccionCliente.Size = new Size(78, 17);
            lblDireccionCliente.TabIndex = 18;
            lblDireccionCliente.Text = "Direccion :";
            // 
            // txtDireccionCliente
            // 
            txtDireccionCliente.CharacterCasing = CharacterCasing.Upper;
            txtDireccionCliente.Font = new Font("Microsoft Sans Serif", 9F);
            txtDireccionCliente.Location = new Point(99, 287);
            txtDireccionCliente.MaxLength = 100;
            txtDireccionCliente.Multiline = true;
            txtDireccionCliente.Name = "txtDireccionCliente";
            txtDireccionCliente.Size = new Size(222, 27);
            txtDireccionCliente.TabIndex = 17;
            // 
            // ClientesForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(997, 534);
            Controls.Add(dataGridView1);
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
            ResumeLayout(false);
        }

        #endregion

        private Panel nuevoCliente;
        private Panel panel1;
        private Label lblTituloCliente;
        private TextBox txtProvCliente;
        private TextBox txtLocCliente;
        private TextBox txtApellidoCliente;
        private TextBox txtDniCliente;
        private TextBox txtNombreCliente;
        private Label lblNuevoCliente;
        private Button btnCancelarCliente;
        private Button GUARDAR;
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
    }
}