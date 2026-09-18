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
            btnCerrar = new Label();
            panelNuevoProducto = new Panel();
            dtpVencimiento = new DateTimePicker();
            btnIngresarStock = new TextBox();
            chkVencimiento = new CheckBox();
            btnNuevoProducto = new Button();
            cmbNombreProducto = new ComboBox();
            label9 = new Label();
            btnNuevaCat = new Button();
            cmbCategoria = new ComboBox();
            chkPorPeso = new CheckBox();
            label8 = new Label();
            nupStockMin = new NumericUpDown();
            nupPrecio = new NumericUpDown();
            btnEliminar = new Button();
            GUARDAR = new Button();
            textBox4 = new TextBox();
            label7 = new Label();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            panel2 = new Panel();
            bucarNombreProducto = new ComboBox();
            buscarCatProd = new ComboBox();
            label6 = new Label();
            panel3 = new Panel();
            label10 = new Label();
            STOCK = new Label();
            listaVencimientos = new ListBox();
            listaStockBajo = new ListBox();
            listView1 = new ListView();
            panelNuevoProducto.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nupStockMin).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nupPrecio).BeginInit();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft Sans Serif", 12F);
            label1.Location = new Point(316, 8);
            label1.Name = "label1";
            label1.Size = new Size(110, 20);
            label1.TabIndex = 6;
            label1.Text = "PRODUCTOS";
            label1.Click += label1_Click;
            // 
            // btnCerrar
            // 
            btnCerrar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCerrar.AutoSize = true;
            btnCerrar.Cursor = Cursors.Hand;
            btnCerrar.Font = new Font("Segoe UI", 11F);
            btnCerrar.ForeColor = SystemColors.ControlDarkDark;
            btnCerrar.Location = new Point(822, 8);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new Size(18, 20);
            btnCerrar.TabIndex = 8;
            btnCerrar.Text = "X";
            btnCerrar.Click += btnCerrar_Click;
            // 
            // panelNuevoProducto
            // 
            panelNuevoProducto.BackColor = Color.WhiteSmoke;
            panelNuevoProducto.Controls.Add(dtpVencimiento);
            panelNuevoProducto.Controls.Add(btnIngresarStock);
            panelNuevoProducto.Controls.Add(chkVencimiento);
            panelNuevoProducto.Controls.Add(btnNuevoProducto);
            panelNuevoProducto.Controls.Add(cmbNombreProducto);
            panelNuevoProducto.Controls.Add(label9);
            panelNuevoProducto.Controls.Add(btnNuevaCat);
            panelNuevoProducto.Controls.Add(cmbCategoria);
            panelNuevoProducto.Controls.Add(chkPorPeso);
            panelNuevoProducto.Controls.Add(label8);
            panelNuevoProducto.Controls.Add(nupStockMin);
            panelNuevoProducto.Controls.Add(nupPrecio);
            panelNuevoProducto.Controls.Add(btnEliminar);
            panelNuevoProducto.Controls.Add(GUARDAR);
            panelNuevoProducto.Controls.Add(textBox4);
            panelNuevoProducto.Controls.Add(label7);
            panelNuevoProducto.Controls.Add(label5);
            panelNuevoProducto.Controls.Add(label4);
            panelNuevoProducto.Controls.Add(label3);
            panelNuevoProducto.Controls.Add(label2);
            panelNuevoProducto.Dock = DockStyle.Left;
            panelNuevoProducto.Font = new Font("Microsoft Sans Serif", 12F);
            panelNuevoProducto.Location = new Point(0, 0);
            panelNuevoProducto.Margin = new Padding(3, 2, 3, 2);
            panelNuevoProducto.Name = "panelNuevoProducto";
            panelNuevoProducto.Size = new Size(306, 426);
            panelNuevoProducto.TabIndex = 9;
            // 
            // dtpVencimiento
            // 
            dtpVencimiento.Format = DateTimePickerFormat.Short;
            dtpVencimiento.Location = new Point(187, 316);
            dtpVencimiento.Name = "dtpVencimiento";
            dtpVencimiento.Size = new Size(100, 26);
            dtpVencimiento.TabIndex = 35;
            // 
            // btnIngresarStock
            // 
            btnIngresarStock.Location = new Point(92, 229);
            btnIngresarStock.Name = "btnIngresarStock";
            btnIngresarStock.Size = new Size(195, 26);
            btnIngresarStock.TabIndex = 34;
            // 
            // chkVencimiento
            // 
            chkVencimiento.AutoSize = true;
            chkVencimiento.Checked = true;
            chkVencimiento.CheckState = CheckState.Checked;
            chkVencimiento.Location = new Point(12, 317);
            chkVencimiento.Name = "chkVencimiento";
            chkVencimiento.Size = new Size(159, 24);
            chkVencimiento.TabIndex = 36;
            chkVencimiento.Text = "Tiene Vencimiento";
            chkVencimiento.UseVisualStyleBackColor = true;
            chkVencimiento.CheckedChanged += chkVencimiento_CheckedChanged;
            // 
            // btnNuevoProducto
            // 
            btnNuevoProducto.Location = new Point(92, 43);
            btnNuevoProducto.Name = "btnNuevoProducto";
            btnNuevoProducto.Size = new Size(28, 28);
            btnNuevoProducto.TabIndex = 33;
            btnNuevoProducto.Text = "+";
            btnNuevoProducto.UseVisualStyleBackColor = true;
            btnNuevoProducto.Click += btnNuevoProducto_Click;
            // 
            // cmbNombreProducto
            // 
            cmbNombreProducto.FormattingEnabled = true;
            cmbNombreProducto.Location = new Point(127, 43);
            cmbNombreProducto.Name = "cmbNombreProducto";
            cmbNombreProducto.Size = new Size(160, 28);
            cmbNombreProducto.TabIndex = 32;
            cmbNombreProducto.SelectedIndexChanged += cmbNombreProducto_SelectedIndexChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Microsoft Sans Serif", 8.999999F);
            label9.Location = new Point(12, 240);
            label9.Name = "label9";
            label9.Size = new Size(40, 15);
            label9.TabIndex = 31;
            label9.Text = "Stock:";
            // 
            // btnNuevaCat
            // 
            btnNuevaCat.Location = new Point(93, 95);
            btnNuevaCat.Name = "btnNuevaCat";
            btnNuevaCat.Size = new Size(28, 28);
            btnNuevaCat.TabIndex = 27;
            btnNuevaCat.Text = "+";
            btnNuevaCat.UseVisualStyleBackColor = true;
            btnNuevaCat.Click += btnNuevaCat_Click;
            // 
            // cmbCategoria
            // 
            cmbCategoria.FormattingEnabled = true;
            cmbCategoria.Location = new Point(127, 95);
            cmbCategoria.Name = "cmbCategoria";
            cmbCategoria.Size = new Size(160, 28);
            cmbCategoria.TabIndex = 26;
            // 
            // chkPorPeso
            // 
            chkPorPeso.AutoSize = true;
            chkPorPeso.Location = new Point(12, 353);
            chkPorPeso.Name = "chkPorPeso";
            chkPorPeso.Size = new Size(161, 24);
            chkPorPeso.TabIndex = 25;
            chkPorPeso.Text = "Se vende por peso";
            chkPorPeso.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Microsoft Sans Serif", 8.999999F);
            label8.Location = new Point(9, 286);
            label8.Name = "label8";
            label8.Size = new Size(64, 15);
            label8.TabIndex = 24;
            label8.Text = "Stock Min:";
            // 
            // nupStockMin
            // 
            nupStockMin.DecimalPlaces = 3;
            nupStockMin.Location = new Point(93, 275);
            nupStockMin.Name = "nupStockMin";
            nupStockMin.Size = new Size(195, 26);
            nupStockMin.TabIndex = 23;
            // 
            // nupPrecio
            // 
            nupPrecio.DecimalPlaces = 2;
            nupPrecio.Location = new Point(93, 138);
            nupPrecio.Margin = new Padding(3, 2, 3, 2);
            nupPrecio.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            nupPrecio.Name = "nupPrecio";
            nupPrecio.Size = new Size(194, 26);
            nupPrecio.TabIndex = 22;
            nupPrecio.TextAlign = HorizontalAlignment.Right;
            nupPrecio.ThousandsSeparator = true;
            nupPrecio.ValueChanged += numericUpDown1_ValueChanged;
            // 
            // btnEliminar
            // 
            btnEliminar.BackColor = Color.Red;
            btnEliminar.Cursor = Cursors.Hand;
            btnEliminar.FlatAppearance.BorderSize = 0;
            btnEliminar.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 192, 192);
            btnEliminar.FlatStyle = FlatStyle.Flat;
            btnEliminar.Font = new Font("Microsoft Sans Serif", 9F);
            btnEliminar.Location = new Point(175, 392);
            btnEliminar.Margin = new Padding(3, 2, 3, 2);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new Size(96, 30);
            btnEliminar.TabIndex = 21;
            btnEliminar.Text = "ELIMINAR";
            btnEliminar.UseVisualStyleBackColor = false;
            // 
            // GUARDAR
            // 
            GUARDAR.BackColor = Color.LightGreen;
            GUARDAR.FlatAppearance.BorderSize = 0;
            GUARDAR.FlatAppearance.MouseOverBackColor = Color.FromArgb(192, 255, 192);
            GUARDAR.FlatStyle = FlatStyle.Flat;
            GUARDAR.Font = new Font("Microsoft Sans Serif", 9F);
            GUARDAR.Location = new Point(43, 392);
            GUARDAR.Margin = new Padding(3, 2, 3, 2);
            GUARDAR.Name = "GUARDAR";
            GUARDAR.Size = new Size(96, 30);
            GUARDAR.TabIndex = 20;
            GUARDAR.Text = "GUARDAR";
            GUARDAR.UseVisualStyleBackColor = false;
            GUARDAR.Click += GUARDAR_Click;
            // 
            // textBox4
            // 
            textBox4.CharacterCasing = CharacterCasing.Upper;
            textBox4.Location = new Point(93, 186);
            textBox4.Margin = new Padding(3, 2, 3, 2);
            textBox4.MaxLength = 100;
            textBox4.Multiline = true;
            textBox4.Name = "textBox4";
            textBox4.ScrollBars = ScrollBars.Vertical;
            textBox4.Size = new Size(195, 26);
            textBox4.TabIndex = 19;
            textBox4.DoubleClick += textBox4_DoubleClick;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Microsoft Sans Serif", 8.999999F);
            label7.Location = new Point(9, 192);
            label7.Name = "label7";
            label7.Size = new Size(81, 15);
            label7.TabIndex = 18;
            label7.Text = "Descripcion  :";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Microsoft Sans Serif", 8.999999F);
            label5.Location = new Point(9, 144);
            label5.Name = "label5";
            label5.Size = new Size(48, 15);
            label5.TabIndex = 15;
            label5.Text = "Precio :";
            label5.Click += label5_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Microsoft Sans Serif", 8.999999F);
            label4.Location = new Point(9, 100);
            label4.Name = "label4";
            label4.Size = new Size(66, 15);
            label4.TabIndex = 14;
            label4.Text = "Categoria :";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Microsoft Sans Serif", 8.999999F);
            label3.Location = new Point(9, 56);
            label3.Name = "label3";
            label3.Size = new Size(58, 15);
            label3.TabIndex = 13;
            label3.Text = "Nombre :";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 12F);
            label2.Location = new Point(70, 6);
            label2.Name = "label2";
            label2.Size = new Size(122, 20);
            label2.TabIndex = 8;
            label2.Text = "Nuevo Producto";
            label2.TextAlign = ContentAlignment.TopCenter;
            label2.Click += label2_Click;
            // 
            // panel2
            // 
            panel2.Controls.Add(bucarNombreProducto);
            panel2.Controls.Add(btnCerrar);
            panel2.Controls.Add(buscarCatProd);
            panel2.Controls.Add(label1);
            panel2.Controls.Add(label6);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(306, 0);
            panel2.Margin = new Padding(3, 2, 3, 2);
            panel2.Name = "panel2";
            panel2.Size = new Size(852, 60);
            panel2.TabIndex = 10;
            panel2.Paint += panel2_Paint;
            // 
            // bucarNombreProducto
            // 
            bucarNombreProducto.Font = new Font("Microsoft Sans Serif", 8.999999F);
            bucarNombreProducto.ForeColor = SystemColors.ButtonShadow;
            bucarNombreProducto.FormattingEnabled = true;
            bucarNombreProducto.Location = new Point(78, 38);
            bucarNombreProducto.Margin = new Padding(3, 2, 3, 2);
            bucarNombreProducto.Name = "bucarNombreProducto";
            bucarNombreProducto.Size = new Size(224, 23);
            bucarNombreProducto.TabIndex = 18;
            bucarNombreProducto.Text = "Nombre del Producto";
            // 
            // buscarCatProd
            // 
            buscarCatProd.Font = new Font("Microsoft Sans Serif", 8.999999F);
            buscarCatProd.ForeColor = SystemColors.ButtonShadow;
            buscarCatProd.FormattingEnabled = true;
            buscarCatProd.Location = new Point(326, 38);
            buscarCatProd.Margin = new Padding(3, 2, 3, 2);
            buscarCatProd.Name = "buscarCatProd";
            buscarCatProd.Size = new Size(224, 23);
            buscarCatProd.TabIndex = 17;
            buscarCatProd.Text = "Categoria";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Microsoft Sans Serif", 8.999999F);
            label6.Location = new Point(14, 39);
            label6.Name = "label6";
            label6.Size = new Size(38, 15);
            label6.TabIndex = 16;
            label6.Text = "Filtrar";
            // 
            // panel3
            // 
            panel3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel3.Controls.Add(label10);
            panel3.Controls.Add(STOCK);
            panel3.Controls.Add(listaVencimientos);
            panel3.Controls.Add(listaStockBajo);
            panel3.Controls.Add(listView1);
            panel3.Location = new Point(306, 60);
            panel3.Margin = new Padding(3, 2, 3, 2);
            panel3.Name = "panel3";
            panel3.Size = new Size(852, 366);
            panel3.TabIndex = 11;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label10.Location = new Point(594, 238);
            label10.Name = "label10";
            label10.Size = new Size(128, 15);
            label10.TabIndex = 4;
            label10.Text = "PRÓXIMOS A VENCER";
            label10.TextAlign = ContentAlignment.TopCenter;
            // 
            // STOCK
            // 
            STOCK.AutoSize = true;
            STOCK.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            STOCK.Location = new Point(168, 238);
            STOCK.Name = "STOCK";
            STOCK.Size = new Size(78, 15);
            STOCK.TabIndex = 3;
            STOCK.Text = "STOCK BAJO";
            STOCK.TextAlign = ContentAlignment.TopCenter;
            STOCK.Click += label10_Click;
            // 
            // listaVencimientos
            // 
            listaVencimientos.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            listaVencimientos.FormattingEnabled = true;
            listaVencimientos.ItemHeight = 15;
            listaVencimientos.Location = new Point(453, 256);
            listaVencimientos.Name = "listaVencimientos";
            listaVencimientos.Size = new Size(387, 94);
            listaVencimientos.TabIndex = 2;
            listaVencimientos.Tag = "";
            // 
            // listaStockBajo
            // 
            listaStockBajo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            listaStockBajo.FormattingEnabled = true;
            listaStockBajo.ItemHeight = 15;
            listaStockBajo.Location = new Point(14, 256);
            listaStockBajo.Name = "listaStockBajo";
            listaStockBajo.Size = new Size(387, 94);
            listaStockBajo.TabIndex = 1;
            // 
            // listView1
            // 
            listView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listView1.CausesValidation = false;
            listView1.Font = new Font("Microsoft Sans Serif", 8.999999F);
            listView1.Location = new Point(14, 34);
            listView1.Margin = new Padding(3, 2, 3, 2);
            listView1.Name = "listView1";
            listView1.Size = new Size(826, 161);
            listView1.TabIndex = 0;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.DoubleClick += listView1_DoubleClick_1;
            // 
            // ProductoForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1158, 426);
            Controls.Add(panel3);
            Controls.Add(panel2);
            Controls.Add(panelNuevoProducto);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(3, 2, 3, 2);
            Name = "ProductoForm";
            Text = "ProductoForm";
            panelNuevoProducto.ResumeLayout(false);
            panelNuevoProducto.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nupStockMin).EndInit();
            ((System.ComponentModel.ISupportInitialize)nupPrecio).EndInit();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Label label1;
        private Label btnCerrar;
        private Panel panelNuevoProducto;
        private Label label2;
        private Panel panel2;
        private Panel panel3;
        private Label label6;
        private Label label5;
        private Label label4;
        private Label label3;
        private ListView listView1;
        private ComboBox buscarCatProd;
        private ComboBox bucarNombreProducto;
        private TextBox textBox4;
        private Label label7;
        private Button GUARDAR;
        private Button btnEliminar;
        private NumericUpDown nupPrecio;
        private CheckBox chkPorPeso;
        private Label label8;
        private NumericUpDown nupStockMin;
        private Button btnNuevaCat;
        private ComboBox cmbCategoria;
        private Label label9;
        private Button btnNuevoProducto;
        private ComboBox cmbNombreProducto;
        private TextBox btnIngresarStock;
        private CheckBox chkVencimiento;
        private DateTimePicker dtpVencimiento;
        private ListBox listaStockBajo;
        private ListBox listaVencimientos;
        private Label STOCK;
        private Label label10;
    }
}