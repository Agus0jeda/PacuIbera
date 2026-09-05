using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public partial class AperturaCajaForm : Form
    {
        public AperturaCajaForm()
        {
            InitializeComponent();

            this.BackColor = ColorTranslator.FromHtml("#88E788");

            // Configuraciones visuales para que quede prolijo
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Intentamos convertir el texto a un número decimal (por si ponen centavos)
            if (!decimal.TryParse(txtMonto.Text.Trim(), out decimal montoInicial) || montoInicial < 0)
            {
                MessageBox.Show("Por favor, ingresá un monto válido (solo números, sin letras).", "Monto inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Frenamos la ejecución
            }

            // falta la tabla inicio de caja en la bdd
            MessageBox.Show($"¡Caja abierta con éxito! Saldo inicial: ${montoInicial}", "Apertura de Caja", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //// creamos y abrimos el Menú Principal
            //MainForm ventanaPrincipal = new MainForm();
            //Nuevo Menu Principal
            PrincipalForm ventanaPrincipal = new PrincipalForm();

            // le decimos a la app que se cierre por completo si el usuario cierra el Menú Principal
            ventanaPrincipal.FormClosed += (s, args) => Application.Exit();

            this.Hide(); // Escondemos esta ventana de caja
            ventanaPrincipal.Show();


            

        }

        private void AperturaCajaForm_Load(object sender, EventArgs e)
        {

        }
    }
}
