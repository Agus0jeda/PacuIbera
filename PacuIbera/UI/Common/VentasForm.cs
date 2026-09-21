using Datos;
using PacuIbera.Datos;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PacuIbera.Dominio;

namespace PacuIbera.UI.Common
{
    public partial class VentasForm : Form
    {
        // --- VARIABLES DE INTERFAZ ---
        private Panel pnlCabecera;
        private Panel pnlCobroLateral;
        private Panel pnlCentro;

        private ComboBox cmbCliente;
        private TextBox txtBuscador;
        private DataGridView dgvCarrito;
        private Label lblTotalGris;
        private Label lblTotalMonto;
        private Button btnCobrar;
        private Button btnEliminarItem;

        // --- VARIABLES DE DATOS ---
        private DataTable dtCarrito;
        private decimal totalVenta = 0;

        // Motor de la BD
        private ProductoDatos productoDatos = new ProductoDatos();
        private ClienteDatos clienteDatos = new ClienteDatos();

        private VentaDatos ventaDatos = new VentaDatos();
        private DataTable dtProductosGlobal;

        // --- VARIABLES DE SESIÓN (A conectar con el Login futuro) ---
        private int provinciaVendedorActual = 7;
        private int localidadVendedorActual = 25;

        public VentasForm()
        {
            InitializeComponent();

            // 1. Mejoras visuales de la ventana
            this.Text = "Punto de Venta";
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.KeyPreview = true;

            CrearInterfazGrafica();

            // Eventos
            this.Load += VentasForm_Load;
            this.KeyDown += VentasForm_KeyDown;
            txtBuscador.KeyDown += TxtBuscador_KeyDown;
            btnEliminarItem.Click += BtnEliminarItem_Click;
            btnCobrar.Click += BtnCobrar_Click;
        }

        private void VentasForm_Load(object sender, EventArgs e)
        {
            ConfigurarTablaCarrito();
            CargarBuscadorDinamico();
            CargarClientes();
            txtBuscador.Focus();
        }

        // --- CARGA SEGURA DEL BUSCADOR ---
        private void CargarBuscadorDinamico()
        {
            try
            {
                dtProductosGlobal = productoDatos.ObtenerProductos();
                AutoCompleteStringCollection coleccion = new AutoCompleteStringCollection();

                // Blindaje: Verificamos si la columna existe en la consulta SQL actual
                bool tieneCodigo = dtProductosGlobal.Columns.Contains("CodigoBarras");

                foreach (DataRow row in dtProductosGlobal.Rows)
                {
                    coleccion.Add(row["Nombre"].ToString());

                    if (tieneCodigo && row["CodigoBarras"] != DBNull.Value && !string.IsNullOrWhiteSpace(row["CodigoBarras"].ToString()))
                    {
                        coleccion.Add(row["CodigoBarras"].ToString());
                    }
                }

                txtBuscador.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                txtBuscador.AutoCompleteSource = AutoCompleteSource.CustomSource;
                txtBuscador.AutoCompleteCustomSource = coleccion;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar productos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarClientes()
        {
            try
            {
                DataTable dtClientes = clienteDatos.ObtenerClientesParaVenta();

                // Blindaje: Solo agregamos Consumidor Final si no existe ya en la tabla
                if (dtClientes.Select("Id = 0").Length == 0)
                {
                    DataRow filaCF = dtClientes.NewRow();
                    filaCF["Id"] = 0;
                    filaCF["Nombre"] = "CONSUMIDOR FINAL";
                    filaCF["Apellido"] = "";
                    dtClientes.Rows.InsertAt(filaCF, 0);
                }

                if (!dtClientes.Columns.Contains("NombreCompleto"))
                    dtClientes.Columns.Add("NombreCompleto", typeof(string), "Nombre + ' ' + Apellido");

                // Cargamos la memoria del autocompletado
                AutoCompleteStringCollection coleccionClientes = new AutoCompleteStringCollection();
                foreach (DataRow row in dtClientes.Rows)
                {
                    coleccionClientes.Add(row["NombreCompleto"].ToString().Trim());
                }

                // LIMPIAMOS el combo antes de recargarlo para evitar mareos
                cmbCliente.DataSource = null;
                cmbCliente.Items.Clear();

                cmbCliente.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                cmbCliente.AutoCompleteSource = AutoCompleteSource.CustomSource;
                cmbCliente.AutoCompleteCustomSource = coleccionClientes;

                cmbCliente.DataSource = dtClientes;
                cmbCliente.DisplayMember = "NombreCompleto";
                cmbCliente.ValueMember = "Id";
                cmbCliente.SelectedIndex = 0;
            }
            catch
            {
                cmbCliente.DataSource = null;
                cmbCliente.Items.Clear();
                cmbCliente.Items.Add("CONSUMIDOR FINAL");
                cmbCliente.SelectedIndex = 0;
            }
        }

        // --- MOTOR INTELIGENTE DE BÚSQUEDA ---
        private void TxtBuscador_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                string busqueda = txtBuscador.Text.Trim();
                if (string.IsNullOrEmpty(busqueda)) return;

                bool tieneCodigo = dtProductosGlobal.Columns.Contains("CodigoBarras");
                DataRow productoEncontrado = null;

                // Búsqueda tolerante a mayúsculas/minúsculas
                foreach (DataRow row in dtProductosGlobal.Rows)
                {
                    string nombreProd = row["Nombre"].ToString();
                    string codBarras = tieneCodigo ? row["CodigoBarras"].ToString() : "";

                    if (nombreProd.Equals(busqueda, StringComparison.OrdinalIgnoreCase) ||
                        codBarras.Equals(busqueda, StringComparison.OrdinalIgnoreCase))
                    {
                        productoEncontrado = row;
                        break;
                    }
                }

                if (productoEncontrado != null)
                {
                    AgregarAlCarrito(productoEncontrado);
                    txtBuscador.Clear();
                }
                else
                {
                    MessageBox.Show("Producto no encontrado. Seleccione uno de la lista autocompletable.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    txtBuscador.SelectAll();
                }

                txtBuscador.Focus();
            }
        }

        private void AgregarAlCarrito(DataRow prod)
        {
            int id = Convert.ToInt32(prod["Id"]);
            string nombre = prod["Nombre"].ToString();
            decimal precio = Convert.ToDecimal(prod["PrecioVenta"]);
            bool porPeso = Convert.ToBoolean(prod["SeVendePorPeso"]);
            // Busca el stock fijándose primero si viene con el nombre nuevo, y si no, usa el viejo.
            decimal stockTotal = 0;
            if (prod.Table.Columns.Contains("StockTotal") && prod["StockTotal"] != DBNull.Value)
            {
                stockTotal = Convert.ToDecimal(prod["StockTotal"]);
            }
            else if (prod.Table.Columns.Contains("StockActual") && prod["StockActual"] != DBNull.Value)
            {
                stockTotal = Convert.ToDecimal(prod["StockActual"]);
            }

            decimal cantidadAIngresar = 1;

            // Si el producto se vende por peso, abrimos la ventanita para que el cajero ingrese los kilos
            if (porPeso)
            {
                string inputPeso = PromptPeso(nombre);
                if (string.IsNullOrWhiteSpace(inputPeso)) return; // Si el cajero cancela, no agregamos nada

                // Filtro a prueba de balas para comas y puntos
                inputPeso = inputPeso.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                                     .Replace(",", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);

                if (!decimal.TryParse(inputPeso, out cantidadAIngresar) || cantidadAIngresar <= 0)
                {
                    MessageBox.Show("Ingrese un peso numérico válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            bool yaExiste = false;
            decimal cantidadPrevia = 0;
            DataGridViewRow filaExistente = null;

            // 1. Verificamos si el producto ya está cargado en la grilla
            foreach (DataGridViewRow row in dgvCarrito.Rows)
            {
                if (Convert.ToInt32(row.Cells["IdProducto"].Value) == id)
                {
                    cantidadPrevia = Convert.ToDecimal(row.Cells["Cantidad"].Value);
                    filaExistente = row;
                    yaExiste = true;
                    break;
                }
            }

            // 2. EL CANDADO DE STOCK: Verificamos que lo previo + lo nuevo no supere el total real de la base de datos
            if ((cantidadPrevia + cantidadAIngresar) > stockTotal)
            {
                MessageBox.Show($"¡Stock insuficiente!\nIntentas cargar {cantidadPrevia + cantidadAIngresar} unidades/kg, pero solo quedan {stockTotal} disponibles en total.",
                                "Falta de Stock", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 3. Agregamos al carrito o sumamos la cantidad a la fila que ya existía
            if (yaExiste)
            {
                // Al modificar la celda, el evento CellValueChanged se dispara y actualiza el subtotal solo
                filaExistente.Cells["Cantidad"].Value = cantidadPrevia + cantidadAIngresar;
            }
            else
            {
                // Respetamos el orden de las columnas: IdProducto, Nombre, Precio, Cantidad, Subtotal, StockActual (oculto)
                dtCarrito.Rows.Add(id, nombre, precio, cantidadAIngresar, precio * cantidadAIngresar, stockTotal);
            }

            ActualizarTotal();
            dgvCarrito.ClearSelection();
        }

        private void ActualizarTotal()
        {
            totalVenta = 0;
            foreach (DataRow row in dtCarrito.Rows)
            {
                totalVenta += Convert.ToDecimal(row["Subtotal"]);
            }
            lblTotalMonto.Text = $"$ {totalVenta:N2}";
        }

        private void BtnEliminarItem_Click(object sender, EventArgs e)
        {
            if (dgvCarrito.CurrentRow != null)
            {
                dgvCarrito.Rows.RemoveAt(dgvCarrito.CurrentRow.Index);
                ActualizarTotal();
            }
            txtBuscador.Focus();
        }

        private void BtnCobrar_Click(object sender, EventArgs e)
        {
            if (dtCarrito.Rows.Count == 0)
            {
                MessageBox.Show("El carrito está vacío.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBuscador.Focus();
                return;
            }

            using (CobroForm cobro = new CobroForm(totalVenta))
            {
                if (cobro.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // SALVAVIDAS: Si probás la pantalla directo sin hacer Login, forzamos la Caja 1
                        if (SesionActiva.IdCaja == 0 || SesionActiva.IdUsuario == 0)
                        {
                            SesionActiva.IdCaja = 1;
                            SesionActiva.IdUsuario = 1;
                        }

                        int idCliente = Convert.ToInt32(cmbCliente.SelectedValue);

                        // Mandamos toda la información conectada a tu usuario real
                        ventaDatos.RegistrarVentaCompleta(
                            SesionActiva.IdCaja,
                            SesionActiva.IdUsuario,
                            idCliente,
                            totalVenta,
                            cobro.PagoEfectivo,
                            cobro.PagoTransferencia,
                            cobro.PagoTarjeta,
                            dtCarrito
                        );

                        MessageBox.Show("¡Venta registrada con éxito y stock descontado!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Limpieza del carrito 
                        dtCarrito.Rows.Clear();
                        ActualizarTotal();
                        cmbCliente.SelectedIndex = 0;
                        CargarBuscadorDinamico();
                        txtBuscador.Focus();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error al procesar la venta", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void VentasForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
                BtnCobrar_Click(null, null);
            }
            else if (e.KeyCode == Keys.Delete && dgvCarrito.Focused)
            {
                BtnEliminarItem_Click(null, null);
            }
        }

        private string PromptPeso(string nombreProducto)
        {
            Form prompt = new Form() { Width = 350, Height = 180, FormBorderStyle = FormBorderStyle.FixedDialog, Text = "Venta por Peso", StartPosition = FormStartPosition.CenterScreen };
            Label lbl = new Label() { Left = 20, Top = 20, Text = $"Ingrese los KILOS (ej: 1,5) para:\n{nombreProducto}", AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            TextBox txt = new TextBox() { Left = 20, Top = 70, Width = 300, Font = new Font("Segoe UI", 14) };
            Button btnOk = new Button() { Text = "Aceptar", Left = 220, Width = 100, Top = 110, DialogResult = DialogResult.OK, BackColor = Color.LightGreen };

            prompt.Controls.Add(lbl);
            prompt.Controls.Add(txt);
            prompt.Controls.Add(btnOk);
            prompt.AcceptButton = btnOk;

            prompt.Shown += (s, ev) => txt.Focus();

            return prompt.ShowDialog() == DialogResult.OK ? txt.Text : "0";
        }

        // --- DISEÑO VISUAL MEJORADO ---
        private void ConfigurarTablaCarrito()
        {
            // 1. Crear el carrito y enlazarlo
            dtCarrito = new DataTable();
            dtCarrito.Columns.Add("IdProducto", typeof(int));
            dtCarrito.Columns.Add("Nombre", typeof(string));
            dtCarrito.Columns.Add("Precio", typeof(decimal));
            dtCarrito.Columns.Add("Cantidad", typeof(decimal));
            dtCarrito.Columns.Add("Subtotal", typeof(decimal));
            dtCarrito.Columns.Add("StockActual", typeof(decimal));

            dgvCarrito.DataSource = dtCarrito;

            // 2. Ocultar columnas internas y ajustar tamaños
            dgvCarrito.Columns["IdProducto"].Visible = false;
            dgvCarrito.Columns["StockActual"].Visible = false;

            dgvCarrito.Columns["Nombre"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvCarrito.Columns["Precio"].Width = 140;
            dgvCarrito.Columns["Cantidad"].Width = 110;
            dgvCarrito.Columns["Subtotal"].Width = 160;

            // 3. CENTRAR ENCABEZADOS (Títulos)
            dgvCarrito.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCarrito.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvCarrito.ColumnHeadersHeight = 35;

            // =========================================================================
            // 4. ESTILOS CENTRADOS PARA TODOS LOS NÚMEROS
            // =========================================================================
            DataGridViewCellStyle estiloCentro = new DataGridViewCellStyle();
            estiloCentro.Alignment = DataGridViewContentAlignment.MiddleCenter; // ¡Todo al centro!
            estiloCentro.Format = "N2";

            DataGridViewCellStyle estiloFondoAmarilloCentro = new DataGridViewCellStyle();
            estiloFondoAmarilloCentro.Alignment = DataGridViewContentAlignment.MiddleCenter;
            estiloFondoAmarilloCentro.Format = "N2";
            estiloFondoAmarilloCentro.BackColor = Color.LightYellow;
            estiloFondoAmarilloCentro.SelectionBackColor = Color.LightYellow;
            estiloFondoAmarilloCentro.SelectionForeColor = Color.Black;

            // Asignamos los estilos a la grilla
            dgvCarrito.Columns["Nombre"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvCarrito.Columns["Precio"].DefaultCellStyle = estiloCentro;
            dgvCarrito.Columns["Subtotal"].DefaultCellStyle = estiloCentro;
            dgvCarrito.Columns["Cantidad"].DefaultCellStyle = estiloFondoAmarilloCentro;

            // 5. Bloquear columnas para no editar precios por accidente
            dgvCarrito.ReadOnly = false;
            foreach (DataGridViewColumn col in dgvCarrito.Columns)
            {
                if (col.Name != "Cantidad") col.ReadOnly = true;
            }

            // 6. Reiniciar suscripciones de eventos
            dgvCarrito.CellValueChanged -= DgvCarrito_CellValueChanged;
            dgvCarrito.CellValueChanged += DgvCarrito_CellValueChanged;
        }

        private void DgvCarrito_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            // Solo auditamos si están tocando la cantidad
            if (dgvCarrito.Columns[e.ColumnIndex].Name == "Cantidad")
            {
                string input = e.FormattedValue.ToString().Replace(".", ",").Trim();

                if (!decimal.TryParse(input, out decimal nuevaCantidad) || nuevaCantidad <= 0)
                {
                    MessageBox.Show("Ingrese una cantidad numérica válida mayor a cero.", "Dato Inválido", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true;
                    return;
                }

                decimal stockDisponible = Convert.ToDecimal(dgvCarrito.Rows[e.RowIndex].Cells["StockActual"].Value);
                if (nuevaCantidad > stockDisponible)
                {
                    MessageBox.Show($"Stock insuficiente. Solo quedan {stockDisponible:N2} disponibles.", "Límite de Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                }
            }
        }

        private void DgvCarrito_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Solo validamos si el cajero modificó la columna "Cantidad"
            if (e.RowIndex >= 0 && dgvCarrito.Columns[e.ColumnIndex].Name == "Cantidad")
            {
                DataGridViewRow row = dgvCarrito.Rows[e.RowIndex];

                // 1. Obtenemos lo que intentó cargar y el stock real oculto
                decimal cantidadIngresada = Convert.ToDecimal(row.Cells["Cantidad"].Value);
                decimal stockDisponible = Convert.ToDecimal(row.Cells["StockActual"].Value);
                decimal precio = Convert.ToDecimal(row.Cells["Precio"].Value);

                // 2. EL CANDADO: Si pone más de lo que hay, salta el mensaje
                if (cantidadIngresada > stockDisponible)
                {
                    MessageBox.Show($"No hay esa cantidad en stock.\nSolo quedan {stockDisponible:N2} disponibles.",
                                    "Stock Insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    // Desconectamos el evento un milisegundo para corregir el número sin que se cuelgue el programa
                    dgvCarrito.CellValueChanged -= DgvCarrito_CellValueChanged;
                    row.Cells["Cantidad"].Value = stockDisponible; // Forzamos la celda al stock real
                    dgvCarrito.CellValueChanged += DgvCarrito_CellValueChanged;

                    cantidadIngresada = stockDisponible; // Ajustamos la variable para recalcular bien
                }
                else if (cantidadIngresada <= 0)
                {
                    dgvCarrito.CellValueChanged -= DgvCarrito_CellValueChanged;
                    row.Cells["Cantidad"].Value = 1;
                    dgvCarrito.CellValueChanged += DgvCarrito_CellValueChanged;
                    cantidadIngresada = 1;
                }

                // 3. Recalculamos el subtotal del producto y el total gigante
                row.Cells["Subtotal"].Value = precio * cantidadIngresada;
                ActualizarTotal();
            }
        }

        private void CrearInterfazGrafica()
        {
            // Ojo al orden de agregado para que el Dock funcione bien
            this.Controls.Clear();

            // 1. Panel Lateral Derecho
            pnlCobroLateral = new Panel { Dock = DockStyle.Right, Width = 320, BackColor = Color.WhiteSmoke, BorderStyle = BorderStyle.FixedSingle };

            lblTotalGris = new Label { Text = "TOTAL A PAGAR", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(20, 30), AutoSize = true };
            lblTotalMonto = new Label { Text = "$ 0.00", Font = new Font("Segoe UI", 38, FontStyle.Bold), ForeColor = Color.DarkGreen, Location = new Point(15, 60), AutoSize = true };

            btnEliminarItem = new Button { Text = "🗑️ Quitar Producto", Font = new Font("Segoe UI", 11, FontStyle.Bold), BackColor = Color.LightCoral, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(20, 160), Size = new Size(280, 45), Cursor = Cursors.Hand };
            btnEliminarItem.FlatAppearance.BorderSize = 0;

            btnCobrar = new Button { Text = "COBRAR (F12)", Font = new Font("Segoe UI", 18, FontStyle.Bold), BackColor = ColorTranslator.FromHtml("#88E788"), FlatStyle = FlatStyle.Flat, Dock = DockStyle.Bottom, Height = 100, Cursor = Cursors.Hand };
            btnCobrar.FlatAppearance.BorderSize = 0;

            pnlCobroLateral.Controls.Add(lblTotalGris);
            pnlCobroLateral.Controls.Add(lblTotalMonto);
            pnlCobroLateral.Controls.Add(btnEliminarItem);
            pnlCobroLateral.Controls.Add(btnCobrar);

            // 2. Panel Cabecera 
            pnlCabecera = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.White };

            Label lblCliente = new Label { Text = "Cliente:", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(15, 18), AutoSize = true };

            // Cambiamos a DropDown para que el cajero pueda escribir y buscar
            cmbCliente = new ComboBox { Location = new Point(90, 15), Size = new Size(350, 30), Font = new Font("Segoe UI", 12), DropDownStyle = ComboBoxStyle.DropDown };

            // Botón para Alta Exprés
            Button btnNuevoCliente = new Button { Text = "+ Nuevo", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(450, 14), Size = new Size(90, 32), BackColor = Color.LightBlue, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnNuevoCliente.FlatAppearance.BorderSize = 0;
            btnNuevoCliente.Click += BtnNuevoCliente_Click;

            txtBuscador = new TextBox { Location = new Point(15, 55), Font = new Font("Segoe UI", 16), PlaceholderText = "🔍 Escriba el producto y presione ENTER..." };

            pnlCabecera.Resize += (s, e) => { txtBuscador.Width = pnlCabecera.Width - 30; };

            pnlCabecera.Controls.Add(lblCliente);
            pnlCabecera.Controls.Add(cmbCliente);
            pnlCabecera.Controls.Add(btnNuevoCliente); // Sumamos el botón a la pantalla
            pnlCabecera.Controls.Add(txtBuscador);

            // 3. Panel Central (Carrito)
            pnlCentro = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15), BackColor = Color.White };

            dgvCarrito = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false, Font = new Font("Segoe UI", 14) };
            dgvCarrito.RowTemplate.Height = 45;
            dgvCarrito.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgvCarrito.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            dgvCarrito.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            pnlCentro.Controls.Add(dgvCarrito);

            // Ensamblaje en orden correcto
            this.Controls.Add(pnlCentro);
            this.Controls.Add(pnlCabecera);
            this.Controls.Add(pnlCobroLateral);
        }
        private void BtnNuevoCliente_Click(object sender, EventArgs e)
        {
            Form modalCliente = new Form { Text = "Alta Exprés de Cliente", Size = new Size(350, 290), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false };

            Label lblNombre = new Label { Text = "Nombre y Apellido:", Location = new Point(20, 15), AutoSize = true };
            TextBox txtNombre = new TextBox { Location = new Point(20, 35), Width = 290, Font = new Font("Segoe UI", 11) };

            Label lblTel = new Label { Text = "Celular (Opcional):", Location = new Point(20, 75), AutoSize = true };
            TextBox txtTel = new TextBox { Location = new Point(20, 95), Width = 290, Font = new Font("Segoe UI", 11) };

            Label lblDir = new Label { Text = "Dirección (Opcional):", Location = new Point(20, 135), AutoSize = true };
            TextBox txtDir = new TextBox { Location = new Point(20, 155), Width = 290, Font = new Font("Segoe UI", 11) };

            Button btnGuardar = new Button { Text = "Guardar y Seleccionar", Location = new Point(20, 205), Width = 290, Height = 35, BackColor = Color.LightGreen, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            btnGuardar.Click += (s, ev) =>
            {
                string nombreIngresado = txtNombre.Text.Trim();
                string telIngresado = txtTel.Text.Trim();

                if (string.IsNullOrWhiteSpace(nombreIngresado))
                {
                    MessageBox.Show("El nombre del cliente es obligatorio.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // =======================================================
                // MAGIA NUEVA: Control de Duplicados
                // =======================================================
                DataTable dt = cmbCliente.DataSource as DataTable;
                if (dt != null)
                {
                    // Buscamos si ya existe alguien con ese nombre exacto (ignorando a Consumidor Final)
                    DataRow[] encontrados = dt.Select($"NombreCompleto = '{nombreIngresado.Replace("'", "''")}'");

                    if (encontrados.Length > 0 && encontrados[0]["Id"].ToString() != "0")
                    {
                        DialogResult res = MessageBox.Show($"Ya existe un cliente registrado como '{nombreIngresado}'.\n\n¿Es la misma persona?\n\n• SÍ: Usar el existente.\n• NO: Crear uno nuevo (requiere celular).",
                                                           "Cliente Existente", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                        if (res == DialogResult.Yes)
                        {
                            // Es el mismo: Cerramos y lo dejamos seleccionado en el combo
                            cmbCliente.SelectedValue = encontrados[0]["Id"];
                            modalCliente.Close();
                            return;
                        }
                        else if (res == DialogResult.No)
                        {
                            // No es el mismo: Exigimos teléfono para no tener dos nombres idénticos sin forma de diferenciarlos
                            if (string.IsNullOrWhiteSpace(telIngresado))
                            {
                                MessageBox.Show("Para registrar a otra persona con este mismo nombre, es obligatorio ingresar su número de celular.", "Falta Celular", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                txtTel.Focus();
                                return;
                            }
                        }
                        else
                        {
                            // Apretó Cancelar
                            return;
                        }
                    }
                }

                // =======================================================
                // GUARDAR EN BASE DE DATOS
                // =======================================================
                try
                {
                    clienteDatos.InsertarClienteRapido(nombreIngresado, telIngresado, txtDir.Text.Trim(), provinciaVendedorActual, localidadVendedorActual);
                    MessageBox.Show("Cliente registrado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    CargarClientes();
                    cmbCliente.Text = nombreIngresado;
                    modalCliente.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al guardar cliente: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            modalCliente.Controls.Add(lblNombre); modalCliente.Controls.Add(txtNombre);
            modalCliente.Controls.Add(lblTel); modalCliente.Controls.Add(txtTel);
            modalCliente.Controls.Add(lblDir); modalCliente.Controls.Add(txtDir);
            modalCliente.Controls.Add(btnGuardar);
            modalCliente.AcceptButton = btnGuardar;

            modalCliente.ShowDialog(this);
        }
    }
}