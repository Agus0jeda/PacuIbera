using System.Drawing;
using System.Globalization;
using Datos;
using PacuIbera.Dominio;

namespace PacuIbera.UI.Common;

public sealed class FormMovimientosCaja : Form
{
    private sealed record CajaOpcion(int CajaId, string Descripcion)
    {
        public override string ToString() => Descripcion;
    }

    private readonly MovimientoCajaDatos datos = new();
    private ComboBox cmbCaja = null!;
    private ComboBox cmbTipo = null!;
    private TextBox txtImporte = null!;
    private TextBox txtMotivo = null!;
    private Label lblDisponible = null!;
    private Label lblEstado = null!;
    private Button btnActualizar = null!;
    private Button btnConfirmar = null!;
    private Button btnCerrar = null!;
    private MovimientoCajaDatos.IntentoMovimiento? intentoPendiente;
    private bool intentoNoVerificable;
    private bool datosCargadosCorrectamente;
    private bool cargando;
    private static readonly CultureInfo Argentina = CultureInfo.GetCultureInfo("es-AR");

    public FormMovimientosCaja()
    {
        if (SesionActiva.Rol != "Administrador" || SesionActiva.IdUsuario <= 0)
            throw new UnauthorizedAccessException("Solo un Administrador activo puede registrar movimientos de efectivo.");

        ConstruirInterfaz();
        Load += async (_, _) => await CargarAsync();
    }

    private void ConstruirInterfaz()
    {
        Text = "Movimientos de efectivo";
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10F);
        AutoScaleMode = AutoScaleMode.Font;
        MinimumSize = new Size(600, 400);
        Size = new Size(650, 450);

        var titulo = new Label
        {
            Text = "MOVIMIENTOS DE EFECTIVO", Dock = DockStyle.Top, Height = 44,
            TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.FromArgb(37, 112, 62),
            ForeColor = Color.White, Font = new Font("Segoe UI", 15F, FontStyle.Bold)
        };
        var contenido = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, Padding = new Padding(20, 10, 20, 8), ColumnCount = 2,
            RowCount = 7, BackColor = Color.White
        };
        contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
        contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        contenido.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        contenido.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        contenido.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        contenido.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        Controls.Add(contenido);
        Controls.Add(titulo);
        titulo.BringToFront();

        cmbCaja = new ComboBox
        {
            Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 9.5F), IntegralHeight = false, DropDownHeight = 180
        };
        btnActualizar = new Button
        {
            Text = "Actualizar", AutoSize = true, Anchor = AnchorStyles.Right,
            FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(232, 245, 232), Cursor = Cursors.Hand
        };
        btnActualizar.FlatAppearance.BorderColor = Color.FromArgb(165, 205, 165);
        btnActualizar.Click += async (_, _) => await CargarAsync();
        contenido.Controls.Add(CrearEtiqueta("Caja abierta"), 0, 0);
        var filaCaja = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        filaCaja.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        filaCaja.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        filaCaja.Controls.Add(cmbCaja, 0, 0);
        filaCaja.Controls.Add(btnActualizar, 1, 0);
        contenido.Controls.Add(filaCaja, 1, 0);

        cmbTipo = new ComboBox { Dock = DockStyle.Left, Width = 190, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbTipo.Items.AddRange(["Ingreso", "Retiro"]);
        cmbTipo.SelectedIndex = 0;
        cmbTipo.SelectedIndexChanged += (_, _) => ActualizarValidacion();
        contenido.Controls.Add(CrearEtiqueta("Tipo de movimiento"), 0, 1);
        contenido.Controls.Add(cmbTipo, 1, 1);

        txtImporte = new TextBox { Dock = DockStyle.Left, Width = 190, Font = new Font("Segoe UI", 12F) };
        txtImporte.TextChanged += (_, _) => ActualizarValidacion();
        var filaImporte = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, FlowDirection = FlowDirection.LeftToRight };
        filaImporte.Controls.Add(txtImporte);
        filaImporte.Controls.Add(new Label
        {
            Text = "ARS · ej.: 1.250,50", AutoSize = true, Margin = new Padding(10, 8, 0, 0),
            ForeColor = Color.DimGray
        });
        contenido.Controls.Add(CrearEtiqueta("Importe"), 0, 2);
        contenido.Controls.Add(filaImporte, 1, 2);

        txtMotivo = new TextBox { Dock = DockStyle.Fill, Multiline = true, MaxLength = 500, ScrollBars = ScrollBars.Vertical };
        txtMotivo.TextChanged += (_, _) => ActualizarValidacion();
        contenido.Controls.Add(CrearEtiqueta("Motivo"), 0, 3);
        contenido.Controls.Add(txtMotivo, 1, 3);

        lblDisponible = new Label
        {
            Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true,
            ForeColor = Color.FromArgb(35, 95, 55), Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };
        contenido.Controls.Add(new Label { Text = "", Dock = DockStyle.Fill }, 0, 4);
        contenido.Controls.Add(lblDisponible, 1, 4);

        lblEstado = new Label
        {
            Dock = DockStyle.Fill, AutoSize = false, ForeColor = Color.FromArgb(90, 90, 90),
            TextAlign = ContentAlignment.MiddleLeft
        };
        contenido.Controls.Add(new Label { Text = "", Dock = DockStyle.Fill }, 0, 5);
        contenido.Controls.Add(lblEstado, 1, 5);

        btnConfirmar = new Button
        {
            Text = "Confirmar movimiento", Width = 190, Height = 38, Anchor = AnchorStyles.Right,
            BackColor = Color.LightGreen, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Cursor = Cursors.Hand, Enabled = false
        };
        btnConfirmar.FlatAppearance.BorderSize = 0;
        btnConfirmar.Click += async (_, _) => await ConfirmarAsync();
        btnCerrar = new Button
        {
            Text = "Cancelar", Width = 110, Height = 38, Anchor = AnchorStyles.Left,
            FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
        };
        btnCerrar.Click += (_, _) => Close();
        contenido.Controls.Add(btnCerrar, 0, 6);
        contenido.Controls.Add(btnConfirmar, 1, 6);
        cmbCaja.SelectedIndexChanged += (_, _) =>
        {
            MostrarDisponible(cajasActuales);
            ActualizarValidacion();
        };
    }

    private static Label CrearEtiqueta(string texto) => new()
    {
        Text = texto, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
        Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(45, 70, 50)
    };

    private async Task CargarAsync()
    {
        if (cargando || IsDisposed) return;
        cargando = true;
        btnActualizar.Enabled = false;
        lblEstado.Text = "Consultando cajas abiertas...";
        try
        {
            if (!MovimientoCajaDatos.DestinoPermitido)
                throw new InvalidOperationException("Esta pantalla requiere un destino PacuIbera permitido.");

            try { intentoPendiente ??= datos.LeerIntentoPendiente(); }
            catch { intentoNoVerificable = true; throw; }
            var cajas = await datos.ObtenerCajasAbiertasAsync();
            if (IsDisposed) return;
            cajasActuales = cajas;
            datosCargadosCorrectamente = true;

            cmbCaja.BeginUpdate();
            cmbCaja.Items.Clear();
            foreach (var caja in cajas)
                cmbCaja.Items.Add(new CajaOpcion(caja.CajaId, caja.Descripcion));

            if (intentoPendiente != null && cajas.All(c => c.CajaId != intentoPendiente.CajaId))
                cmbCaja.Items.Add(new CajaOpcion(intentoPendiente.CajaId,
                    $"Caja N.º {intentoPendiente.CajaId} · intento pendiente"));
            cmbCaja.EndUpdate();

            if (intentoPendiente != null)
            {
                for (int i = 0; i < cmbCaja.Items.Count; i++)
                    if (cmbCaja.Items[i] is CajaOpcion op && op.CajaId == intentoPendiente.CajaId) cmbCaja.SelectedIndex = i;
                cmbTipo.SelectedItem = intentoPendiente.TipoMovimiento;
                txtImporte.Text = intentoPendiente.Importe.ToString("0.00", Argentina);
                txtMotivo.Text = intentoPendiente.Motivo;
                cmbCaja.Enabled = cmbTipo.Enabled = txtImporte.Enabled = txtMotivo.Enabled = false;
                btnConfirmar.Text = "Reintentar mismo movimiento";
                lblEstado.Text = "Hay un intento anterior pendiente. Solo se podrá volver a enviar con la misma clave y los mismos datos.";
                btnConfirmar.Enabled = true;
            }
            else
            {
                if (cmbCaja.Items.Count > 0) cmbCaja.SelectedIndex = 0;
                lblEstado.Text = cajas.Count == 0
                    ? "No hay cajas abiertas para registrar movimientos."
                    : "Los retiros se validan nuevamente al confirmar.";
            }

            MostrarDisponible(cajas);
            ActualizarValidacion();
        }
        catch
        {
            datosCargadosCorrectamente = false;
            lblEstado.Text = intentoNoVerificable
                ? "Hay un intento anterior que no se pudo leer. No se habilitarán movimientos."
                : "No se pudo verificar el Administrador o la base configurada. Actualice la pantalla para volver a consultar.";
            lblDisponible.Text = "Efectivo disponible: no disponible";
            DeshabilitarOperacion();
        }
        finally
        {
            cargando = false;
            if (!IsDisposed) btnActualizar.Enabled = !intentoNoVerificable;
        }
    }

    private void MostrarDisponible(IReadOnlyList<MovimientoCajaDatos.CajaAbiertaMovimiento> cajas)
    {
        if (cmbCaja.SelectedItem is not CajaOpcion seleccionada)
        {
            lblDisponible.Text = "Efectivo disponible: no hay una caja abierta seleccionada.";
            return;
        }
        var caja = cajas.FirstOrDefault(c => c.CajaId == seleccionada.CajaId);
        if (caja == null)
        {
            lblDisponible.Text = intentoPendiente != null
                ? "El efectivo se volverá a validar al comprobar el intento pendiente."
                : "Efectivo disponible: no disponible.";
        }
        else if (caja.EfectivoDisponible.HasValue)
            lblDisponible.Text = $"Efectivo disponible estimado: $ {caja.EfectivoDisponible.Value.ToString("N2", Argentina)}";
        else
            lblDisponible.Text = $"Efectivo disponible: no disponible. {caja.MotivoNoDisponible}";
    }

    private bool IntentarLeerFormulario(out decimal importe, out string error)
    {
        importe = 0;
        error = "";
        if (cmbCaja.SelectedItem is not CajaOpcion)
        { error = "Seleccione una caja abierta."; return false; }
        if (cmbTipo.SelectedItem is not string tipo || tipo is not ("Ingreso" or "Retiro"))
        { error = "Seleccione Ingreso o Retiro."; return false; }
        if (!CobroForm.IntentarLeerImporte(txtImporte.Text, out importe) || importe <= 0
            || importe > 9999999999999999.99m || decimal.Round(importe, 2) != importe)
        { error = "Ingrese un importe positivo con formato argentino y hasta dos decimales."; return false; }
        if (string.IsNullOrWhiteSpace(txtMotivo.Text))
        { error = "El motivo es obligatorio."; return false; }
        if (txtMotivo.Text.Trim().Length > 500)
        { error = "El motivo no puede superar los 500 caracteres."; return false; }

        var caja = cajasActuales.FirstOrDefault(c => c.CajaId == ((CajaOpcion)cmbCaja.SelectedItem).CajaId);
        if (caja == null || !caja.EfectivoDisponible.HasValue)
        { error = caja?.MotivoNoDisponible ?? "No hay una caja abierta disponible."; return false; }
        if (tipo == "Retiro" && importe > caja.EfectivoDisponible.Value)
        { error = "El retiro supera el efectivo disponible."; return false; }
        return true;
    }

    private IReadOnlyList<MovimientoCajaDatos.CajaAbiertaMovimiento> cajasActuales = [];

    private void ActualizarValidacion()
    {
        if (intentoNoVerificable || cargando || intentoPendiente != null) return;
        if (!datosCargadosCorrectamente) { btnConfirmar.Enabled = false; return; }
        try { lblEstado.Text = IntentarLeerFormulario(out _, out string error) ? "" : error; }
        catch { lblEstado.Text = "Revise los datos del movimiento."; }
        btnConfirmar.Enabled = !string.IsNullOrEmpty(lblEstado.Text) ? false
            : cmbCaja.SelectedItem is CajaOpcion && cmbTipo.SelectedItem is string
                && CobroForm.IntentarLeerImporte(txtImporte.Text, out decimal importe) && importe > 0
                && !string.IsNullOrWhiteSpace(txtMotivo.Text) && txtMotivo.Text.Trim().Length <= 500
                && ((string)cmbTipo.SelectedItem != "Retiro" ||
                    (cajasActuales.FirstOrDefault(c => c.CajaId == ((CajaOpcion)cmbCaja.SelectedItem).CajaId)?.EfectivoDisponible is decimal disponible && importe <= disponible));
    }

    private void DeshabilitarOperacion()
    {
        cmbCaja.Enabled = cmbTipo.Enabled = txtImporte.Enabled = txtMotivo.Enabled = false;
        btnConfirmar.Enabled = false;
    }

    private async Task ConfirmarAsync()
    {
        if (intentoNoVerificable || btnConfirmar.Enabled == false) return;
        MovimientoCajaDatos.IntentoMovimiento intento;
        bool eraPendiente = intentoPendiente != null;
        if (eraPendiente)
        {
            intento = intentoPendiente!;
            if (MessageBox.Show(this, "No se pudo comprobar el intento anterior. ¿Reintentar el mismo movimiento con la misma clave?",
                    "Comprobar movimiento", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        }
        else
        {
            if (!IntentarLeerFormulario(out decimal importe, out string error))
            {
                MessageBox.Show(this, error, "Revise el movimiento", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ActualizarValidacion();
                return;
            }
            var caja = (CajaOpcion)cmbCaja.SelectedItem!;
            if (MessageBox.Show(this, $"¿Confirmar {cmbTipo.SelectedItem?.ToString()?.ToLowerInvariant()} de $ {importe.ToString("N2", Argentina)} en la caja N.º {caja.CajaId}?",
                    "Confirmar movimiento", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                intento = datos.CrearIntento(caja.CajaId, (string)cmbTipo.SelectedItem!, importe, txtMotivo.Text);
                intentoPendiente = intento;
                cmbCaja.Enabled = cmbTipo.Enabled = txtImporte.Enabled = txtMotivo.Enabled = false;
            }
            catch (IOException)
            {
                MessageBox.Show(this, "Ya existe un intento pendiente. No se iniciará otro movimiento.", "Movimiento pendiente",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                await CargarAsync();
                return;
            }
        }

        btnConfirmar.Enabled = btnActualizar.Enabled = false;
        lblEstado.Text = "Registrando movimiento...";
        try
        {
            var resultado = await datos.RegistrarPendienteAsync(intento);
            if (resultado.PendienteLocal)
            {
                intentoPendiente = intento;
                btnConfirmar.Text = "Comprobar mismo movimiento";
                lblEstado.Text = "El movimiento fue confirmado. Se conserva el intento hasta poder archivar la confirmación.";
            }
            else
            {
                intentoPendiente = null;
                lblEstado.Text = "Movimiento registrado correctamente.";
                cmbCaja.Enabled = cmbTipo.Enabled = txtImporte.Enabled = txtMotivo.Enabled = true;
                txtImporte.Clear(); txtMotivo.Clear();
                btnConfirmar.Text = "Confirmar movimiento";
            }
            MessageBox.Show(this,
                $"Movimiento N.º {resultado.MovimientoCajaId} {resultado.TipoMovimiento.ToLowerInvariant()} confirmado por $ {resultado.Importe.ToString("N2", Argentina)}.\n"
                + $"Fecha: {resultado.FechaHora:dd/MM/yyyy HH:mm}"
                + (resultado.Reutilizado ? "\nSe verificó que este movimiento ya estaba registrado." : "")
                + (resultado.PendienteLocal ? "\nEl intento queda conservado para una comprobación segura." : ""),
                "Movimiento confirmado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (!resultado.PendienteLocal)
            {
                await CargarAsync();
                if (cmbCaja.Items.Count > 0) cmbCaja.SelectedIndex = Math.Min(cmbCaja.SelectedIndex, cmbCaja.Items.Count - 1);
            }
        }
        catch (MovimientoCajaDatos.RechazoMovimientoException ex)
        {
            intentoPendiente = null;
            cmbCaja.Enabled = cmbTipo.Enabled = txtImporte.Enabled = txtMotivo.Enabled = true;
            btnConfirmar.Text = "Confirmar movimiento";
            lblEstado.Text = ex.Message;
            MessageBox.Show(this, ex.Message, "No se registró el movimiento", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            await CargarAsync();
        }
        catch
        {
            intentoPendiente = intento;
            cmbCaja.Enabled = cmbTipo.Enabled = txtImporte.Enabled = txtMotivo.Enabled = false;
            btnConfirmar.Text = "Reintentar mismo movimiento";
            lblEstado.Text = "No se pudo confirmar el resultado. El intento quedó guardado y no se generará otra clave.";
            MessageBox.Show(this, "No se pudo confirmar el resultado. No cree otro movimiento. Actualice o reintente este mismo intento cuando se restablezca la conexión.",
                "Resultado pendiente de comprobación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            if (!IsDisposed)
            {
                btnActualizar.Enabled = !intentoNoVerificable;
                if (intentoPendiente != null) btnConfirmar.Enabled = true;
                else ActualizarValidacion();
            }
        }
    }

}
