using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Printing;

namespace PacuIbera.UI.Common
{
    public partial class PrincipalForm : Form
    {



        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        public PrincipalForm()
        {
            InitializeComponent();
        }

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void AjustarMenu()
        {
            PanelContenedor.Left = MenuVertical.Width;
            PanelContenedor.Width = this.ClientRectangle.Width - MenuVertical.Width;

            PanelContenedor.Height = this.ClientRectangle.Height - PanelContenedor.Top;
        }

        private void AjustarBotones()
        {
            if(MenuVertical.Width == 250)
            {
                btnProducto.Text = "        Productos"; 
                btnProducto.TextAlign = ContentAlignment.MiddleLeft;
                btnProducto.ImageAlign = ContentAlignment.MiddleLeft;
                btnProducto.TextImageRelation = TextImageRelation.ImageBeforeText;
                btnVentas.Text = "        Ventas";
                btnVentas.TextAlign = ContentAlignment.MiddleLeft;
                btnVentas.ImageAlign = ContentAlignment.MiddleLeft;
                btnVentas.TextImageRelation = TextImageRelation.ImageBeforeText;
                btnClientes.Text = "        Clientes";
                btnClientes.TextAlign = ContentAlignment.MiddleLeft;
                btnClientes.ImageAlign = ContentAlignment.MiddleLeft;
                btnClientes.TextImageRelation = TextImageRelation.ImageBeforeText;
                btnCompras.Text = "        Compras";
                btnCompras.TextAlign = ContentAlignment.MiddleLeft;
                btnCompras.ImageAlign = ContentAlignment.MiddleLeft;
                btnCompras.TextImageRelation = TextImageRelation.ImageBeforeText;
                btnProveedores.Text = "        Proveedores";
                btnProveedores.TextAlign = ContentAlignment.MiddleLeft;
                btnProveedores.ImageAlign = ContentAlignment.MiddleLeft;
                btnProveedores.TextImageRelation = TextImageRelation.ImageBeforeText;
                btnEmpleados.Text = "       Empleados";
                btnEmpleados.TextAlign = ContentAlignment.MiddleLeft;
                btnEmpleados.ImageAlign = ContentAlignment.MiddleLeft;
                btnEmpleados.TextImageRelation = TextImageRelation.ImageBeforeText;
                btnPagos.Text = "        Pagos";
                btnPagos.TextAlign = ContentAlignment.MiddleLeft;
                btnPagos.ImageAlign = ContentAlignment.MiddleLeft;
                btnPagos.TextImageRelation = TextImageRelation.ImageBeforeText;
                btnReportes.Text = "        Reportes";
                btnReportes.TextAlign = ContentAlignment.MiddleLeft;
                btnReportes.ImageAlign = ContentAlignment.MiddleLeft;
                btnReportes.TextImageRelation = TextImageRelation.ImageBeforeText;

            }
            else
            {
                btnProducto.Text = "";
                btnProducto.Padding = new Padding(10, 0, 0, 0);
                btnVentas.Text = "";
                btnVentas.Padding = new Padding(10, 0, 0, 0);
                btnClientes.Text = "";
                btnClientes.Padding = new Padding(10, 0, 0, 0);
                btnCompras.Text = "";
                btnCompras.Padding = new Padding(10, 0, 0, 0);
                btnProveedores.Text = "";
                btnProveedores.Padding = new Padding(10, 0, 0, 0);
                btnEmpleados.Text = "";
                btnEmpleados.Padding = new Padding(10, 0, 0, 0);
                btnPagos.Text = "";
                btnPagos.Padding = new Padding(10, 0, 0, 0);
                btnReportes.Text = "";
                btnReportes.Padding = new Padding(10, 0, 0, 0);
            }
        }
        private void btnMenu_Click(object sender, EventArgs e)
        {
            if (MenuVertical.Width == 250)
            {
                MenuVertical.Width = 96;
                Logo.Width = 85;
                Logo.Left = (MenuVertical.Width - Logo.Width) / 2;
           
            }
            else
            {
                MenuVertical.Width = 250;
                Logo.Width = 220;
                Logo.Left = (MenuVertical.Width - Logo.Width) / 2;
                
            }
            AjustarBotones();
            AjustarMenu();
        }


        private void PrincipalForm_Resize(object sender, EventArgs e)
        {
            AjustarMenu();
        }
        #endregion

        private void MenuVertical_Paint(object sender, PaintEventArgs e)
        {

        }

        private void iconCerrar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void iconMaximizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            iconRestaurar.Visible = true;
            iconMaximizar.Visible = false;

        }

        private void iconRestaurar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            iconRestaurar.Visible = false;
            iconMaximizar.Visible = true;
        }

        private void iconMinimizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;

        }

        private void PanelContenedor_Paint(object sender, PaintEventArgs e)
        {

        }

        private void BarraTitulo_Paint(object sender, PaintEventArgs e)
        {

        }

        private void BarraTitulo_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }


        private void AbrirFormularioPanel(object Formhijo)
        {
            if (this.PanelContenedor.Controls.Count > 0)
                this.PanelContenedor.Controls.RemoveAt(0);
            Form fh = Formhijo as Form;
            fh.TopLevel = false;
            fh.Dock = DockStyle.Fill;
            this.PanelContenedor.Controls.Add(fh);
            this.PanelContenedor.Tag = fh;
            fh.Show();
        }

        private void btnProducto_Click(object sender, EventArgs e)
        {
            AbrirFormularioPanel(new ProductoForm());
        }

        private void btnClientes_Click(object sender, EventArgs e)
        {
            AbrirFormularioPanel(new ClientesForm());
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }
    }
}
