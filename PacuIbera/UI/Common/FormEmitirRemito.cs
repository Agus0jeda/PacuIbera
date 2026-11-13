using System.Globalization;
using PacuIbera.Datos;
using PacuIbera.Dominio;

namespace PacuIbera.UI.Common
{
    // Emision por Administrador: stock y lotes se modifican exclusivamente en el SP.
    public sealed class FormEmitirRemito : Form
    {
        private static readonly CultureInfo Argentina = CultureInfo.GetCultureInfo("es-AR");

        private readonly RemitosDatos datos = new();
        private readonly DataGridView catalogo = FormRemitos.CrearGrilla();
        private readonly DataGridView seleccion = FormRemitos.CrearGrilla();
        private readonly TextBox buscar = new() { Dock = DockStyle.Fill, PlaceholderText = "Buscar producto..." };
        private readonly TextBox cantidad = new() { Width = 135, TextAlign = HorizontalAlignment.Right, MaxLength = 16 };
        private readonly Button agregar = new() { Text = "Agregar producto", AutoSize = true, Enabled = false };
        private readonly Button editar = new() { Text = "Editar cantidad", AutoSize = true, Enabled = false };
        private readonly Button quitar = new() { Text = "Quitar producto", AutoSize = true, Enabled = false };
        private readonly Button actualizar = new() { Text = "Actualizar disponibles", AutoSize = true };
        private readonly Button revisar = new() { Text = "Emitir remito", AutoSize = true, Enabled = false };
        private readonly Label mensaje = new() { Dock = DockStyle.Fill, AutoEllipsis = true };
        private readonly Label unidad = new() { AutoSize = true, Margin = new Padding(6, 7, 6, 3) };
        private List<ProductoDisponibleRemito> disponibles = new();
        private readonly List<ProductoEmisionRemito> borrador = new();
        private int? editando;
        private bool ocupado;
        private bool catalogoValido;
        public bool HuboEmisiones { get; private set; }

        public FormEmitirRemito()
        {
            if (!RemitosDatos.PuedePrepararEmision) throw new UnauthorizedAccessException("Solo Administrador puede preparar remitos.");
            Text = "Emitir remito interno";
            Font = new Font("Segoe UI", 9F);
            Size = new Size(1100, 730);
            MinimumSize = new Size(900, 590);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            var titulo = new Label
            {
                Text = "EMITIR REMITO INTERNO\nSeleccione los productos y cantidades que enviará al local.",
                Dock = DockStyle.Top, Height = 70, Padding = new Padding(14),
                BackColor = Color.FromArgb(45, 55, 65), ForeColor = Color.White,
                Font = new Font(Font, FontStyle.Bold)
            };
            var filtros = new TableLayoutPanel { Dock = DockStyle.Top, Height = 52, ColumnCount = 2, Padding = new Padding(10) };
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            filtros.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            filtros.Controls.Add(buscar, 0, 0);
            filtros.Controls.Add(actualizar, 1, 0);
            var contenido = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(10) };
            contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            contenido.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 46));
            contenido.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            contenido.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 54));
            contenido.Controls.Add(new Label { Text = "PRODUCTOS EXISTENTES · Stock utilizable de Depósito", AutoSize = true }, 0, 0);
            FormRemitos.AgregarColumna(catalogo, "Producto", "Producto", 300);
            FormRemitos.AgregarColumna(catalogo, "Unidad", "Venta", 120);
            FormRemitos.AgregarColumna(catalogo, "StockDisponible", "Disponible", 155);
            catalogo.CellFormatting += FormatearCantidadGrilla;
            contenido.Controls.Add(catalogo, 0, 1);
            var entrada = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(0, 5, 0, 0), WrapContents = false };
            entrada.Controls.Add(new Label { Text = "Cantidad:", AutoSize = true, Margin = new Padding(3, 7, 6, 3) });
            entrada.Controls.Add(cantidad);
            entrada.Controls.Add(agregar);
            entrada.Controls.Add(unidad);
            contenido.Controls.Add(entrada, 0, 2);
            contenido.Controls.Add(new Label { Text = "PRODUCTOS DEL BORRADOR", AutoSize = true }, 0, 3);
            FormRemitos.AgregarColumna(seleccion, "Producto", "Producto", 300);
            FormRemitos.AgregarColumna(seleccion, "StockDisponible", "Disponible consultado", 170);
            FormRemitos.AgregarColumna(seleccion, "Cantidad", "A enviar", 145);
            seleccion.CellFormatting += FormatearCantidadGrilla;
            contenido.Controls.Add(seleccion, 0, 4);
            var pie = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 108, RowCount = 2, ColumnCount = 1, Padding = new Padding(10) };
            pie.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            pie.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            pie.Controls.Add(mensaje, 0, 0);
            var acciones = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            var cancelar = new Button { Text = "Cerrar", AutoSize = true };
            cancelar.Click += (_, _) => Close();
            acciones.Controls.Add(editar);
            acciones.Controls.Add(quitar);
            acciones.Controls.Add(revisar);
            acciones.Controls.Add(cancelar);
            foreach (var boton in new[] { agregar, actualizar, revisar })
                EstiloUI.AplicarEstiloBotonAccion(boton, EstiloUI.TipoAccion.Positivo);
            foreach (var boton in new[] { editar, quitar, cancelar })
                EstiloUI.AplicarEstiloBotonAccion(boton, EstiloUI.TipoAccion.Editar);
            pie.Controls.Add(acciones, 0, 1);
            Controls.Add(contenido);
            Controls.Add(pie);
            Controls.Add(filtros);
            Controls.Add(titulo);
            CancelButton = cancelar;
            buscar.TextChanged += (_, _) => FiltrarCatalogo();
            catalogo.SelectionChanged += (_, _) => PrepararCantidad();
            seleccion.SelectionChanged += (_, _) => ActualizarAcciones();
            actualizar.Click += async (_, _) => await CargarDisponiblesAsync();
            agregar.Click += (_, _) => AgregarProducto();
            editar.Click += (_, _) => EditarProducto();
            quitar.Click += (_, _) => QuitarProducto();
            revisar.Click += async (_, _) => await EmitirAsync();
            Load += async (_, _) => await CargarDisponiblesAsync();
            FormClosing += (_, e) => { if (ocupado) e.Cancel = true; };
        }

        private ProductoDisponibleRemito? DisponibleSeleccionado() => catalogo.SelectedRows.Count == 1
            ? catalogo.SelectedRows[0].DataBoundItem as ProductoDisponibleRemito : null;
        private ProductoEmisionRemito? ProductoSeleccionado() => seleccion.SelectedRows.Count == 1
            ? seleccion.SelectedRows[0].DataBoundItem as ProductoEmisionRemito : null;

        private static void FormatearCantidadGrilla(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (sender is not DataGridView grilla || e.RowIndex < 0 || e.ColumnIndex < 0
                || e.Value is not decimal valor)
                return;

            bool? porPeso = grilla.Rows[e.RowIndex].DataBoundItem switch
            {
                ProductoDisponibleRemito p => p.SeVendePorPeso,
                ProductoEmisionRemito p => p.SeVendePorPeso,
                _ => null
            };
            if (!porPeso.HasValue) return;

            string columna = grilla.Columns[e.ColumnIndex].DataPropertyName;
            if (columna is not ("StockDisponible" or "Cantidad")) return;
            e.Value = valor.ToString(porPeso.Value ? "0.### 'kg'" : "0 'un.'", Argentina);
            e.FormattingApplied = true;
            e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private async Task CargarDisponiblesAsync()
        {
            if (ocupado) return;
            RemitosDatos.ResultadoEmision? comprobado = null;
            ocupado = true;
            catalogoValido = false;
            ActualizarAcciones();
            UseWaitCursor = true;
            try
            {
                await datos.ValidarDestinoEmisionAsync();
                comprobado = await datos.ConciliarEmisionPendienteAsync();
                if (comprobado != null) return;
                bool pendiente = File.Exists(RemitosDatos.ArchivoEmisionPendiente);
                if (pendiente)
                {
                    var intento = RemitosDatos.LeerEmisionPendiente();
                    borrador.Clear();
                    borrador.AddRange(intento.Productos);
                }
                var filas = await datos.ObtenerProductosEmisionAsync();
                if (IsDisposed) return;
                disponibles = filas;
                catalogoValido = true;
                if (!pendiente)
                    for (int i = 0; i < borrador.Count; i++)
                        borrador[i] = borrador[i] with { StockDisponible = disponibles.FirstOrDefault(p => p.ProductoId == borrador[i].ProductoId)?.StockDisponible ?? 0 };
                FiltrarCatalogo();
                RefrescarBorrador();
                mensaje.Text = pendiente ? "La emisión anterior sigue sin resultado confirmado. No se permite otra emisión; no borre su registro."
                    : disponibles.Count == 0 ? "No hay productos disponibles para este usuario."
                    : "La disponibilidad puede cambiar. Este borrador no reserva ni descuenta mercadería.";
            }
            catch (Exception)
            {
                if (!IsDisposed)
                {
                    mensaje.Text = "No se pudo comprobar el destino, el stock o la emisión anterior. Actualice antes de continuar.";
                    MessageBox.Show(this, "No se pudo cargar la información. Compruebe la conexión configurada y sus permisos. Si existe un intento pendiente, conserve su registro.", "Emitir remito", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally
            {
                ocupado = false;
                if (!IsDisposed)
                {
                    UseWaitCursor = false;
                    PrepararCantidad();
                    ActualizarAcciones();
                    if (comprobado != null) MostrarEmisionConfirmada(comprobado);
                }
            }
        }

        private void FiltrarCatalogo()
        {
            catalogo.DataSource = disponibles.Where(p => p.Producto.Contains(buscar.Text.Trim(), StringComparison.CurrentCultureIgnoreCase)).ToList();
            catalogo.ClearSelection();
            catalogo.CurrentCell = null;
            PrepararCantidad();
        }

        private void PrepararCantidad()
        {
            editando = null;
            agregar.Text = "Agregar producto";
            var p = DisponibleSeleccionado();
            decimal limite = Math.Max(0, Math.Min(9999999.999M, p?.StockDisponible ?? 0));
            cantidad.Clear();
            unidad.Text = p == null ? "Seleccione un producto." : p.SeVendePorPeso ? $"Hasta 3 decimales · máximo {limite.ToString("0.###", Argentina)} kg." : $"Solo unidades enteras · máximo {decimal.Floor(limite).ToString("0", Argentina)} un.";
            if (p != null && !p.SeVendePorPeso && decimal.Truncate(p.StockDisponible) != p.StockDisponible)
                unidad.Text = "Saldo fraccionario por unidad: requiere conciliación.";
            ActualizarAcciones();
        }

        private void ActualizarAcciones()
        {
            var p = DisponibleSeleccionado();
            bool autorizado = RemitosDatos.PuedePrepararEmision;
            bool bloqueado = File.Exists(RemitosDatos.ArchivoEmisionPendiente);
            bool editarBorrador = !ocupado && catalogoValido && autorizado && !bloqueado;
            catalogo.Enabled = cantidad.Enabled = seleccion.Enabled = buscar.Enabled = editarBorrador;
            agregar.Enabled = editarBorrador && p != null && p.StockDisponible > 0
                && (p.SeVendePorPeso || decimal.Truncate(p.StockDisponible) == p.StockDisponible);
            editar.Enabled = quitar.Enabled = editarBorrador && ProductoSeleccionado() != null;
            bool valido = false;
            try { using var entrada = RemitosDatos.ConstruirProductosEntrada(borrador); valido = true; }
            catch (InvalidOperationException) { }
            revisar.Enabled = editarBorrador && RemitosDatos.DestinoEmisionPermitido && valido && !editando.HasValue;
            actualizar.Enabled = !ocupado && autorizado;
            UseWaitCursor = ocupado;
        }

        private void RefrescarBorrador()
        {
            seleccion.DataSource = borrador.ToList();
            seleccion.ClearSelection();
            seleccion.CurrentCell = null;
            ActualizarAcciones();
        }

        private void AgregarProducto()
        {
            var p = DisponibleSeleccionado();
            if (ocupado || !agregar.Enabled || !RemitosDatos.PuedePrepararEmision || !catalogoValido || p == null) return;
            if (!IntentarLeerCantidad(p.SeVendePorPeso, cantidad.Text, out decimal valor, out string errorCantidad))
            {
                cantidad.BackColor = Color.MistyRose;
                cantidad.Focus();
                MessageBox.Show(this, errorCantidad, "Revise la cantidad", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            cantidad.BackColor = SystemColors.Window;

            decimal yaComprometida = borrador
                .Where(f => f.ProductoId == p.ProductoId && (!editando.HasValue || f.ProductoId != editando.Value))
                .Sum(f => f.Cantidad);
            decimal disponibleParaEstaLinea = Math.Max(0, p.StockDisponible - yaComprometida);
            if (valor > disponibleParaEstaLinea)
            {
                MostrarErrorStock(p, valor, disponibleParaEstaLinea);
                return;
            }

            if (!editando.HasValue && borrador.Any(f => f.ProductoId == p.ProductoId))
            {
                MessageBox.Show(this,
                    $"{p.Producto} ya está en el borrador ({FormatearCantidad(yaComprometida, p.SeVendePorPeso)}). " +
                    "Selecciónelo y use Editar cantidad para cambiar el total.",
                    "Producto ya agregado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var fila = new ProductoEmisionRemito(p.ProductoId, p.Producto, p.SeVendePorPeso, p.StockDisponible, valor);
            try
            {
                using var entrada = RemitosDatos.ConstruirProductosEntrada(new[] { fila });
                if (editando.HasValue)
                {
                    int indice = borrador.FindIndex(f => f.ProductoId == editando.Value);
                    if (indice < 0) return;
                    borrador[indice] = fila;
                }
                else borrador.Add(fila);
                RefrescarBorrador();
                PrepararCantidad();
            }
            catch (InvalidOperationException ex) { MessageBox.Show(this, ex.Message, "Borrador de remito"); }
        }

        private void EditarProducto()
        {
            var p = ProductoSeleccionado();
            if (ocupado || !editar.Enabled || p == null || !catalogoValido) return;
            buscar.Clear();
            foreach (DataGridViewRow fila in catalogo.Rows)
                if (fila.DataBoundItem is ProductoDisponibleRemito d && d.ProductoId == p.ProductoId)
                {
                    catalogo.CurrentCell = fila.Cells[0];
                    fila.Selected = true;
                    PrepararCantidad();
                    editando = p.ProductoId;
                    agregar.Text = "Aplicar cambio";
                    // Conservar el valor aunque exceda la disponibilidad recien consultada.
                    cantidad.Text = p.Cantidad.ToString(p.SeVendePorPeso ? "0.###" : "0", Argentina);
                    cantidad.SelectAll();
                    ActualizarAcciones();
                    return;
                }
            MessageBox.Show(this, "El producto ya no está disponible. Puede quitarlo del borrador.");
        }

        private static bool IntentarLeerCantidad(bool seVendePorPeso, string texto, out decimal valor, out string error)
        {
            valor = 0;
            texto = texto.Trim();
            error = string.Empty;
            if (texto.Length == 0)
            {
                error = "Ingrese una cantidad para continuar.";
                return false;
            }

            if (texto.Contains(',') && texto.Count(c => c == ',') == 1
                && texto.Length - texto.IndexOf(',') - 1 > 3)
            {
                error = "La cantidad admite como máximo tres decimales.";
                return false;
            }

            if (!seVendePorPeso && texto.Contains(','))
            {
                error = "Este producto se vende por unidad. Ingrese solo una cantidad entera, sin decimales.";
                return false;
            }

            if (texto.Contains('.') || texto.Count(c => c == ',') > 1
                || !System.Text.RegularExpressions.Regex.IsMatch(texto, @"\A[+-]?[0-9]+(,[0-9]{1,3})?\z"))
            {
                error = "Formato inválido. Use formato argentino, con coma decimal (por ejemplo, 5,5); no use puntos de miles.";
                return false;
            }

            int coma = texto.IndexOf(',');
            if (coma >= 0 && texto.Length - coma - 1 is < 1 or > 3)
            {
                error = "La cantidad admite como máximo tres decimales y debe tener dígitos después de la coma.";
                return false;
            }

            if (!decimal.TryParse(texto, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                    Argentina, out valor))
            {
                error = "Formato inválido. Use números con coma decimal argentina, por ejemplo 5,5.";
                return false;
            }

            if (valor <= 0)
            {
                error = "La cantidad debe ser mayor que cero.";
                return false;
            }

            return true;
        }

        private void MostrarErrorStock(ProductoDisponibleRemito producto, decimal solicitada, decimal disponible)
        {
            decimal faltante = Math.Max(0, solicitada - disponible);
            MessageBox.Show(this,
                $"{producto.Producto}. Solicitado: {FormatearCantidad(solicitada, producto.SeVendePorPeso)}.\n" +
                $"Disponible para este remito: {FormatearCantidad(disponible, producto.SeVendePorPeso)}.\n" +
                $"Faltante: {FormatearCantidad(faltante, producto.SeVendePorPeso)}.",
                "Stock insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static string FormatearCantidad(decimal cantidad, bool seVendePorPeso) =>
            $"{cantidad.ToString(seVendePorPeso ? "0.###" : "0", Argentina)} {(seVendePorPeso ? "kg" : "un.")}";

        private void QuitarProducto()
        {
            var p = ProductoSeleccionado();
            if (ocupado || !quitar.Enabled || p == null) return;
            borrador.RemoveAll(f => f.ProductoId == p.ProductoId);
            RefrescarBorrador();
            PrepararCantidad();
        }

        private async Task EmitirAsync()
        {
            if (ocupado || !revisar.Enabled || !RemitosDatos.PuedePrepararEmision) return;
            ocupado = true;
            ActualizarAcciones();
            RemitosDatos.ResultadoEmision? resultado = null;
            Guid? claveIntento = null;
            try
            {
                var filas = await datos.ValidarProductosEmisionActualAsync(borrador.ToArray());
                if (IsDisposed) return;
                string resumen = string.Join("\n", filas.Take(5).Select(p =>
                    $"{p.Producto}: {p.Cantidad.ToString(p.SeVendePorPeso ? "0.###" : "0", Argentina)} {(p.SeVendePorPeso ? "kg" : "un.")}"));
                if (filas.Length > 5) resumen += $"\nY {filas.Length - 5} productos más, según la lista.";
                if (MessageBox.Show(this, $"¿Emitir este remito al local?\n{resumen}\nSe descontará la mercadería del depósito y quedará en tránsito.",
                    "Confirmar emisión", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
                claveIntento = Guid.NewGuid();
                resultado = await datos.EmitirRemitoAsync(claveIntento.Value, filas);
            }
            catch (Exception ex)
            {
                if (IsDisposed) return;
                bool pendiente = File.Exists(RemitosDatos.ArchivoEmisionPendiente);
                if (pendiente && claveIntento.HasValue)
                {
                    // Solo consulta la clave persistida. Nunca reintenta la emision.
                    try { resultado = await datos.ConciliarEmisionPendienteAsync(claveIntento.Value); }
                    catch { /* Mantener el registro pendiente y el bloqueo. */ }
                }
                if (resultado == null && !IsDisposed)
                {
                    string mensajeError = pendiente
                        ? "No se pudo confirmar el resultado. Se conservaron los productos, las cantidades y la clave. No vuelva a enviarlo ni borre su registro. Pulse Actualizar disponibles para comprobar la misma operación."
                        : "No se emitió el remito. " + ex.Message;
                    MessageBox.Show(this, mensajeError, "Emisión sin resultado confirmado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally
            {
                ocupado = false;
                if (!IsDisposed) ActualizarAcciones();
            }
            if (resultado != null && !IsDisposed) MostrarEmisionConfirmada(resultado);
        }

        private void MostrarEmisionConfirmada(RemitosDatos.ResultadoEmision resultado)
        {
            HuboEmisiones = true;
            MessageBox.Show(this, $"Remito N.º {resultado.RemitoId} emitido.\nEstado: {resultado.Estado}\nFecha: {resultado.FechaEmision:dd/MM/yyyy HH:mm:ss}",
                "Emisión confirmada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
