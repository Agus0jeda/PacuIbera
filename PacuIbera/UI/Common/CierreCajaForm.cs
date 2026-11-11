using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Datos;
using PacuIbera.Dominio;

namespace PacuIbera.UI.Common
{
    public partial class CierreCajaForm : Form
    {
        private readonly CajaDatos cajaDatos = new();
        private readonly CultureInfo cultura = CultureInfo.GetCultureInfo("es-AR");
        private readonly Label lblCaja = new();
        private readonly Label lblFondo = new();
        private readonly Label lblCobros = new();
        private readonly Label lblIngresos = new();
        private readonly Label lblRetiros = new();
        private readonly Label lblEsperado = new();
        private readonly Label lblDiferencia = new();
        private readonly Label lblEstado = new();
        private readonly TextBox txtContado = new();
        private readonly Button btnCerrar = new();

        public bool CierreExitoso { get; private set; }
        private CajaDatos.ResumenCierreCaja? resumenActual;

        public CierreCajaForm()
        {
            ConfigurarInterfaz();
            Load += (_, _) => CargarResumen();
        }

        private void ConfigurarInterfaz()
        {
            Text = "Cierre de caja";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(700, 570);
            MinimumSize = new Size(700, 570);
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10F);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(26, 20, 26, 18),
                ColumnCount = 2,
                RowCount = 4,
                BackColor = Color.White
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));

            var header = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            header.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            header.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            var title = new Label
            {
                Text = "Cierre de turno",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 91, 62),
                TextAlign = ContentAlignment.MiddleLeft
            };
            lblCaja.Dock = DockStyle.Fill;
            lblCaja.TextAlign = ContentAlignment.MiddleLeft;
            lblCaja.ForeColor = Color.FromArgb(80, 80, 80);
            header.Controls.Add(title, 0, 0);
            header.Controls.Add(lblCaja, 0, 1);
            layout.Controls.Add(header, 0, 0);
            layout.SetColumnSpan(header, 2);

            var resumen = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7,
                Padding = new Padding(0, 8, 0, 4)
            };
            resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            for (int i = 0; i < 7; i++) resumen.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 7));

            AgregarFila(resumen, 0, "Fondo inicial", lblFondo);
            AgregarFila(resumen, 1, "Cobros en efectivo retenidos (sin vuelto)", lblCobros);
            AgregarFila(resumen, 2, "Ingresos adicionales", lblIngresos);
            AgregarFila(resumen, 3, "Retiros", lblRetiros);
            AgregarFila(resumen, 4, "Efectivo esperado", lblEsperado, true);
            AgregarFila(resumen, 5, "Efectivo contado por el Vendedor", txtContado);
            AgregarFila(resumen, 6, "Diferencia (contado - esperado)", lblDiferencia, true);
            txtContado.Dock = DockStyle.Fill;
            txtContado.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            txtContado.TextAlign = HorizontalAlignment.Right;
            txtContado.Margin = new Padding(6, 6, 6, 6);
            txtContado.MaxLength = 20;
            txtContado.KeyPress += TxtContado_KeyPress;
            txtContado.TextChanged += (_, _) => ActualizarDiferencia();
            layout.Controls.Add(resumen, 0, 1);
            layout.SetColumnSpan(resumen, 2);

            lblEstado.Dock = DockStyle.Fill;
            lblEstado.TextAlign = ContentAlignment.MiddleLeft;
            lblEstado.ForeColor = Color.FromArgb(80, 80, 80);
            lblEstado.Padding = new Padding(0, 6, 0, 0);
            layout.Controls.Add(lblEstado, 0, 2);
            layout.SetColumnSpan(lblEstado, 2);

            var botones = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(0, 8, 0, 0)
            };
            btnCerrar.Text = "CONFIRMAR CIERRE";
            btnCerrar.Size = new Size(190, 40);
            btnCerrar.FlatStyle = FlatStyle.Flat;
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.BackColor = ColorTranslator.FromHtml("#88E788");
            btnCerrar.ForeColor = Color.Black;
            btnCerrar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCerrar.Cursor = Cursors.Hand;
            btnCerrar.Enabled = false;
            btnCerrar.Click += BtnCerrar_Click;

            var btnCancelar = new Button
            {
                Text = "CANCELAR",
                Size = new Size(120, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(238, 241, 238),
                ForeColor = Color.FromArgb(50, 70, 58),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            botones.Controls.Add(btnCerrar);
            botones.Controls.Add(btnCancelar);
            layout.Controls.Add(botones, 0, 3);
            layout.SetColumnSpan(botones, 2);
            CancelButton = btnCancelar;
            AcceptButton = btnCerrar;
            Controls.Add(layout);
        }

        private static void AgregarFila(TableLayoutPanel tabla, int fila, string texto, Control valor, bool destacado = false)
        {
            var etiqueta = new Label
            {
                Text = texto,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(65, 65, 65),
                Padding = new Padding(6, 0, 4, 0),
                AutoEllipsis = true
            };
            valor.Dock = DockStyle.Fill;
            valor.Margin = new Padding(6, 5, 6, 5);
            if (valor is Label valorLabel) valorLabel.TextAlign = ContentAlignment.MiddleRight;
            valor.Font = new Font("Segoe UI", destacado ? 11F : 10F, destacado ? FontStyle.Bold : FontStyle.Regular);
            if (destacado) valor.ForeColor = Color.FromArgb(34, 91, 62);
            tabla.Controls.Add(etiqueta, 0, fila);
            tabla.Controls.Add(valor, 1, fila);
        }

        private void CargarResumen()
        {
            resumenActual = null;
            if (SesionActiva.Rol != "Vendedor" || SesionActiva.IdUsuario <= 0 || SesionActiva.IdCaja <= 0)
            {
                MostrarBloqueo("Solo el Vendedor conectado puede consultar y cerrar su caja.");
                return;
            }

            try
            {
                resumenActual = cajaDatos.ObtenerResumenCierreCaja(SesionActiva.IdCaja, SesionActiva.IdUsuario);
                if (resumenActual == null)
                {
                    MostrarBloqueo("No se encontró una caja abierta para este turno. Actualizá la sesión antes de continuar.");
                    return;
                }

                lblCaja.Text = $"Caja N.º {resumenActual.CajaId} · {SesionActiva.Nombre} {SesionActiva.Apellido}".Trim();
                lblFondo.Text = FormatearImporte(resumenActual.FondoInicial);
                lblCobros.Text = FormatearImporte(resumenActual.CobrosEfectivo);
                lblIngresos.Text = FormatearImporte(resumenActual.Ingresos);
                lblRetiros.Text = FormatearImporte(resumenActual.Retiros);
                lblEsperado.Text = resumenActual.EfectivoEsperado.HasValue
                    ? FormatearImporte(resumenActual.EfectivoEsperado.Value)
                    : "No disponible";

                if (resumenActual.Bloqueada || !resumenActual.EfectivoEsperado.HasValue)
                {
                    MostrarBloqueo(TraducirObservacion(resumenActual.Observacion));
                    return;
                }

                lblEstado.Text = "El resumen se actualiza desde SQL. El cierre volverá a calcularlo antes de guardar.";
                lblEstado.ForeColor = Color.FromArgb(80, 80, 80);
                btnCerrar.Enabled = true;
                ActualizarDiferencia();
            }
            catch (Exception ex)
            {
                MostrarBloqueo("No se pudo consultar el efectivo de la caja. " + ex.Message);
            }
        }

        private void MostrarBloqueo(string mensaje)
        {
            lblEstado.Text = mensaje;
            lblEstado.ForeColor = Color.FromArgb(170, 45, 45);
            btnCerrar.Enabled = false;
        }

        private static string TraducirObservacion(string observacion)
        {
            if (observacion.Contains("anuladas", StringComparison.OrdinalIgnoreCase))
                return "No se puede cerrar: hay efectivo asociado a ventas anuladas cuya devolución no está verificada.";
            if (observacion.Contains("pagos desconocidos", StringComparison.OrdinalIgnoreCase))
                return "No se puede calcular el efectivo: hay pagos incompletos o que no concilian.";
            if (observacion.Contains("medios de pago", StringComparison.OrdinalIgnoreCase))
                return "No se puede calcular el efectivo: hay que revisar los medios de pago configurados.";
            if (observacion.Contains("fondo inicial", StringComparison.OrdinalIgnoreCase))
                return "No se puede calcular el efectivo: falta el fondo inicial de la caja.";
            return "No se puede calcular el efectivo esperado con los movimientos registrados. Consultá al Administrador.";
        }

        private static string FormatearImporte(decimal importe) => importe.ToString("C2", CultureInfo.GetCultureInfo("es-AR"));

        private static bool TryParseImporte(string texto, out decimal importe)
        {
            importe = 0;
            texto = texto.Trim();
            if (texto.Length == 0 || texto.Any(c => !char.IsDigit(c) && c != '.' && c != ',')) return false;
            if (texto.Count(c => c == ',') > 1) return false;

            int coma = texto.IndexOf(',');
            string parteEntera = coma >= 0 ? texto[..coma] : texto;
            string parteDecimal = coma >= 0 ? texto[(coma + 1)..] : string.Empty;
            if (parteEntera.Length == 0 || (coma >= 0 && (parteDecimal.Length is < 1 or > 2 || !parteDecimal.All(char.IsDigit)))) return false;

            if (parteEntera.Contains('.'))
            {
                string[] grupos = parteEntera.Split('.');
                if (grupos.Length < 2 || grupos[0].Length is < 1 or > 3 || !grupos[0].All(char.IsDigit)
                    || grupos.Skip(1).Any(g => g.Length != 3 || !g.All(char.IsDigit))) return false;
            }
            else if (!parteEntera.All(char.IsDigit)) return false;

            return decimal.TryParse(texto, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint,
                CultureInfo.GetCultureInfo("es-AR"), out importe)
                && importe >= 0
                && importe <= 9999999999999999.99m;
        }

        private void TxtContado_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != ',' && e.KeyChar != '.')
                e.Handled = true;
        }

        private void ActualizarDiferencia()
        {
            if (resumenActual?.EfectivoEsperado is not decimal esperado || !TryParseImporte(txtContado.Text, out decimal contado))
            {
                lblDiferencia.Text = "—";
                return;
            }

            decimal diferencia = contado - esperado;
            lblDiferencia.Text = (diferencia > 0 ? "+" : string.Empty) + FormatearImporte(diferencia);
            lblDiferencia.ForeColor = diferencia == 0 ? Color.FromArgb(34, 91, 62) : Color.FromArgb(155, 87, 31);
        }

        private void BtnCerrar_Click(object? sender, EventArgs e)
        {
            if (SesionActiva.Rol != "Vendedor" || SesionActiva.IdUsuario <= 0 || SesionActiva.IdCaja <= 0)
            {
                MostrarBloqueo("Solo el Vendedor conectado puede cerrar su caja.");
                return;
            }
            if (!TryParseImporte(txtContado.Text, out decimal montoContado))
            {
                MessageBox.Show("Ingresá el efectivo contado con formato argentino, por ejemplo 15.450,50.",
                    "Revisar importe", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtContado.Focus();
                return;
            }

            btnCerrar.Enabled = false;
            UseWaitCursor = true;
            try
            {
                // Refrescar antes de pedir confirmación. El procedimiento de cierre
                // recalcula otra vez bajo sus bloqueos y devuelve el resultado definitivo.
                CargarResumen();
                if (resumenActual == null || resumenActual.Bloqueada || !resumenActual.EfectivoEsperado.HasValue) return;

                decimal diferenciaPrevista = montoContado - resumenActual.EfectivoEsperado.Value;
                DialogResult respuesta = MessageBox.Show(
                    $"¿Confirmás el cierre de la caja N.º {SesionActiva.IdCaja}?\n\n" +
                    $"Efectivo esperado: {FormatearImporte(resumenActual.EfectivoEsperado.Value)}\n" +
                    $"Efectivo contado: {FormatearImporte(montoContado)}\n" +
                    $"Diferencia: {FormatearImporte(diferenciaPrevista)}",
                    "Confirmar cierre", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (respuesta != DialogResult.Yes) return;

                btnCerrar.Enabled = false;
                CajaDatos.ResumenCierreCaja resultado = cajaDatos.CerrarCaja(
                    SesionActiva.IdCaja, montoContado, SesionActiva.IdUsuario);

                MessageBox.Show(
                    $"El turno se cerró correctamente.\n\n" +
                    $"Efectivo esperado: {FormatearImporte(resultado.EfectivoEsperado!.Value)}\n" +
                    $"Efectivo contado: {FormatearImporte(resultado.EfectivoContado!.Value)}\n" +
                    $"Diferencia: {FormatearImporte(resultado.Diferencia!.Value)}",
                    "Cierre confirmado", MessageBoxButtons.OK, MessageBoxIcon.Information);

                SesionActiva.IdCaja = 0;
                SesionActiva.IdUsuario = 0;
                SesionActiva.Nombre = string.Empty;
                SesionActiva.Apellido = string.Empty;
                SesionActiva.Rol = string.Empty;
                CierreExitoso = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo confirmar el resultado del cierre. Verificá el estado de la caja antes de intentar nuevamente.\n\n" + ex.Message,
                    "Error al cerrar caja", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnCerrar.Enabled = false;
            }
            finally
            {
                UseWaitCursor = false;
            }
        }
    }
}
