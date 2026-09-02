using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

        }

        private void pnlNavegacion_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pnlContenedor_Paint(object sender, PaintEventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            pnlEncabezado.BackColor = ColorTranslator.FromHtml("#88E788");

            pnlNavegacion.BackColor = ColorTranslator.FromHtml("#5CB85C");

            pnlContenedor.BackColor = ColorTranslator.FromHtml("#5CB85C");
        }

        // Variable para llevar el control de qué pantalla está abierta
        private Form? formularioActivo = null;

        // Función  para incrustar ventanas en tu panel central
        private void AbrirFormularioEnPanel(Form nuevoFormulario)
        {
            // Si ya hay una pantalla abierta, la cerramos para no acumular memoria
            if (formularioActivo != null)
            {
                formularioActivo.Close();
            }

            formularioActivo = nuevoFormulario;

            nuevoFormulario.TopLevel = false;
            nuevoFormulario.FormBorderStyle = FormBorderStyle.None;
            nuevoFormulario.Dock = DockStyle.Fill;

            // Le damos el mismo verde de la barra de botones (#5CB85C)
            nuevoFormulario.BackColor = ColorTranslator.FromHtml("#5CB85C");

            pnlContenedor.Controls.Add(nuevoFormulario);
            pnlContenedor.Tag = nuevoFormulario;
            nuevoFormulario.BringToFront();
            nuevoFormulario.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AbrirFormularioEnPanel(new DashboardForm());
        }
    }
}
