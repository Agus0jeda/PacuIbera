namespace PacuIbera.UI.Common
{
    partial class MainForm
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
            pnlEncabezado = new Panel();
            pnlContenedor = new Panel();
            panel1 = new Panel();
            pnlNavegacion = new Panel();
            button5 = new Button();
            button4 = new Button();
            button3 = new Button();
            button2 = new Button();
            button1 = new Button();
            pnlContenedor.SuspendLayout();
            pnlNavegacion.SuspendLayout();
            SuspendLayout();
            // 
            // pnlEncabezado
            // 
            pnlEncabezado.BackColor = SystemColors.ControlDark;
            pnlEncabezado.Dock = DockStyle.Top;
            pnlEncabezado.Location = new Point(0, 0);
            pnlEncabezado.Name = "pnlEncabezado";
            pnlEncabezado.Size = new Size(532, 136);
            pnlEncabezado.TabIndex = 1;
            // 
            // pnlContenedor
            // 
            pnlContenedor.Controls.Add(panel1);
            pnlContenedor.Controls.Add(pnlNavegacion);
            pnlContenedor.Dock = DockStyle.Fill;
            pnlContenedor.Location = new Point(0, 136);
            pnlContenedor.Name = "pnlContenedor";
            pnlContenedor.Padding = new Padding(40);
            pnlContenedor.Size = new Size(532, 402);
            pnlContenedor.TabIndex = 2;
            pnlContenedor.Paint += pnlContenedor_Paint;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.BackColor = Color.FromArgb(128, 255, 128);
            panel1.Location = new Point(73, 159);
            panel1.Name = "panel1";
            panel1.Size = new Size(370, 162);
            panel1.TabIndex = 1;
            // 
            // pnlNavegacion
            // 
            pnlNavegacion.Controls.Add(button5);
            pnlNavegacion.Controls.Add(button4);
            pnlNavegacion.Controls.Add(button3);
            pnlNavegacion.Controls.Add(button2);
            pnlNavegacion.Controls.Add(button1);
            pnlNavegacion.Dock = DockStyle.Top;
            pnlNavegacion.Location = new Point(40, 40);
            pnlNavegacion.Name = "pnlNavegacion";
            pnlNavegacion.Size = new Size(452, 100);
            pnlNavegacion.TabIndex = 0;
            pnlNavegacion.Paint += pnlNavegacion_Paint;
            // 
            // button5
            // 
            button5.FlatAppearance.BorderSize = 0;
            button5.FlatStyle = FlatStyle.Flat;
            button5.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            button5.ForeColor = Color.White;
            button5.Location = new Point(631, 42);
            button5.Name = "button5";
            button5.Size = new Size(75, 23);
            button5.TabIndex = 4;
            button5.Text = "Reportes";
            button5.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            button4.FlatAppearance.BorderSize = 0;
            button4.FlatStyle = FlatStyle.Flat;
            button4.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            button4.ForeColor = Color.White;
            button4.Location = new Point(477, 38);
            button4.Name = "button4";
            button4.Size = new Size(111, 37);
            button4.TabIndex = 3;
            button4.Text = "Clientes";
            button4.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.FlatAppearance.BorderSize = 0;
            button3.FlatStyle = FlatStyle.Flat;
            button3.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            button3.ForeColor = Color.White;
            button3.Location = new Point(337, 42);
            button3.Name = "button3";
            button3.Size = new Size(97, 23);
            button3.TabIndex = 2;
            button3.Text = "Productos";
            button3.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.FlatAppearance.BorderSize = 0;
            button2.FlatStyle = FlatStyle.Flat;
            button2.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            button2.ForeColor = Color.White;
            button2.Location = new Point(185, 36);
            button2.Name = "button2";
            button2.Size = new Size(101, 29);
            button2.TabIndex = 1;
            button2.Text = "Ventas";
            button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            button1.ForeColor = Color.White;
            button1.Location = new Point(48, 36);
            button1.Name = "button1";
            button1.Size = new Size(101, 35);
            button1.TabIndex = 0;
            button1.Text = "Panel";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(532, 538);
            Controls.Add(pnlContenedor);
            Controls.Add(pnlEncabezado);
            Name = "MainForm";
            Text = "Pacú Iberá - Sistema de Gestión";
            WindowState = FormWindowState.Maximized;
            Load += MainForm_Load;
            pnlContenedor.ResumeLayout(false);
            pnlNavegacion.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Panel pnlEncabezado;
        private Panel pnlContenedor;
        private Panel pnlNavegacion;
        private Button button5;
        private Button button4;
        private Button button3;
        private Button button2;
        private Button button1;
        private Panel panel1;
    }
}