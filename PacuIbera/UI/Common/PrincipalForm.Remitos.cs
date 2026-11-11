using PacuIbera.Datos;
using PacuIbera.Dominio;

namespace PacuIbera.UI.Common
{
    public partial class PrincipalForm
    {
        private Button btnConsultaRemitos = null!;

        private void CrearEntradaConsultaRemitos()
        {
            // Acceso independiente: no dar al Vendedor ni al Gerente acceso a Deposito,
            // ni desplazar los botones actuales del menu lateral.
            btnConsultaRemitos = new Button
            {
                Name = "btnConsultaRemitos", Text = "Remitos internos", Location = new Point(60, 2),
                Size = new Size(155, 28), Anchor = AnchorStyles.Top | AnchorStyles.Left,
                FlatStyle = FlatStyle.Flat, BackColor = Color.LightGreen,
                Font = new Font("Segoe UI", 9F), Cursor = Cursors.Hand, Visible = false
            };
            btnConsultaRemitos.FlatAppearance.BorderSize = 0;
            btnConsultaRemitos.Click += async (_, _) =>
            {
                if (!RemitosDatos.PuedeConsultar)
                {
                    MessageBox.Show(this, "Su rol no tiene acceso a la consulta de remitos.");
                    return;
                }
                if (RemitosDatos.PuedePrepararRecepcion)
                {
                    btnConsultaRemitos.Enabled = false;
                    try
                    {
                        await new RemitosDatos().ValidarVendedorActivoAsync();
                        if (IsDisposed) return;
                        var recepcion = new FormRecepcionRemito { FormBorderStyle = FormBorderStyle.None };
                        AbrirFormularioPanel(recepcion, btnConsultaRemitos);
                    }
                    catch (Exception)
                    {
                        if (!IsDisposed) MessageBox.Show(this,
                            "No se pudo abrir la recepción. Compruebe la conexión y que su usuario vendedor esté activo.",
                            "Remitos internos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    finally { if (!IsDisposed) btnConsultaRemitos.Enabled = true; }
                    return;
                }
                if (SesionActiva.Rol == "Gerente")
                {
                    btnConsultaRemitos.Enabled = false;
                    try
                    {
                        await new RemitosDatos().ValidarConsultaRemitosActivaAsync();
                        if (!IsDisposed) AbrirFormularioPanel(new FormRemitos(), btnConsultaRemitos);
                    }
                    catch (Exception)
                    {
                        if (!IsDisposed) MessageBox.Show(this,
                            "No se pudo abrir el historial. Compruebe la conexión y que su usuario Gerente esté activo.",
                            "Remitos internos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    finally { if (!IsDisposed) btnConsultaRemitos.Enabled = true; }
                    return;
                }
                AbrirFormularioPanel(new FormRemitos(), btnConsultaRemitos);
            };
            BarraTitulo.Controls.Add(btnConsultaRemitos);
        }

        private void ConfigurarAccesoConsultaRemitos() =>
            btnConsultaRemitos.Visible = RemitosDatos.PuedeConsultar;

        internal void AbrirRemitosDesdeDeposito()
        {
            if (!RemitosDatos.PuedePrepararEmision) return;
            AbrirFormularioPanel(new FormRemitos(), btnConsultaRemitos);
        }
    }
}
