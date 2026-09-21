namespace PacuIbera.UI.Common
{
    partial class PrincipalForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrincipalForm));
            MenuVertical = new Panel();
            Logo = new PictureBox();
            btnReportes = new Button();
            btnPagos = new Button();
            btnEmpleados = new Button();
            btnProveedores = new Button();
            btnCompras = new Button();
            btnClientes = new Button();
            btnVentas = new Button();
            btnProducto = new Button();
            BarraTitulo = new Panel();
            iconRestaurar = new PictureBox();
            iconMaximizar = new PictureBox();
            iconMinimizar = new PictureBox();
            iconCerrar = new PictureBox();
            btnMenu = new PictureBox();
            PanelContenedor = new Panel();
            MenuVertical.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)Logo).BeginInit();
            BarraTitulo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)iconRestaurar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)iconMaximizar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)iconMinimizar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)iconCerrar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnMenu).BeginInit();
            SuspendLayout();
            // 
            // MenuVertical
            // 
            MenuVertical.BackColor = Color.LightGreen;
            MenuVertical.Controls.Add(Logo);
            MenuVertical.Controls.Add(btnReportes);
            MenuVertical.Controls.Add(btnPagos);
            MenuVertical.Controls.Add(btnEmpleados);
            MenuVertical.Controls.Add(btnProveedores);
            MenuVertical.Controls.Add(btnCompras);
            MenuVertical.Controls.Add(btnClientes);
            MenuVertical.Controls.Add(btnVentas);
            MenuVertical.Controls.Add(btnProducto);
            MenuVertical.Cursor = Cursors.Hand;
            MenuVertical.Dock = DockStyle.Left;
            MenuVertical.Location = new Point(0, 0);
            MenuVertical.Margin = new Padding(3, 2, 3, 2);
            MenuVertical.Name = "MenuVertical";
            MenuVertical.Size = new Size(219, 452);
            MenuVertical.TabIndex = 0;
            MenuVertical.Paint += MenuVertical_Paint;
            // 
            // Logo
            // 
            Logo.Image = (Image)resources.GetObject("Logo.Image");
            Logo.Location = new Point(0, 0);
            Logo.Name = "Logo";
            Logo.Size = new Size(219, 80);
            Logo.SizeMode = PictureBoxSizeMode.Zoom;
            Logo.TabIndex = 8;
            Logo.TabStop = false;
            // 
            // btnReportes
            // 
            btnReportes.FlatAppearance.BorderSize = 0;
            btnReportes.FlatAppearance.MouseOverBackColor = Color.WhiteSmoke;
            btnReportes.FlatStyle = FlatStyle.Flat;
            btnReportes.Font = new Font("Century Gothic", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnReportes.ForeColor = Color.Black;
            btnReportes.Image = (Image)resources.GetObject("btnReportes.Image");
            btnReportes.ImageAlign = ContentAlignment.MiddleLeft;
            btnReportes.Location = new Point(0, 366);
            btnReportes.Margin = new Padding(3, 2, 3, 2);
            btnReportes.Name = "btnReportes";
            btnReportes.Size = new Size(278, 36);
            btnReportes.TabIndex = 7;
            btnReportes.Text = "Reporte";
            btnReportes.UseVisualStyleBackColor = true;
            btnReportes.Click += btnReportes_Click;
            // 
            // btnPagos
            // 
            btnPagos.FlatAppearance.BorderSize = 0;
            btnPagos.FlatAppearance.MouseOverBackColor = Color.WhiteSmoke;
            btnPagos.FlatStyle = FlatStyle.Flat;
            btnPagos.Font = new Font("Century Gothic", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnPagos.ForeColor = Color.Black;
            btnPagos.Image = (Image)resources.GetObject("btnPagos.Image");
            btnPagos.ImageAlign = ContentAlignment.MiddleLeft;
            btnPagos.Location = new Point(0, 326);
            btnPagos.Margin = new Padding(3, 2, 3, 2);
            btnPagos.Name = "btnPagos";
            btnPagos.Size = new Size(278, 36);
            btnPagos.TabIndex = 6;
            btnPagos.Text = "Pagos";
            btnPagos.UseVisualStyleBackColor = true;
            // 
            // btnEmpleados
            // 
            btnEmpleados.FlatAppearance.BorderSize = 0;
            btnEmpleados.FlatAppearance.MouseOverBackColor = Color.WhiteSmoke;
            btnEmpleados.FlatStyle = FlatStyle.Flat;
            btnEmpleados.Font = new Font("Century Gothic", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnEmpleados.ForeColor = Color.Black;
            btnEmpleados.Image = (Image)resources.GetObject("btnEmpleados.Image");
            btnEmpleados.ImageAlign = ContentAlignment.MiddleLeft;
            btnEmpleados.Location = new Point(0, 285);
            btnEmpleados.Margin = new Padding(3, 2, 3, 2);
            btnEmpleados.Name = "btnEmpleados";
            btnEmpleados.Size = new Size(278, 36);
            btnEmpleados.TabIndex = 5;
            btnEmpleados.Text = "Empleados";
            btnEmpleados.UseVisualStyleBackColor = true;
            btnEmpleados.Click += button4_Click;
            // 
            // btnProveedores
            // 
            btnProveedores.FlatAppearance.BorderSize = 0;
            btnProveedores.FlatAppearance.MouseOverBackColor = Color.WhiteSmoke;
            btnProveedores.FlatStyle = FlatStyle.Flat;
            btnProveedores.Font = new Font("Century Gothic", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnProveedores.ForeColor = Color.Black;
            btnProveedores.Image = (Image)resources.GetObject("btnProveedores.Image");
            btnProveedores.ImageAlign = ContentAlignment.MiddleLeft;
            btnProveedores.Location = new Point(0, 244);
            btnProveedores.Margin = new Padding(3, 2, 3, 2);
            btnProveedores.Name = "btnProveedores";
            btnProveedores.Size = new Size(278, 36);
            btnProveedores.TabIndex = 4;
            btnProveedores.Text = "Proveedores";
            btnProveedores.UseVisualStyleBackColor = true;
            // 
            // btnCompras
            // 
            btnCompras.FlatAppearance.BorderSize = 0;
            btnCompras.FlatAppearance.MouseOverBackColor = Color.WhiteSmoke;
            btnCompras.FlatStyle = FlatStyle.Flat;
            btnCompras.Font = new Font("Century Gothic", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnCompras.ForeColor = Color.Black;
            btnCompras.Image = (Image)resources.GetObject("btnCompras.Image");
            btnCompras.ImageAlign = ContentAlignment.MiddleLeft;
            btnCompras.Location = new Point(0, 204);
            btnCompras.Margin = new Padding(3, 2, 3, 2);
            btnCompras.Name = "btnCompras";
            btnCompras.Size = new Size(278, 36);
            btnCompras.TabIndex = 3;
            btnCompras.Text = "Compras";
            btnCompras.UseVisualStyleBackColor = true;
            // 
            // btnClientes
            // 
            btnClientes.FlatAppearance.BorderSize = 0;
            btnClientes.FlatAppearance.MouseOverBackColor = Color.WhiteSmoke;
            btnClientes.FlatStyle = FlatStyle.Flat;
            btnClientes.Font = new Font("Century Gothic", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnClientes.ForeColor = Color.Black;
            btnClientes.Image = (Image)resources.GetObject("btnClientes.Image");
            btnClientes.ImageAlign = ContentAlignment.MiddleLeft;
            btnClientes.Location = new Point(0, 166);
            btnClientes.Margin = new Padding(3, 2, 3, 2);
            btnClientes.Name = "btnClientes";
            btnClientes.Size = new Size(278, 36);
            btnClientes.TabIndex = 2;
            btnClientes.Text = "Clientes";
            btnClientes.UseVisualStyleBackColor = true;
            btnClientes.Click += btnClientes_Click;
            // 
            // btnVentas
            // 
            btnVentas.FlatAppearance.BorderSize = 0;
            btnVentas.FlatAppearance.MouseOverBackColor = Color.WhiteSmoke;
            btnVentas.FlatStyle = FlatStyle.Flat;
            btnVentas.Font = new Font("Century Gothic", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnVentas.ForeColor = Color.Black;
            btnVentas.Image = (Image)resources.GetObject("btnVentas.Image");
            btnVentas.ImageAlign = ContentAlignment.MiddleLeft;
            btnVentas.Location = new Point(0, 126);
            btnVentas.Margin = new Padding(3, 2, 3, 2);
            btnVentas.Name = "btnVentas";
            btnVentas.Size = new Size(278, 36);
            btnVentas.TabIndex = 1;
            btnVentas.Text = "Ventas";
            btnVentas.UseVisualStyleBackColor = true;
            btnVentas.Click += btnVentas_Click;
            // 
            // btnProducto
            // 
            btnProducto.FlatAppearance.BorderSize = 0;
            btnProducto.FlatAppearance.MouseOverBackColor = Color.WhiteSmoke;
            btnProducto.FlatStyle = FlatStyle.Flat;
            btnProducto.Font = new Font("Century Gothic", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnProducto.ForeColor = Color.Black;
            btnProducto.Image = (Image)resources.GetObject("btnProducto.Image");
            btnProducto.ImageAlign = ContentAlignment.MiddleLeft;
            btnProducto.Location = new Point(0, 86);
            btnProducto.Margin = new Padding(3, 2, 3, 2);
            btnProducto.Name = "btnProducto";
            btnProducto.Size = new Size(278, 36);
            btnProducto.TabIndex = 0;
            btnProducto.Text = "Productos";
            btnProducto.UseVisualStyleBackColor = true;
            btnProducto.Click += btnProducto_Click;
            // 
            // BarraTitulo
            // 
            BarraTitulo.BackColor = Color.WhiteSmoke;
            BarraTitulo.Controls.Add(iconRestaurar);
            BarraTitulo.Controls.Add(iconMaximizar);
            BarraTitulo.Controls.Add(iconMinimizar);
            BarraTitulo.Controls.Add(iconCerrar);
            BarraTitulo.Controls.Add(btnMenu);
            BarraTitulo.Dock = DockStyle.Top;
            BarraTitulo.Location = new Point(219, 0);
            BarraTitulo.Margin = new Padding(3, 2, 3, 2);
            BarraTitulo.Name = "BarraTitulo";
            BarraTitulo.Size = new Size(903, 38);
            BarraTitulo.TabIndex = 1;
            BarraTitulo.Paint += BarraTitulo_Paint;
            BarraTitulo.MouseDown += BarraTitulo_MouseDown;
            // 
            // iconRestaurar
            // 
            iconRestaurar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            iconRestaurar.Cursor = Cursors.Hand;
            iconRestaurar.Image = (Image)resources.GetObject("iconRestaurar.Image");
            iconRestaurar.Location = new Point(844, 9);
            iconRestaurar.Margin = new Padding(3, 2, 3, 2);
            iconRestaurar.Name = "iconRestaurar";
            iconRestaurar.Size = new Size(22, 19);
            iconRestaurar.SizeMode = PictureBoxSizeMode.Zoom;
            iconRestaurar.TabIndex = 4;
            iconRestaurar.TabStop = false;
            iconRestaurar.Visible = false;
            iconRestaurar.Click += iconRestaurar_Click;
            iconRestaurar.Resize += PrincipalForm_Resize;
            // 
            // iconMaximizar
            // 
            iconMaximizar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            iconMaximizar.Cursor = Cursors.Hand;
            iconMaximizar.Image = (Image)resources.GetObject("iconMaximizar.Image");
            iconMaximizar.Location = new Point(844, 9);
            iconMaximizar.Margin = new Padding(3, 2, 3, 2);
            iconMaximizar.Name = "iconMaximizar";
            iconMaximizar.Size = new Size(22, 19);
            iconMaximizar.SizeMode = PictureBoxSizeMode.Zoom;
            iconMaximizar.TabIndex = 3;
            iconMaximizar.TabStop = false;
            iconMaximizar.Click += iconMaximizar_Click;
            // 
            // iconMinimizar
            // 
            iconMinimizar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            iconMinimizar.Cursor = Cursors.Hand;
            iconMinimizar.Image = (Image)resources.GetObject("iconMinimizar.Image");
            iconMinimizar.Location = new Point(816, 9);
            iconMinimizar.Margin = new Padding(3, 2, 3, 2);
            iconMinimizar.Name = "iconMinimizar";
            iconMinimizar.Size = new Size(22, 19);
            iconMinimizar.SizeMode = PictureBoxSizeMode.Zoom;
            iconMinimizar.TabIndex = 2;
            iconMinimizar.TabStop = false;
            iconMinimizar.Click += iconMinimizar_Click;
            // 
            // iconCerrar
            // 
            iconCerrar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            iconCerrar.Cursor = Cursors.Hand;
            iconCerrar.Image = (Image)resources.GetObject("iconCerrar.Image");
            iconCerrar.Location = new Point(871, 9);
            iconCerrar.Margin = new Padding(3, 2, 3, 2);
            iconCerrar.Name = "iconCerrar";
            iconCerrar.Size = new Size(22, 19);
            iconCerrar.SizeMode = PictureBoxSizeMode.Zoom;
            iconCerrar.TabIndex = 1;
            iconCerrar.TabStop = false;
            iconCerrar.Click += iconCerrar_Click;
            // 
            // btnMenu
            // 
            btnMenu.Cursor = Cursors.Hand;
            btnMenu.Image = (Image)resources.GetObject("btnMenu.Image");
            btnMenu.Location = new Point(5, 2);
            btnMenu.Margin = new Padding(3, 2, 3, 2);
            btnMenu.Name = "btnMenu";
            btnMenu.Size = new Size(45, 31);
            btnMenu.SizeMode = PictureBoxSizeMode.Zoom;
            btnMenu.TabIndex = 0;
            btnMenu.TabStop = false;
            btnMenu.Click += btnMenu_Click;
            // 
            // PanelContenedor
            // 
            PanelContenedor.BackColor = Color.WhiteSmoke;
            PanelContenedor.Dock = DockStyle.Fill;
            PanelContenedor.Location = new Point(219, 38);
            PanelContenedor.Margin = new Padding(3, 2, 3, 2);
            PanelContenedor.Name = "PanelContenedor";
            PanelContenedor.Size = new Size(903, 414);
            PanelContenedor.TabIndex = 2;
            PanelContenedor.Paint += PanelContenedor_Paint;
            // 
            // PrincipalForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1122, 452);
            Controls.Add(PanelContenedor);
            Controls.Add(BarraTitulo);
            Controls.Add(MenuVertical);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(3, 2, 3, 2);
            Name = "PrincipalForm";
            Text = "PrincipalForm";
            MenuVertical.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)Logo).EndInit();
            BarraTitulo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)iconRestaurar).EndInit();
            ((System.ComponentModel.ISupportInitialize)iconMaximizar).EndInit();
            ((System.ComponentModel.ISupportInitialize)iconMinimizar).EndInit();
            ((System.ComponentModel.ISupportInitialize)iconCerrar).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnMenu).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel MenuVertical;
        private Panel BarraTitulo;
        private Panel PanelContenedor;
        private PictureBox btnMenu;
        private PictureBox iconMaximizar;
        private PictureBox iconMinimizar;
        private PictureBox iconCerrar;
        private PictureBox iconRestaurar;
        private Button btnProducto;
        private Button btnClientes;
        private Button btnVentas;
        private Button btnEmpleados;
        private Button btnProveedores;
        private Button btnCompras;
        private Button btnPagos;
        private Button btnReportes;
        private PictureBox Logo;
    }
}