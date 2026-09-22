using System;
using System.Drawing;
using System.Windows.Forms;
using Datos; // Ajustá al namespace de tu CajaDatos
using PacuIbera.Dominio; // Ajustá al namespace de tu SesionActiva

namespace PacuIbera.UI.Common
{
    public partial class CierreCajaForm : Form
    {
        // Variable para avisarle al menú principal si se cerró correctamente
        public bool CierreExitoso { get; private set; } = false;

        private TextBox txtMontoFinal;
        private Button btnCerrar;

        public CierreCajaForm()
        {
            ConfigurarInterfaz();
        }

        private void ConfigurarInterfaz()
        {
            this.Text = "Cierre de Caja";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            Label lblTitulo = new Label
            {
                Text = "¿Cuánto efectivo físico hay en caja?",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(35, 30),
                AutoSize = true
            };

            Label lblSimbolo = new Label
            {
                Text = "$",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(90, 80),
                AutoSize = true
            };

            txtMontoFinal = new TextBox
            {
                Font = new Font("Segoe UI", 16),
                Location = new Point(120, 77),
                Width = 150,
                TextAlign = HorizontalAlignment.Right
            };
            // Controlamos que solo se puedan ingresar números
            txtMontoFinal.KeyPress += TxtMontoFinal_KeyPress;

            btnCerrar = new Button
            {
                Text = "CERRAR TURNO",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = ColorTranslator.FromHtml("#88E788"),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(100, 140),
                Size = new Size(180, 40),
                Cursor = Cursors.Hand
            };
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.Click += BtnCerrar_Click;

            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblSimbolo);
            this.Controls.Add(txtMontoFinal);
            this.Controls.Add(btnCerrar);
        }

        private void TxtMontoFinal_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != ',') && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
        }

        private void BtnCerrar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMontoFinal.Text))
            {
                MessageBox.Show("Por favor, ingresá el monto final.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMontoFinal.Focus();
                return;
            }

            try
            {
                // Reemplazamos punto por coma por seguridad regional y convertimos
                decimal montoFinal = Convert.ToDecimal(txtMontoFinal.Text.Replace(".", ","));

                DialogResult dialogo = MessageBox.Show($"¿Estás seguro que querés cerrar la caja declarando ${montoFinal} en efectivo?", "Confirmar Cierre", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogo == DialogResult.Yes)
                {
                    // Guardamos el cierre en SQL 
                    CajaDatos cajaDatos = new CajaDatos();
                    cajaDatos.CerrarCaja(SesionActiva.IdCaja, montoFinal);

                    MessageBox.Show("¡Turno cerrado con éxito! El sistema volverá al inicio de sesión.", "Cierre de Caja", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Vaciamos la sesión global por seguridad
                    SesionActiva.IdCaja = 0;
                    SesionActiva.IdUsuario = 0;
                    SesionActiva.Nombre = "";

                    CierreExitoso = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cerrar la caja: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}