using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public partial class CobroForm : Form
    {
        private static readonly CultureInfo Argentina = CultureInfo.GetCultureInfo("es-AR");
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
        private Button btnConfirmar;

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
            txtEfectivo.Click += SeleccionarTodo;
            txtTransferencia.Click += SeleccionarTodo;
            txtTarjeta.Click += SeleccionarTodo;

            lblFaltaMonto = new Label { Text = $"Falta: $ {totalAPagar:N2}", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.DarkOrange, Location = new Point(30, 240), AutoSize = true };

            Label lblVuelto = new Label { Text = "Vuelto:", Font = new Font("Segoe UI", 12), ForeColor = Color.Gray, Location = new Point(30, 285), AutoSize = true };
            lblVueltoMonto = new Label { Text = "$ 0.00", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.DarkRed, Location = new Point(25, 305), AutoSize = true };

            btnConfirmar = new Button { Text = "CONFIRMAR PAGO (Enter)", Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = Color.LightGreen, FlatStyle = FlatStyle.Flat, Location = new Point(30, 360), Size = new Size(320, 45), Cursor = Cursors.Hand };
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

        internal static bool IntentarLeerImporte(string texto, out decimal valor)
        {
            valor = 0;
            texto = texto.Trim();
            if (texto.Length == 0 || texto.StartsWith('-') || texto.StartsWith('+')) return false;

            int coma = texto.IndexOf(',');
            if (coma >= 0 && (texto.LastIndexOf(',') != coma || texto.Length - coma - 1 is < 1 or > 2))
                return false;

            string entero = coma >= 0 ? texto[..coma] : texto;
            string fraccion = coma >= 0 ? texto[(coma + 1)..] : "";
            bool agrupado = entero.Contains('.');
            if (agrupado)
            {
                string[] grupos = entero.Split('.');
                if (grupos.Length < 2 || grupos[0].Length is < 1 or > 3
                    || grupos.Skip(1).Any(g => g.Length != 3)
                    || grupos.Any(g => g.Any(c => !char.IsAsciiDigit(c))))
                    return false;
            }
            else if (entero.Length == 0 || entero.Any(c => !char.IsAsciiDigit(c)))
                return false;
            if (fraccion.Any(c => !char.IsAsciiDigit(c))) return false;

            return decimal.TryParse(texto,
                NumberStyles.AllowDecimalPoint | (agrupado ? NumberStyles.AllowThousands : NumberStyles.None),
                Argentina, out valor) && valor >= 0
                   && decimal.Round(valor, 2, MidpointRounding.AwayFromZero) == valor;
        }

        private void SeleccionarTodo(object sender, EventArgs e)
        {
            (sender as TextBox).SelectAll();
        }

        private void Recalcular(object sender, EventArgs e)
        {
            decimal ef = 0, tr = 0, ta = 0;
            bool efValido = IntentarLeerImporte(txtEfectivo.Text, out ef);
            bool trValido = IntentarLeerImporte(txtTransferencia.Text, out tr);
            bool taValido = IntentarLeerImporte(txtTarjeta.Text, out ta);
            bool valido = efValido && trValido && taValido;
            if (!valido)
            {
                lblFaltaMonto.Text = "Revise los importes. Use, por ejemplo, 15450,50 o 15.450,50.";
                lblFaltaMonto.Visible = true;
                lblVueltoMonto.Text = "$ 0,00";
                lblVueltoMonto.ForeColor = Color.DarkRed;
                btnConfirmar.Enabled = false;
                return;
            }

            decimal otrosMedios = tr + ta;
            if (otrosMedios > totalAPagar)
            {
                lblFaltaMonto.Text = "Transferencia y tarjeta no pueden superar el total de la venta.";
                lblFaltaMonto.Visible = true;
                lblVueltoMonto.Text = "$ 0,00";
                lblVueltoMonto.ForeColor = Color.DarkRed;
                btnConfirmar.Enabled = false;
                return;
            }

            decimal ingresado = ef + tr + ta;
            decimal diferencia = ingresado - totalAPagar;
            btnConfirmar.Enabled = ingresado >= totalAPagar;

            if (diferencia < 0)
            {
                lblFaltaMonto.Text = $"Falta: $ {Math.Abs(diferencia).ToString("N2", Argentina)}";
                lblFaltaMonto.Visible = true;
                lblVueltoMonto.Text = "$ 0,00";
                lblVueltoMonto.ForeColor = Color.DarkRed;
            }
            else
            {
                lblFaltaMonto.Visible = false;
                // Solo el efectivo puede generar vuelto; los demás medios ya están limitados al total.
                lblVueltoMonto.Text = $"$ {Math.Max(0, diferencia).ToString("N2", Argentina)}";
                lblVueltoMonto.ForeColor = Color.DarkGreen;
            }
        }

        private void BtnConfirmar_Click(object sender, EventArgs e)
        {
            if (!IntentarLeerImporte(txtEfectivo.Text, out decimal ef)
                || !IntentarLeerImporte(txtTransferencia.Text, out decimal tr)
                || !IntentarLeerImporte(txtTarjeta.Text, out decimal ta))
            {
                MessageBox.Show(this, "Revise los importes. Se admiten hasta dos decimales y formato argentino.", "Importe inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (tr + ta <= totalAPagar && ef + tr + ta >= totalAPagar)
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
