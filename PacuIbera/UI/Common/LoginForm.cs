using System;
using System.Windows.Forms;
using PacuIbera.Dominio;
using Negocio;
using Datos; // Para usar CajaDatos

namespace PacuIbera.UI.Common
{
    public partial class PacuIbera_IniciarSesion : Form
    {
        private UsuarioNegocio usuarioNegocio = new UsuarioNegocio();

        public PacuIbera_IniciarSesion()
        {
            InitializeComponent();
            this.BackColor = ColorTranslator.FromHtml("#88E788");
            lblFecha.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            try
            {
                string dni = txtUsuario.Text.Trim();
                string clave = txtClave.Text.Trim();

                // 1. Validamos credenciales mediante la capa de negocio
                Usuario user = usuarioNegocio.ValidarLogin(dni, clave);

                if (user != null)
                {
                    // 2. Guardamos los datos en la Sesión Activa Global
                    SesionActiva.IdUsuario = user.Id;
                    SesionActiva.Nombre = user.Nombre;
                    SesionActiva.Apellido = user.Apellido;
                    SesionActiva.Rol = user.Rol; // "Administrador", "Vendedor", etc.

                    this.Hide(); // Ocultamos el login

                    // 3. Verificamos en la BDD si este usuario ya tiene una caja abierta hoy
                    CajaDatos cajaDatos = new CajaDatos();
                    int cajaId = cajaDatos.VerificarCajaAbierta(user.Id);

                    if (cajaId > 0)
                    {
                        // Si ya tiene caja abierta, pasamos directo al menú principal
                        PrincipalForm ventanaPrincipal = new PrincipalForm();
                        ventanaPrincipal.FormClosed += (s, args) => Application.Exit();
                        ventanaPrincipal.Show();
                    }
                    else
                    {
                        // Si no tiene caja abierta, abrimos el formulario de apertura pasándole el ID
                        AperturaCajaForm formCaja = new AperturaCajaForm(user.Id);
                        formCaja.FormClosed += (s, args) => Application.Exit();
                        formCaja.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error de Inicio de Sesión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void PacuIbera_IniciarSesion_Load(object sender, EventArgs e) { }
    }
}