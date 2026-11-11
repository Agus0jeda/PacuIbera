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
        private Button btnCerrarTurno;
        private Button? botonModuloActivo;
        private Form? vistaActiva;
        private readonly Dictionary<Button, (Color Fondo, Color Texto, Color Hover, Color Pulsado, bool Visual)> estilosMenu = new();


        public PrincipalForm()
        {
            InitializeComponent();
            CrearBotonCerrarTurno();
            CrearEntradaConsultaRemitos();
            ConfigurarIndicadorMenu();

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


        private void ConfigurarPermisosMenu()
        {
            // Apagamos los botones sensibles por defecto 
            btnEmpleados.Visible = false;
            btnProveedores.Visible = false;
            btnCompras.Visible = false;
            if (btnDeposito != null) btnDeposito.Visible = false;
            btnReportes.Visible = false;
            btnPagos.Visible = false;
            if (btnAnalisis != null) btnAnalisis.Visible = false;

            // Evaluamos el rol para encender lo que corresponda
            switch (SesionActiva.Rol)
            {
                case "Gerente":
                case "SuperAdministrador":
                    btnEmpleados.Visible = true;
                    btnReportes.Visible = true;
                    if (btnAnalisis != null) btnAnalisis.Visible = true; // El gerente sí ve el análisis

                    // Ocultamos explícitamente la parte operativa y clientes/proveedores
                    btnVentas.Visible = false;
                    btnProducto.Visible = SesionActiva.Rol == "Gerente";
                    btnPagos.Visible = false;
                    btnHistorial.Visible = false;
                    btnClientes.Visible = false;
                    btnProveedores.Visible = false;
                    btnCompras.Visible = false;
                    btnDeposito.Visible = false;

                    btnCerrarTurno.Text = "CERRAR SESIÓN";
                    btnCerrarTurno.BackColor = Color.DimGray;
                    break;

                case "Administrador":
                    btnEmpleados.Visible = true;
                    btnProveedores.Visible = true;
                    btnCompras.Visible = true;// por ahora (Pedidos)
                    btnDeposito.Visible = true;
                    btnVentas.Visible = false;
                    if (btnAnalisis != null) btnAnalisis.Visible = false;
                    btnCerrarTurno.Text = "CERRAR SESIÓN";
                    btnCerrarTurno.BackColor = Color.DimGray;
                    break;

                case "Vendedor":
                    // El vendedor solo verá Productos, Clientes, Ventas, Historial y Pagos
                    break;
            }
            ConfigurarAccesoConsultaRemitos();
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
            // Ahora incluimos btnDeposito entre Proveedores y Empleados
            Button[] botonesMenu = { btnProducto, btnVentas, btnClientes, btnCompras, btnProveedores, btnDeposito, btnEmpleados, btnPagos, btnReportes, btnAnalisis, btnHistorial };

            foreach (var btn in botonesMenu)
            {
                if (btn != null && btn.Visible)
                {
                    // Si el botón está visible, lo posicionamos en la siguiente línea disponible
                    btn.Top = posYActual;
                    // Asegurar altura y margen uniforme
                    btn.Height = 45;
                    btn.Margin = new Padding(3);
                    posYActual += btn.Height + espacioEntreBotones; // Movemos la referencia hacia abajo para el próximo
                }
            }
        }

        private void AjustarBotones()
        {
            if (MenuVertical.Width == 250)
            {
                // Menú Expandido
                btnProducto.Text = "        Productos";
                btnProducto.TextAlign = ContentAlignment.MiddleLeft;
                btnProducto.ImageAlign = ContentAlignment.MiddleLeft;
                btnProducto.TextImageRelation = TextImageRelation.ImageBeforeText;
                btnProducto.Height = 45;

                btnVentas.Text = "        Ventas";
                btnVentas.TextAlign = ContentAlignment.MiddleLeft;
                btnVentas.ImageAlign = ContentAlignment.MiddleLeft;
                btnVentas.TextImageRelation = TextImageRelation.ImageBeforeText;
                btnVentas.Height = 45;

                btnClientes.Text = "        Clientes";
                btnClientes.TextAlign = ContentAlignment.MiddleLeft;
                btnClientes.ImageAlign = ContentAlignment.MiddleLeft;
                btnClientes.TextImageRelation = TextImageRelation.ImageBeforeText;
                btnClientes.Height = 45;

                btnCompras.Text = "        Compras";
                btnCompras.TextAlign = ContentAlignment.MiddleLeft;
                btnCompras.ImageAlign = ContentAlignment.MiddleLeft;
                btnCompras.TextImageRelation = TextImageRelation.ImageBeforeText;
                btnCompras.Height = 45;

                btnProveedores.Text = "        Proveedores";
                btnProveedores.TextAlign = ContentAlignment.MiddleLeft;
                btnProveedores.ImageAlign = ContentAlignment.MiddleLeft;
                btnProveedores.TextImageRelation = TextImageRelation.ImageBeforeText;
                btnProveedores.Height = 45;

                if (btnDeposito != null)
                {
                    btnDeposito.Text = "        Depósito";
                    btnDeposito.TextAlign = ContentAlignment.MiddleLeft;
                    btnDeposito.ImageAlign = ContentAlignment.MiddleLeft;
                    btnDeposito.TextImageRelation = TextImageRelation.ImageBeforeText;
                    btnDeposito.Height = 45;
                }

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

                // --- ACÁ LE DAMOS FORMATO AL NUEVO BOTÓN CUANDO SE AGRANDA ---
                if (btnAnalisis != null)
                {
                    btnAnalisis.Text = "        Análisis Estratégico";
                    btnAnalisis.TextAlign = ContentAlignment.MiddleLeft;
                    btnAnalisis.ImageAlign = ContentAlignment.MiddleLeft;
                    btnAnalisis.TextImageRelation = TextImageRelation.ImageBeforeText;
                    btnAnalisis.Width = 250; // Aseguramos que ocupe todo el ancho
                }

                btnHistorial.Text = "        Historial";
                btnHistorial.TextAlign = ContentAlignment.MiddleLeft;
                btnHistorial.ImageAlign = ContentAlignment.MiddleLeft;
                btnHistorial.TextImageRelation = TextImageRelation.ImageBeforeText;

                btnCerrarTurno.Text = SesionActiva.Rol == "Gerente" ? "        Cerrar Sesión" : "        Cerrar Turno";
                btnCerrarTurno.TextAlign = ContentAlignment.MiddleLeft;
                btnCerrarTurno.ImageAlign = ContentAlignment.MiddleLeft;
                btnCerrarTurno.TextImageRelation = TextImageRelation.ImageBeforeText;
            }
            else
            {
                // Menú Contraído (Chiquito)
                btnProducto.Text = "";
                btnProducto.Padding = new Padding(10, 0, 0, 0);
                btnProducto.Height = 45;
                btnVentas.Text = "";
                btnVentas.Padding = new Padding(10, 0, 0, 0);
                btnVentas.Height = 45;
                btnClientes.Text = "";
                btnClientes.Padding = new Padding(10, 0, 0, 0);
                btnClientes.Height = 45;
                btnCompras.Text = "";
                btnCompras.Padding = new Padding(10, 0, 0, 0);
                btnCompras.Height = 45;
                btnProveedores.Text = "";
                btnProveedores.Padding = new Padding(10, 0, 0, 0);
                btnProveedores.Height = 45;
                if (btnDeposito != null) { btnDeposito.Text = ""; btnDeposito.Padding = new Padding(10, 0, 0, 0); btnDeposito.Height = 45; }
                btnEmpleados.Text = "";
                btnEmpleados.Padding = new Padding(10, 0, 0, 0);
                btnEmpleados.Height = 45;
                btnPagos.Text = "";
                btnPagos.Padding = new Padding(10, 0, 0, 0);
                btnPagos.Height = 45;
                btnReportes.Text = "";
                btnReportes.Padding = new Padding(10, 0, 0, 0);

                // --- ACÁ LE SACAMOS EL TEXTO CUANDO SE ACHICA ---
                if (btnAnalisis != null)
                {
                    btnAnalisis.Text = "";
                    btnAnalisis.Padding = new Padding(10, 0, 0, 0);
                    btnAnalisis.Width = 96; // El ancho de tu menú contraído
                }

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
            ConfigurarPermisosMenu(); // 1. Oculta y muestra según el rol
            ReorganizarMenu();        // 2. Apila los que quedaron visibles uno abajo del otro
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

        private void ConfigurarIndicadorMenu()
        {
            Button[] modulos = { btnProducto, btnVentas, btnClientes, btnCompras, btnProveedores,
                btnDeposito, btnEmpleados, btnPagos, btnReportes, btnAnalisis, btnHistorial, btnConsultaRemitos };
            foreach (Button boton in modulos)
            {
                estilosMenu.Add(boton, (boton.BackColor, boton.ForeColor,
                    boton.FlatAppearance.MouseOverBackColor, boton.FlatAppearance.MouseDownBackColor,
                    boton.UseVisualStyleBackColor));
                boton.Paint += PintarIndicadorModulo;
            }
        }

        private void PintarIndicadorModulo(object? sender, PaintEventArgs e)
        {
            if (sender is Button boton && boton == botonModuloActivo)
            {
                using var pincel = new SolidBrush(Color.FromArgb(24, 100, 56));
                e.Graphics.FillRectangle(pincel, 0, 0, 4, boton.ClientSize.Height);
            }
        }

        private void ActivarModulo(Button? boton)
        {
            botonModuloActivo = boton;
            foreach (var entrada in estilosMenu)
            {
                Button actual = entrada.Key;
                var original = entrada.Value;
                bool activo = actual == boton;
                actual.BackColor = activo ? Color.FromArgb(190, 230, 190) : original.Fondo;
                actual.ForeColor = activo ? Color.FromArgb(18, 72, 38) : original.Texto;
                actual.FlatAppearance.MouseOverBackColor = activo ? Color.FromArgb(175, 220, 175) : original.Hover;
                actual.FlatAppearance.MouseDownBackColor = activo ? Color.FromArgb(160, 210, 160) : original.Pulsado;
                actual.UseVisualStyleBackColor = activo ? false : original.Visual;
                actual.Invalidate();
            }
        }

        private void VistaActivaCerrada(object? sender, FormClosedEventArgs e)
        {
            if (sender != vistaActiva) return;
            vistaActiva.FormClosed -= VistaActivaCerrada;
            vistaActiva = null;
            PanelContenedor.Tag = null;
            ActivarModulo(null);
        }

        private void AbrirFormularioPanel(Form fh, Button? modulo = null)
        {
            if (vistaActiva != null)
                vistaActiva.FormClosed -= VistaActivaCerrada;
            if (this.PanelContenedor.Controls.Count > 0)
                this.PanelContenedor.Controls.RemoveAt(0);
            fh.TopLevel = false;
            fh.Dock = DockStyle.Fill;
            PanelContenedor.Controls.Clear();
            this.PanelContenedor.Controls.Add(fh);
            this.PanelContenedor.Tag = fh;
            vistaActiva = fh;
            fh.FormClosed += VistaActivaCerrada;
            ActivarModulo(modulo);
            try
            {
                fh.Show();
            }
            catch
            {
                fh.FormClosed -= VistaActivaCerrada;
                vistaActiva = null;
                ActivarModulo(null);
                throw;
            }
        }

        // EVENTOS DE LOS BOTONES
        private void btnProducto_Click(object sender, EventArgs e)
        {
            if (!global::Datos.ProductoDatos.PuedeConsultarFichas) return;
            AbrirFormularioPanel(new ProductoForm(), btnProducto);
        }
        private void btnClientes_Click(object sender, EventArgs e) { AbrirFormularioPanel(new ClientesForm(), btnClientes); }
        private void btnReportes_Click(object sender, EventArgs e) { AbrirFormularioPanel(new ReportesForm(), btnReportes); }
        private void btnVentas_Click(object sender, EventArgs e) { AbrirFormularioPanel(new VentasForm(), btnVentas); }
        private void btnEmpleados_Click(object sender, EventArgs e) { AbrirFormularioPanel(new EmpleadosForm(), btnEmpleados); }
        private void btnPagos_Click(object sender, EventArgs e) { AbrirFormularioPanel(new EnConstruccionForm("Pagos"), btnPagos); }
        private void btnCompras_Click(object sender, EventArgs e) { AbrirFormularioPanel(new FormPedidos(), btnCompras); }
        private void btnDeposito_Click(object sender, EventArgs e) { AbrirFormularioPanel(new FormDeposito(), btnDeposito); }

        // Función del nuevo botón Análisis
        private void btnAnalisis_Click(object sender, EventArgs e)
        {
            AbrirFormularioPanel(new AnalisisForm(), btnAnalisis);
        }

        private void btnHistorial_Click(object sender, EventArgs e)
        {
            AbrirFormularioPanel(new HistorialVentasForm(), btnHistorial);
        }

        private void BtnCerrarTurno_Click(object sender, EventArgs e)
        {

            if (SesionActiva.Rol == "Gerente" || SesionActiva.Rol == "Administrador")
            {
                Application.Exit();
                return;
            }

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
            AbrirFormularioPanel(new ProveedorForm(), btnProveedores);
        }

        private void btnAnalisis_Click_1(object sender, EventArgs e)
        {
            AbrirFormularioPanel(new AnalisisForm(), btnAnalisis);
        }
    }
}
