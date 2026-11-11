using PacuIbera.Datos;
using PacuIbera.Dominio;
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public partial class FormDeposito : Form
    {
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private DataGridView dgvStock;
        private Button btnConfigurarMinimo;
        private Button btnTransferir;
        private Button btnVerLotes = null!;
        private Button btnRemitosInternos = null!;
        private Panel panel1;
        private Panel panelHeader;
        private Panel panel2;
        private TabControl tabDeposito = null!;
        private TabPage tabPedidos = null!;
        private TabPage tabStock = null!;
        private TabPage tabMovimientos = null!;
        private DataGridView dgvPedidos = null!;
        private TextBox txtBuscarPedidos = null!;
        private Button btnDetallePedido = null!;
        private Button btnRecibirPedido = null!;
        private Label lblEstadoPedidos = null!;
        private DataTable? tablaPedidos;
        private PedidosDatos pedidosDatos = new PedidosDatos();
        private Label lblFiltrar;
        private TextBox txtFiltrar;
        private BindingSource stockBinding = new BindingSource();
        private DataTable tablaStock;

        public FormDeposito()
        {
            InitializeComponent();
            Load += FormDeposito_Load;
            this.DoubleBuffered = true;
        }

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            dgvStock = new DataGridView();
            panelHeader = new Panel();
            lblFiltrar = new Label();
            txtFiltrar = new TextBox();
            btnConfigurarMinimo = new Button();
            btnTransferir = new Button();
            btnVerLotes = new Button { Text = "Ver lotes", Enabled = false, Size = new Size(120, 36) };
            btnVerLotes.Click += (_, _) => VerLotesProducto();
            dgvStock.SelectionChanged += (_, _) =>
                btnVerLotes.Enabled = dgvStock.CurrentRow?.DataBoundItem is DataRowView;
            panel1 = new Panel();
            panel2 = new Panel();
            tabDeposito = new TabControl();
            tabPedidos = new TabPage("Pedidos por recibir");
            tabStock = new TabPage("Stock actual");
            tabMovimientos = new TabPage("Movimientos");
            ((System.ComponentModel.ISupportInitialize)dgvStock).BeginInit();
            panelHeader.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // dgvStock
            // 
            dgvStock.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(45, 55, 65);
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = Color.White;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dgvStock.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvStock.ColumnHeadersHeight = 34;
            dgvStock.Dock = DockStyle.Fill;
            dgvStock.EnableHeadersVisualStyles = false;
            dgvStock.Location = new Point(0, 72);
            dgvStock.Name = "dgvStock";
            dgvStock.RowHeadersVisible = false;
            dgvStock.RowHeadersWidth = 51;
            dgvStock.Size = new Size(882, 331);
            dgvStock.TabIndex = 0;
            dgvStock.CellFormatting += DgvStock_CellFormatting;
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.WhiteSmoke;
            panelHeader.Controls.Add(lblFiltrar);
            panelHeader.Controls.Add(txtFiltrar);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(882, 72);
            panelHeader.TabIndex = 4;
            panelHeader.MouseDown += PanelHeader_MouseDown;
            // 
            // lblFiltrar
            // 
            lblFiltrar.AutoSize = true;
            lblFiltrar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblFiltrar.Location = new Point(14, 12);
            lblFiltrar.Name = "lblFiltrar";
            lblFiltrar.Size = new Size(56, 20);
            lblFiltrar.TabIndex = 0;
            lblFiltrar.Text = "Filtrar:";
            // 
            // txtFiltrar
            // 
            txtFiltrar.Location = new Point(65, 8);
            txtFiltrar.Name = "txtFiltrar";
            txtFiltrar.PlaceholderText = "Buscar por nombre o descripción...";
            txtFiltrar.Size = new Size(280, 27);
            txtFiltrar.TabIndex = 1;
            txtFiltrar.TextChanged += TxtFiltrar_TextChanged;
            InicializarFiltrosStock();
            // 
            // btnConfigurarMinimo
            // 
            btnConfigurarMinimo.Location = new Point(45, 3);
            btnConfigurarMinimo.Name = "btnConfigurarMinimo";
            btnConfigurarMinimo.Size = new Size(190, 36);
            btnConfigurarMinimo.TabIndex = 2;
            btnConfigurarMinimo.Text = "Configurar Minimo";
            btnConfigurarMinimo.Click += BtnConfigurarMinimo_Click;
            // 
            // btnTransferir
            // 
            btnTransferir.Location = new Point(309, 3);
            btnTransferir.Name = "btnTransferir";
            btnTransferir.Size = new Size(160, 36);
            btnTransferir.TabIndex = 1;
            btnTransferir.Text = "Transferir";
            // Conservar el control y el manejador antiguo, sin conectar su evento.
            btnTransferir.Visible = false;
            btnTransferir.Enabled = false;
            btnTransferir.TabStop = false;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnConfigurarMinimo);
            panel1.Controls.Add(btnTransferir);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 403);
            panel1.Name = "panel1";
            panel1.Size = new Size(882, 50);
            panel1.TabIndex = 3;
            // 
            // panel2
            // 
            panel2.Dock = DockStyle.Top;
            panel2.BackColor = Color.WhiteSmoke;
            panel2.Name = "panel2";
            panel2.Size = new Size(882, 46);
            panel2.TabIndex = 5;
            panel2.MouseDown += PanelHeader_MouseDown;
            panel2.Controls.Add(new Label
            {
                Text = "DEPÓSITO CENTRAL",
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 55, 65),
                Location = new Point(14, 12)
            });
            btnRemitosInternos = new Button
            {
                Name = "btnRemitosInternos", Text = "Remitos internos",
                Size = new Size(160, 30), Location = new Point(panel2.ClientSize.Width - 220, 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Visible = RemitosDatos.PuedePrepararEmision, Enabled = RemitosDatos.PuedePrepararEmision
            };
            EstiloUI.AplicarEstiloBotonAccion(btnRemitosInternos, EstiloUI.TipoAccion.Positivo);
            btnRemitosInternos.Size = new Size(160, 30);
            btnRemitosInternos.Click += (_, _) =>
            {
                if (!RemitosDatos.PuedePrepararEmision) return;
                if (Parent?.FindForm() is PrincipalForm principal)
                    principal.AbrirRemitosDesdeDeposito();
            };
            panel2.Controls.Add(btnRemitosInternos);

            tabDeposito.Name = "tabDeposito";
            tabDeposito.Dock = DockStyle.Fill;
            tabDeposito.Font = new Font("Segoe UI", 9F);
            tabDeposito.Padding = new Point(16, 7);
            tabPedidos.Name = "tabPedidos";
            tabStock.Name = "tabStock";
            tabMovimientos.Name = "tabMovimientos";
            tabPedidos.BackColor = Color.White;
            tabStock.BackColor = Color.White;
            tabMovimientos.BackColor = Color.White;
            InicializarPedidosPorRecibir();
            InicializarMovimientos();
            tabStock.Controls.Add(dgvStock);
            tabStock.Controls.Add(panel1);
            tabStock.Controls.Add(panelHeader);
            tabDeposito.TabPages.AddRange(new[] { tabPedidos, tabStock, tabMovimientos });
            // Mantener el stock como vista inicial del circuito existente.
            tabDeposito.SelectedTab = tabStock;
            // 
            // FormDeposito
            // 
            ClientSize = new Size(882, 453);
            Controls.Add(tabDeposito);
            Controls.Add(panel2);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FormDeposito";
            Text = "Depósito Central";
            ((System.ComponentModel.ISupportInitialize)dgvStock).EndInit();
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        private void InicializarPedidosPorRecibir()
        {
            var filtros = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 54, Padding = new Padding(10),
                BackColor = Color.WhiteSmoke, WrapContents = false
            };
            txtBuscarPedidos = new TextBox
            {
                Width = 280, PlaceholderText = "Buscar por pedido o proveedor..."
            };
            var actualizar = new Button { Text = "Actualizar", AutoSize = true };
            EstiloUI.AplicarEstiloBotonAccion(actualizar, EstiloUI.TipoAccion.Positivo);
            txtBuscarPedidos.TextChanged += (_, _) => FiltrarPedidos();
            actualizar.Click += (_, _) => CargarPedidosPorRecibir();
            filtros.Controls.Add(txtBuscarPedidos);
            filtros.Controls.Add(actualizar);
            dgvPedidos = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false
            };
            string[,] columnas = {
                { "Id", "Pedido" }, { "Proveedor", "Proveedor" },
                { "FechaPedido", "Fecha" }, { "CantidadItems", "Cantidad de productos" }, { "Estado", "Estado" }
            };
            for (int i = 0; i < columnas.GetLength(0); i++)
                dgvPedidos.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = columnas[i, 0], DataPropertyName = columnas[i, 0], HeaderText = columnas[i, 1]
                });
            dgvPedidos.Columns["FechaPedido"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            EstiloUI.AplicarEstiloGrid(dgvPedidos);
            var pie = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom, Height = 80, Padding = new Padding(10),
                FlowDirection = FlowDirection.TopDown, WrapContents = false
            };
            lblEstadoPedidos = new Label { AutoSize = true };
            btnDetallePedido = new Button { Text = "Ver detalle", AutoSize = true, Enabled = false };
            btnRecibirPedido = new Button { Text = "Recibir pedido", AutoSize = true, Enabled = false };
            EstiloUI.AplicarEstiloBotonAccion(btnRecibirPedido, EstiloUI.TipoAccion.Positivo);
            btnRecibirPedido.Click += (_, _) => AbrirRecepcionPedido();
            EstiloUI.AplicarEstiloBotonAccion(btnDetallePedido, EstiloUI.TipoAccion.Editar);
            btnDetallePedido.Click += (_, _) =>
            {
                if (dgvPedidos.CurrentRow?.DataBoundItem is DataRowView fila)
                {
                    using (var detalle = new FormDetallePedido(Convert.ToInt32(fila["Id"])))
                        detalle.ShowDialog(this);
                }
            };
            dgvPedidos.SelectionChanged += (_, _) => ActualizarAccionesPedido();
            pie.Controls.Add(lblEstadoPedidos);
            var acciones = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
            acciones.Controls.Add(btnDetallePedido);
            acciones.Controls.Add(btnRecibirPedido);
            pie.Controls.Add(acciones);
            tabPedidos.Controls.Add(dgvPedidos);
            tabPedidos.Controls.Add(pie);
            tabPedidos.Controls.Add(filtros);
        }

        private void CargarPedidosPorRecibir()
        {
            try
            {
                tablaPedidos = pedidosDatos.ObtenerPedidosPorRecibir();
                FiltrarPedidos();
            }
            catch (Exception ex)
            {
                tablaPedidos = null;
                dgvPedidos.DataSource = null;
                ActualizarAccionesPedido();
                lblEstadoPedidos.Text = "No se pudo cargar la lista. Intente actualizar.";
                MessageBox.Show(this, "Error consultando pedidos: " + ex.Message,
                    "Pedidos por recibir", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FiltrarPedidos()
        {
            if (tablaPedidos == null) return;
            string texto = txtBuscarPedidos.Text.Trim().Replace("'", "''")
                .Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]");
            tablaPedidos.DefaultView.RowFilter = texto.Length == 0 ? "" :
                $"CONVERT(Id, 'System.String') LIKE '%{texto}%' OR Proveedor LIKE '%{texto}%'";
            dgvPedidos.DataSource = tablaPedidos.DefaultView;
            ActualizarAccionesPedido();
            lblEstadoPedidos.Text = tablaPedidos.Rows.Count == 0
                ? "No hay pedidos pendientes disponibles para recibir."
                : tablaPedidos.DefaultView.Count == 0 ? "No hay pedidos que coincidan con la búsqueda."
                : $"Pedidos disponibles: {tablaPedidos.DefaultView.Count}";
        }

        private void ActualizarAccionesPedido()
        {
            bool seleccionado = dgvPedidos.CurrentRow?.DataBoundItem is DataRowView;
            btnDetallePedido.Enabled = seleccionado;
            btnRecibirPedido.Enabled = seleccionado;
        }

        private void AbrirRecepcionPedido()
        {
            if (dgvPedidos.CurrentRow?.DataBoundItem is not DataRowView fila) return;
            int pedidoId = Convert.ToInt32(fila["Id"]);
            try
            {
                // Revalidar la elegibilidad al abrir: la lista puede haber quedado desactualizada.
                DataTable elegibles = pedidosDatos.ObtenerPedidosPorRecibir();
                if (!elegibles.AsEnumerable().Any(p => p.Field<int>("Id") == pedidoId))
                {
                    MessageBox.Show(this, "El pedido ya no está disponible para recibir. Actualice la lista.");
                    CargarPedidosPorRecibir();
                    return;
                }
                DataTable cabecera = pedidosDatos.ObtenerCabeceraPedido(pedidoId);
                DataTable detalles = pedidosDatos.ObtenerDetalleParaRecepcion(pedidoId);
                if (cabecera.Rows.Count == 0 || detalles.Rows.Count == 0)
                    throw new InvalidOperationException("El pedido no tiene cabecera o productos disponibles.");
                using (var recepcion = new FormRecepcionPedido(pedidoId,
                    Convert.ToString(cabecera.Rows[0]["Proveedor"]) ?? "", detalles))
                {
                    recepcion.ShowDialog(this);
                    if (recepcion.RecepcionConfirmada)
                    {
                        CargarPedidosPorRecibir();
                        CargarStock();
                        CargarMovimientos();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo abrir la recepción: " + ex.Message,
                    "Recepción de pedido", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static Label CrearMensajePreparacion(string mensaje)
        {
            return new Label
            {
                Text = mensaje,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(64, 64, 64),
                Padding = new Padding(20)
            };
        }

        private void PanelHeader_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void FormDeposito_Load(object sender, EventArgs e)
        {
            // Aplicar estilos estandarizados
            EstiloUI.CrearBotonCerrar(this);
            EstiloUI.AplicarEstiloGrid(dgvStock);

            // Asignar textos explicitos
            btnTransferir.Text = "Transferir a Local";
            btnConfigurarMinimo.Text = "Stock Mínimo";

            // Aplicar estilos a botones inferiores
            EstiloUI.AplicarEstiloBotonAccion(btnConfigurarMinimo, EstiloUI.TipoAccion.Editar);
            EstiloUI.AplicarEstiloBotonAccion(btnTransferir, EstiloUI.TipoAccion.Positivo);
            EstiloUI.AplicarEstiloBotonAccion(btnVerLotes, EstiloUI.TipoAccion.Editar);

            // Recolocar botones en panel inferior (centrados)
            try
            {
                panel1.Controls.Clear();
                EstiloUI.AlinearBotonesInferiores(panel1, 6, btnConfigurarMinimo, btnVerLotes);
            }
            catch { }

            CargarStock();
            ConfigurarPermisos();
            CargarPedidosPorRecibir();
            CargarMovimientos();
        }

        private void ConfigurarPermisos()
        {
            btnConfigurarMinimo.Enabled = SesionActiva.Rol == "Administrador" || SesionActiva.Rol == "SuperAdministrador";
            btnRemitosInternos.Visible = btnRemitosInternos.Enabled = RemitosDatos.PuedePrepararEmision;
            btnTransferir.Visible = btnTransferir.Enabled = false;
        }

        private void CargarStock()
        {
            int? seleccionado = ProductoStockSeleccionado();
            try
            {
                var datos = new OtrosDatos();
                var tabla = datos.ObtenerStockDeposito();
                AgregarResumenVencimientos(tabla);
                // StockDeposito ya es el stock utilizable calculado por SQL.
                tabla.Columns["EstadoStock"]!.ReadOnly = false;
                tabla.Columns["EstadoStock"]!.MaxLength = -1;
                foreach (DataRow fila in tabla.Rows)
                {
                    decimal stock = Convert.ToDecimal(fila["StockDeposito"]);
                    decimal minimo = Convert.ToDecimal(fila["StockMinimoDeposito"]);
                    fila["EstadoStock"] = stock == 0 ? "SIN STOCK"
                        : stock < minimo ? "STOCK BAJO"
                        : minimo > 0 && stock == minimo ? "EN EL MÍNIMO" : "STOCK SUFICIENTE";
                }
                tablaStock = tabla;
                stockBinding.DataSource = tablaStock;
                dgvStock.DataSource = stockBinding;
                dgvStock.ColumnHeadersVisible = true;
                dgvStock.EnableHeadersVisualStyles = false;
                dgvStock.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                dgvStock.ScrollBars = ScrollBars.Both;
                ConfigurarNombresColumnas();
                foreach (string columna in new[] { "ProductoId", "Id", "SeVendePorPeso" })
                    if (dgvStock.Columns.Contains(columna)) dgvStock.Columns[columna].Visible = false;
                foreach (DataGridViewColumn columna in dgvStock.Columns)
                    columna.Width = columna.Name == "Nombre" || columna.Name == "Descripcion" ? 170 : 125;
                dgvStock.Columns["EstadoStock"].Width = 155;
                dgvStock.Columns["EstadoVencimiento"].Width = 170;
                dgvStock.Columns["ProximoVencimiento"].DefaultCellStyle.Format = "dd/MM/yyyy";
                dgvStock.Columns["ProximoVencimiento"].DefaultCellStyle.NullValue = "—";
                dgvStock.Columns["ProximoVencimiento"].ToolTipText = "Próxima fecha vigente de un lote activo con saldo. Los vencidos se indican por separado.";
                CargarCategoriasStock();
                AplicarFiltrosStock(seleccionado);


            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando stock: " + ex.Message);
            }
        }

        private void DgvStock_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && e.CellStyle != null
                && dgvStock.Rows[e.RowIndex].DataBoundItem is DataRowView fila
                && (dgvStock.Columns[e.ColumnIndex].DataPropertyName == "StockDeposito"
                    || dgvStock.Columns[e.ColumnIndex].DataPropertyName == "StockMinimoDeposito"
                    || dgvStock.Columns[e.ColumnIndex].DataPropertyName == "CantidadVencida"
                    || dgvStock.Columns[e.ColumnIndex].DataPropertyName == "CantidadProxima"))
            {
                // Solo formato de celda: e.Value y el DataTable siguen siendo numericos.
                bool porPeso = fila.Row.Field<bool>("SeVendePorPeso");
                string unidad = porPeso ? "kg" : "un.";
                e.CellStyle.Format = porPeso ? $"0.### '{unidad}'" : $"0 '{unidad}'";
                e.CellStyle.FormatProvider = CultureInfo.GetCultureInfo("es-AR");
            }
            if (e.RowIndex < 0 || !dgvStock.Columns.Contains("EstadoStock"))
                return;

            string estado = dgvStock.Rows[e.RowIndex]
                .Cells["EstadoStock"].Value?.ToString();

            if (estado == "SIN STOCK" || estado == "STOCK BAJO" || estado == "EN EL MÍNIMO")
            {
                Color fondo = estado == "SIN STOCK" ? Color.MistyRose
                    : estado == "STOCK BAJO" ? Color.PeachPuff : Color.LightGoldenrodYellow;
                e.CellStyle.BackColor = fondo;
                e.CellStyle.ForeColor = Color.FromArgb(64, 64, 64);
                e.CellStyle.SelectionBackColor = fondo;
                e.CellStyle.SelectionForeColor = Color.Black;
            }
            else
            {
                e.CellStyle.BackColor = e.RowIndex % 2 == 0
                    ? Color.White
                    : Color.FromArgb(245, 245, 245);
                e.CellStyle.ForeColor = Color.FromArgb(64, 64, 64);
            }
            if (e.ColumnIndex >= 0 && e.CellStyle != null
                && new[] { "EstadoVencimiento", "CantidadVencida", "CantidadProxima", "ProximoVencimiento" }
                    .Contains(dgvStock.Columns[e.ColumnIndex].Name))
            {
                string vencimiento = Convert.ToString(dgvStock.Rows[e.RowIndex].Cells["EstadoVencimiento"].Value) ?? "";
                // Colores limitados a vencimientos: no reemplazar la alerta de stock.
                e.CellStyle.BackColor = vencimiento == "VENCIDO" ? Color.MistyRose
                    : vencimiento == "PRÓXIMO A VENCER" ? Color.LightGoldenrodYellow : Color.White;
                e.CellStyle.ForeColor = vencimiento == "VENCIDO" ? Color.Maroon : Color.FromArgb(64, 64, 64);
                e.CellStyle.SelectionBackColor = Color.FromArgb(45, 55, 65);
                e.CellStyle.SelectionForeColor = Color.White;
            }
        }

        private void ConfigurarNombresColumnas()
        {
            string[,] encabezados =
            {
                { "ProductoId", "ID" },
                { "Id", "ID" },
                { "Nombre", "Producto" },
                { "Producto", "Producto" },
                { "NombreProducto", "Producto" },
                { "Descripcion", "Descripción" },
                { "Categoria", "Categoría" },
                { "NombreCategoria", "Categoría" },
                { "StockDeposito", "Stock en depósito" },
                { "StockMinimoDeposito", "Stock mínimo" },
                { "EstadoStock", "Estado del stock" },
                { "ProximoVencimiento", "Próximo vencimiento" },
                { "EstadoVencimiento", "Estado del vencimiento" },
                { "CantidadVencida", "Cant. vencida" },
                { "CantidadProxima", "Cant. próxima a vencer" }
            };
            for (int i = 0; i < encabezados.GetLength(0); i++)
            {
                string nombre = encabezados[i, 0];
                if (dgvStock.Columns.Contains(nombre))
                    dgvStock.Columns[nombre].HeaderText = encabezados[i, 1];
            }
        }

        private void TxtFiltrar_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltrosStock(ProductoStockSeleccionado());
        }
        private void BtnConfigurarMinimo_Click(object sender, EventArgs e)
        {
            if (SesionActiva.Rol != "Administrador" && SesionActiva.Rol != "SuperAdministrador")
            {
                MessageBox.Show(this, "No tiene permiso para modificar el mínimo.");
                return;
            }
            if (dgvStock.CurrentRow?.DataBoundItem is not DataRowView fila)
            {
                MessageBox.Show(this, "Seleccione un producto.");
                return;
            }
            try
            {
                int productoId = Convert.ToInt32(fila["ProductoId"]);
                var configuracion = new StockMinimoDatos().ObtenerConfiguracion(productoId);
                using var editor = new FormStockMinimo(configuracion.Nombre, configuracion.PorPeso, configuracion.Minimo);
                if (editor.ShowDialog(this) != DialogResult.OK) return;
                if (SesionActiva.Rol != "Administrador" && SesionActiva.Rol != "SuperAdministrador")
                    throw new InvalidOperationException("No tiene permiso para modificar el mínimo.");
                new OtrosDatos().ActualizarStockMinimoDeposito(productoId, editor.Minimo);
                MessageBox.Show(this, "Stock mínimo actualizado.", "Stock mínimo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarStock();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo completar el guardado del mínimo. Verifique el valor antes de reintentar.\n" + ex.Message,
                    "Stock mínimo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void VerLotesProducto()
        {
            if (dgvStock.CurrentRow?.DataBoundItem is not DataRowView fila) return;
            try
            {
                DataTable lotes = new StockLotesDatos().ObtenerLotes(Convert.ToInt32(fila["ProductoId"]));
                if (lotes.Rows.Count == 0)
                {
                    MessageBox.Show(this, "El producto seleccionado ya no está disponible.");
                    return;
                }
                using var detalle = new FormStockLotes(lotes);
                detalle.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudieron consultar los lotes: " + ex.Message,
                    "Stock por lote", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnTransferir_Click(object sender, EventArgs e)
        {
            // Circuito retirado de la interfaz. El codigo se conserva para revision posterior.
            if (!btnTransferir.Enabled || !btnTransferir.Visible) return;
            if (dgvStock.CurrentRow == null) { MessageBox.Show("Seleccione un producto"); return; }
            int productoId = Convert.ToInt32(dgvStock.CurrentRow.Cells["ProductoId"].Value);
            int stockDisponible = Convert.ToInt32(dgvStock.CurrentRow.Cells["StockDeposito"].Value);

            using (var form = new FormTransferirDeposito(productoId, stockDisponible))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var otros = new OtrosDatos();
                        otros.TransferirDepositoALocal(SesionActiva.IdUsuario, productoId, form.CantidadTransferir, form.FechaVencimiento);
                        MessageBox.Show("Transferencia realizada");
                        CargarStock();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error en transferencia: " + ex.Message);
                    }
                }
            }
        }
    }
}
