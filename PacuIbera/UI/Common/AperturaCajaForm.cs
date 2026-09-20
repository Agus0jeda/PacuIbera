using System;
using System.Windows.Forms;
using Datos; // Para usar CajaDatos

namespace PacuIbera.UI.Common
{
    partial class AperturaCajaForm : Form
    {
        private int usuarioIdSesion;

        // Modificamos el constructor para recibir el ID del usuario logueado
        public AperturaCajaForm(int usuarioId)
        {
            InitializeComponent();
            this.usuarioIdSesion = usuarioId;

            this.BackColor = ColorTranslator.FromHtml("#88E788");
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(txtMonto.Text.Trim(), out decimal montoInicial) || montoInicial < 0)
            {
                MessageBox.Show("Por favor, ingresá un monto válido (solo números, sin letras).", "Monto inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Registramos la apertura en la Base de Datos con tu clase CajaDatos
                CajaDatos cajaDatos = new CajaDatos();
                int nuevaCajaId = cajaDatos.AbrirCaja(usuarioIdSesion, montoInicial);

                MessageBox.Show($"¡Caja abierta con éxito! (ID de Caja: {nuevaCajaId}) - Saldo inicial: ${montoInicial}", "Apertura de Caja", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al registrar la apertura de caja: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AperturaCajaForm_Load(object sender, EventArgs e) { }
    }
}