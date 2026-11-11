using System.Globalization;
using PacuIbera.Datos;
using PacuIbera.Dominio;

namespace PacuIbera.UI.Common
{
    // Recepción de remitos exclusiva del Vendedor autenticado.
    public sealed class FormRecepcionRemito : Form
    {
        private static readonly CultureInfo Argentina = CultureInfo.GetCultureInfo("es-AR");
        private sealed class CantidadEditor : NumericUpDown
        {
            public bool IntentarLeer(out decimal valor)
            {
                string texto = Text.Trim();
                int coma = texto.IndexOf(',');
                return decimal.TryParse(texto, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                    Argentina, out valor) && (coma < 0 || (DecimalPlaces > 0 && texto.Length - coma - 1 <= DecimalPlaces))
                    && valor >= 0 && valor <= Maximum && decimal.Round(valor, DecimalPlaces) == valor;
            }
            protected override void ValidateEditText()
            {
                if (!IntentarLeer(out decimal valor)) { BackColor = Color.MistyRose; return; }
                UserEdit = false;
                Value = valor;
                BackColor = SystemColors.Window;
                UpdateEditText();
            }
            protected override void UpdateEditText()
            {
                UserEdit = false;
                Text = Value.ToString(DecimalPlaces == 0 ? "0" : "0.###", Argentina);
            }
            public override void UpButton() { if (IntentarLeer(out _)) base.UpButton(); else BackColor = Color.MistyRose; }
            public override void DownButton() { if (IntentarLeer(out _)) base.DownButton(); else BackColor = Color.MistyRose; }
        }

        private sealed record EditorLote(RemitoLoteDetalle Origen, CantidadEditor Cantidad, Label Faltante, Label Aviso);
        private readonly RemitosDatos datos = new();
        private readonly DataGridView pendientes = FormRemitos.CrearGrilla();
        private readonly Button actualizar = new() { Text = "Actualizar pendientes", AutoSize = true };
        private readonly Button confirmar = new() { Text = "Confirmar recepción definitiva", AutoSize = true, Enabled = false };
        private readonly Dictionary<int, string> borradores = new();
        private readonly Label estadoLista = new() { Dock = DockStyle.Bottom, Height = 26, Padding = new Padding(10, 3, 0, 0) };
        private readonly Label cabecera = new() { Dock = DockStyle.Top, Height = 62, Padding = new Padding(12), AutoEllipsis = true };
        private readonly Label resumen = new() { Dock = DockStyle.Top, Height = 60, Padding = new Padding(10) };
        private readonly FlowLayoutPanel contenido = new()
        {
            Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown,
            WrapContents = false, Padding = new Padding(10)
        };
        private readonly List<EditorLote> editores = new();
        private DetalleRemitoInterno? cargado;
        private DateTime fechaServidor;
        private bool ocupado;
        private bool autorizado;

        public FormRecepcionRemito()
        {
            if (!RemitosDatos.PuedePrepararRecepcion) throw new UnauthorizedAccessException("Solo Vendedor puede preparar recepciones.");
            Text = "Recepción de remitos";
            Font = new Font("Segoe UI", 9F);
            Size = new Size(1100, 830);
            MinimumSize = new Size(920, 690);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            var titulo = new Label
            {
                Text = "RECEPCIÓN DE REMITOS INTERNOS\nSeleccione un remito e indique cuánto recibió antes de confirmar.",
                Dock = DockStyle.Top, Height = 68, Padding = new Padding(14),
                BackColor = Color.FromArgb(45, 55, 65), ForeColor = Color.White,
                Font = new Font(Font, FontStyle.Bold)
            };
            var lista = new Panel { Dock = DockStyle.Top, Height = 210 };
            var accionesLista = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(10), WrapContents = false };
            accionesLista.Controls.Add(actualizar);

            FormRemitos.AgregarColumna(pendientes, "Id", "Remito", 100);
            FormRemitos.AgregarColumna(pendientes, "FechaEmision", "Fecha de emisión", 170, true);
            FormRemitos.AgregarColumna(pendientes, "Emisor", "Administrador emisor", 260);

            lista.Controls.Add(pendientes);
            lista.Controls.Add(estadoLista);
            lista.Controls.Add(accionesLista);
            var pie = new Panel { Dock = DockStyle.Bottom, Height = 120, BackColor = Color.WhiteSmoke };
            var acciones = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 56, Padding = new Padding(10), FlowDirection = FlowDirection.RightToLeft };
            var cancelar = new Button { Text = "Cerrar", AutoSize = true };
            cancelar.Click += (_, _) => Close();
            foreach (var boton in new[] { actualizar, confirmar })
                EstiloUI.AplicarEstiloBotonAccion(boton, EstiloUI.TipoAccion.Positivo);
            EstiloUI.AplicarEstiloBotonAccion(cancelar, EstiloUI.TipoAccion.Editar);
            acciones.Controls.Add(cancelar);
            acciones.Controls.Add(confirmar);
            confirmar.Click += async (_, _) => await ConfirmarAsync();
            pie.Controls.Add(resumen);
            pie.Controls.Add(acciones);
            Controls.Add(contenido);
            Controls.Add(pie);
            Controls.Add(cabecera);
            Controls.Add(lista);
            Controls.Add(titulo);
            CancelButton = cancelar;
            FormClosing += (_, e) =>
            {
                if (ocupado) e.Cancel = true;
            };
            pendientes.SelectionChanged += async (_, _) => await SeleccionarRemitoAsync();

            pendientes.DataBindingComplete += (_, _) =>
            {
                pendientes.ClearSelection();
                pendientes.CurrentCell = null;
                ActualizarAcciones();
            };
            actualizar.Click += async (_, _) => await CargarPendientesAsync();


            contenido.ClientSizeChanged += (_, _) =>
            {
                foreach (Control bloque in contenido.Controls)
                    bloque.Width = Math.Max(800, contenido.ClientSize.Width - 40);
            };
            Load += async (_, _) => await CargarPendientesAsync();
            cabecera.Text = "Seleccione un remito para ver sus productos e ingresar lo recibido.";
        }

        private RemitoInterno? Seleccionado() => pendientes.SelectedRows.Count == 1
            && !pendientes.SelectedRows[0].IsNewRow
            && pendientes.SelectedRows[0].DataBoundItem is RemitoInterno r && r.Id > 0
            && r.Estado == "En tránsito" && r.FechaEmision.HasValue && !r.FechaRecepcion.HasValue ? r : null;

        private void LimpiarBorrador()
        {
            cargado = null;
            editores.Clear();
            foreach (Control control in contenido.Controls.Cast<Control>().ToList()) control.Dispose();
            contenido.Controls.Clear();
            resumen.Text = "No hay una recepción preparada. No se modificaron datos.";
            cabecera.Text = "Seleccione un remito para ver sus productos e ingresar lo recibido.";
        }

        private async Task CargarPendientesAsync()
        {
            if (ocupado) return;
            GuardarBorrador();
            ocupado = true;
            autorizado = false;
            ActualizarAcciones();
            try
            {
                await datos.ValidarDestinoRecepcionAsync();
                var lista = await datos.ListarPendientesRecepcionAsync();
                if (IsDisposed) return;
                autorizado = true;
                LimpiarBorrador();
                pendientes.DataSource = lista;
                pendientes.ClearSelection();
                pendientes.CurrentCell = null;
                estadoLista.Text = lista.Count == 0 ? "No hay remitos En tránsito pendientes de recepción."
                    : $"Remitos pendientes: {lista.Count} · Único local de ventas";
            }
            catch (Exception)
            {
                if (!IsDisposed)
                {
                    pendientes.DataSource = null;
                    estadoLista.Text = "No se pudieron consultar los remitos pendientes.";
                    LimpiarBorrador();
                    MessageBox.Show(this, "No se pudo cargar la lista. Compruebe su conexión y que su usuario vendedor esté activo.", "Recepción de remitos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally { ocupado = false; if (!IsDisposed) ActualizarAcciones(); }
        }

        private async Task AbrirRemitoAsync()
        {
            var seleccionado = Seleccionado();
            if (ocupado || !autorizado || seleccionado == null) return;
            GuardarBorrador();
            ocupado = true;
            ActualizarAcciones();
            try
            {
                var resultado = await datos.ObtenerRemitoParaRecepcionAsync(seleccionado.Id);
                if (IsDisposed) return;
                LimpiarBorrador();
                cargado = resultado.Remito;
                fechaServidor = resultado.FechaServidor;
                bool anterior = File.Exists(RemitosDatos.ArchivoIntentoRecepcion(seleccionado.Id));
                if (anterior)
                {
                    // Restaurar exactamente lo enviado; un archivo ilegible tambien bloquea el envio.
                    try
                    {
                        var intento = RemitosDatos.LeerIntentoRecepcion(seleccionado.Id);
                        foreach (var lote in intento.Lotes)
                            borradores[lote.RemitoLoteId] = Formato(lote.Recibido, lote.SeVendePorPeso);
                    }
                    catch { /* No reemplazar ni borrar el registro anterior. */ }
                }
                cabecera.Text = $"REMITO N.º {cargado.Cabecera.Id} · Emisión: {cargado.Cabecera.FechaEmision:dd/MM/yyyy HH:mm}\n"
                    + $"Enviado por: {cargado.Cabecera.Emisor} · Ingrese cero si el producto no llegó o está vencido.";
                foreach (var lote in cargado.Lotes) contenido.Controls.Add(CrearEditor(lote));
                ActualizarResumen();
                if (anterior)
                    resumen.Text = "Existe un intento anterior. No se volverá a enviar. "
                        + await datos.ConsultarEstadoConfirmacionAsync(seleccionado.Id);
            }
            catch (Exception)
            {
                if (!IsDisposed)
                {
                    autorizado = false;
                    LimpiarBorrador();
                    MessageBox.Show(this, "No se pudo abrir el remito. Puede haber sido recibido por otra persona. Actualice la lista antes de continuar.", "Recepción de remitos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally { ocupado = false; if (!IsDisposed) ActualizarAcciones(); }
        }

        private Control CrearEditor(RemitoLoteDetalle lote)
        {
            bool vencido = lote.Vencimiento.HasValue && lote.Vencimiento.Value.Date < fechaServidor;
            var bloque = new FlowLayoutPanel
            {
                Width = Math.Max(800, contenido.ClientSize.Width - 40), MinimumSize = new Size(800, 0), AutoSize = true,
                FlowDirection = FlowDirection.TopDown, WrapContents = false,
                Padding = new Padding(8), Margin = new Padding(0, 0, 0, 10), BorderStyle = BorderStyle.FixedSingle
            };
            bloque.Controls.Add(new Label { Text = $"{lote.Producto}", AutoSize = true,
                Font = new Font(Font, FontStyle.Bold) });
            if (lote.Vencimiento.HasValue)
                bloque.Controls.Add(new Label { Text = $"Vence: {lote.Vencimiento:dd/MM/yyyy}", AutoSize = true });
            var cantidad = new CantidadEditor
            {
                DecimalPlaces = lote.SeVendePorPeso ? 3 : 0, Minimum = 0,
                Maximum = vencido ? 0 : Math.Max(0, lote.Enviado),
                Increment = lote.SeVendePorPeso ? 0.001M : 1M, Width = 130
            };
            var fila = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
            var faltante = new Label { AutoSize = true, Margin = new Padding(8, 6, 0, 0) };
            var aviso = new Label { AutoSize = true, ForeColor = Color.Maroon };
            fila.Controls.Add(new Label { Text = $"Enviado: {Formato(lote.Enviado, lote.SeVendePorPeso)} · Recibido:", AutoSize = true, Margin = new Padding(0, 6, 6, 0) });
            fila.Controls.Add(cantidad);
            fila.Controls.Add(faltante);
            bloque.Controls.Add(fila);
            bloque.Controls.Add(aviso);
            if (borradores.TryGetValue(lote.Id, out string? texto)) cantidad.Text = texto;
            var editor = new EditorLote(lote, cantidad, faltante, aviso);
            editores.Add(editor);
            cantidad.TextChanged += (_, _) => { ActualizarResumen(); ActualizarAcciones(); };
            cantidad.ValueChanged += (_, _) => { ActualizarResumen(); ActualizarAcciones(); };
            return bloque;
        }

        private List<LoteRecepcionRemitoBorrador> LeerBorrador()
        {
            var filas = new List<LoteRecepcionRemitoBorrador>();
            foreach (var e in editores)
            {
                if (!e.Cantidad.IntentarLeer(out decimal valor))
                    throw new InvalidOperationException($"{e.Origen.Producto}: ingrese una cantidad entre cero y lo enviado; use coma para los decimales.");
                var l = e.Origen;
                filas.Add(new(l.Id, l.RemitoDetalleId, l.DepositoStockId, l.Producto, l.SeVendePorPeso, l.Enviado, valor, l.Vencimiento));
            }
            return filas;
        }

        private static string Formato(decimal valor, bool peso) => valor.ToString(peso ? "0.###" : "0", Argentina);

        private static string Totales(IEnumerable<LoteRecepcionRemitoBorrador> filas)
        {
            var lista = filas.ToList();
            string Total(bool peso)
            {
                var grupo = lista.Where(l => l.SeVendePorPeso == peso).ToList();
                return $"{(peso ? "Por peso" : "Por unidad")}: enviado {Formato(grupo.Sum(l => l.Enviado), peso)}"
                    + $" · recibido {Formato(grupo.Sum(l => l.Recibido), peso)} · faltante {Formato(grupo.Sum(l => l.Faltante), peso)}";
            }
            return string.Join("\n", new[] { false, true }.Where(peso => lista.Any(l => l.SeVendePorPeso == peso)).Select(Total));
        }

        private void ActualizarResumen()
        {
            foreach (var e in editores)
            {
                e.Cantidad.BackColor = e.Cantidad.IntentarLeer(out _) ? SystemColors.Window : Color.MistyRose;
                e.Faltante.Text = e.Cantidad.IntentarLeer(out decimal valor)
                    ? $"Faltante: {Formato(e.Origen.Enviado - valor, e.Origen.SeVendePorPeso)}" : "Cantidad inválida";
                e.Aviso.Text = e.Origen.Vencimiento.HasValue && e.Origen.Vencimiento.Value.Date < fechaServidor
                    ? "VENCIDO: recibido debe ser cero; no puede ingresar al stock vendible." : "";
            }
            try { resumen.Text = Totales(LeerBorrador()); }
            catch (InvalidOperationException) { resumen.Text = "Revise las cantidades marcadas antes de continuar."; }
        }

        private void ActualizarAcciones()
        {
            bool permitido = autorizado && RemitosDatos.PuedePrepararRecepcion;
            pendientes.Enabled = !ocupado && permitido;
            actualizar.Enabled = !ocupado && RemitosDatos.PuedePrepararRecepcion;
            bool anterior = cargado != null && File.Exists(RemitosDatos.ArchivoIntentoRecepcion(cargado.Cabecera.Id));
            bool valido = editores.Count > 0 && editores.All(e => e.Cantidad.IntentarLeer(out decimal valor)
                && (!(e.Origen.Vencimiento?.Date < fechaServidor) || valor == 0));
            confirmar.Enabled = !ocupado && permitido && RemitosDatos.DestinoRecepcionPermitido
                && cargado != null && Seleccionado()?.Id == cargado.Cabecera.Id && valido && !anterior;
            foreach (var e in editores)
            {
                bool vencido = e.Origen.Vencimiento.HasValue && e.Origen.Vencimiento.Value.Date < fechaServidor;
                // Si vencio durante la edicion, permitir corregir a cero sin truncar el texto.
                e.Cantidad.Enabled = !ocupado && permitido && !anterior && Seleccionado()?.Id == cargado?.Cabecera.Id
                    && (!vencido || !e.Cantidad.IntentarLeer(out decimal valor) || valor > 0);
            }
            UseWaitCursor = ocupado;
        }

        private void GuardarBorrador()
        {
            foreach (var editor in editores) borradores[editor.Origen.Id] = editor.Cantidad.Text;
        }

        private async Task ConfirmarAsync()
        {
            if (ocupado || !confirmar.Enabled || cargado == null || !RemitosDatos.PuedePrepararRecepcion) return;
            ocupado = true;
            ActualizarAcciones();
            bool exito = false;
            int remitoId = cargado.Cabecera.Id;
            try
            {
                var filas = LeerBorrador().ToArray();
                var actual = await datos.ObtenerRemitoParaRecepcionAsync(remitoId);
                if (IsDisposed) return;
                fechaServidor = actual.FechaServidor;
                ActualizarResumen();
                using var validacion = RemitosDatos.ConstruirLotesRecepcionEntrada(actual.Remito, filas, fechaServidor);
                if (MessageBox.Show(this, "¿Confirmar la recepción de esta mercadería?", "Confirmar recepción",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
                var resultado = await datos.ConfirmarRecepcionRemitoAsync(actual.Remito, filas);
                exito = true;
                foreach (var fila in filas) borradores.Remove(fila.RemitoLoteId);
                if (!IsDisposed)
                {
                    LimpiarBorrador();
                    MessageBox.Show(this, $"Recepción N.º {resultado.RecepcionRemitoId} del remito N.º {resultado.RemitoId}\n"
                        + $"Estado: {resultado.Estado}\nFecha: {resultado.FechaRecepcion:dd/MM/yyyy HH:mm:ss}"
                        + (resultado.Reutilizado ? "\nSQL Server devolvió una recepción ya registrada." : ""),
                        "Resultado de la recepción", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                if (IsDisposed) return;
                bool registrado = File.Exists(RemitosDatos.ArchivoIntentoRecepcion(remitoId));
                string mensaje = registrado
                    ? "No se pudo asegurar el resultado de la recepción. No vuelva a enviarla ni borre el registro del intento. Las cantidades y la clave se conservaron."
                    : "No se envió la recepción. Revise la conexión configurada, los permisos y las cantidades. " + ex.Message;
                if (registrado)
                {
                    if (ex is Microsoft.Data.SqlClient.SqlException sql && sql.Number >= 50000)
                        mensaje += "\nSQL Server informó: " + sql.Message;
                    try { mensaje += "\n\n" + await datos.ConsultarEstadoConfirmacionAsync(remitoId); }
                    catch { mensaje += "\nNo se pudo consultar el estado. Espere a recuperar la conexión y pulse Actualizar."; }
                    mensaje += "\n\nRegistro para verificar el intento: " + RemitosDatos.ArchivoIntentoRecepcion(remitoId);
                }
                if (!IsDisposed) MessageBox.Show(this, mensaje, "Recepción sin resultado confirmado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                ocupado = false;
                if (!IsDisposed) ActualizarAcciones();
            }
            if (exito && !IsDisposed) await CargarPendientesAsync();
        }

        private async Task SeleccionarRemitoAsync()
        {
            ActualizarAcciones();
            if (ocupado || !autorizado) return;
            var seleccionado = Seleccionado();
            if (seleccionado?.Id == cargado?.Cabecera.Id) return;
            if (seleccionado == null)
            {
                GuardarBorrador();
                LimpiarBorrador();
                ActualizarAcciones();
                return;
            }
            await AbrirRemitoAsync();
        }
    }
}
