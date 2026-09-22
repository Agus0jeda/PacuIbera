using PacuIbera.Dominio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public partial class PrincipalForm : Form
    {
        public PrincipalForm()
        {
            InitializeComponent();
            CrearBotonCerrarTurno();
            this.Load += PrincipalForm_Load;

        }

        private void CrearBotonCerrarTurno()
        {
            btnCerrarTurno = new Button
            {
                Text = "        Cerrar Turno",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = ColorTranslator.FromHtml("#FF4C4C"), // Rojo para destacar
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 40,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0),
                Dock = DockStyle.Bottom
            };
            btnCerrarTurno.FlatAppearance.BorderSize = 0;
            btnCerrarTurno.Click += BtnCerrarTurno_Click;

            // Lo agregamos a tu panel lateral
            MenuVertical.Controls.Add(btnCerrarTurno);
        }

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private Button btnCerrarTurno;

        private void ConfigurarPermisosMenu()
        {
            // Apagamos los botones sensibles por defecto
            btnEmpleados.Visible = false;
            btnProveedores.Visible = false;
            btnCompras.Visible = false;
            btnReportes.Visible = false;

            // Evaluamos el rol para encender lo que corresponda
            switch (SesionActiva.Rol)
            {
                case "Gerente":
                case "SuperAdministrador":
                    btnEmpleados.Visible = true;
                    btnProveedores.Visible = true;
                    btnReportes.Visible = true;
                    break;
                case "Administrador":
                    btnEmpleados.Visible = true;
                    btnProveedores.Visible = true;
                    btnCompras.Visible = true;
                    break;
                case "Vendedor":
                    // El vendedor solo verá Productos, Clientes, Ventas y Pagos que nunca se ocultaron.
                    break;
            }
        }

        private void AjustarMenu()
        {
            PanelContenedor.Left = MenuVertical.Width;
            PanelContenedor.Width = this.ClientRectangle.Width - MenuVertical.Width;
            PanelContenedor.Height = this.ClientRectangle.Height - PanelContenedor.Top;
        }

        private void ReorganizarMenu()
        {
            // Define la posición Y donde arranca el primer botón debajo de tu logo
            int posYInicial = 120; // Podes ajustarlo según la altura de tu logo en el panel
            int espacioEntreBotones = 5; // Espacio prolijo entre cada botón

            int posYActual = posYInicial;

            // Agrupamos todos los botones del menú en un array ordenados de arriba hacia abajo
            Button[] botonesMenu = { btnProducto, btnVentas, btnClientes, btnCompras, btnProveedores, btnEmpleados, btnPagos, btnReportes, btnHistorial };

            foreach (var btn in botonesMenu)
            {
                if (btn != null)
                {
                    if (btn.Visible)
                    {
                        // Si el botón está visible, lo posicionamos en la siguiente línea disponible
                        btn.Top = posYActual;
                        posYActual += btn.Height + espacioEntreBotones; // Movemos la referencia hacia abajo para el próximo
                    }
                }
            }
        }

        private void AjustarBotones()
        {
            if (MenuVertical.Width == 250)
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

                btnHistorial.Text = "        Historial";
                btnHistorial.TextAlign = ContentAlignment.MiddleLeft;
                btnHistorial.ImageAlign = ContentAlignment.MiddleLeft;
                btnHistorial.TextImageRelation = TextImageRelation.ImageBeforeText;

                btnCerrarTurno.Text = "        Cerrar Turno";
                btnCerrarTurno.TextAlign = ContentAlignment.MiddleLeft;
                btnCerrarTurno.ImageAlign = ContentAlignment.MiddleLeft;
                btnCerrarTurno.TextImageRelation = TextImageRelation.ImageBeforeText;
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
                btnHistorial.Text = "";
                btnHistorial.Padding = new Padding(10, 0, 0, 0);
                btnCerrarTurno.Text = "";
                btnCerrarTurno.Padding = new Padding(10, 0, 0, 0);
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

        private void PrincipalForm_Load(object sender, EventArgs e)
        {
            ConfigurarPermisosMenu(); // Los permisos se leen una sola vez al entrar
            ReorganizarMenu();        // Los botones se ordenan una sola vez al arrancar
            AbrirFormularioPanel(new BienvenidaForm());
        }
        private void PrincipalForm_Resize(object sender, EventArgs e)
        {
            AjustarMenu();
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
            PanelContenedor.Controls.Clear();
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

        private void btnReportes_Click(object sender, EventArgs e)
        {
            AbrirFormularioPanel(new ReportesForm());
        }

        private void btnVentas_Click(object sender, EventArgs e)
        {
            AbrirFormularioPanel(new VentasForm());
        }

        private void btnEmpleados_Click(object sender, EventArgs e)
        {
            AbrirFormularioPanel(new EmpleadosForm());
        }


        private void btnHistorial_Click(object sender, EventArgs e)
        {
            HistorialVentasForm historial = new HistorialVentasForm();
            historial.TopLevel = false;
            historial.Dock = DockStyle.Fill;

            // Reemplazá "pnlContenedor" por el nombre real que tenga tu panel gris derecho
            PanelContenedor.Controls.Clear();
            PanelContenedor.Controls.Add(historial);
            historial.Show();
        }

        private void BtnCerrarTurno_Click(object sender, EventArgs e)
        {
            CierreCajaForm formCierre = new CierreCajaForm();
            formCierre.ShowDialog();

            if (formCierre.CierreExitoso)
            {
                this.Hide();
                PacuIbera_IniciarSesion login = new PacuIbera_IniciarSesion();
                login.Show();
            }
        }

        private void btnProveedores_Click(object sender, EventArgs e)
        {
            AbrirFormularioPanel(new EnConstruccionForm("Proveedores"));
        }

        private void btnPagos_Click(object sender, EventArgs e)
        {
            AbrirFormularioPanel(new EnConstruccionForm("Pagos"));

        }
    }
}