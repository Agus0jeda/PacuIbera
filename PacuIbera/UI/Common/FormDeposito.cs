using PacuIbera.Datos;
using PacuIbera.Dominio;
using System;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public class FormDeposito : Form
    {
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private DataGridView dgvStock;
        private Button btnConfigurarMinimo;
        private Button btnTransferir;
        private Panel panel1;
        private Panel panelHeader;
        private Panel panel2;
        private PedidosDatos pedidosDatos = new PedidosDatos();

        public FormDeposito()
        {
            InitializeComponent();
            Load += FormDeposito_Load;
            this.DoubleBuffered = true;
        }

        private void InitializeComponent()
        {
            dgvStock = new DataGridView();
            panelHeader = new Panel();
            btnConfigurarMinimo = new Button();
            btnTransferir = new Button();
            panel1 = new Panel();
            panel2 = new Panel();
            ((System.ComponentModel.ISupportInitialize)dgvStock).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // dgvStock
            // 
            dgvStock.ColumnHeadersHeight = 29;
            dgvStock.Dock = DockStyle.Fill;
            dgvStock.Location = new Point(0, 0);
            dgvStock.Name = "dgvStock";
            dgvStock.RowHeadersWidth = 51;
            dgvStock.Size = new Size(882, 453);
            dgvStock.TabIndex = 0;
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.WhiteSmoke;
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(882, 36);
            panelHeader.TabIndex = 4;
            panelHeader.MouseDown += PanelHeader_MouseDown;
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
            btnTransferir.Click += BtnTransferir_Click;
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
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(882, 453);
            panel2.TabIndex = 5;
            // 
            // FormDeposito
            // 
            ClientSize = new Size(882, 453);
            Controls.Add(panel1);
            Controls.Add(panelHeader);
            Controls.Add(dgvStock);
            Controls.Add(panel2);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FormDeposito";
            Text = "Depósito Central";
            ((System.ComponentModel.ISupportInitialize)dgvStock).EndInit();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
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

            // Recolocar botones en panel inferior (centrados)
            try
            {
                panel1.Controls.Clear();
                EstiloUI.AlinearBotonesInferiores(panel1, 6, btnTransferir, btnConfigurarMinimo);
            }
            catch { }

            CargarStock();
            ConfigurarPermisos();
        }

        private void ConfigurarPermisos()
        {
            btnConfigurarMinimo.Enabled = SesionActiva.Rol == "Administrador" || SesionActiva.Rol == "SuperAdministrador";
        }

        private void CargarStock()
        {
            try
            {
                var datos = new OtrosDatos();
                var tabla = datos.ObtenerStockDeposito();
                dgvStock.DataSource = tabla;

                // formato condicional
                foreach (DataGridViewRow row in dgvStock.Rows)
                {
                    if (row.Cells["EstadoStock"].Value != null && row.Cells["EstadoStock"].Value.ToString() == "ALERTA")
                    {
                        row.DefaultCellStyle.BackColor = Color.MistyRose;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando stock: " + ex.Message);
            }
        }

        private void BtnConfigurarMinimo_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvStock.CurrentRow == null)
                {
                    MessageBox.Show("Seleccione un producto", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var currentRow = dgvStock.CurrentRow;
                if (currentRow.Cells[0].Value == null)
                {
                    MessageBox.Show("Producto inválido seleccionado", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int productoId = 0;
                if (!int.TryParse(currentRow.Cells[0].Value.ToString(), out productoId))
                {
                    MessageBox.Show("El Id del producto no es válido", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Pedimos el nuevo valor via InputBox
                string input = Microsoft.VisualBasic.Interaction.InputBox("Ingrese nuevo Stock Mínimo:", "Stock Mínimo", currentRow.Cells["StockMinimoDeposito"].Value?.ToString() ?? "0");
                if (string.IsNullOrWhiteSpace(input)) return;

                if (!decimal.TryParse(input, out decimal nuevoMin))
                {
                    MessageBox.Show("El valor ingresado no es un número válido", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Llamamos al SP por medio de OtrosDatos
                var otros = new OtrosDatos();
                otros.ActualizarStockMinimoDeposito(productoId, nuevoMin);
                MessageBox.Show("Stock mínimo actualizado", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarStock();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error actualizando stock mínimo: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnTransferir_Click(object sender, EventArgs e)
        {
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
