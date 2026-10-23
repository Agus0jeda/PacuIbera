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
            dgvProductos = new ListView();
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
            label1.Location = new Point(361, 11);
            label1.Name = "label1";
            label1.Size = new Size(140, 25);
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
            btnCerrar.Location = new Point(938, 11);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new Size(23, 25);
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
            panelNuevoProducto.Name = "panelNuevoProducto";
            panelNuevoProducto.Size = new Size(350, 568);
            panelNuevoProducto.TabIndex = 9;
            // 
            // dtpVencimiento
            // 
            dtpVencimiento.Format = DateTimePickerFormat.Short;
            dtpVencimiento.Location = new Point(214, 421);
            dtpVencimiento.Margin = new Padding(3, 4, 3, 4);
            dtpVencimiento.Name = "dtpVencimiento";
            dtpVencimiento.Size = new Size(114, 30);
            dtpVencimiento.TabIndex = 35;
            // 
            // btnIngresarStock
            // 
            btnIngresarStock.Location = new Point(105, 305);
            btnIngresarStock.Margin = new Padding(3, 4, 3, 4);
            btnIngresarStock.Name = "btnIngresarStock";
            btnIngresarStock.Size = new Size(222, 30);
            btnIngresarStock.TabIndex = 34;
            // 
            // chkVencimiento
            // 
            chkVencimiento.AutoSize = true;
            chkVencimiento.Checked = true;
            chkVencimiento.CheckState = CheckState.Checked;
            chkVencimiento.Location = new Point(14, 423);
            chkVencimiento.Margin = new Padding(3, 4, 3, 4);
            chkVencimiento.Name = "chkVencimiento";
            chkVencimiento.Size = new Size(197, 29);
            chkVencimiento.TabIndex = 36;
            chkVencimiento.Text = "Tiene Vencimiento";
            chkVencimiento.UseVisualStyleBackColor = true;
            chkVencimiento.CheckedChanged += chkVencimiento_CheckedChanged;
            // 
            // btnNuevoProducto
            // 
            btnNuevoProducto.Location = new Point(105, 57);
            btnNuevoProducto.Margin = new Padding(3, 4, 3, 4);
            btnNuevoProducto.Name = "btnNuevoProducto";
            btnNuevoProducto.Size = new Size(32, 37);
            btnNuevoProducto.TabIndex = 33;
            btnNuevoProducto.Text = "+";
            btnNuevoProducto.UseVisualStyleBackColor = true;
            btnNuevoProducto.Click += btnNuevoProducto_Click;
            // 
            // cmbNombreProducto
            // 
            cmbNombreProducto.FormattingEnabled = true;
            cmbNombreProducto.Location = new Point(145, 57);
            cmbNombreProducto.Margin = new Padding(3, 4, 3, 4);
            cmbNombreProducto.Name = "cmbNombreProducto";
            cmbNombreProducto.Size = new Size(182, 33);
            cmbNombreProducto.TabIndex = 32;
            cmbNombreProducto.SelectedIndexChanged += cmbNombreProducto_SelectedIndexChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Microsoft Sans Serif", 8.999999F);
            label9.Location = new Point(14, 320);
            label9.Name = "label9";
            label9.Size = new Size(51, 18);
            label9.TabIndex = 31;
            label9.Text = "Stock:";
            // 
            // btnNuevaCat
            // 
            btnNuevaCat.Location = new Point(106, 127);
            btnNuevaCat.Margin = new Padding(3, 4, 3, 4);
            btnNuevaCat.Name = "btnNuevaCat";
            btnNuevaCat.Size = new Size(32, 37);
            btnNuevaCat.TabIndex = 27;
            btnNuevaCat.Text = "+";
            btnNuevaCat.UseVisualStyleBackColor = true;
            btnNuevaCat.Click += btnNuevaCat_Click;
            // 
            // cmbCategoria
            // 
            cmbCategoria.FormattingEnabled = true;
            cmbCategoria.Location = new Point(145, 127);
            cmbCategoria.Margin = new Padding(3, 4, 3, 4);
            cmbCategoria.Name = "cmbCategoria";
            cmbCategoria.Size = new Size(182, 33);
            cmbCategoria.TabIndex = 26;
            // 
            // chkPorPeso
            // 
            chkPorPeso.AutoSize = true;
            chkPorPeso.Location = new Point(14, 471);
            chkPorPeso.Margin = new Padding(3, 4, 3, 4);
            chkPorPeso.Name = "chkPorPeso";
            chkPorPeso.Size = new Size(199, 29);
            chkPorPeso.TabIndex = 25;
            chkPorPeso.Text = "Se vende por peso";
            chkPorPeso.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Microsoft Sans Serif", 8.999999F);
            label8.Location = new Point(10, 381);
            label8.Name = "label8";
            label8.Size = new Size(79, 18);
            label8.TabIndex = 24;
            label8.Text = "Stock Min:";
            // 
            // nupStockMin
            // 
            nupStockMin.DecimalPlaces = 3;
            nupStockMin.Location = new Point(106, 367);
            nupStockMin.Margin = new Padding(3, 4, 3, 4);
            nupStockMin.Name = "nupStockMin";
            nupStockMin.Size = new Size(223, 30);
            nupStockMin.TabIndex = 23;
            // 
            // nupPrecio
            // 
            nupPrecio.DecimalPlaces = 2;
            nupPrecio.Location = new Point(106, 184);
            nupPrecio.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            nupPrecio.Name = "nupPrecio";
            nupPrecio.Size = new Size(222, 30);
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
            btnEliminar.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            btnEliminar.ForeColor = Color.WhiteSmoke;
            btnEliminar.Location = new Point(200, 523);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new Size(110, 40);
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
            GUARDAR.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            GUARDAR.ForeColor = Color.WhiteSmoke;
            GUARDAR.Location = new Point(49, 523);
            GUARDAR.Name = "GUARDAR";
            GUARDAR.Size = new Size(110, 40);
            GUARDAR.TabIndex = 20;
            GUARDAR.Text = "GUARDAR";
            GUARDAR.UseVisualStyleBackColor = false;
            GUARDAR.Click += GUARDAR_Click;
            // 
            // textBox4
            // 
            textBox4.CharacterCasing = CharacterCasing.Upper;
            textBox4.Location = new Point(106, 248);
            textBox4.MaxLength = 100;
            textBox4.Multiline = true;
            textBox4.Name = "textBox4";
            textBox4.ScrollBars = ScrollBars.Vertical;
            textBox4.Size = new Size(222, 33);
            textBox4.TabIndex = 19;
            textBox4.DoubleClick += textBox4_DoubleClick;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Microsoft Sans Serif", 8.999999F);
            label7.Location = new Point(10, 256);
            label7.Name = "label7";
            label7.Size = new Size(99, 18);
            label7.TabIndex = 18;
            label7.Text = "Descripcion  :";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Microsoft Sans Serif", 8.999999F);
            label5.Location = new Point(10, 192);
            label5.Name = "label5";
            label5.Size = new Size(59, 18);
            label5.TabIndex = 15;
            label5.Text = "Precio :";
            label5.Click += label5_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Microsoft Sans Serif", 8.999999F);
            label4.Location = new Point(10, 133);
            label4.Name = "label4";
            label4.Size = new Size(80, 18);
            label4.TabIndex = 14;
            label4.Text = "Categoria :";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Microsoft Sans Serif", 8.999999F);
            label3.Location = new Point(10, 75);
            label3.Name = "label3";
            label3.Size = new Size(70, 18);
            label3.TabIndex = 13;
            label3.Text = "Nombre :";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 12F);
            label2.Location = new Point(80, 8);
            label2.Name = "label2";
            label2.Size = new Size(152, 25);
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
            panel2.Location = new Point(350, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(973, 80);
            panel2.TabIndex = 10;
            panel2.Paint += panel2_Paint;
            // 
            // bucarNombreProducto
            // 
            bucarNombreProducto.Font = new Font("Microsoft Sans Serif", 8.999999F);
            bucarNombreProducto.ForeColor = SystemColors.ButtonShadow;
            bucarNombreProducto.FormattingEnabled = true;
            bucarNombreProducto.Location = new Point(89, 51);
            bucarNombreProducto.Name = "bucarNombreProducto";
            bucarNombreProducto.Size = new Size(255, 26);
            bucarNombreProducto.TabIndex = 18;
            bucarNombreProducto.Text = "Nombre del Producto";
            // 
            // buscarCatProd
            // 
            buscarCatProd.Font = new Font("Microsoft Sans Serif", 8.999999F);
            buscarCatProd.ForeColor = SystemColors.ButtonShadow;
            buscarCatProd.FormattingEnabled = true;
            buscarCatProd.Location = new Point(373, 51);
            buscarCatProd.Name = "buscarCatProd";
            buscarCatProd.Size = new Size(255, 26);
            buscarCatProd.TabIndex = 17;
            buscarCatProd.Text = "Categoria";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Microsoft Sans Serif", 8.999999F);
            label6.Location = new Point(16, 52);
            label6.Name = "label6";
            label6.Size = new Size(45, 18);
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
            panel3.Controls.Add(dgvProductos);
            panel3.Location = new Point(350, 80);
            panel3.Name = "panel3";
            panel3.Size = new Size(974, 488);
            panel3.TabIndex = 11;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label10.Location = new Point(679, 317);
            label10.Name = "label10";
            label10.Size = new Size(163, 20);
            label10.TabIndex = 4;
            label10.Text = "PRÓXIMOS A VENCER";
            label10.TextAlign = ContentAlignment.TopCenter;
            // 
            // STOCK
            // 
            STOCK.AutoSize = true;
            STOCK.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            STOCK.Location = new Point(192, 317);
            STOCK.Name = "STOCK";
            STOCK.Size = new Size(99, 20);
            STOCK.TabIndex = 3;
            STOCK.Text = "STOCK BAJO";
            STOCK.TextAlign = ContentAlignment.TopCenter;
            STOCK.Click += label10_Click;
            // 
            // listaVencimientos
            // 
            listaVencimientos.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            listaVencimientos.FormattingEnabled = true;
            listaVencimientos.Location = new Point(518, 341);
            listaVencimientos.Margin = new Padding(3, 4, 3, 4);
            listaVencimientos.Name = "listaVencimientos";
            listaVencimientos.Size = new Size(442, 124);
            listaVencimientos.TabIndex = 2;
            listaVencimientos.Tag = "";
            // 
            // listaStockBajo
            // 
            listaStockBajo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            listaStockBajo.FormattingEnabled = true;
            listaStockBajo.Location = new Point(16, 341);
            listaStockBajo.Margin = new Padding(3, 4, 3, 4);
            listaStockBajo.Name = "listaStockBajo";
            listaStockBajo.Size = new Size(442, 124);
            listaStockBajo.TabIndex = 1;
            // 
            // dgvProductos
            // 
            dgvProductos.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvProductos.CausesValidation = false;
            dgvProductos.Font = new Font("Microsoft Sans Serif", 8.999999F);
            dgvProductos.Location = new Point(16, 45);
            dgvProductos.Name = "dgvProductos";
            dgvProductos.Size = new Size(943, 213);
            dgvProductos.TabIndex = 0;
            dgvProductos.UseCompatibleStateImageBehavior = false;
            dgvProductos.DoubleClick += listView1_DoubleClick_1;
            // 
            // ProductoForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1323, 568);
            Controls.Add(panel3);
            Controls.Add(panel2);
            Controls.Add(panelNuevoProducto);
            FormBorderStyle = FormBorderStyle.None;
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
        private ListView dgvProductos;
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

        private void btnNuevoProducto_Click(object sender, EventArgs e)
        {
            // Limpia todo el formulario para cargar un producto desde cero
            idProductoSeleccionado = 0;
            GUARDAR.Text = "GUARDAR";
            btnEliminar.Visible = false;

            cmbNombreProducto.Text = "";

            // Si no usás este botón como texto, podés borrar esta línea:
            // btnIngresarStock.Text = ""; 

            textBox4.Clear();
            nupPrecio.Value = 0;
            nupStockMin.Value = 0;
            chkPorPeso.Checked = false;

            if (cmbCategoria.Items.Count > 0) cmbCategoria.SelectedIndex = 0;
            if (dgvProductos.SelectedItems.Count > 0) dgvProductos.SelectedItems[0].Selected = false;

            cmbNombreProducto.Focus();
        }
    }
}