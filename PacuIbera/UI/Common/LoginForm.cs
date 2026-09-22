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

                    // 1. Usamos tu clase CajaDatos en lugar de hacer SQL acá
                    CajaDatos cajaDatos = new CajaDatos();
                    int cajaId = cajaDatos.VerificarCajaAbierta(user.Id);

                    // 2. Si no tiene caja abierta (cajaId es 0), lo obligamos a abrirla
                    if (cajaId <= 0)
                    {
                        AperturaCajaForm formCaja = new AperturaCajaForm(user.Id);
                        formCaja.ShowDialog();

                        // 3. Volvemos a consultar a la base para ver si realmente la abrió o cerró la ventana
                        cajaId = cajaDatos.VerificarCajaAbierta(user.Id);

                        if (cajaId <= 0)
                        {
                            MessageBox.Show("Es obligatorio realizar la apertura de caja para comenzar el turno.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            this.Show();
                            return;
                        }
                    }

                    // 4. EL PASO CLAVE: Guardamos el ID de la caja en la sesión global
                    SesionActiva.IdCaja = cajaId;

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