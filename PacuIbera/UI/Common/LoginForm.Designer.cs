namespace PacuIbera.UI.Common
{
    partial class PacuIbera_IniciarSesion
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PacuIbera_IniciarSesion));
            txtUsuario = new TextBox();
            txtClave = new TextBox();
            btnIngresar = new Button();
            btnSalir = new Button();
            txt = new Label();
            label1 = new Label();
            Logo = new PictureBox();
            label2 = new Label();
            lblFecha = new Label();
            ((System.ComponentModel.ISupportInitialize)Logo).BeginInit();
            SuspendLayout();
            // 
            // txtUsuario
            // 
            txtUsuario.BorderStyle = BorderStyle.FixedSingle;
            txtUsuario.Font = new Font("Segoe UI", 9.75F);
            txtUsuario.Location = new Point(167, 167);
            txtUsuario.Name = "txtUsuario";
            txtUsuario.Size = new Size(128, 25);
            txtUsuario.TabIndex = 0;
            // 
            // txtClave
            // 
            txtClave.BorderStyle = BorderStyle.FixedSingle;
            txtClave.Font = new Font("Segoe UI", 9.75F);
            txtClave.Location = new Point(166, 221);
            txtClave.Name = "txtClave";
            txtClave.Size = new Size(128, 25);
            txtClave.TabIndex = 1;
            txtClave.UseSystemPasswordChar = true;
            // 
            // btnIngresar
            // 
            btnIngresar.Cursor = Cursors.Hand;
            btnIngresar.FlatAppearance.BorderColor = Color.Lime;
            btnIngresar.FlatStyle = FlatStyle.Flat;
            btnIngresar.Location = new Point(231, 290);
            btnIngresar.Name = "btnIngresar";
            btnIngresar.Size = new Size(75, 23);
            btnIngresar.TabIndex = 2;
            btnIngresar.Text = "Ingresar";
            btnIngresar.UseVisualStyleBackColor = true;
            btnIngresar.Click += btnIngresar_Click;
            // 
            // btnSalir
            // 
            btnSalir.Cursor = Cursors.Hand;
            btnSalir.FlatAppearance.BorderColor = Color.Lime;
            btnSalir.FlatStyle = FlatStyle.Flat;
            btnSalir.Location = new Point(140, 290);
            btnSalir.Name = "btnSalir";
            btnSalir.Size = new Size(75, 23);
            btnSalir.TabIndex = 3;
            btnSalir.Text = "Salir";
            btnSalir.UseVisualStyleBackColor = true;
            btnSalir.Click += btnSalir_Click;
            // 
            // txt
            // 
            txt.AutoSize = true;
            txt.Location = new Point(81, 177);
            txt.Name = "txt";
            txt.Size = new Size(47, 15);
            txt.TabIndex = 4;
            txt.Text = "Usuario";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(74, 231);
            label1.Name = "label1";
            label1.Size = new Size(67, 15);
            label1.TabIndex = 5;
            label1.Text = "Contraseña";
            // 
            // Logo
            // 
            Logo.Image = (Image)resources.GetObject("Logo.Image");
            Logo.Location = new Point(151, -10);
            Logo.Name = "Logo";
            Logo.Size = new Size(155, 130);
            Logo.SizeMode = PictureBoxSizeMode.Zoom;
            Logo.TabIndex = 6;
            Logo.TabStop = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(129, 127);
            label2.Name = "label2";
            label2.Size = new Size(201, 21);
            label2.TabIndex = 7;
            label2.Text = "Bienvenidos a Pacú Iberá";
            // 
            // lblFecha
            // 
            lblFecha.AutoSize = true;
            lblFecha.Location = new Point(375, 318);
            lblFecha.Name = "lblFecha";
            lblFecha.Size = new Size(0, 15);
            lblFecha.TabIndex = 8;
            // 
            // PacuIbera_IniciarSesion
            // 
            AcceptButton = btnIngresar;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnSalir;
            ClientSize = new Size(449, 342);
            Controls.Add(lblFecha);
            Controls.Add(label2);
            Controls.Add(Logo);
            Controls.Add(label1);
            Controls.Add(txt);
            Controls.Add(btnSalir);
            Controls.Add(btnIngresar);
            Controls.Add(txtClave);
            Controls.Add(txtUsuario);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            Name = "PacuIbera_IniciarSesion";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Iniciar Sesion - Pacú Ibera";
            ((System.ComponentModel.ISupportInitialize)Logo).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtUsuario;
        private TextBox txtClave;
        private Button btnIngresar;
        private Button btnSalir;
        private Label txt;
        private Label label1;
        private PictureBox Logo;
        private Label label2;
        private Label lblFecha;
    }
}