using System;
using System.Windows.Forms;
using PacuIbera.Dominio;

namespace PacuIbera.UI.Common
{
    public partial class BienvenidaForm : Form
    {
        public BienvenidaForm()
        {
            InitializeComponent();
        }

        private void BienvenidaForm_Load(object sender, EventArgs e)
        {
            lblBienvenida.Text = $"¡Hola {SesionActiva.Nombre}! Bienvenido a tu turno.";
            //centramos el texto en el formulario
            lblBienvenida.Left = (this.ClientSize.Width - lblBienvenida.Width) / 2;
        }
    }
}