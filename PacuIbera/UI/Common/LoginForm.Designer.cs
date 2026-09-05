namespace PacuIbera.UI.Common
{
    partial class PacuIbera_IniciarSesion
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

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
            txtUsuario.Location = new Point(189, 223);
            txtUsuario.Margin = new Padding(3, 4, 3, 4);
            txtUsuario.Name = "txtUsuario";
            txtUsuario.Size = new Size(146, 29);
            txtUsuario.TabIndex = 0;
            // 
            // txtClave
            // 
            txtClave.BorderStyle = BorderStyle.FixedSingle;
            txtClave.Font = new Font("Segoe UI", 9.75F);
            txtClave.Location = new Point(189, 295);
            txtClave.Margin = new Padding(3, 4, 3, 4);
            txtClave.Name = "txtClave";
            txtClave.Size = new Size(146, 29);
            txtClave.TabIndex = 1;
            txtClave.UseSystemPasswordChar = true;
            // 
            // btnIngresar
            // 
            btnIngresar.Cursor = Cursors.Hand;
            btnIngresar.FlatAppearance.BorderColor = Color.Lime;
            btnIngresar.FlatStyle = FlatStyle.Flat;
            btnIngresar.Location = new Point(280, 387);
            btnIngresar.Margin = new Padding(3, 4, 3, 4);
            btnIngresar.Name = "btnIngresar";
            btnIngresar.Size = new Size(86, 31);
            btnIngresar.TabIndex = 2;
            btnIngresar.Text = "Ingresar";
            btnIngresar.UseVisualStyleBackColor = true;
            btnIngresar.Click += btnIngresar_Click;
            // 
            // btnSalir
            // 
            btnSalir.FlatAppearance.BorderColor = Color.Lime;
            btnSalir.FlatStyle = FlatStyle.Flat;
            btnSalir.Location = new Point(147, 387);
            btnSalir.Margin = new Padding(3, 4, 3, 4);
            btnSalir.Name = "btnSalir";
            btnSalir.Size = new Size(86, 31);
            btnSalir.TabIndex = 3;
            btnSalir.Text = "Salir";
            btnSalir.UseVisualStyleBackColor = true;
            btnSalir.Click += btnSalir_Click;
            // 
            // txt
            // 
            txt.AutoSize = true;
            txt.Location = new Point(77, 236);
            txt.Name = "txt";
            txt.Size = new Size(59, 20);
            txt.TabIndex = 4;
            txt.Text = "Usuario";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(77, 308);
            label1.Name = "label1";
            label1.Size = new Size(83, 20);
            label1.TabIndex = 5;
            label1.Text = "Contraseña";
            // 
            // Logo
            // 
            Logo.Image = (Image)resources.GetObject("Logo.Image");
            Logo.Location = new Point(174, -13);
            Logo.Margin = new Padding(3, 4, 3, 4);
            Logo.Name = "Logo.Image";
            Logo.Size = new Size(177, 173);
            Logo.SizeMode = PictureBoxSizeMode.Zoom;
            Logo.TabIndex = 6;
            Logo.TabStop = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(147, 169);
            label2.Name = "label2";
            label2.Size = new Size(249, 28);
            label2.TabIndex = 7;
            label2.Text = "Bienvenidos a Pacú Iberá";
            // 
            // lblFecha
            // 
            lblFecha.AutoSize = true;
            lblFecha.Location = new Point(429, 424);
            lblFecha.Name = "lblFecha";
            lblFecha.Size = new Size(0, 20);
            lblFecha.TabIndex = 8;
            // 
            // PacuIbera_IniciarSesion
            // 
            AcceptButton = btnIngresar;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnSalir;
            ClientSize = new Size(513, 456);
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
            Margin = new Padding(3, 4, 3, 4);
            MinimizeBox = false;
            Name = "PacuIbera_IniciarSesion";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Iniciar Sesion - Pacú Ibera";
            Load += PacuIbera_IniciarSesion_Load;
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