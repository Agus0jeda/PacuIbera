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

                Usuario user = usuarioNegocio.ValidarLogin(dni, clave);

                if (user != null)
                {
                    SesionActiva.IdUsuario = user.Id;
                    SesionActiva.Nombre = user.Nombre;
                    SesionActiva.Apellido = user.Apellido;
                    SesionActiva.Rol = user.Rol;

                    this.Hide();

                    // REGLA NUEVA: La caja es obligatoria ÚNICAMENTE para el Vendedor
                    if (user.Rol == "Vendedor")
                    {
                        CajaDatos cajaDatos = new CajaDatos();
                        int cajaId = cajaDatos.VerificarCajaAbierta(user.Id);

                        if (cajaId <= 0)
                        {
                            AperturaCajaForm formCaja = new AperturaCajaForm(user.Id);
                            formCaja.ShowDialog();

                            cajaId = cajaDatos.VerificarCajaAbierta(user.Id);

                            if (cajaId <= 0)
                            {
                                MessageBox.Show("Es obligatorio realizar la apertura de caja para comenzar el turno.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                this.Show();
                                return;
                            }
                        }
                        SesionActiva.IdCaja = cajaId;
                    }
                    else
                    {
                        // Si es Administrador o Gerente, no manejan caja operativa de mostrador
                        SesionActiva.IdCaja = 0;
                    }

                    PrincipalForm ventanaPrincipal = new PrincipalForm();
                    ventanaPrincipal.FormClosed += (s, args) => Application.Exit();
                    ventanaPrincipal.Show();
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