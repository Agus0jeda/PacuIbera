using System.Data;
using System.Globalization;
using PacuIbera.Datos;
using PacuIbera.Dominio;

namespace PacuIbera.UI.Common
{
    // Borrador en memoria hasta la confirmacion explicita desde la revision.
    public sealed class FormRecepcionPedido : Form
    {
        private sealed class CantidadEditor : NumericUpDown
        {
            protected override void UpdateEditText()
            {
                if (UserEdit)
                {
                    base.UpdateEditText();
                    return;
                }

                Text = Value.ToString(DecimalPlaces == 0 ? "0" : "0.###", CultureInfo.CurrentCulture);
            }

            protected override void ValidateEditText()
            {
                if (!decimal.TryParse(Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal valor)
                    || valor < Minimum || valor > Maximum
                    || decimal.Round(valor, DecimalPlaces) != valor)
                {
                    // Conservar el texto para que Revisar informe el error, sin truncarlo.
                    BackColor = Color.MistyRose;
                    return;
                }
                BackColor = SystemColors.Window;
                base.ValidateEditText();
            }
        }

        private sealed class LoteEditor
        {
            public NumericUpDown Cantidad { get; init; } = null!;
            public CheckBox TieneVencimiento { get; init; } = null!;
            public DateTimePicker Fecha { get; init; } = null!;
        }

        private sealed class ProductoEditor
        {
            public int DetallePedidoProveedorId { get; init; }
            public string Nombre { get; init; } = "";
            public decimal Solicitada { get; init; }
            public bool PorPeso { get; init; }
            public bool ControlaVencimiento { get; init; }
            public NumericUpDown Recibida { get; init; } = null!;
            public List<LoteEditor> Lotes { get; } = new();
        }

        private readonly List<ProductoEditor> productos = new();
        private readonly int pedidoId;
        private readonly string proveedor;
        private readonly RecepcionesDatos recepcionesDatos = new();
        private bool confirmando;
        public bool RecepcionConfirmada { get; private set; }

        public FormRecepcionPedido(int pedidoId, string proveedor, DataTable detalles)
        {
            this.pedidoId = pedidoId;
            this.proveedor = proveedor;
            FormClosing += (_, e) => { if (confirmando) e.Cancel = true; };
            Text = $"Recepción del pedido {pedidoId}";
            Font = new Font("Segoe UI", 9F);
            BackColor = Color.White;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1000, 700);
            MinimumSize = new Size(900, 500);
            MaximizeBox = false;
            var cabecera = new Label
            {
                Text = $"PEDIDO N.º {pedidoId} — {proveedor}\nBorrador de recepción: revise antes de confirmar definitivamente.",
                Dock = DockStyle.Top, Height = 70, Padding = new Padding(16),
                BackColor = Color.FromArgb(45, 55, 65), ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            var contenido = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, AutoScroll = true,
                FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(12)
            };
            foreach (DataRow detalle in detalles.Rows)
                contenido.Controls.Add(CrearProducto(detalle));
            contenido.ClientSizeChanged += (_, _) =>
            {
                foreach (Control control in contenido.Controls)
                    control.Width = Math.Max(820, contenido.ClientSize.Width - 42);
            };
            var pie = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom, Height = 62, Padding = new Padding(12),
                FlowDirection = FlowDirection.RightToLeft, BackColor = Color.WhiteSmoke
            };
            var revisar = new Button { Text = "Revisar recepción", AutoSize = true };
            var cancelar = new Button { Text = "Cancelar", AutoSize = true, DialogResult = DialogResult.Cancel };
            EstiloUI.AplicarEstiloBotonAccion(revisar, EstiloUI.TipoAccion.Positivo);
            EstiloUI.AplicarEstiloBotonAccion(cancelar, EstiloUI.TipoAccion.Editar);
            revisar.Click += (_, _) => RevisarRecepcion();
            CancelButton = cancelar;
            pie.Controls.Add(revisar);
            pie.Controls.Add(cancelar);
            Controls.Add(contenido);
            Controls.Add(pie);
            Controls.Add(cabecera);
        }

        private static NumericUpDown CrearCantidad(bool porPeso, decimal maximo)
        {
            return new CantidadEditor
            {
                DecimalPlaces = porPeso ? 3 : 0, Increment = porPeso ? 0.001M : 1M,
                Minimum = 0, Maximum = maximo, Width = 115
            };
        }

        private static Label Etiqueta(string texto) => new Label
        {
            Text = texto, AutoSize = true, Margin = new Padding(6, 7, 6, 3)
        };

        private Control CrearProducto(DataRow detalle)
        {
            bool porPeso = Convert.ToBoolean(detalle["SeVendePorPeso"]);
            decimal solicitada = Convert.ToDecimal(detalle["Cantidad"]);
            var producto = new ProductoEditor
            {
                DetallePedidoProveedorId = Convert.ToInt32(detalle["DetallePedidoProveedorId"]),
                Nombre = Convert.ToString(detalle["Producto"]) ?? "",
                PorPeso = porPeso, Solicitada = solicitada,
                ControlaVencimiento = Convert.ToBoolean(detalle["ControlaVencimiento"]),
                Recibida = CrearCantidad(porPeso, solicitada)
            };
            productos.Add(producto);
            var bloque = new FlowLayoutPanel
            {
                Width = 900, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(820, 0), FlowDirection = FlowDirection.TopDown,
                WrapContents = false, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(8),
                Margin = new Padding(0, 0, 0, 12)
            };
            var titulo = Etiqueta(producto.Nombre + (porPeso ? " (por peso)" : " (por unidad)"));
            titulo.Font = new Font(Font, FontStyle.Bold);
            bloque.Controls.Add(titulo);
            bloque.Controls.Add(Etiqueta(producto.ControlaVencimiento
                ? "Este producto requiere una fecha de vencimiento por lote recibido."
                : "Fecha de vencimiento opcional por lote; si se informa, se conservará."));
            var cantidades = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
            string Formatear(decimal valor) =>
                $"{valor.ToString(porPeso ? "0.###" : "0", CultureInfo.GetCultureInfo("es-AR"))}{(porPeso ? " kg" : " un.")}";
            var faltante = Etiqueta($"Faltante: {Formatear(solicitada)}");
            producto.Recibida.ValueChanged += (_, _) =>
                faltante.Text = $"Faltante: {Formatear(solicitada - producto.Recibida.Value)}";
            cantidades.Controls.Add(Etiqueta($"Solicitada: {Formatear(solicitada)}"));
            cantidades.Controls.Add(Etiqueta("Recibida:"));
            cantidades.Controls.Add(producto.Recibida);
            cantidades.Controls.Add(Etiqueta(porPeso ? "kg" : "un."));
            cantidades.Controls.Add(faltante);
            bloque.Controls.Add(cantidades);
            var lotes = new FlowLayoutPanel
            {
                AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false
            };
            bloque.Controls.Add(lotes);
            var agregar = new Button { Text = "Agregar lote", AutoSize = true };
            EstiloUI.AplicarEstiloBotonAccion(agregar, EstiloUI.TipoAccion.Positivo);
            agregar.Click += (_, _) => AgregarLote(producto, lotes);
            bloque.Controls.Add(agregar);
            return bloque;
        }

        private void AgregarLote(ProductoEditor producto, FlowLayoutPanel contenedor)
        {
            var lote = new LoteEditor
            {
                Cantidad = CrearCantidad(producto.PorPeso, producto.Solicitada),
                TieneVencimiento = new CheckBox
                {
                    Text = producto.ControlaVencimiento ? "Vencimiento obligatorio" : "Tiene vencimiento",
                    Checked = producto.ControlaVencimiento, Enabled = !producto.ControlaVencimiento,
                    AutoSize = true, Margin = new Padding(8, 6, 8, 0)
                },
                Fecha = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 145,
                    ShowCheckBox = true, Checked = false, Enabled = producto.ControlaVencimiento }
            };
            var fila = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
            fila.Controls.Add(Etiqueta("Cantidad del lote:"));
            fila.Controls.Add(lote.Cantidad);
            fila.Controls.Add(Etiqueta(producto.PorPeso ? "kg" : "un."));
            fila.Controls.Add(lote.TieneVencimiento);
            fila.Controls.Add(lote.Fecha);
            lote.TieneVencimiento.CheckedChanged += (_, _) => lote.Fecha.Enabled = lote.TieneVencimiento.Checked;
            var quitar = new Button { Text = "Quitar lote", AutoSize = true };
            quitar.Click += (_, _) =>
            {
                producto.Lotes.Remove(lote);
                contenedor.Controls.Remove(fila);
                fila.Dispose();
            };
            fila.Controls.Add(quitar);
            producto.Lotes.Add(lote);
            contenedor.Controls.Add(fila);
        }

        private static decimal LeerCantidad(NumericUpDown control, bool porPeso)
        {
            // Revisar el texto pendiente antes de que NumericUpDown lo redondee o limite.
            if (!decimal.TryParse(control.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal valor)
                || valor < 0 || valor > control.Maximum
                || (porPeso ? decimal.Round(valor, 3) != valor : decimal.Truncate(valor) != valor))
            {
                control.Focus();
                throw new InvalidOperationException(porPeso
                    ? "Ingrese una cantidad válida, dentro de lo solicitado y con hasta tres decimales."
                    : "Ingrese una cantidad entera válida, dentro de lo solicitado.");
            }
            return valor;
        }

        private void RevisarRecepcion()
        {
            try
            {
                if (productos.Count == 0) throw new InvalidOperationException("El pedido no tiene productos.");
                var resumen = new DataTable();
                resumen.Columns.Add("Producto", typeof(string));
                resumen.Columns.Add("Solicitado", typeof(decimal));
                resumen.Columns.Add("Recibido", typeof(decimal));
                resumen.Columns.Add("Faltante", typeof(decimal));
                var lotes = new DataTable();
                lotes.Columns.Add("Producto", typeof(string));
                lotes.Columns.Add("Lote", typeof(int));
                lotes.Columns.Add("Cantidad", typeof(decimal));
                lotes.Columns.Add("Vencimiento", typeof(string));
                var entradaDetalles = new DataTable();
                entradaDetalles.Columns.Add("DetallePedidoProveedorId", typeof(int));
                entradaDetalles.Columns.Add("CantidadRecibida", typeof(decimal));
                var entradaLotes = new DataTable();
                entradaLotes.Columns.Add("DetallePedidoProveedorId", typeof(int));
                entradaLotes.Columns.Add("LoteNumero", typeof(int));
                entradaLotes.Columns.Add("Cantidad", typeof(decimal));
                entradaLotes.Columns.Add("TieneVencimiento", typeof(bool));
                entradaLotes.Columns.Add("FechaVencimiento", typeof(DateTime));
                foreach (var producto in productos)
                {
                    decimal recibida = LeerCantidad(producto.Recibida, producto.PorPeso);
                    decimal suma = 0;
                    for (int i = 0; i < producto.Lotes.Count; i++)
                    {
                        var lote = producto.Lotes[i];
                        decimal cantidad = LeerCantidad(lote.Cantidad, producto.PorPeso);
                        if (cantidad <= 0) throw new InvalidOperationException($"{producto.Nombre}: quite los lotes vacíos; cada lote debe tener cantidad positiva.");
                        if (producto.ControlaVencimiento && !lote.TieneVencimiento.Checked)
                            throw new InvalidOperationException($"{producto.Nombre}: cada lote recibido requiere vencimiento.");
                        if (lote.TieneVencimiento.Checked && !lote.Fecha.Checked)
                        {
                            lote.Fecha.Focus();
                            throw new InvalidOperationException($"{producto.Nombre}: seleccione la fecha de vencimiento del lote {i + 1}.");
                        }
                        if (lote.TieneVencimiento.Checked && lote.Fecha.Value.Date < DateTime.Today)
                            throw new InvalidOperationException($"{producto.Nombre}: el lote {i + 1} tiene fecha vencida.");
                        suma += cantidad;
                        string fecha = lote.TieneVencimiento.Checked ? lote.Fecha.Value.ToString("dd/MM/yyyy") : "Sin vencimiento";
                        lotes.Rows.Add(producto.Nombre, i + 1, cantidad, fecha);
                        entradaLotes.Rows.Add(producto.DetallePedidoProveedorId, i + 1, cantidad,
                            lote.TieneVencimiento.Checked,
                            lote.TieneVencimiento.Checked ? (object)lote.Fecha.Value.Date : DBNull.Value);
                    }
                    if (suma != recibida)
                        throw new InvalidOperationException($"{producto.Nombre}: los lotes suman {suma:0.###} y la cantidad recibida es {recibida:0.###}.");
                    resumen.Rows.Add(producto.Nombre, producto.Solicitada, recibida, producto.Solicitada - recibida);
                    entradaDetalles.Rows.Add(producto.DetallePedidoProveedorId, recibida);
                }
                MostrarRevision(resumen, lotes, entradaDetalles, entradaLotes);
                if (RecepcionConfirmada)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(this, ex.Message, "Revisar recepción", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void MostrarRevision(DataTable resumen, DataTable lotes, DataTable entradaDetalles, DataTable entradaLotes)
        {
                // Las tablas son una copia de valores; la revision no recibe controles del borrador.
                using var revision = new Form
                {
                    Text = "Revisar recepción", Size = new Size(900, 500), MinimumSize = new Size(780, 420),
                    StartPosition = FormStartPosition.CenterParent, Font = Font, MinimizeBox = false,
                    BackColor = Color.White
                };
                var cabecera = new Label
                {
                    Text = $"REVISIÓN DE RECEPCIÓN · PEDIDO N.º {pedidoId}\nProveedor: {proveedor}",
                    Dock = DockStyle.Fill, Padding = new Padding(14),
                    BackColor = Color.FromArgb(45, 55, 65), ForeColor = Color.White,
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold), AutoEllipsis = true
                };
                int recibidos = resumen.AsEnumerable().Count(f => f.Field<decimal>("Recibido") > 0);
                int conFaltantes = resumen.AsEnumerable().Count(f => f.Field<decimal>("Faltante") > 0);
                var totales = new Label
                {
                    Text = $"Productos recibidos: {recibidos} de {resumen.Rows.Count}   ·   Productos con faltantes: {conFaltantes}\nCantidades faltantes destacadas por producto. Confirme para guardar la recepción.",
                    Dock = DockStyle.Fill, Padding = new Padding(12),
                    BackColor = Color.FromArgb(250, 248, 243),
                    ForeColor = Color.FromArgb(45, 55, 65), Font = new Font(Font, FontStyle.Bold)
                };
                var tablaProductos = CrearTablaRevision(resumen);
                tablaProductos.Columns["Producto"].FillWeight = 220;
                AplicarFormatoCantidades(tablaProductos);
                tablaProductos.CellFormatting += (_, e) =>
                {
                    if (e.RowIndex >= 0 && tablaProductos.Columns[e.ColumnIndex].Name == "Faltante"
                        && e.Value is decimal cantidad && cantidad > 0 && e.CellStyle != null)
                    {
                        e.CellStyle.BackColor = Color.MistyRose;
                        e.CellStyle.ForeColor = Color.Maroon;
                        e.CellStyle.SelectionBackColor = Color.Maroon;
                        e.CellStyle.SelectionForeColor = Color.White;
                    }
                };
                var tablaLotes = CrearTablaRevision(lotes);
                tablaLotes.Columns["Producto"].FillWeight = 200;
                AplicarFormatoCantidades(tablaLotes);
                var tituloLotes = new Label
                {
                    Text = lotes.Rows.Count == 0 ? "LOTES · No hay lotes: no se recibieron productos."
                        : "LOTES POR PRODUCTO · Los productos no recibidos no tienen lotes.",
                    Dock = DockStyle.Fill, Padding = new Padding(12, 6, 0, 0),
                    ForeColor = Color.FromArgb(45, 55, 65), Font = new Font(Font, FontStyle.Bold)
                };
                var pie = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft,
                    Padding = new Padding(12), BackColor = Color.WhiteSmoke
                };
                var cerrar = new Button { Text = "Cerrar revisión", AutoSize = true, DialogResult = DialogResult.Cancel };
                var volver = new Button { Text = "Volver a editar", AutoSize = true, DialogResult = DialogResult.OK };
                var confirmar = new Button { Text = "Confirmar recepción definitiva", AutoSize = true };
                EstiloUI.AplicarEstiloBotonAccion(confirmar, EstiloUI.TipoAccion.Positivo);
                revision.FormClosing += (_, e) => { if (confirmando) e.Cancel = true; };
                confirmar.Click += async (_, _) =>
                {
                    if (confirmando || RecepcionConfirmada) return;
                    confirmando = true;
                    confirmar.Enabled = volver.Enabled = cerrar.Enabled = false;
                    revision.UseWaitCursor = true;
                    bool enviado = false;
                    try
                    {
                        // Siempre consultar antes de enviar, incluso al reabrir una revision fallida.
                        var estado = await recepcionesDatos.ConsultarEstadoAsync(pedidoId);
                        if (estado == RecepcionesDatos.EstadoRecepcion.Confirmada)
                        {
                            RecepcionConfirmada = true;
                            MessageBox.Show(revision, "El pedido ya tiene una recepción definitiva. Se actualizará Depósito.");
                            return;
                        }
                        if (estado != RecepcionesDatos.EstadoRecepcion.Disponible)
                            throw new InvalidOperationException("El pedido ya no está disponible para recibir. Cierre el borrador y actualice la lista.");
                        if (SesionActiva.IdUsuario <= 0)
                            throw new InvalidOperationException("No hay un usuario receptor válido. Inicie sesión nuevamente.");
                        // Comprobar configuracion vigente y version SQL antes de pedir confirmacion.
                        await recepcionesDatos.ValidarVencimientosAsync(pedidoId, entradaLotes);
                        if (entradaLotes.AsEnumerable().Any(l => l.Field<bool>("TieneVencimiento")
                            && l.Field<DateTime>("FechaVencimiento").Date < DateTime.Today))
                            throw new InvalidOperationException("Un lote venció desde que abrió la revisión. Vuelva a editar la recepción.");
                        if (MessageBox.Show(revision,
                            "La recepción cerrará el pedido y no podrá repetirse. ¿Desea confirmar definitivamente?",
                            "Confirmar recepción", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
                        enviado = true;
                        await recepcionesDatos.ConfirmarAsync(pedidoId, SesionActiva.IdUsuario, entradaDetalles, entradaLotes);
                        RecepcionConfirmada = true;
                        MessageBox.Show(revision, "Recepción confirmada. Se actualizarán los pedidos y el stock.");
                    }
                    catch (Exception ex)
                    {
                        if (enviado)
                        {
                            try
                            {
                                var estado = await recepcionesDatos.ConsultarEstadoAsync(pedidoId);
                                if (estado == RecepcionesDatos.EstadoRecepcion.Confirmada)
                                {
                                    RecepcionConfirmada = true;
                                    MessageBox.Show(revision, "Se verificó que el pedido tiene una recepción definitiva. No se volverá a enviar.");
                                }
                                else
                                    MessageBox.Show(revision, "No se confirmó la recepción. Se conserva el borrador. Antes de otro envío se verificará nuevamente el pedido.\n" + ex.Message);
                            }
                            catch
                            {
                                MessageBox.Show(revision, "No se pudo verificar el resultado. Se conserva el borrador. No vuelva a cargar mercadería: al intentar confirmar se consultará el estado y solo se enviará si puede verificarse como pendiente sin recepción.");
                            }
                        }
                        else
                            MessageBox.Show(revision, "No se envió la recepción. Se conserva el borrador.\n" + ex.Message,
                                "Recepción de pedido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    finally
                    {
                        confirmando = false;
                        revision.UseWaitCursor = false;
                        confirmar.Enabled = volver.Enabled = cerrar.Enabled = !RecepcionConfirmada;
                        if (RecepcionConfirmada) revision.Close();
                    }
                };
                EstiloUI.AplicarEstiloBotonAccion(cerrar, EstiloUI.TipoAccion.Editar);
                EstiloUI.AplicarEstiloBotonAccion(volver, EstiloUI.TipoAccion.Positivo);
                pie.Controls.Add(cerrar);
                pie.Controls.Add(volver);
                pie.Controls.Add(confirmar);
                int altoProductos = Math.Clamp((resumen.Rows.Count + 1) * 34 + 8, 72, 190);
                int altoLotes = Math.Clamp((lotes.Rows.Count + 1) * 32 + 8, 72, 190);
                revision.ClientSize = new Size(900, Math.Clamp(78 + 58 + altoProductos + 30 + altoLotes + 64, 420, 720));
                var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6 };
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, altoProductos));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, altoLotes));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
                layout.Controls.Add(cabecera, 0, 0);
                layout.Controls.Add(totales, 0, 1);
                layout.Controls.Add(tablaProductos, 0, 2);
                layout.Controls.Add(tituloLotes, 0, 3);
                layout.Controls.Add(tablaLotes, 0, 4);
                layout.Controls.Add(pie, 0, 5);
                revision.Controls.Add(layout);
                revision.CancelButton = cerrar;
                revision.ShowDialog(this);
        }

        private void AplicarFormatoCantidades(DataGridView tabla)
        {
            var tiposPorProducto = productos.ToDictionary(p => p.Nombre, p => p.PorPeso, StringComparer.Ordinal);
            tabla.CellFormatting += (_, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Value is not decimal valor)
                    return;

                var fila = tabla.Rows[e.RowIndex];
                if (!fila.Cells.Contains(fila.Cells["Producto"]) || fila.Cells["Producto"].Value is not string nombre
                    || !tiposPorProducto.TryGetValue(nombre, out bool porPeso))
                    return;

                string columna = tabla.Columns[e.ColumnIndex].Name;
                if (columna is not ("Solicitada" or "Recibido" or "Faltante" or "Cantidad"))
                    return;

                e.Value = valor.ToString(porPeso ? "0.### 'kg'" : "0 'un.'", CultureInfo.GetCultureInfo("es-AR"));
                e.FormattingApplied = true;
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            };
        }

        private static DataGridView CrearTablaRevision(DataTable datos)
        {
            var tabla = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                AllowUserToDeleteRows = false, AllowUserToOrderColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
                RowHeadersVisible = false, ScrollBars = ScrollBars.Both,
                BindingContext = new BindingContext(), DataSource = datos,
                BackgroundColor = Color.White, Margin = new Padding(12, 0, 12, 0)
            };
            EstiloUI.AplicarEstiloGrid(tabla);
            foreach (DataGridViewColumn columna in tabla.Columns)
            {
                columna.MinimumWidth = columna.Name == "Producto" ? 220 : 110;
                if (columna.ValueType == typeof(decimal))
                {
                    columna.DefaultCellStyle.Format = "0.###";
                    columna.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
            return tabla;
        }
    }
}
