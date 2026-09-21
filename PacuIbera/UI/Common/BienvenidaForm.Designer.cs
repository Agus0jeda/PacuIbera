namespace PacuIbera.UI.Common
{
    partial class BienvenidaForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BienvenidaForm));
            lblBienvenida = new Label();
            Logo = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)Logo).BeginInit();
            SuspendLayout();
            // 
            // lblBienvenida
            // 
            lblBienvenida.AutoSize = true;
            lblBienvenida.Font = new Font("Microsoft Sans Serif", 24F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblBienvenida.Location = new Point(267, 40);
            lblBienvenida.Name = "lblBienvenida";
            lblBienvenida.Size = new Size(183, 37);
            lblBienvenida.TabIndex = 1;
            lblBienvenida.Text = "Bienvenida";
            // 
            // Logo
            // 
            Logo.Image = (Image)resources.GetObject("Logo.Image");
            Logo.Location = new Point(199, 78);
            Logo.Name = "Logo";
            Logo.Size = new Size(271, 193);
            Logo.SizeMode = PictureBoxSizeMode.Zoom;
            Logo.TabIndex = 7;
            Logo.TabStop = false;
            // 
            // BienvenidaForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(700, 338);
            Controls.Add(Logo);
            Controls.Add(lblBienvenida);
            Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(3, 2, 3, 2);
            Name = "BienvenidaForm";
            Text = "BienvenidaForm";
            Load += BienvenidaForm_Load;
            ((System.ComponentModel.ISupportInitialize)Logo).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private Label lblBienvenida;
        private PictureBox Logo;
    }
}