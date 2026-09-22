using System;
using System.Drawing;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public partial class EnConstruccionForm : Form
    {
        // Modificamos el constructor para que reciba el nombre de la sección
        public EnConstruccionForm(string nombreModulo)
        {
            InitializeComponent();
            ConfigurarPantalla(nombreModulo);
        }

        private void ConfigurarPantalla(string nombreModulo)
        {
            this.BackColor = Color.White; // Fondo limpio

            // Creamos el título dinámico
            Label lblTitulo = new Label();
            lblTitulo.Text = $"Módulo de {nombreModulo}";
            lblTitulo.Font = new Font("Segoe UI", 26, FontStyle.Bold);
            lblTitulo.ForeColor = Color.DarkSlateGray;
            lblTitulo.AutoSize = true;
            lblTitulo.Location = new Point(100, 50);
            this.Controls.Add(lblTitulo);

            // Creamos el subtítulo de aviso
            Label lblMensaje = new Label();
            lblMensaje.Text = "Esta vista aún se encuentra en fase de desarrollo.\nEstará disponible en la próxima actualización del sistema.";
            lblMensaje.Font = new Font("Segoe UI", 14, FontStyle.Regular);
            lblMensaje.ForeColor = Color.Gray;
            lblMensaje.AutoSize = true;
            lblMensaje.Location = new Point(115, 110);
            this.Controls.Add(lblMensaje);

            // Opcional: Un pequeño ícono o emoji usando otra etiqueta
            Label lblIcono = new Label();
            lblIcono.Text = "🚧";
            lblIcono.Font = new Font("Segoe UI", 72);
            lblIcono.AutoSize = true;
            lblIcono.Location = new Point(100, 160);
            this.Controls.Add(lblIcono);
        }
    }
}