using PacuIbera.Datos;
using PacuIbera.Dominio;
using System.Globalization;

namespace PacuIbera.UI.Common
{
    public sealed class FormDetalleRemito : Form
    {
        public FormDetalleRemito(DetalleRemitoInterno detalle)
        {
            if (!RemitosDatos.PuedeConsultar) throw new UnauthorizedAccessException("Su rol no puede consultar remitos.");
            var r = detalle.Cabecera;
            bool sinRecepcion = !r.FechaRecepcion.HasValue;
            bool conDiferencias = detalle.Productos.Any(p => p.DiferenciaOriginal > 0);
            bool conResoluciones = detalle.Resoluciones.Count > 0 || detalle.Productos.Any(p => p.Resuelto > 0);
            bool mezclaUnidades = detalle.Productos.Select(p => p.SeVendePorPeso).Distinct().Count() > 1;
            int registros = Math.Max(detalle.Productos.Count, Math.Max(detalle.Lotes.Count, detalle.Resoluciones.Count));
            Text = $"Consulta del remito {r.Id}";
            Font = new Font("Segoe UI", 9F);
            BackColor = Color.White;
            Size = new Size(1050, Math.Min(700, 440 + Math.Min(registros, 9) * 26));
            MinimumSize = new Size(900, 440);
            StartPosition = FormStartPosition.CenterParent;
            static string Fecha(DateTime? fecha) => fecha?.ToString("dd/MM/yyyy HH:mm") ?? "Pendiente";
            var encabezado = new TableLayoutPanel
            {
                Dock = DockStyle.Top, Height = 84, ColumnCount = 3, RowCount = 2,
                Padding = new Padding(10, 4, 10, 6), BackColor = Color.FromArgb(45, 55, 65), ForeColor = Color.White
            };
            for (int i = 0; i < 3; i++) encabezado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 3));
            encabezado.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            encabezado.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var titulo = new Label
            {
                Text = $"REMITO N.º {r.Id}  ·  {r.Estado}", Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter, Font = new Font(Font.FontFamily, 12F, FontStyle.Bold), AutoEllipsis = true
            };
            encabezado.Controls.Add(titulo, 0, 0); encabezado.SetColumnSpan(titulo, 3);
            Label Dato(string nombre, string valor) => new()
            {
                Text = nombre + "\n" + valor, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(8, 2, 8, 0), AutoEllipsis = true
            };
            encabezado.Controls.Add(Dato("Enviado por", string.IsNullOrWhiteSpace(r.Emisor) ? "Pendiente" : r.Emisor), 0, 1);
            encabezado.Controls.Add(Dato("Fecha de emisión", Fecha(r.FechaEmision)), 1, 1);
            encabezado.Controls.Add(Dato("Recibido por", string.IsNullOrWhiteSpace(r.Receptor) ? "Pendiente" : r.Receptor), 2, 1);
            var resumen = new TableLayoutPanel
            {
                Dock = DockStyle.Top, Height = mezclaUnidades ? 76 : 60, RowCount = 1, Padding = new Padding(10, 5, 10, 5),
                BackColor = Color.WhiteSmoke
            };
            string Total(Func<RemitoProductoDetalle, decimal?> valor, string pendiente)
            {
                if (detalle.Productos.Count == 0) return "Sin productos";
                if (detalle.Productos.Any(p => !valor(p).HasValue)) return pendiente;
                return string.Join("\n", detalle.Productos.GroupBy(p => p.SeVendePorPeso).OrderBy(g => g.Key)
                    .Select(g => g.Sum(p => valor(p)!.Value).ToString(g.Key ? "0.###" : "0", CultureInfo.GetCultureInfo("es-AR"))
                        + (g.Key ? " por peso" : " unid.")));
            }
            var tarjetas = new List<(string Nombre, string Valor)> { ("Enviados", Total(p => p.Enviado, "Pendiente")) };
            if (sinRecepcion) tarjetas.Add(("Recepción", "Aún no registrada"));
            else
            {
                tarjetas.Add(("Recibidos al llegar", Total(p => p.Recibido, "Sin recepción")));
                if (conDiferencias) tarjetas.Add(("Por resolver", Total(p => p.Pendiente, "Sin recepción")));
                if (conResoluciones) tarjetas.Add(("Resueltos", Total(p => p.Resuelto, "Pendiente")));
            }
            resumen.ColumnCount = tarjetas.Count;
            void Tarjeta(int columna, string nombre, string valor)
            {
                resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / tarjetas.Count));
                var tarjeta = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(4, 0, 4, 0),
                    ColumnCount = 1, RowCount = 2
                };
                tarjeta.RowStyles.Add(new RowStyle(SizeType.Absolute, 21));
                tarjeta.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                tarjeta.Controls.Add(new Label { Text = nombre, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomCenter,
                    Margin = Padding.Empty, ForeColor = Color.DimGray }, 0, 0);
                tarjeta.Controls.Add(new Label { Text = valor, Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopCenter, Margin = Padding.Empty,
                    Font = new Font(Font, FontStyle.Bold), ForeColor = Color.FromArgb(45, 55, 65), AutoEllipsis = true }, 0, 1);
                resumen.Controls.Add(tarjeta, columna, 0);
            }
            for (int i = 0; i < tarjetas.Count; i++) Tarjeta(i, tarjetas[i].Nombre, tarjetas[i].Valor);
            var tabs = new TabControl { Dock = DockStyle.Fill, Padding = new Point(20, 7) };
            var productos = AgregarTabla(tabs, "Productos", detalle.Productos);
            Col(productos, "Producto", "Producto", 210);
            Col(productos, "Enviado", "Enviado");
            Col(productos, "Recibido", "Recibido al llegar", 135);
            Col(productos, "DiferenciaOriginal", "Diferencia inicial", 130);
            Col(productos, "Resuelto", "Resuelto");
            Col(productos, "Pendiente", "Por resolver");
            productos.Columns["Recibido"].Visible = !sinRecepcion;
            productos.Columns["DiferenciaOriginal"].Visible = !sinRecepcion && conDiferencias;
            productos.Columns["Pendiente"].Visible = !sinRecepcion && conDiferencias;
            productos.Columns["Resuelto"].Visible = !sinRecepcion && conResoluciones;
            if (sinRecepcion)
            {
                productos.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "EstadoEnvio", HeaderText = "Estado del envío", Width = 180,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                });
                productos.CellFormatting += (_, e) =>
                {
                    if (e.RowIndex >= 0 && productos.Columns[e.ColumnIndex].Name == "EstadoEnvio")
                    {
                        e.Value = r.Estado;
                        e.FormattingApplied = true;
                    }
                };
            }
            productos.Columns["Producto"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            productos.Columns["Producto"].MinimumWidth = 190;
            productos.Columns["Recibido"].DefaultCellStyle.NullValue = "Sin recepción";
            productos.Columns["DiferenciaOriginal"].DefaultCellStyle.NullValue = "Sin recepción";
            productos.Columns["Pendiente"].DefaultCellStyle.NullValue = "Pendiente";
            var lotes = AgregarTabla(tabs, "Lotes enviados", detalle.Lotes);
            Col(lotes, "Producto", "Producto", 200);
            Col(lotes, "Id", "Lote del remito");
            Col(lotes, "DepositoStockId", "Lote origen depósito", 145);
            Col(lotes, "RecepcionRemitoLoteId", "Detalle de recepción", 145);
            Col(lotes, "LoteLocalId", "Lote local inicial");
            Col(lotes, "FechaIngresoOrigen", "Ingreso de origen", 145, true);
            Col(lotes, "Vencimiento", "Vencimiento", 145, true);
            lotes.Columns["Vencimiento"].DefaultCellStyle.NullValue = "Sin vencimiento";
            Col(lotes, "Enviado", "Enviado");
            Col(lotes, "Recibido", "Aceptado inicialmente", 155);
            Col(lotes, "DiferenciaOriginal", "Faltante original", 145);
            Col(lotes, "Resuelto", "Resuelto");
            Col(lotes, "Pendiente", "Pendiente");
            Col(lotes, "Observacion", "Observación", 300);
            lotes.Columns["Recibido"].DefaultCellStyle.NullValue = "Sin recepción";
            lotes.Columns["DiferenciaOriginal"].DefaultCellStyle.NullValue = "Sin recepción";
            lotes.Columns["Pendiente"].DefaultCellStyle.NullValue = "Pendiente";
            var resoluciones = AgregarTabla(tabs, "Resoluciones", detalle.Resoluciones);
            Col(resoluciones, "ResolucionId", "Resolución");
            Col(resoluciones, "Producto", "Producto", 200);
            Col(resoluciones, "RemitoLoteId", "Lote del remito");
            Col(resoluciones, "RecepcionRemitoLoteId", "Diferencia de origen", 145);
            Col(resoluciones, "Tipo", "Tipo", 170);
            Col(resoluciones, "Cantidad", "Cantidad");
            Col(resoluciones, "Estado", "Estado");
            Col(resoluciones, "Responsable", "Responsable", 170);
            Col(resoluciones, "Fecha", "Fecha", 150, true);
            Col(resoluciones, "Vencimiento", "Vencimiento", 145, true);
            resoluciones.Columns["Vencimiento"].DefaultCellStyle.NullValue = "Sin vencimiento";
            Col(resoluciones, "LoteLocalDestinoId", "Lote destino local", 145);
            Col(resoluciones, "DepositoStockDestinoId", "Lote destino depósito", 155);
            Col(resoluciones, "Motivo", "Motivo", 350);
            var notas = new List<string>();
            if (sinRecepcion) notas.Add(r.Estado == "En tránsito"
                ? "En tránsito: aún no se registró recepción. No hay faltantes confirmados."
                : "Aún no se registró recepción para este remito.");
            else if (detalle.Resoluciones.Count == 0) notas.Add("No hay resoluciones registradas.");
            var nota = new Label
            {
                Text = string.Join("\n", notas), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.DimGray, AutoEllipsis = true
            };
            var pie = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 64, Padding = new Padding(16, 8, 16, 8),
                ColumnCount = 2, RowCount = 1, BackColor = Color.WhiteSmoke };
            pie.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pie.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            var cerrar = new Button { Text = "Cerrar", AutoSize = true, DialogResult = DialogResult.Cancel };
            EstiloUI.AplicarEstiloBotonAccion(cerrar, EstiloUI.TipoAccion.Editar);
            cerrar.Anchor = AnchorStyles.Right;
            cerrar.Margin = new Padding(12, 0, 0, 0);
            pie.Controls.Add(nota, 0, 0); pie.Controls.Add(cerrar, 1, 0);
            var areaTablas = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 8, 12, 0) };
            areaTablas.Controls.Add(tabs);
            var historial = new Label
            {
                Text = $"Creado por {r.Creador} · {Fecha(r.FechaCreacion)}"
                    + (r.FechaRecepcion.HasValue ? $"   |   Recepción: {Fecha(r.FechaRecepcion)}" : "")
                    + (r.FechaCancelacion.HasValue ? $"\nCancelado por {r.Cancelador} · {Fecha(r.FechaCancelacion)} · {r.MotivoCancelacion}" : ""),
                Dock = DockStyle.Top, Height = r.FechaCancelacion.HasValue ? 48 : 26,
                Padding = new Padding(12, 4, 12, 0), TextAlign = ContentAlignment.TopCenter,
                ForeColor = Color.DimGray, AutoEllipsis = true
            };
            Controls.Add(areaTablas);
            Controls.Add(pie);
            // Conservar las fechas y responsables de auditoria dentro de Lotes, sin repetirlos en el encabezado.
            ((TabPage)lotes.Parent!).Controls.Add(historial);
            Controls.Add(resumen);
            Controls.Add(encabezado);
            CancelButton = cerrar;
        }

        private static DataGridView AgregarTabla<T>(TabControl tabs, string titulo, List<T> filas)
        {
            var pagina = new TabPage(titulo) { BackColor = Color.White, Padding = new Padding(6) };
            var tabla = FormRemitos.CrearGrilla();
            tabla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            tabla.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(246, 249, 247);
            tabla.DataSource = filas;
            pagina.Controls.Add(tabla);
            if (filas.Count == 0)
                pagina.Controls.Add(new Label { Text = titulo == "Resoluciones" ? "No hay resoluciones registradas." : "No hay registros en esta sección.",
                    Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.DimGray,
                    Height = 38, Padding = new Padding(8) });
            tabs.TabPages.Add(pagina);
            return tabla;
        }

        private static void Col(DataGridView tabla, string propiedad, string titulo, int ancho = 125, bool fecha = false)
            => FormRemitos.AgregarColumna(tabla, propiedad, titulo, ancho, fecha);
    }
}
