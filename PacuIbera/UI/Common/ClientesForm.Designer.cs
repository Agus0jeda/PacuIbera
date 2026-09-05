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
            panel1 = new Panel();
            label1 = new Label();
            label2 = new Label();
            textBox1 = new TextBox();
            textBox2 = new TextBox();
            textBox3 = new TextBox();
            textBox4 = new TextBox();
            textBox5 = new TextBox();
            GUARDAR = new Button();
            btnCancelarCliente = new Button();
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
            nuevoCliente.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)usuarioDatosBindingSource).BeginInit();
            SuspendLayout();
            // 
            // nuevoCliente
            // 
            nuevoCliente.BackColor = Color.WhiteSmoke;
            nuevoCliente.Controls.Add(btnCancelarCliente);
            nuevoCliente.Controls.Add(GUARDAR);
            nuevoCliente.Controls.Add(textBox5);
            nuevoCliente.Controls.Add(textBox4);
            nuevoCliente.Controls.Add(textBox3);
            nuevoCliente.Controls.Add(textBox2);
            nuevoCliente.Controls.Add(textBox1);
            nuevoCliente.Controls.Add(label2);
            nuevoCliente.Dock = DockStyle.Left;
            nuevoCliente.Location = new Point(0, 0);
            nuevoCliente.Name = "nuevoCliente";
            nuevoCliente.Size = new Size(250, 534);
            nuevoCliente.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Controls.Add(label1);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(250, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(747, 50);
            panel1.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft Sans Serif", 12F);
            label1.Location = new Point(313, 9);
            label1.Name = "label1";
            label1.Size = new Size(110, 25);
            label1.TabIndex = 0;
            label1.Text = "CLIENTES";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 12F);
            label2.Location = new Point(56, 49);
            label2.Name = "label2";
            label2.Size = new Size(135, 25);
            label2.TabIndex = 0;
            label2.Text = "Nuevo Cliente";
            // 
            // textBox1
            // 
            textBox1.Font = new Font("Microsoft Sans Serif", 9F);
            textBox1.Location = new Point(12, 98);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(220, 24);
            textBox1.TabIndex = 6;
            textBox1.Text = "Nombre";
            // 
            // textBox2
            // 
            textBox2.Font = new Font("Microsoft Sans Serif", 9F);
            textBox2.Location = new Point(12, 191);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(220, 24);
            textBox2.TabIndex = 7;
            textBox2.Text = "DNI";
            // 
            // textBox3
            // 
            textBox3.Font = new Font("Microsoft Sans Serif", 9F);
            textBox3.Location = new Point(12, 144);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(220, 24);
            textBox3.TabIndex = 8;
            textBox3.Text = "Apellido";
            // 
            // textBox4
            // 
            textBox4.Font = new Font("Microsoft Sans Serif", 9F);
            textBox4.Location = new Point(12, 286);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(220, 24);
            textBox4.TabIndex = 9;
            textBox4.Text = "Localidad";
            // 
            // textBox5
            // 
            textBox5.Font = new Font("Microsoft Sans Serif", 9F);
            textBox5.Location = new Point(12, 236);
            textBox5.Name = "textBox5";
            textBox5.Size = new Size(220, 24);
            textBox5.TabIndex = 10;
            textBox5.Text = "Provincia";
            // 
            // GUARDAR
            // 
            GUARDAR.BackColor = Color.LightGreen;
            GUARDAR.FlatAppearance.BorderSize = 0;
            GUARDAR.FlatAppearance.MouseOverBackColor = Color.FromArgb(192, 255, 192);
            GUARDAR.FlatStyle = FlatStyle.Flat;
            GUARDAR.Font = new Font("Microsoft Sans Serif", 9F);
            GUARDAR.Location = new Point(56, 374);
            GUARDAR.Name = "GUARDAR";
            GUARDAR.Size = new Size(110, 40);
            GUARDAR.TabIndex = 11;
            GUARDAR.Text = "GUARDAR";
            GUARDAR.UseVisualStyleBackColor = false;
            // 
            // btnCancelarCliente
            // 
            btnCancelarCliente.BackColor = Color.Red;
            btnCancelarCliente.Cursor = Cursors.Hand;
            btnCancelarCliente.FlatAppearance.BorderSize = 0;
            btnCancelarCliente.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 192, 192);
            btnCancelarCliente.FlatStyle = FlatStyle.Flat;
            btnCancelarCliente.Font = new Font("Microsoft Sans Serif", 9F);
            btnCancelarCliente.Location = new Point(56, 430);
            btnCancelarCliente.Name = "btnCancelarCliente";
            btnCancelarCliente.Size = new Size(110, 40);
            btnCancelarCliente.TabIndex = 12;
            btnCancelarCliente.Text = "CANCELAR";
            btnCancelarCliente.UseVisualStyleBackColor = false;
            // 
            // dataGridView1
            // 
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { id, Nombre, Column1, DNI, Provincia, Localidad, Rol, Estado });
            dataGridView1.DataSource = usuarioDatosBindingSource;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Location = new Point(250, 50);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.Size = new Size(747, 484);
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
        private Label label1;
        private TextBox textBox5;
        private TextBox textBox4;
        private TextBox textBox3;
        private TextBox textBox2;
        private TextBox textBox1;
        private Label label2;
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
    }
}