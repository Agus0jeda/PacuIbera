using System.Globalization;
using PacuIbera.Datos;
using PacuIbera.Dominio;

namespace PacuIbera.UI.Common
{
    public sealed class FormRemitos : Form
    {
        private readonly RemitosDatos datos = new();
        private readonly DataGridView grilla = CrearGrilla();
        private readonly TextBox numero = new() { Width = 170, PlaceholderText = "Número exacto de remito" };
        private readonly ComboBox estado = new() { Width = 190, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Button actualizar = new() { Text = "Actualizar", AutoSize = true };
        private readonly Button verDetalle = new() { Text = "Ver detalle", AutoSize = true, Enabled = false };
        private readonly Button preparar = new() { Text = "Emitir remito", AutoSize = true };
        private readonly Button recibir = new() { Text = "Recepción de remitos", AutoSize = true };
        private readonly Button resolver = new() { Text = "Resolver diferencias", AutoSize = true };
        private readonly Label mensaje = new() { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
        private List<RemitoInterno> remitos = new();
        private bool ocupado;

        public FormRemitos()
        {
            if (!RemitosDatos.PuedeConsultar) throw new UnauthorizedAccessException("Su rol no puede consultar remitos.");
            Text = "Remitos internos — consulta";
            Font = new Font("Segoe UI", 9F);
            BackColor = Color.White;
            FormBorderStyle = FormBorderStyle.None;
            var titulo = new Label
            {
                Text = SesionActiva.Rol == "Gerente" ? "REMITOS INTERNOS · SOLO LECTURA" : "REMITOS INTERNOS", Dock = DockStyle.Top, Height = 54,
                Padding = new Padding(14), Font = new Font(Font, FontStyle.Bold),
                BackColor = Color.WhiteSmoke, ForeColor = Color.FromArgb(45, 55, 65)
            };
            var filtros = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(10), WrapContents = true };
            estado.Items.Add("Todos los estados");
            if (SesionActiva.Rol is "Administrador" or "Gerente") estado.Items.AddRange(new object[] { "Borrador", "Cancelado" });
            estado.Items.AddRange(new object[] { "En tránsito", "Recibido", "Con diferencias", "Cerrado" });
            estado.SelectedIndex = 0;
            filtros.Controls.Add(numero);
            filtros.Controls.Add(estado);
            filtros.Controls.Add(actualizar);
            EstiloUI.AplicarEstiloBotonAccion(actualizar, EstiloUI.TipoAccion.Positivo);
            EstiloUI.AplicarEstiloBotonAccion(verDetalle, EstiloUI.TipoAccion.Editar);
            AgregarColumna(grilla, nameof(RemitoInterno.Id), "Remito", 80);
            AgregarColumna(grilla, nameof(RemitoInterno.Estado), "Estado", 150);
            AgregarColumna(grilla, nameof(RemitoInterno.Emisor), "Emisor", 160);
            AgregarColumna(grilla, nameof(RemitoInterno.FechaEmision), "Emisión", 145, fecha: true);
            AgregarColumna(grilla, nameof(RemitoInterno.Receptor), "Receptor", 160);
            AgregarColumna(grilla, nameof(RemitoInterno.FechaRecepcion), "Recepción", 145, fecha: true);
            var pie = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 64, ColumnCount = 2, Padding = new Padding(10) };
            pie.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pie.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            pie.Controls.Add(mensaje, 0, 0);
            var acciones = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
            acciones.Controls.Add(verDetalle);
            preparar.Visible = RemitosDatos.PuedePrepararEmision;
            EstiloUI.AplicarEstiloBotonAccion(preparar, EstiloUI.TipoAccion.Positivo);
            preparar.Click += async (_, _) => await AbrirEmisionAsync();
            acciones.Controls.Add(preparar);
            recibir.Visible = RemitosDatos.PuedePrepararRecepcion;
            EstiloUI.AplicarEstiloBotonAccion(recibir, EstiloUI.TipoAccion.Positivo);
            recibir.Click += async (_, _) => await AbrirRecepcionBorradorAsync();
            acciones.Controls.Add(recibir);
            resolver.Visible = RemitosDatos.PuedePrepararResolucion;
            EstiloUI.AplicarEstiloBotonAccion(resolver, EstiloUI.TipoAccion.Editar);
            resolver.Click += async (_, _) => await AbrirResolucionBorradorAsync();
            acciones.Controls.Add(resolver);
            pie.Controls.Add(acciones, 1, 0);
            Controls.Add(grilla);
            Controls.Add(pie);
            Controls.Add(filtros);
            Controls.Add(titulo);
            numero.TextChanged += (_, _) => Filtrar();
            estado.SelectedIndexChanged += (_, _) => Filtrar();
            grilla.SelectionChanged += (_, _) => ActualizarAcciones();
            grilla.DataBindingComplete += (_, _) => ActualizarAcciones();
            actualizar.Click += async (_, _) => await CargarAsync();
            verDetalle.Click += async (_, _) => await AbrirDetalleAsync();
            Load += async (_, _) =>
            {
                EstiloUI.CrearBotonCerrar(this);
                await CargarAsync();
            };
            ActualizarAcciones();
        }

        private void Filtrar()
        {
            int? seleccionado = RemitoSeleccionado()?.Id;
            int? filtroNumero = null;
            if (!string.IsNullOrWhiteSpace(numero.Text))
            {
                if (!int.TryParse(numero.Text.Trim(), out int valor) || valor <= 0)
                {
                    grilla.DataSource = null;
                    verDetalle.Enabled = false;
                    mensaje.Text = "Ingrese un número de remito entero y positivo.";
                    return;
                }
                filtroNumero = valor;
            }
            var visibles = remitos.Where(r => (!filtroNumero.HasValue || r.Id == filtroNumero.Value)
                && (estado.SelectedIndex == 0 || r.Estado == estado.Text)).ToList();
            grilla.DataSource = visibles;
            grilla.ClearSelection();
            grilla.CurrentCell = null;
            if (seleccionado.HasValue)
                foreach (DataGridViewRow fila in grilla.Rows)
                    if (fila.DataBoundItem is RemitoInterno r && r.Id == seleccionado.Value)
                    {
                        grilla.CurrentCell = fila.Cells[0];
                        fila.Selected = true;
                    }
            ActualizarAcciones();
            mensaje.Text = remitos.Count == 0 ? "No hay remitos disponibles para su usuario."
                : visibles.Count == 0 ? "No hay remitos que coincidan con los filtros." : $"Remitos: {visibles.Count} · Solo lectura";
        }

        private async Task CargarAsync()
        {
            if (ocupado) return;
            ocupado = true;
            actualizar.Enabled = verDetalle.Enabled = false;
            UseWaitCursor = true;
            try
            {
                if (SesionActiva.Rol == "Gerente") await datos.ValidarConsultaRemitosActivaAsync();
                var resultado = await datos.ListarAsync();
                if (IsDisposed) return;
                remitos = resultado;
                Filtrar();
            }
            catch (Exception ex)
            {
                if (IsDisposed) return;
                remitos.Clear();
                grilla.DataSource = null;
                mensaje.Text = "No se pudo cargar la lista. Intente actualizar.";
                MessageBox.Show(this, ex.Message, "Consulta de remitos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ocupado = false;
                if (!IsDisposed)
                {
                    UseWaitCursor = false;
                    actualizar.Enabled = true;
                    ActualizarAcciones();
                }
            }
        }

        private async Task AbrirDetalleAsync()
        {
            var remito = RemitoSeleccionado();
            if (ocupado || remito == null) return;
            ocupado = true;
            actualizar.Enabled = verDetalle.Enabled = false;
            UseWaitCursor = true;
            try
            {
                var detalle = await datos.ObtenerDetalleAsync(remito.Id);
                if (IsDisposed) return;
                using var ventana = new FormDetalleRemito(detalle);
                ventana.ShowDialog(this);
            }
            catch (Exception ex)
            {
                if (!IsDisposed) MessageBox.Show(this, ex.Message, "Detalle de remito", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ocupado = false;
                if (!IsDisposed)
                {
                    UseWaitCursor = false;
                    actualizar.Enabled = true;
                    ActualizarAcciones();
                }
            }
        }

        private RemitoInterno? RemitoSeleccionado() => grilla.SelectedRows.Count == 1
            && !grilla.SelectedRows[0].IsNewRow && grilla.SelectedRows[0].DataBoundItem is RemitoInterno r && r.Id > 0 ? r : null;

        private void ActualizarAcciones()
        {
            verDetalle.Enabled = !ocupado && RemitoSeleccionado() != null;
            preparar.Enabled = !ocupado && RemitosDatos.PuedePrepararEmision;
            recibir.Enabled = !ocupado && RemitosDatos.PuedePrepararRecepcion;
            resolver.Enabled = !ocupado && RemitosDatos.PuedePrepararResolucion;
        }

        private async Task AbrirEmisionAsync()
        {
            if (ocupado || !RemitosDatos.PuedePrepararEmision) return;
            ocupado = true;
            bool refrescar = false;
            actualizar.Enabled = false;
            ActualizarAcciones();
            try
            {
                await datos.ValidarDestinoEmisionAsync();
                if (IsDisposed) return;
                using var ventana = new FormEmitirRemito();
                ventana.ShowDialog(this);
                refrescar = ventana.HuboEmisiones;
            }
            catch (Exception)
            {
                if (!IsDisposed) MessageBox.Show(this,
                    "No se pudo abrir la emisión. Compruebe la conexión configurada y que su usuario Administrador esté activo.",
                    "Emitir remito", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                ocupado = false;
                if (!IsDisposed) { actualizar.Enabled = true; ActualizarAcciones(); }
            }
            if (refrescar && !IsDisposed) await CargarAsync();
        }

        private async Task AbrirResolucionBorradorAsync()
        {
            if (ocupado || !RemitosDatos.PuedePrepararResolucion) return;
            bool refrescar = false;
            ocupado = true;
            actualizar.Enabled = false;
            ActualizarAcciones();
            try
            {
                await datos.ValidarAdministradorActivoAsync();
                if (IsDisposed) return;
                using var ventana = new FormResolverDiferencia();
                ventana.ShowDialog(this);
                refrescar = ventana.HuboResoluciones;
            }
            catch (Exception ex)
            {
                if (!IsDisposed) MessageBox.Show(this, ex.Message, "Resolver diferencias", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                ocupado = false;
                if (!IsDisposed) { actualizar.Enabled = true; ActualizarAcciones(); }
            }
            if (refrescar && !IsDisposed) await CargarAsync();
        }

        private async Task AbrirRecepcionBorradorAsync()
        {
            // Guardia de navegacion, ademas de Visible y de la validacion del formulario/datos.
            if (ocupado || !RemitosDatos.PuedePrepararRecepcion) return;
            ocupado = true;
            ActualizarAcciones();
            actualizar.Enabled = false;
            try
            {
                await datos.ValidarVendedorActivoAsync();
                if (IsDisposed) return;
                using var ventana = new FormRecepcionRemito();
                ventana.ShowDialog(this);
            }
            catch (Exception ex)
            {
                if (!IsDisposed) MessageBox.Show(this, ex.Message, "Recepción de remitos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                ocupado = false;
                if (!IsDisposed) { actualizar.Enabled = true; ActualizarAcciones(); }
            }
        }

        internal static DataGridView CrearGrilla()
        {
            var tabla = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false, RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                ScrollBars = ScrollBars.Both, BackgroundColor = Color.White
            };
            EstiloUI.AplicarEstiloGrid(tabla);
            tabla.CellFormatting += (_, e) =>
            {
                if (e.RowIndex < 0 || e.Value is not decimal || e.CellStyle == null) return;
                bool peso = tabla.Rows[e.RowIndex].DataBoundItem switch
                {
                    RemitoProductoDetalle p => p.SeVendePorPeso,
                    RemitoLoteDetalle l => l.SeVendePorPeso,
                    RemitoResolucionDetalle r => r.SeVendePorPeso,
                    ProductoDisponibleRemito p => p.SeVendePorPeso,
                    ProductoEmisionRemito p => p.SeVendePorPeso,
                    LoteRecepcionRemitoBorrador l => l.SeVendePorPeso,
                    ResolucionRemitoBorrador r => r.SeVendePorPeso,
                    _ => true
                };
                e.CellStyle.Format = peso ? "0.###" : "0";
                e.CellStyle.FormatProvider = CultureInfo.GetCultureInfo("es-AR");
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            };
            return tabla;
        }

        internal static void AgregarColumna(DataGridView tabla, string propiedad, string titulo, int ancho = 125, bool fecha = false)
        {
            var columna = new DataGridViewTextBoxColumn
            {
                Name = propiedad, DataPropertyName = propiedad, HeaderText = titulo, Width = ancho,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            columna.DefaultCellStyle.NullValue = "—";
            if (fecha) columna.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            tabla.Columns.Add(columna);
        }
    }
}
