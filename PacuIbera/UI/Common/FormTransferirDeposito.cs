using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public class FormTransferirDeposito : Form
    {
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private Label lblProducto;
        private NumericUpDown nudCantidad;
        private DateTimePicker dtpVencimiento;
        private Button btnConfirmar;
        private Button btnCancelar;
        private Panel header;

        public int CantidadTransferir { get; private set; }
        public DateTime? FechaVencimiento { get; private set; }

        public FormTransferirDeposito(int productoId, int maxCantidad, string productoNombre = null)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(450, 300);
            this.DoubleBuffered = true;

            lblProducto.Text = productoNombre ?? $"Producto: {productoId}";
            nudCantidad.Maximum = maxCantidad;
        }

        private void InitializeComponent()
        {
            header = new Panel();
            lblProducto = new Label();
            nudCantidad = new NumericUpDown();
            dtpVencimiento = new DateTimePicker();
            btnConfirmar = new Button();
            btnCancelar = new Button();
            Button btnCerrar = new Button();

            // header
            header.Dock = DockStyle.Top;
            header.Height = 36;
            header.BackColor = Color.WhiteSmoke;
            header.MouseDown += Header_MouseDown;

            // btnCerrar
            btnCerrar.Text = "X";
            btnCerrar.Size = new Size(30, 30);
            btnCerrar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCerrar.Location = new Point(this.ClientSize.Width - 40, 4);
            btnCerrar.Click += (s, e) => this.Close();
            header.Controls.Add(btnCerrar);

            // lblProducto
            lblProducto.AutoSize = true;
            lblProducto.Location = new Point(12, 50);
            lblProducto.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            // nudCantidad
            nudCantidad.Location = new Point(12, 90);
            nudCantidad.Minimum = 1;
            nudCantidad.Maximum = 100000;
            nudCantidad.Width = 120;

            // dtpVencimiento
            dtpVencimiento.Location = new Point(12, 130);
            dtpVencimiento.Format = DateTimePickerFormat.Short;
            dtpVencimiento.Width = 120;

            // botones
            btnConfirmar.Text = "Confirmar Transferencia";
            btnConfirmar.Size = new Size(180, 36);
            btnConfirmar.Click += BtnConfirmar_Click;

            btnCancelar.Text = "Cancelar";
            btnCancelar.Size = new Size(100, 36);
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            // agrego controles
            this.Controls.Add(header);
            this.Controls.Add(lblProducto);
            this.Controls.Add(nudCantidad);
            this.Controls.Add(dtpVencimiento);

            // panel inferior para botones
            Panel pnlBottom = new Panel() { Dock = DockStyle.Bottom, Height = 60 };
            pnlBottom.Controls.Add(btnConfirmar);
            pnlBottom.Controls.Add(btnCancelar);
            this.Controls.Add(pnlBottom);

            // posicionamiento
            btnConfirmar.Location = new Point((this.ClientSize.Width / 2) - btnConfirmar.Width - 10, 12);
            btnCancelar.Location = new Point((this.ClientSize.Width / 2) + 10, 12);

            // ensure controls reposition on resize
            this.Resize += (s, e) =>
            {
                btnCerrar.Location = new Point(this.ClientSize.Width - 40, 4);
                btnConfirmar.Location = new Point((this.ClientSize.Width / 2) - btnConfirmar.Width - 10, 12);
                btnCancelar.Location = new Point((this.ClientSize.Width / 2) + 10, 12);
            };
        }

        private void Header_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void BtnConfirmar_Click(object sender, EventArgs e)
        {
            CantidadTransferir = (int)nudCantidad.Value;
            FechaVencimiento = dtpVencimiento.Value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
