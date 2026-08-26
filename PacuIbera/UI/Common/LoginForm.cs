using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();


            this.BackColor = ColorTranslator.FromHtml("#88E788");
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            try
            {
                string dniUsuario = txtUsuario.Text.Trim();
                string clave = txtClave.Text.Trim();

                if (string.IsNullOrEmpty(dniUsuario) || string.IsNullOrEmpty(clave))
                {
                    MessageBox.Show("Por favor, completá tu DNI y contraseña.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // aca simulamos por ahora la validación de usuario, en un futuro se debería consultar a la base de datos
                if (dniUsuario == "123456" && clave == "admin")
                {
                    MainForm ventanaPrincipal = new MainForm();
                    ventanaPrincipal.FormClosed += (s, args) => this.Close();
                    this.Hide();
                    ventanaPrincipal.Show();
                }
                else
                {
                    MessageBox.Show("DNI o contraseña incorrectos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Si la base de datos falla o hay cualquier error crítico, lo atrapamos acá
                MessageBox.Show($"Ocurrió un error inesperado al intentar iniciar sesión: {ex.Message}", "Error Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
