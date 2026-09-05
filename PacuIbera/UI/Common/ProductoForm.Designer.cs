namespace PacuIbera.UI.Common
{
    partial class ProductoForm
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
            label1 = new Label();
            btnGuardar = new Button();
            btnCerrar = new Label();
            panel1 = new Panel();
            panel2 = new Panel();
            panel3 = new Panel();
            label2 = new Label();
            textBox1 = new TextBox();
            textBox2 = new TextBox();
            textBox3 = new TextBox();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            btnCancelar = new Button();
            listView1 = new ListView();
            buscarCatProd = new ComboBox();
            bucarNombreProducto = new ComboBox();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Sans Serif Collection", 13F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(413, 8);
            label1.Name = "label1";
            label1.Size = new Size(172, 75);
            label1.TabIndex = 6;
            label1.Text = "PRODUCTOS";
            label1.Click += label1_Click;
            // 
            // btnGuardar
            // 
            btnGuardar.BackColor = Color.LightGreen;
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.FlatAppearance.MouseOverBackColor = Color.WhiteSmoke;
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.Font = new Font("Century Gothic", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnGuardar.ForeColor = Color.Black;
            btnGuardar.ImageAlign = ContentAlignment.MiddleLeft;
            btnGuardar.Location = new Point(39, 444);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(143, 27);
            btnGuardar.TabIndex = 7;
            btnGuardar.Text = "GUARDAR";
            btnGuardar.UseVisualStyleBackColor = false;
            // 
            // btnCerrar
            // 
            btnCerrar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCerrar.AutoSize = true;
            btnCerrar.Cursor = Cursors.Hand;
            btnCerrar.Font = new Font("Segoe UI", 11F);
            btnCerrar.ForeColor = SystemColors.ControlDarkDark;
            btnCerrar.Location = new Point(1291, 9);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new Size(23, 25);
            btnCerrar.TabIndex = 8;
            btnCerrar.Text = "X";
            btnCerrar.Click += btnCerrar_Click;
            // 
            // panel1
            // 
            panel1.BackColor = Color.WhiteSmoke;
            panel1.Controls.Add(btnCancelar);
            panel1.Controls.Add(btnGuardar);
            panel1.Controls.Add(label5);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(textBox3);
            panel1.Controls.Add(textBox2);
            panel1.Controls.Add(textBox1);
            panel1.Controls.Add(label2);
            panel1.Dock = DockStyle.Left;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(250, 568);
            panel1.TabIndex = 9;
            // 
            // panel2
            // 
            panel2.Controls.Add(bucarNombreProducto);
            panel2.Controls.Add(buscarCatProd);
            panel2.Controls.Add(label1);
            panel2.Controls.Add(label6);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(250, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(1073, 83);
            panel2.TabIndex = 10;
            panel2.Paint += panel2_Paint;
            // 
            // panel3
            // 
            panel3.Controls.Add(listView1);
            panel3.Dock = DockStyle.Fill;
            panel3.Location = new Point(250, 83);
            panel3.Name = "panel3";
            panel3.Size = new Size(1073, 485);
            panel3.TabIndex = 11;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("SansSerif", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 2);
            label2.Location = new Point(57, 13);
            label2.Name = "label2";
            label2.Size = new Size(139, 21);
            label2.TabIndex = 8;
            label2.Text = "Nuevo Producto";
            label2.TextAlign = ContentAlignment.TopCenter;
            label2.Click += label2_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(12, 96);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(232, 27);
            textBox1.TabIndex = 9;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(12, 182);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(232, 27);
            textBox2.TabIndex = 10;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(12, 265);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(232, 27);
            textBox3.TabIndex = 11;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 63);
            label3.Name = "label3";
            label3.Size = new Size(64, 20);
            label3.TabIndex = 13;
            label3.Text = "Nombre";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 145);
            label4.Name = "label4";
            label4.Size = new Size(74, 20);
            label4.TabIndex = 14;
            label4.Text = "Categoria";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(12, 231);
            label5.Name = "label5";
            label5.Size = new Size(112, 20);
            label5.TabIndex = 15;
            label5.Text = "Precio de Venta";
            label5.Click += label5_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(16, 52);
            label6.Name = "label6";
            label6.Size = new Size(47, 20);
            label6.TabIndex = 16;
            label6.Text = "Filtrar";
            // 
            // btnCancelar
            // 
            btnCancelar.BackColor = Color.Red;
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.FlatAppearance.MouseOverBackColor = Color.WhiteSmoke;
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.Font = new Font("Century Gothic", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCancelar.ForeColor = Color.Black;
            btnCancelar.ImageAlign = ContentAlignment.MiddleLeft;
            btnCancelar.Location = new Point(39, 486);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(143, 27);
            btnCancelar.TabIndex = 17;
            btnCancelar.Text = "CANCELAR";
            btnCancelar.UseVisualStyleBackColor = false;
            // 
            // listView1
            // 
            listView1.Location = new Point(89, 76);
            listView1.Name = "listView1";
            listView1.Size = new Size(719, 291);
            listView1.TabIndex = 0;
            listView1.UseCompatibleStateImageBehavior = false;
            // 
            // buscarCatProd
            // 
            buscarCatProd.Font = new Font("Century Gothic", 7F, FontStyle.Regular, GraphicsUnit.Point, 0);
            buscarCatProd.ForeColor = SystemColors.ButtonShadow;
            buscarCatProd.FormattingEnabled = true;
            buscarCatProd.Location = new Point(381, 52);
            buscarCatProd.Name = "buscarCatProd";
            buscarCatProd.Size = new Size(255, 25);
            buscarCatProd.TabIndex = 17;
            buscarCatProd.Text = "Categoria";
            // 
            // bucarNombreProducto
            // 
            bucarNombreProducto.Font = new Font("Century Gothic", 7F, FontStyle.Regular, GraphicsUnit.Point, 0);
            bucarNombreProducto.ForeColor = SystemColors.ButtonShadow;
            bucarNombreProducto.FormattingEnabled = true;
            bucarNombreProducto.Location = new Point(109, 52);
            bucarNombreProducto.Name = "bucarNombreProducto";
            bucarNombreProducto.Size = new Size(255, 25);
            bucarNombreProducto.TabIndex = 18;
            bucarNombreProducto.Text = "Nombre del Producto";
            // 
            // ProductoForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1323, 568);
            Controls.Add(panel3);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Controls.Add(btnCerrar);
            FormBorderStyle = FormBorderStyle.None;
            Name = "ProductoForm";
            Text = "ProductoForm";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel3.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label1;
        private Button btnGuardar;
        private Label btnCerrar;
        private Panel panel1;
        private Label label2;
        private Panel panel2;
        private Panel panel3;
        private Label label6;
        private Label label5;
        private Label label4;
        private Label label3;
        private TextBox textBox3;
        private TextBox textBox2;
        private TextBox textBox1;
        private Button btnCancelar;
        private ListView listView1;
        private ComboBox buscarCatProd;
        private ComboBox bucarNombreProducto;
    }
}