using System.Globalization;

namespace PacuIbera.UI.Common
{
    public sealed class FormStockMinimo : Form
    {
        private sealed class CantidadMinimo : NumericUpDown
        {
            public bool IntentarLeer(out decimal valor) =>
                decimal.TryParse(Text, NumberStyles.Number, CultureInfo.CurrentCulture, out valor)
                && valor >= Minimum && valor <= Maximum && decimal.Round(valor, DecimalPlaces) == valor;

            protected override void ValidateEditText()
            {
                if (!IntentarLeer(out _))
                {
                    BackColor = Color.MistyRose;
                    return; // Conservar el texto invalido, sin redondearlo ni limitarlo.
                }
                BackColor = SystemColors.Window;
                base.ValidateEditText();
            }
        }

        public decimal Minimo { get; private set; }

        public FormStockMinimo(string producto, bool porPeso, decimal actual)
        {
            Text = "Stock mínimo de Depósito";
            Font = new Font("Segoe UI", 9F);
            Size = new Size(580, 320);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            var titulo = new Label { Text = producto, Dock = DockStyle.Top, Height = 54,
                Padding = new Padding(14), BackColor = Color.FromArgb(45, 55, 65), ForeColor = Color.White };
            var cantidad = new CantidadMinimo
            {
                DecimalPlaces = porPeso ? 3 : 0, Minimum = 0, Maximum = porPeso ? 9999999.999M : 9999999M,
                Increment = porPeso ? 0.001M : 1M, Width = 180, Location = new Point(18, 100)
            };
            // No truncar configuraciones anteriores que no respetaban la unidad de venta.
            cantidad.Text = actual.ToString("0.###", CultureInfo.CurrentCulture);
            string unidad = porPeso ? "kg" : "un.";
            var stockActual = new Label
            {
                Text = $"Stock actual: {actual.ToString(porPeso ? "0.###" : "0", CultureInfo.GetCultureInfo("es-AR"))} {unidad}",
                Location = new Point(18, 72), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            var ayuda = new Label { Text = $"El mínimo se expresa en {unidad}. " + (porPeso ? "Hasta tres decimales." : "Solo cantidades enteras.")
                + "\nMínimo cero desactiva la alerta por mínimo.\nEl estado SIN STOCK se mantiene cuando no hay stock utilizable.",
                Location = new Point(18, 164), Size = new Size(530, 65) };
            var guardar = new Button { Text = "Guardar mínimo", AutoSize = true };
            var cancelar = new Button { Text = "Cancelar", AutoSize = true, DialogResult = DialogResult.Cancel };
            EstiloUI.AplicarEstiloBotonAccion(guardar, EstiloUI.TipoAccion.Positivo);
            EstiloUI.AplicarEstiloBotonAccion(cancelar, EstiloUI.TipoAccion.Editar);
            var pie = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 58, Padding = new Padding(10),
                FlowDirection = FlowDirection.RightToLeft, BackColor = Color.WhiteSmoke };
            pie.Controls.Add(cancelar);
            pie.Controls.Add(guardar);
            guardar.Click += (_, _) =>
            {
                if (!cantidad.IntentarLeer(out decimal valor))
                {
                    MessageBox.Show(this, porPeso ? "Ingrese un valor entre 0 y 9999999,999, con hasta tres decimales."
                        : "Ingrese un entero entre 0 y 9999999.", "Stock mínimo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cantidad.Focus();
                    return;
                }
                Minimo = valor;
                DialogResult = DialogResult.OK;
            };
            Controls.Add(ayuda);
            Controls.Add(cantidad);
            Controls.Add(new Label { Text = $"Nuevo mínimo ({unidad}):", Location = new Point(18, 105), AutoSize = true });
            cantidad.Location = new Point(165, 100);
            Controls.Add(stockActual);
            Controls.Add(pie);
            Controls.Add(titulo);
            CancelButton = cancelar;
        }
    }
}
