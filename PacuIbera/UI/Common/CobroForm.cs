using System;
using System.Drawing;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public partial class CobroForm : Form
    {
        private decimal totalAPagar;

        // Propiedades públicas para que VentasForm sepa exactamente cuánto entró de cada cosa
        public decimal PagoEfectivo { get; private set; } = 0;
        public decimal PagoTransferencia { get; private set; } = 0;
        public decimal PagoTarjeta { get; private set; } = 0;

        private TextBox txtEfectivo;
        private TextBox txtTransferencia;
        private TextBox txtTarjeta;
        private Label lblVueltoMonto;
        private Label lblFaltaMonto;

        public CobroForm(decimal total)
        {
            InitializeComponent();
            totalAPagar = total;
            ConfigurarInterfaz();
        }

        private void ConfigurarInterfaz()
        {
            this.Text = "Cobro Mixto";
            this.Size = new Size(400, 470);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.KeyPreview = true;

            Label lblTotal = new Label { Text = "TOTAL A PAGAR:", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(30, 15), AutoSize = true };
            Label lblTotalMonto = new Label { Text = $"$ {totalAPagar:N2}", Font = new Font("Segoe UI", 26, FontStyle.Bold), ForeColor = Color.DarkGreen, Location = new Point(25, 35), AutoSize = true };

            // Cajas de texto para los pagos mixtos
            Label lblEfectivo = new Label { Text = "💵 Efectivo:", Font = new Font("Segoe UI", 12), Location = new Point(30, 100), AutoSize = true };
            txtEfectivo = new TextBox { Location = new Point(180, 95), Width = 170, Font = new Font("Segoe UI", 14), Text = "0" };

            Label lblTransf = new Label { Text = "📱 Transferencia:", Font = new Font("Segoe UI", 12), Location = new Point(30, 145), AutoSize = true };
            txtTransferencia = new TextBox { Location = new Point(180, 140), Width = 170, Font = new Font("Segoe UI", 14), Text = "0" };

            Label lblTarjeta = new Label { Text = "💳 Tarjeta:", Font = new Font("Segoe UI", 12), Location = new Point(30, 190), AutoSize = true };
            txtTarjeta = new TextBox { Location = new Point(180, 185), Width = 170, Font = new Font("Segoe UI", 14), Text = "0" };

            // Eventos para calcular todo en vivo
            txtEfectivo.TextChanged += Recalcular;
            txtTransferencia.TextChanged += Recalcular;
            txtTarjeta.TextChanged += Recalcular;
            txtEfectivo.KeyPress += SoloNumeros;
            txtTransferencia.KeyPress += SoloNumeros;
            txtTarjeta.KeyPress += SoloNumeros;
            txtEfectivo.Click += SeleccionarTodo;
            txtTransferencia.Click += SeleccionarTodo;
            txtTarjeta.Click += SeleccionarTodo;

            lblFaltaMonto = new Label { Text = $"Falta: $ {totalAPagar:N2}", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.DarkOrange, Location = new Point(30, 240), AutoSize = true };

            Label lblVuelto = new Label { Text = "Vuelto:", Font = new Font("Segoe UI", 12), ForeColor = Color.Gray, Location = new Point(30, 285), AutoSize = true };
            lblVueltoMonto = new Label { Text = "$ 0.00", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.DarkRed, Location = new Point(25, 305), AutoSize = true };

            Button btnConfirmar = new Button { Text = "CONFIRMAR PAGO (Enter)", Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = Color.LightGreen, FlatStyle = FlatStyle.Flat, Location = new Point(30, 360), Size = new Size(320, 45), Cursor = Cursors.Hand };
            btnConfirmar.FlatAppearance.BorderSize = 0;
            btnConfirmar.Click += BtnConfirmar_Click;

            this.Controls.Add(lblTotal); this.Controls.Add(lblTotalMonto);
            this.Controls.Add(lblEfectivo); this.Controls.Add(txtEfectivo);
            this.Controls.Add(lblTransf); this.Controls.Add(txtTransferencia);
            this.Controls.Add(lblTarjeta); this.Controls.Add(txtTarjeta);
            this.Controls.Add(lblFaltaMonto);
            this.Controls.Add(lblVuelto); this.Controls.Add(lblVueltoMonto);
            this.Controls.Add(btnConfirmar);

            this.AcceptButton = btnConfirmar;
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.DialogResult = DialogResult.Cancel; };
            this.Shown += (s, e) => txtEfectivo.Focus();
        }

        private void SoloNumeros(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != ',') e.Handled = true;
            if (e.KeyChar == ',' && (sender as TextBox).Text.IndexOf(',') > -1) e.Handled = true;
        }

        private void SeleccionarTodo(object sender, EventArgs e)
        {
            (sender as TextBox).SelectAll();
        }

        private void Recalcular(object sender, EventArgs e)
        {
            decimal.TryParse(txtEfectivo.Text, out decimal ef);
            decimal.TryParse(txtTransferencia.Text, out decimal tr);
            decimal.TryParse(txtTarjeta.Text, out decimal ta);

            decimal ingresado = ef + tr + ta;
            decimal diferencia = ingresado - totalAPagar;

            if (diferencia < 0)
            {
                lblFaltaMonto.Text = $"Falta: $ {Math.Abs(diferencia):N2}";
                lblFaltaMonto.Visible = true;
                lblVueltoMonto.Text = "$ 0.00";
                lblVueltoMonto.ForeColor = Color.DarkRed;
            }
            else
            {
                lblFaltaMonto.Visible = false;
                // El vuelto siempre se da en efectivo, por lógica de caja
                lblVueltoMonto.Text = $"$ {diferencia:N2}";
                lblVueltoMonto.ForeColor = Color.DarkGreen;
            }
        }

        private void BtnConfirmar_Click(object sender, EventArgs e)
        {
            decimal.TryParse(txtEfectivo.Text, out decimal ef);
            decimal.TryParse(txtTransferencia.Text, out decimal tr);
            decimal.TryParse(txtTarjeta.Text, out decimal ta);

            if ((ef + tr + ta) >= totalAPagar)
            {
                // Guardamos los montos limpios en las propiedades públicas
                PagoEfectivo = ef;
                PagoTransferencia = tr;
                PagoTarjeta = ta;

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("El monto ingresado no cubre el total de la venta.", "Falta dinero", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}