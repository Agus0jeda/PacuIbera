using Datos;
using PacuIbera.Dominio;

namespace PacuIbera.UI.Common;

public partial class PrincipalForm
{
    private Button btnMovimientosCaja = null!;
    private Button btnHistorialMovimientosCaja = null!;

    private void CrearEntradaMovimientosCaja()
    {
        btnMovimientosCaja = new Button
        {
            Name = "btnMovimientosCaja", Text = "Movimientos de efectivo",
            Location = btnDeposito.Location, Size = btnDeposito.Size,
            FlatStyle = btnDeposito.FlatStyle, Font = btnDeposito.Font,
            ForeColor = btnDeposito.ForeColor, BackColor = btnDeposito.BackColor,
            Image = btnPagos.Image, ImageAlign = btnPagos.ImageAlign,
            TextAlign = btnDeposito.TextAlign, TextImageRelation = btnPagos.TextImageRelation,
            Margin = btnDeposito.Margin, UseVisualStyleBackColor = btnDeposito.UseVisualStyleBackColor,
            Cursor = Cursors.Hand, Visible = false
        };
        btnMovimientosCaja.FlatAppearance.BorderSize = 0;
        btnMovimientosCaja.FlatAppearance.MouseOverBackColor = btnDeposito.FlatAppearance.MouseOverBackColor;
        btnMovimientosCaja.Click += async (_, _) =>
        {
            if (SesionActiva.IdUsuario <= 0 || SesionActiva.Rol != "Administrador")
            {
                MessageBox.Show(this, "Solo un Administrador activo puede registrar movimientos de efectivo.",
                    "Acceso restringido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnMovimientosCaja.Enabled = false;
            try
            {
                await new MovimientoCajaDatos().ValidarAdministradorActivoAsync();
                if (!IsDisposed) AbrirFormularioPanel(new FormMovimientosCaja(), btnMovimientosCaja);
            }
            catch
            {
                if (!IsDisposed) MessageBox.Show(this,
                    "No se pudo verificar el Administrador activo o la base configurada.",
                    "Movimientos de efectivo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                if (!IsDisposed) btnMovimientosCaja.Enabled = true;
            }
        };
        MenuVertical.Controls.Add(btnMovimientosCaja);
    }

    private void CrearEntradaHistorialMovimientosCaja()
    {
        btnHistorialMovimientosCaja = new Button
        {
            Name = "btnHistorialMovimientosCaja", Text = "Historial de caja",
            Location = btnMovimientosCaja.Location, Size = btnMovimientosCaja.Size,
            FlatStyle = btnMovimientosCaja.FlatStyle, Font = btnMovimientosCaja.Font,
            ForeColor = btnMovimientosCaja.ForeColor, BackColor = btnMovimientosCaja.BackColor,
            Image = btnHistorial.Image, ImageAlign = btnHistorial.ImageAlign,
            TextAlign = btnHistorial.TextAlign, TextImageRelation = btnHistorial.TextImageRelation,
            Margin = btnHistorial.Margin, UseVisualStyleBackColor = btnHistorial.UseVisualStyleBackColor,
            Cursor = Cursors.Hand, Visible = false
        };
        btnHistorialMovimientosCaja.FlatAppearance.BorderSize = 0;
        btnHistorialMovimientosCaja.FlatAppearance.MouseOverBackColor = btnHistorial.FlatAppearance.MouseOverBackColor;
        btnHistorialMovimientosCaja.Click += async (_, _) =>
        {
            if (!MovimientoCajaDatos.PuedeConsultarHistorial)
            {
                MessageBox.Show(this, "Solo un Administrador o Gerente activo puede consultar el historial.",
                    "Acceso restringido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnHistorialMovimientosCaja.Enabled = false;
            try
            {
                await new MovimientoCajaDatos().ValidarAccesoHistorialAsync();
                if (!IsDisposed) AbrirFormularioPanel(new FormHistorialMovimientosCaja(), btnHistorialMovimientosCaja);
            }
            catch
            {
                if (!IsDisposed) MessageBox.Show(this,
                    "No se pudo verificar el acceso o la conexión a la base configurada.",
                    "Historial de caja", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                if (!IsDisposed) btnHistorialMovimientosCaja.Enabled = true;
            }
        };
        MenuVertical.Controls.Add(btnHistorialMovimientosCaja);
    }

    private void ConfigurarAccesoMovimientosCaja()
    {
        if (btnMovimientosCaja != null)
            btnMovimientosCaja.Visible = SesionActiva.IdUsuario > 0 && SesionActiva.Rol == "Administrador";
    }

    private void ConfigurarAccesoHistorialMovimientosCaja()
    {
        if (btnHistorialMovimientosCaja != null)
            btnHistorialMovimientosCaja.Visible = MovimientoCajaDatos.PuedeConsultarHistorial;
    }
}
