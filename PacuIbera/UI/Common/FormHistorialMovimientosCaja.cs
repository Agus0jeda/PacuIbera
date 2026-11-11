using System.Drawing;
using System.Globalization;
using Datos;
using PacuIbera.Dominio;

namespace PacuIbera.UI.Common;

public sealed class FormHistorialMovimientosCaja : Form
{
    private sealed record CajaFiltro(int? Id, string Texto)
    {
        public override string ToString() => Texto;
    }

    private readonly MovimientoCajaDatos datos = new();
    private DateTimePicker dtDesde = null!;
    private DateTimePicker dtHasta = null!;
    private ComboBox cmbCaja = null!;
    private ComboBox cmbTipo = null!;
    private DataGridView grilla = null!;
    private TextBox txtMotivo = null!;
    private Label lblEstado = null!;
    private Button btnActualizar = null!;
    private bool cargando;

    public FormHistorialMovimientosCaja()
    {
        if (!MovimientoCajaDatos.PuedeConsultarHistorial)
            throw new UnauthorizedAccessException("Solo un Administrador o Gerente activo puede consultar el historial.");

        ConstruirInterfaz();
        Load += async (_, _) => await CargarFiltrosAsync();
    }

    private void ConstruirInterfaz()
    {
        Text = "Historial de movimientos de caja";
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9.5F);
        AutoScaleMode = AutoScaleMode.Font;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(12, 8, 12, 8),
            ColumnCount = 1, RowCount = 5
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        Controls.Add(layout);

        var titulo = new Label
        {
            Text = "HISTORIAL DE MOVIMIENTOS DE EFECTIVO", Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(32, 96, 50),
            Font = new Font("Segoe UI", 14F, FontStyle.Bold)
        };
        layout.Controls.Add(titulo, 0, 0);

        var filtros = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, WrapContents = true, AutoScroll = true,
            FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0, 5, 0, 2),
            Margin = Padding.Empty
        };
        dtDesde = CrearFecha();
        dtHasta = CrearFecha();
        cmbCaja = new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(4, 4, 8, 4) };
        cmbTipo = new ComboBox { Width = 125, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(4, 4, 8, 4) };
        cmbTipo.Items.AddRange(["Todos", "Ingreso", "Retiro"]);
        cmbTipo.SelectedIndex = 0;
        filtros.Controls.Add(CrearGrupoFiltro("Desde", dtDesde));
        filtros.Controls.Add(CrearGrupoFiltro("Hasta", dtHasta));
        filtros.Controls.Add(CrearGrupoFiltro("Caja", cmbCaja));
        filtros.Controls.Add(CrearGrupoFiltro("Tipo", cmbTipo));
        btnActualizar = new Button
        {
            Text = "Actualizar", Width = 96, Height = 32, Margin = new Padding(4, 22, 4, 4),
            BackColor = Color.LightGreen, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
        };
        btnActualizar.FlatAppearance.BorderSize = 0;
        btnActualizar.Click += async (_, _) => await AplicarFiltrosAsync();
        filtros.Controls.Add(btnActualizar);
        layout.Controls.Add(filtros, 0, 1);

        grilla = new DataGridView
        {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
            AllowUserToDeleteRows = false, AllowUserToResizeRows = false,
            MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoGenerateColumns = false, AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
            BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle,
            RowHeadersVisible = false, ColumnHeadersHeight = 34,
            EnableHeadersVisualStyles = false
        };
        grilla.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(226, 241, 226);
        grilla.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grilla.DefaultCellStyle.SelectionBackColor = Color.FromArgb(196, 226, 196);
        grilla.DefaultCellStyle.SelectionForeColor = Color.Black;
        grilla.Columns.Add(Columna("Fecha y hora", "FechaHora", 135, "dd/MM/yyyy HH:mm"));
        grilla.Columns.Add(Columna("Caja", "CajaId", 65));
        grilla.Columns.Add(Columna("Vendedor responsable", "ResponsableCaja", 150));
        grilla.Columns.Add(Columna("Movimiento", "TipoMovimiento", 105));
        grilla.Columns.Add(Columna("Importe", "ImporteConSigno", 135));
        grilla.Columns.Add(Columna("Administrador", "Administrador", 155));
        grilla.SelectionChanged += (_, _) => MostrarMotivoSeleccionado();
        layout.Controls.Add(grilla, 0, 2);

        lblEstado = new Label
        {
            Dock = DockStyle.Fill, Text = "Seleccioná un movimiento para consultar el motivo.",
            TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.DimGray,
            AutoEllipsis = true
        };
        layout.Controls.Add(lblEstado, 0, 3);

        var panelMotivo = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(0, 2, 0, 0)
        };
        panelMotivo.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        panelMotivo.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panelMotivo.Controls.Add(new Label
        {
            Text = "Motivo del movimiento", Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(45, 70, 50)
        }, 0, 0);
        txtMotivo = new TextBox
        {
            Dock = DockStyle.Fill, Multiline = true, ReadOnly = true,
            ScrollBars = ScrollBars.Vertical, BackColor = Color.FromArgb(248, 250, 248)
        };
        panelMotivo.Controls.Add(txtMotivo, 0, 1);
        layout.Controls.Add(panelMotivo, 0, 4);
    }

    private static DateTimePicker CrearFecha() => new()
    {
        Width = 125, Format = DateTimePickerFormat.Short, ShowCheckBox = true,
        Checked = false, Margin = new Padding(4, 4, 8, 4)
    };

    private static Control CrearGrupoFiltro(string titulo, Control control)
    {
        var grupo = new FlowLayoutPanel
        {
            AutoSize = true, WrapContents = false, FlowDirection = FlowDirection.TopDown,
            Margin = new Padding(2, 0, 5, 0), Padding = Padding.Empty
        };
        grupo.Controls.Add(new Label
        {
            Text = titulo, AutoSize = true, Margin = new Padding(4, 0, 0, 0),
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = Color.FromArgb(60, 75, 60)
        });
        grupo.Controls.Add(control);
        return grupo;
    }

    private static DataGridViewTextBoxColumn Columna(string titulo, string propiedad, int ancho, string? formato = null)
    {
        var columna = new DataGridViewTextBoxColumn
        {
            HeaderText = titulo, DataPropertyName = propiedad, Width = ancho,
            MinimumWidth = Math.Min(ancho, 60), SortMode = DataGridViewColumnSortMode.NotSortable
        };
        if (formato != null) columna.DefaultCellStyle.Format = formato;
        return columna;
    }

    private async Task CargarFiltrosAsync()
    {
        try
        {
            btnActualizar.Enabled = false;
            var cajas = await datos.ObtenerCajasConMovimientosAsync();
            cmbCaja.BeginUpdate();
            cmbCaja.Items.Clear();
            cmbCaja.Items.Add(new CajaFiltro(null, "Todas las cajas"));
            foreach (var caja in cajas)
                cmbCaja.Items.Add(new CajaFiltro(caja.CajaId, $"Caja N.º {caja.CajaId} · {caja.Responsable}"));
            cmbCaja.SelectedIndex = 0;
            cmbCaja.EndUpdate();
            await AplicarFiltrosAsync();
        }
        catch
        {
            lblEstado.Text = "No se pudo cargar el historial. Verificá tu acceso y la conexión configurada.";
            grilla.DataSource = null;
            txtMotivo.Clear();
        }
        finally { if (!IsDisposed) btnActualizar.Enabled = true; }
    }

    private async Task AplicarFiltrosAsync()
    {
        if (cargando || IsDisposed) return;
        cargando = true;
        btnActualizar.Enabled = false;
        lblEstado.Text = "Consultando movimientos...";
        try
        {
            DateTime? desde = dtDesde.Checked ? dtDesde.Value.Date : null;
            DateTime? hasta = dtHasta.Checked ? dtHasta.Value.Date : null;
            int? cajaId = (cmbCaja.SelectedItem as CajaFiltro)?.Id;
            string? tipo = cmbTipo.SelectedItem?.ToString() is "Ingreso" or "Retiro"
                ? cmbTipo.SelectedItem.ToString() : null;

            var movimientos = await datos.ObtenerHistorialAsync(desde, hasta, cajaId, tipo);
            grilla.DataSource = movimientos;
            txtMotivo.Clear();
            if (movimientos.Count == 0)
                lblEstado.Text = "No se encontraron movimientos con esos filtros.";
            else
            {
                lblEstado.Text = $"Se encontraron {movimientos.Count} movimiento(s). Seleccioná una fila para ver el motivo completo.";
                if (grilla.Rows.Count > 0) grilla.Rows[0].Selected = true;
                MostrarMotivoSeleccionado();
            }
        }
        catch (ArgumentException ex)
        {
            lblEstado.Text = ex.Message;
            grilla.DataSource = null;
            txtMotivo.Clear();
        }
        catch
        {
            lblEstado.Text = "No se pudo actualizar el historial. Verificá tu acceso y la conexión configurada.";
            grilla.DataSource = null;
            txtMotivo.Clear();
        }
        finally
        {
            cargando = false;
            if (!IsDisposed) btnActualizar.Enabled = true;
        }
    }

    private void MostrarMotivoSeleccionado()
    {
        if (grilla.CurrentRow?.DataBoundItem is MovimientoCajaDatos.MovimientoCajaHistorial movimiento)
            txtMotivo.Text = movimiento.Motivo;
        else
            txtMotivo.Clear();
    }
}
