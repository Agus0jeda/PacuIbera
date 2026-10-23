using System;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public class FormIngresoCantidad : Form
    {
        private NumericUpDown nudCantidad;
        private DateTimePicker dtpVencimiento;
        private Button btnOk;
        private Button btnCancelar;
        public int CantidadIngresada { get; private set; }
        public DateTime? FechaVencimiento { get; private set; }

        public FormIngresoCantidad(int maxCantidad)
        {
            InitializeComponent(maxCantidad);
        }

        private void InitializeComponent(int maxCantidad)
        {
            nudCantidad = new NumericUpDown() { Minimum = 0, Maximum = maxCantidad, Value = maxCantidad, Dock = DockStyle.Top };
            dtpVencimiento = new DateTimePicker() { Format = DateTimePickerFormat.Short, Dock = DockStyle.Top }; 
            btnOk = new Button() { Text = "OK", Dock = DockStyle.Left }; btnOk.Click += BtnOk_Click;
            btnCancelar = new Button() { Text = "Cancelar", Dock = DockStyle.Right }; btnCancelar.Click += BtnCancelar_Click;
            this.Controls.Add(nudCantidad);
            this.Controls.Add(dtpVencimiento);
            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancelar);
            this.Text = "Ingresar cantidad recibida";
            this.Height = 150;
            this.Width = 300;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            CantidadIngresada = (int)nudCantidad.Value;
            FechaVencimiento = dtpVencimiento.Value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
