using System.Data;
using PacuIbera.Datos;

namespace PacuIbera.UI.Common
{
    public partial class FormDeposito
    {
        private DataGridView dgvMovimientos = null!;
        private TextBox txtBuscarMovimientos = null!;
        private Label lblMovimientos = null!;
        private Button btnDetalleMovimiento = null!;
        private DataTable? tablaMovimientos;

        private void InicializarMovimientos()
        {
            var cabecera = new TableLayoutPanel { Dock = DockStyle.Top, Height = 88, RowCount = 2, ColumnCount = 1 };
            cabecera.Controls.Add(new Label
            {
                Text = "Ingresos por recepción de proveedores · Esta versión no incluye transferencias.",
                Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(10, 8, 0, 0),
                ForeColor = Color.FromArgb(45, 55, 65)
            });
            var filtros = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8), WrapContents = false };
            txtBuscarMovimientos = new TextBox { Width = 300, PlaceholderText = "Buscar por pedido, proveedor o usuario..." };
            var actualizar = new Button { Text = "Actualizar", AutoSize = true };
            EstiloUI.AplicarEstiloBotonAccion(actualizar, EstiloUI.TipoAccion.Positivo);
            actualizar.Click += (_, _) => CargarMovimientos();
            txtBuscarMovimientos.TextChanged += (_, _) => FiltrarMovimientos();
            filtros.Controls.Add(txtBuscarMovimientos);
            filtros.Controls.Add(actualizar);
            cabecera.Controls.Add(filtros);
            dgvMovimientos = FormDetalleRecepcion.CrearTabla();
            dgvMovimientos.AutoGenerateColumns = false;
            string[,] columnas = {
                { "RecepcionId", "Recepción" }, { "PedidoId", "Pedido" }, { "Proveedor", "Proveedor" },
                { "FechaRecepcion", "Fecha y hora" }, { "Usuario", "Recibió" }, { "Estado", "Estado" }
            };
            for (int i = 0; i < columnas.GetLength(0); i++)
                dgvMovimientos.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = columnas[i, 0], DataPropertyName = columnas[i, 0], HeaderText = columnas[i, 1], MinimumWidth = 90
                });
            dgvMovimientos.Columns["FechaRecepcion"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            dgvMovimientos.Columns["Estado"].MinimumWidth = 155;
            var pie = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 80, Padding = new Padding(10), FlowDirection = FlowDirection.TopDown, WrapContents = false };
            lblMovimientos = new Label { AutoSize = true };
            btnDetalleMovimiento = new Button { Text = "Ver detalle", AutoSize = true, Enabled = false };
            EstiloUI.AplicarEstiloBotonAccion(btnDetalleMovimiento, EstiloUI.TipoAccion.Editar);
            btnDetalleMovimiento.Click += (_, _) => VerDetalleMovimiento();
            dgvMovimientos.SelectionChanged += (_, _) => btnDetalleMovimiento.Enabled = dgvMovimientos.CurrentRow?.DataBoundItem is DataRowView;
            pie.Controls.Add(lblMovimientos);
            pie.Controls.Add(btnDetalleMovimiento);
            tabMovimientos.Controls.Add(dgvMovimientos);
            tabMovimientos.Controls.Add(pie);
            tabMovimientos.Controls.Add(cabecera);
        }

        private void CargarMovimientos()
        {
            try
            {
                tablaMovimientos = new RecepcionesDatos().ObtenerHistorial();
                FiltrarMovimientos();
            }
            catch (Exception ex)
            {
                tablaMovimientos = null;
                dgvMovimientos.DataSource = null;
                btnDetalleMovimiento.Enabled = false;
                lblMovimientos.Text = "No se pudo cargar el historial. Intente actualizar.";
                MessageBox.Show(this, "Error consultando recepciones: " + ex.Message, "Movimientos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FiltrarMovimientos()
        {
            if (tablaMovimientos == null) return;
            string texto = txtBuscarMovimientos.Text.Trim();
            // Filtrar copias con busqueda literal, sin interpretar comodines ni expresiones.
            DataTable visibles = tablaMovimientos.Clone();
            foreach (DataRow fila in tablaMovimientos.Rows)
                if (new[] { "PedidoId", "Proveedor", "Usuario" }.Any(c =>
                    Convert.ToString(fila[c])?.Contains(texto, StringComparison.CurrentCultureIgnoreCase) == true))
                    visibles.ImportRow(fila);
            dgvMovimientos.DataSource = visibles;
            btnDetalleMovimiento.Enabled = dgvMovimientos.CurrentRow?.DataBoundItem is DataRowView;
            lblMovimientos.Text = tablaMovimientos.Rows.Count == 0 ? "No hay recepciones registradas."
                : visibles.Rows.Count == 0 ? "No hay recepciones que coincidan con la búsqueda."
                : $"Recepciones: {visibles.Rows.Count}";
        }

        private void VerDetalleMovimiento()
        {
            if (dgvMovimientos.CurrentRow?.DataBoundItem is not DataRowView fila) return;
            try
            {
                var datos = new RecepcionesDatos().ObtenerDetalleHistorial(Convert.ToInt32(fila["RecepcionId"]));
                if (datos.Tables.Count != 3 || datos.Tables[0].Rows.Count == 0)
                    throw new InvalidOperationException("La recepción seleccionada ya no está disponible.");
                using var detalle = new FormDetalleRecepcion(datos);
                detalle.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo consultar el detalle: " + ex.Message, "Recepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
