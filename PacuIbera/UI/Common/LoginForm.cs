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


                    // 3. Verificamos si el usuario tiene una caja abierta EXCLUSIVAMENTE HOY
                    // 3. Verificamos si el usuario tiene un registro de caja creado HOY
                    int cajaId = 0;
                    //string stringConexion = "Server=(localdb)\\MSSQLLocalDB; DataBase=PacuIberaDB; Integrated Security=True; TrustServerCertificate=True;";
                    string stringConexion = "Server=Server=AGUS\\SQLEXPRESS; DataBase=PacuIberaDB; Integrated Security=True; TrustServerCertificate=True;";


                    using (Microsoft.Data.SqlClient.SqlConnection conexion = new Microsoft.Data.SqlClient.SqlConnection(stringConexion))
                    {
                        // Buscamos que exista una caja de HOY para este usuario, sin importar qué palabra tenga en "Estado"
                        string query = "SELECT TOP 1 Id FROM Caja WHERE UsuarioId = @UsuarioId AND CAST(FechaHoraApertura AS DATE) = CAST(GETDATE() AS DATE)";
                        Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand(query, conexion);
                        cmd.Parameters.AddWithValue("@UsuarioId", user.Id);

                        conexion.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            cajaId = Convert.ToInt32(result);
                        }
                    }

                    // Si NO tiene caja HOY, lo obligamos a abrirla
                    if (cajaId <= 0)
                    {
                        AperturaCajaForm formCaja = new AperturaCajaForm(user.Id);
                        formCaja.ShowDialog(); // Frena el código hasta que cierre la ventana

                        // Verificamos de nuevo: ¿Realmente se guardó el turno de hoy o apretó la "X"?
                        using (Microsoft.Data.SqlClient.SqlConnection conexion = new Microsoft.Data.SqlClient.SqlConnection(stringConexion))
                        {
                            string query = "SELECT TOP 1 Id FROM Caja WHERE UsuarioId = @UsuarioId AND CAST(FechaHoraApertura AS DATE) = CAST(GETDATE() AS DATE)";
                            Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand(query, conexion);
                            cmd.Parameters.AddWithValue("@UsuarioId", user.Id);

                            conexion.Open();
                            object result = cmd.ExecuteScalar();

                            // Si sigue siendo null, cerró con la X o el guardado falló
                            if (result == null)
                            {
                                MessageBox.Show("Es obligatorio realizar la apertura de caja del día para comenzar el turno.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                this.Show(); // Mostramos el login de nuevo
                                return;      // Cortamos para que no abra el menú
                            }
                        }
                    }

                    // 4. Si la caja de HOY ya está creada, pasamos al menú principal
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