using System.Globalization;
using PacuIbera.Datos;
using PacuIbera.Dominio;

namespace PacuIbera.UI.Common
{
    // La pantalla prepara cantidades; la capa de datos conserva las protecciones SQL y del intento.
    public sealed class FormResolverDiferencia : Form
    {
        private static readonly CultureInfo Argentina = CultureInfo.GetCultureInfo("es-AR");
        private readonly RemitosDatos datos = new();
        private readonly ComboBox remitos = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
        private readonly ComboBox tipo = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 240 };
        private readonly TextBox motivo = new() { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };
        private readonly DataGridView pendientes = FormRemitos.CrearGrilla();
        private readonly TextBox cantidad = new() { Width = 130, TextAlign = HorizontalAlignment.Right, MaxLength = 16 };
        private readonly Button actualizar = new() { Text = "Actualizar", AutoSize = true };
        private readonly Button confirmar = new() { Text = "Confirmar resolución", AutoSize = true, Enabled = false };
        private readonly Label cabecera = new() { Dock = DockStyle.Top, Height = 58, Padding = new Padding(12), AutoEllipsis = true };
        private readonly Label seleccionado = new() { Dock = DockStyle.Fill, AutoEllipsis = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
        private readonly Label mensaje = new() { Dock = DockStyle.Fill, AutoEllipsis = true };
        private readonly Label aviso = new() { Dock = DockStyle.Fill, AutoEllipsis = true, ForeColor = Color.Maroon };
        // Texto por diferencia: no se truncan cantidades invalidas al cambiar de fila.
        private readonly Dictionary<int, string> cantidades = new();
        private readonly Dictionary<int, (Dictionary<int, string> Cantidades, string Motivo, int Tipo)> borradores = new();
        private DetalleRemitoInterno? cargado;
        private DateTime fechaServidor;
        private bool ocupado;
        private bool autorizado;
        private bool cambiandoEditor;
        public bool HuboResoluciones { get; private set; }

        public FormResolverDiferencia()
        {
            if (!RemitosDatos.PuedePrepararResolucion) throw new UnauthorizedAccessException("Solo Administrador puede resolver faltantes.");
            Text = "Resolver faltantes de mercadería";
            Font = new Font("Segoe UI", 9F);
            BackColor = Color.White;
            Size = new Size(1100, 780);
            MinimumSize = new Size(940, 690);
            StartPosition = FormStartPosition.CenterParent;
            var titulo = new Label
            {
                Text = "RESOLVER FALTANTES DE MERCADERÍA\nSeleccione un remito y complete qué ocurrió con la mercadería pendiente.",
                Dock = DockStyle.Top, Height = 68, Padding = new Padding(14),
                BackColor = Color.FromArgb(45, 55, 65), ForeColor = Color.White, Font = new Font(Font, FontStyle.Bold)
            };
            var selector = new TableLayoutPanel { Dock = DockStyle.Top, Height = 52, ColumnCount = 3, Padding = new Padding(10) };
            selector.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 65));
            selector.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            selector.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            selector.Controls.Add(new Label { Text = "Remito:", AutoSize = true, Margin = new Padding(3, 6, 3, 0) }, 0, 0);
            selector.Controls.Add(remitos, 1, 0);
            selector.Controls.Add(actualizar, 2, 0);
            remitos.DisplayMember = nameof(RemitoInterno.Id);
            remitos.FormattingEnabled = true;
            remitos.Format += (_, e) =>
            {
                if (e.ListItem is RemitoInterno r) e.Value = $"Remito {r.Id} · {r.Emisor}";
            };
            tipo.Items.AddRange(new object[] { "Llegó después al local", "Volvió al depósito", "Se perdió o dañó" });
            tipo.SelectedIndex = 0;
            FormRemitos.AgregarColumna(pendientes, "Producto", "Producto", 240);
            FormRemitos.AgregarColumna(pendientes, "Enviado", "Enviado", 105);
            FormRemitos.AgregarColumna(pendientes, "Recibido", "Recibido inicialmente", 145);
            FormRemitos.AgregarColumna(pendientes, "Pendiente", "Faltante pendiente", 135);
            pendientes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AResolver", HeaderText = "A resolver", Width = 110, SortMode = DataGridViewColumnSortMode.NotSortable
            });
            FormRemitos.AgregarColumna(pendientes, "Vencimiento", "Vence", 110);
            pendientes.Columns["Vencimiento"].DefaultCellStyle.Format = "dd/MM/yyyy";
            pendientes.Columns["Vencimiento"].DefaultCellStyle.NullValue = "";
            pendientes.Columns["Vencimiento"].Visible = false;
            pendientes.CellFormatting += (_, e) =>
            {
                if (e.RowIndex < 0 || pendientes.Rows[e.RowIndex].DataBoundItem is not RemitoLoteDetalle l)
                    return;
                if (e.CellStyle != null && l.Vencimiento.HasValue && l.Vencimiento.Value.Date < fechaServidor)
                    e.CellStyle.ForeColor = Color.Maroon;
                if (e.Value is decimal numero && pendientes.Columns[e.ColumnIndex].Name is "Enviado" or "Recibido" or "Pendiente" or "AResolver")
                {
                    e.Value = $"{Formato(numero, l.SeVendePorPeso)} {(l.SeVendePorPeso ? "kg" : "un.")}";
                    e.FormattingApplied = true;
                }
            };
            var pie = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 265, ColumnCount = 2, RowCount = 6, Padding = new Padding(12) };
            pie.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            pie.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            foreach (int alto in new[] { 34, 38, 38, 68, 32, 40 }) pie.RowStyles.Add(new RowStyle(SizeType.Absolute, alto));
            pie.Controls.Add(seleccionado, 0, 0); pie.SetColumnSpan(seleccionado, 2);
            pie.Controls.Add(new Label { Text = "¿Qué ocurrió?", AutoSize = true, Margin = new Padding(3, 7, 3, 0) }, 0, 1);
            var opciones = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            opciones.Controls.Add(tipo);
            opciones.Controls.Add(new Label { Text = "Cantidad:", AutoSize = true, Margin = new Padding(16, 7, 6, 0) });
            opciones.Controls.Add(cantidad);
            pie.Controls.Add(opciones, 1, 1);
            pie.Controls.Add(aviso, 0, 2); pie.SetColumnSpan(aviso, 2);
            pie.Controls.Add(new Label { Text = "Motivo obligatorio\nHasta 500 caracteres", AutoSize = true }, 0, 3);
            pie.Controls.Add(motivo, 1, 3);
            pie.Controls.Add(mensaje, 0, 4); pie.SetColumnSpan(mensaje, 2);
            var acciones = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, FlowDirection = FlowDirection.RightToLeft };
            var cerrar = new Button { Text = "Cerrar", AutoSize = true };
            cerrar.Click += (_, _) => Close();
            acciones.Controls.Add(cerrar); acciones.Controls.Add(confirmar);
            pie.Controls.Add(acciones, 0, 5); pie.SetColumnSpan(acciones, 2);
            foreach (var boton in new[] { actualizar, confirmar }) EstiloUI.AplicarEstiloBotonAccion(boton, EstiloUI.TipoAccion.Positivo);
            EstiloUI.AplicarEstiloBotonAccion(cerrar, EstiloUI.TipoAccion.Editar);
            Controls.Add(pendientes); Controls.Add(pie); Controls.Add(cabecera); Controls.Add(selector); Controls.Add(titulo);
            CancelButton = cerrar;
            remitos.SelectedIndexChanged += async (_, _) => await AbrirSeleccionadoAsync();
            pendientes.SelectionChanged += (_, _) => SeleccionarDiferencia();
            tipo.SelectedIndexChanged += (_, _) => ActualizarAcciones();
            motivo.TextChanged += (_, _) => ActualizarAcciones();
            cantidad.TextChanged += (_, _) => CambiarCantidad();
            actualizar.Click += async (_, _) => await CargarAsync();
            confirmar.Click += async (_, _) => await ConfirmarAsync();
            Load += async (_, _) => await CargarAsync();
            FormClosing += (_, e) => { if (ocupado) e.Cancel = true; };
            Limpiar();
        }

        // Etiquetas sencillas; los valores enviados a SQL permanecen exactamente iguales.
        private string Tipo => tipo.SelectedIndex switch
        {
            0 => "Recepción posterior", 1 => "Devolución depósito", 2 => "Pérdida documentada", _ => ""
        };
        private RemitoLoteDetalle? DiferenciaSeleccionada() => pendientes.SelectedRows.Count == 1
            && !pendientes.SelectedRows[0].IsNewRow ? pendientes.SelectedRows[0].DataBoundItem as RemitoLoteDetalle : null;
        private static string Formato(decimal valor, bool peso) => valor.ToString(peso ? "0.###" : "0", Argentina);

        private void Limpiar()
        {
            cargado = null;
            cantidades.Clear();
            pendientes.DataSource = null;
            motivo.Clear();
            cabecera.Text = "Seleccione un remito para consultar sus faltantes.";
            seleccionado.Text = "Seleccione un producto de la lista.";
            mensaje.Text = "Seleccione un remito para comenzar.";
            cambiandoEditor = true;
            cantidad.Text = "0";
            cambiandoEditor = false;
            ActualizarAcciones();
        }

        private async Task CargarAsync()
        {
            if (ocupado) return;
            GuardarBorrador();
            ocupado = true; autorizado = false; ActualizarAcciones();
            try
            {
                await datos.ValidarDestinoResolucionAsync();
                var comprobaciones = new List<string>();
                foreach (int id in RemitosDatos.RemitosConResolucionPendiente())
                {
                    try
                    {
                        var resultado = await datos.ConciliarResolucionPendienteAsync(id);
                        if (resultado != null)
                        {
                            HuboResoluciones = true;
                            borradores.Remove(id);
                            comprobaciones.Add($"Remito {id}: resolución comprobada. Estado: {resultado.EstadoRemitoActual}.");
                        }
                        else comprobaciones.Add($"Remito {id}: aún no se pudo confirmar el resultado anterior. Se mantiene bloqueado.");
                    }
                    catch { comprobaciones.Add($"Remito {id}: no se pudo comprobar el intento anterior. No lo vuelva a enviar ni borre su registro."); }
                }
                var lista = await datos.ListarRemitosConDiferenciasAsync();
                if (IsDisposed) return;
                Limpiar();
                remitos.DataSource = lista;
                remitos.SelectedIndex = -1;
                autorizado = true;
                mensaje.Text = lista.Count == 0 ? "No hay remitos con faltantes pendientes." : "Seleccione un remito para comenzar.";
                if (comprobaciones.Count > 0)
                    MessageBox.Show(this, string.Join("\n", comprobaciones), "Comprobación de resoluciones", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {
                if (!IsDisposed)
                {
                    Limpiar(); remitos.DataSource = null;
                    mensaje.Text = "No se pudieron cargar los faltantes. Compruebe la conexión configurada y los permisos.";
                }
            }
            finally { ocupado = false; if (!IsDisposed) ActualizarAcciones(); }
        }

        private async Task AbrirSeleccionadoAsync()
        {
            if (ocupado || !autorizado) return;
            GuardarBorrador();
            if (remitos.SelectedItem is not RemitoInterno r || r.Estado != "Con diferencias") { Limpiar(); return; }
            if (cargado?.Cabecera.Id == r.Id) return;
            ocupado = true; ActualizarAcciones();
            try
            {
                var resultado = await datos.ObtenerRemitoParaResolucionAsync(r.Id);
                if (IsDisposed) return;
                Limpiar();
                cargado = resultado.Remito;
                fechaServidor = resultado.FechaServidor;
                cabecera.Text = $"Remito N.º {r.Id} · Enviado por {cargado.Cabecera.Emisor}\n"
                    + "Indique las cantidades a resolver. Puede completar varios lotes antes de confirmar; use cero para dejarlos pendientes.";
                pendientes.Columns["Vencimiento"].Visible = cargado.Lotes.Any(l => l.Pendiente > 0 && l.Vencimiento.HasValue);
                pendientes.DataSource = cargado.Lotes.Where(l => l.Pendiente > 0).ToList();
                pendientes.ClearSelection(); pendientes.CurrentCell = null;
                if (borradores.TryGetValue(r.Id, out var anterior))
                {
                    foreach (var l in cargado.Lotes)
                        if (l.RecepcionRemitoLoteId is int id && anterior.Cantidades.TryGetValue(id, out string? texto))
                            cantidades[id] = texto;
                    motivo.Text = anterior.Motivo;
                    tipo.SelectedIndex = anterior.Tipo;
                }
                SeleccionarDiferencia();
                ActualizarCantidadesTabla();
            }
            catch (Exception)
            {
                if (!IsDisposed)
                {
                    autorizado = false; Limpiar();
                    mensaje.Text = "No se pudo abrir el remito. Actualice la lista; sus faltantes pueden haber cambiado.";
                }
            }
            finally { ocupado = false; if (!IsDisposed) ActualizarAcciones(); }
        }

        private void SeleccionarDiferencia()
        {
            var l = DiferenciaSeleccionada();
            cambiandoEditor = true;
            cantidad.Text = l?.RecepcionRemitoLoteId is int id && cantidades.TryGetValue(id, out string? texto) ? texto : "0";
            cambiandoEditor = false;
            seleccionado.Text = l == null ? "Seleccione un producto de la lista."
                : $"{l.Producto} · Enviado: {Formato(l.Enviado,l.SeVendePorPeso)} · Recibido: {Formato(l.Recibido ?? 0,l.SeVendePorPeso)}"
                + $" · Faltante: {Formato(l.Pendiente ?? 0,l.SeVendePorPeso)}"
                + (l.Resuelto > 0 ? $" · Ya resuelto: {Formato(l.Resuelto,l.SeVendePorPeso)}" : "");
            ActualizarAcciones();
        }

        private void CambiarCantidad()
        {
            if (cambiandoEditor || ocupado || DiferenciaSeleccionada()?.RecepcionRemitoLoteId is not int id) return;
            cantidades[id] = cantidad.Text;
            ActualizarCantidadesTabla();
            ActualizarAcciones();
        }

        private void GuardarBorrador()
        {
            if (cargado != null)
                borradores[cargado.Cabecera.Id] = (new Dictionary<int, string>(cantidades), motivo.Text, tipo.SelectedIndex);
        }

        private static bool LeerCantidad(RemitoLoteDetalle lote, string texto, out decimal valor)
        {
            texto = texto.Trim();
            valor = 0;
            if (texto.Contains('.') || texto.Count(c => c == ',') > 1) return false;
            int coma = texto.IndexOf(',');
            if (coma >= 0 && (!lote.SeVendePorPeso || texto.Length - coma - 1 is < 1 or > 3)) return false;
            return decimal.TryParse(texto, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                       Argentina, out valor)
                   && valor >= 0 && valor <= lote.Pendiente && valor <= 9999999.999M
                   && (lote.SeVendePorPeso || decimal.Truncate(valor) == valor)
                   && decimal.Round(valor, lote.SeVendePorPeso ? 3 : 0) == valor;
        }

        private List<ResolucionRemitoBorrador> LeerPropuesta()
        {
            var filas = new List<ResolucionRemitoBorrador>();
            if (cargado == null) return filas;
            foreach (var l in cargado.Lotes.Where(l => l.Pendiente > 0))
            {
                if (!l.RecepcionRemitoLoteId.HasValue) throw new InvalidOperationException("No se pudo identificar el faltante. Actualice la lista.");
                if (!cantidades.TryGetValue(l.RecepcionRemitoLoteId.Value, out string? texto)) continue;
                if (!LeerCantidad(l, texto, out decimal valor)) throw new InvalidOperationException("Revise las cantidades: no pueden superar el faltante. Use enteros para unidades y hasta tres decimales para peso.");
                if (valor == 0) continue;
                filas.Add(new(l.RecepcionRemitoLoteId.Value, l.Id, l.DepositoStockId, l.Producto, l.SeVendePorPeso,
                    l.Enviado, l.Recibido!.Value, l.Resuelto, l.Pendiente!.Value, valor, l.Vencimiento));
            }
            return filas;
        }

        private void ActualizarCantidadesTabla()
        {
            foreach (DataGridViewRow fila in pendientes.Rows)
            {
                if (fila.DataBoundItem is not RemitoLoteDetalle l) continue;
                string texto = l.RecepcionRemitoLoteId is int id && cantidades.TryGetValue(id, out string? guardado) ? guardado : "0";
                fila.Cells["AResolver"].Value = LeerCantidad(l, texto, out decimal valor)
                    ? $"{Formato(valor, l.SeVendePorPeso)} {(l.SeVendePorPeso ? "kg" : "un.")}" : "Revisar";
            }
        }

        private void ActualizarAcciones()
        {
            bool permite = autorizado && RemitosDatos.PuedePrepararResolucion && !ocupado;
            bool bloqueado = cargado != null && File.Exists(RemitosDatos.ArchivoResolucionPendiente(cargado.Cabecera.Id));
            bool editar = permite && cargado != null && remitos.SelectedItem is RemitoInterno r && r.Id == cargado.Cabecera.Id && !bloqueado;
            actualizar.Enabled = !ocupado && RemitosDatos.PuedePrepararResolucion;
            remitos.Enabled = permite;
            pendientes.Enabled = tipo.Enabled = motivo.Enabled = editar;
            var lote = DiferenciaSeleccionada();
            bool vencido = lote?.Vencimiento is DateTime fecha && fecha.Date < fechaServidor;
            cantidad.Enabled = editar && lote?.Pendiente > 0
                && (!(Tipo == "Recepción posterior" && vencido) || !LeerCantidad(lote!, cantidad.Text, out decimal valor) || valor > 0);
            cantidad.BackColor = lote != null && !LeerCantidad(lote, cantidad.Text, out _) ? Color.MistyRose : Color.White;
            motivo.BackColor = motivo.Text.Length > 500 ? Color.MistyRose : Color.White;
            aviso.Text = cargado == null ? "" : bloqueado ? "Hay un intento anterior sin resultado confirmado. Pulse Actualizar; no vuelva a enviarlo."
                : Tipo switch
                {
                    "Recepción posterior" when vencido => "Este lote está vencido: no puede ingresar al stock del local. Indique cero o elija lo que ocurrió.",
                    "Recepción posterior" => "Solo se sumará al local la cantidad indicada de mercadería vigente.",
                    "Devolución depósito" => "Se registrará la devolución. La mercadería vencida quedará inactiva en el depósito.",
                    _ => "Se registrará la pérdida o daño, sin aumentar el stock."
                };
            bool valido = false;
            if (editar && cargado != null)
            {
                try
                {
                    var filas = LeerPropuesta();
                    if (filas.Count > 0)
                    {
                        if (string.IsNullOrWhiteSpace(motivo.Text))
                            throw new InvalidOperationException("Escriba el motivo antes de confirmar.");
                        if (motivo.Text.Length > 500)
                            throw new InvalidOperationException("El motivo admite hasta 500 caracteres.");
                        if (Tipo == "Recepción posterior" && filas.Any(f => f.Vencimiento.HasValue && f.Vencimiento.Value.Date < fechaServidor))
                            throw new InvalidOperationException("Hay mercadería vencida entre las cantidades indicadas. No puede sumarla al local; use cero o elija lo que ocurrió.");
                        using var entrada = RemitosDatos.ConstruirResolucionEntrada(cargado, filas, Tipo, motivo.Text, fechaServidor);
                        valido = true;
                        mensaje.Text = $"Productos/lotes a resolver: {filas.Count}. Los demás seguirán pendientes.";
                    }
                    else mensaje.Text = "Indique una cantidad mayor que cero y escriba el motivo.";
                }
                catch (InvalidOperationException ex) { mensaje.Text = ex.Message; }
            }
            confirmar.Enabled = editar && valido && RemitosDatos.DestinoResolucionPermitido;
            UseWaitCursor = ocupado;
        }

        private async Task ConfirmarAsync()
        {
            if (ocupado || !confirmar.Enabled || cargado == null || !RemitosDatos.PuedePrepararResolucion) return;
            int remitoId = cargado.Cabecera.Id;
            ocupado = true; ActualizarAcciones();
            bool exito = false;
            try
            {
                var filas = LeerPropuesta().ToArray();
                string tipoConfirmado = Tipo, motivoConfirmado = motivo.Text;
                var actual = await datos.ObtenerRemitoParaResolucionAsync(remitoId);
                if (IsDisposed) return;
                fechaServidor = actual.FechaServidor;
                using var entrada = RemitosDatos.ConstruirResolucionEntrada(actual.Remito, filas, tipoConfirmado, motivoConfirmado, fechaServidor);
                string resumen = string.Join("\n", filas.Take(3).Select(f =>
                    $"{f.Producto}: {Formato(f.Cantidad,f.SeVendePorPeso)} {(f.SeVendePorPeso ? "kg" : "un.")}"));
                if (filas.Length > 3) resumen += $"\nY {filas.Length - 3} productos/lotes más, según lo indicado en la tabla.";
                if (MessageBox.Show(this, $"¿Confirmar la resolución del remito {remitoId}?\n{tipo.Text}\n{resumen}",
                    "Confirmar resolución", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
                var resultado = await datos.ConfirmarResolucionRemitoAsync(remitoId, filas, tipoConfirmado, motivoConfirmado);
                HuboResoluciones = exito = true;
                borradores.Remove(remitoId);
                Limpiar();
                MessageBox.Show(this, $"Resolución N.º {resultado.ResolucionId}: {resultado.EstadoResolucion}\n"
                    + $"Remito N.º {resultado.RemitoId}: {resultado.EstadoRemitoActual}\nFecha: {resultado.FechaResolucion:dd/MM/yyyy HH:mm:ss}",
                    "Resolución registrada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {
                if (!IsDisposed)
                {
                    bool pendiente = File.Exists(RemitosDatos.ArchivoResolucionPendiente(remitoId));
                    MessageBox.Show(this, pendiente
                        ? "No se pudo confirmar el resultado. Se conservaron la clave y las cantidades. Pulse Actualizar para comprobarlo; no vuelva a enviarlo ni borre el registro."
                        : "No se envió la resolución. Revise la conexión, las cantidades y el motivo; los faltantes pueden haber cambiado.",
                        "Resolución sin resultado confirmado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally { ocupado = false; if (!IsDisposed) ActualizarAcciones(); }
            if (exito && !IsDisposed) await CargarAsync();
        }
    }
}
