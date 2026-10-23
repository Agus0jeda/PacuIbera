using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public static class EstiloUI
    {
        // Crea o aplica un botón de cerrar en la esquina superior derecha del formulario
        public static void CrearBotonCerrar(Form form)
        {
            // Si ya existe un control llamado btnCerrar en el formulario, lo aplicamos
            var existente = form.Controls.Find("btnCerrar", true).FirstOrDefault();
            if (existente is Control c)
            {
                AplicarBotonCerrar(c);
                return;
            }

            // Crear nuevo botón
            Button btn = new Button();
            btn.Name = "btnCerrar";
            btn.Text = "X";
            btn.Size = new Size(30, 30);
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
            btn.BackColor = Color.Transparent;
            btn.ForeColor = SystemColors.ControlDarkDark;

            // Ubicar en la esquina superior derecha dentro del cliente del form
            int margin = 8;
            btn.Location = new Point(Math.Max(0, form.ClientSize.Width - btn.Width - margin), margin);
            btn.Click += (s, e) => form.Close();

            // Ajustar la posicion cuando se redimensiona
            form.Resize += (s, e) =>
            {
                btn.Location = new Point(Math.Max(0, form.ClientSize.Width - btn.Width - margin), margin);
            };

            form.Controls.Add(btn);
            btn.BringToFront();
        }

        public static void AplicarBotonCerrar(Control ctrl)
        {
            if (ctrl == null) return;
            ctrl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ctrl.Cursor = Cursors.Hand;
            ctrl.Font = new Font("Segoe UI", 11, FontStyle.Regular);
            ctrl.ForeColor = SystemColors.ControlDarkDark;
            // If it's a Button, apply button-specific styles
            if (ctrl is Button btn)
            {
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.Size = new Size(30, 30);
                btn.Text = "X";
                btn.Click -= null; // noop
                btn.Click += (s, e) => ((Control)s).FindForm()?.Close();
            }
            else
            {
                // For label or other controls, ensure click closes
                ctrl.Click += (s, e) => ctrl.FindForm()?.Close();
            }
        }

        public static void AplicarEstiloGrid(DataGridView dgv)
        {
            if (dgv == null) return;

            dgv.EnableHeadersVisualStyles = false;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = Color.LightGray;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;

            dgv.MultiSelect = false;
            dgv.RowHeadersVisible = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersHeight = 40;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(64, 64, 64);
            dgv.DefaultCellStyle.SelectionBackColor = Color.DarkSlateGray;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgv.RowTemplate.Height = 35;

            // Alternating rows
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);

            // Apply common column formatting for numeric alignment if columns exist
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                // align header text
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }
        }

        public enum TipoAccion { Ver, Positivo, Editar, Eliminar, Cancelar }

        public static void AplicarEstiloBotonAccion(Button btn, TipoAccion tipo)
        {
            if (btn == null) return;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.Cursor = Cursors.Hand;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btn.Size = new Size(110, 36);
            btn.Padding = new Padding(8, 0, 8, 0);
            btn.Margin = new Padding(6, 6, 6, 6);

            switch (tipo)
            {
                case TipoAccion.Ver:
                    btn.BackColor = ColorTranslator.FromHtml("#64748B"); // slate gray
                    break;
                case TipoAccion.Positivo:
                    btn.BackColor = ColorTranslator.FromHtml("#10B981"); // emerald
                    break;
                case TipoAccion.Editar:
                    btn.BackColor = ColorTranslator.FromHtml("#3B82F6"); // blue
                    break;
                case TipoAccion.Eliminar:
                    btn.BackColor = ColorTranslator.FromHtml("#EF4444"); // red
                    break;
                case TipoAccion.Cancelar:
                    btn.BackColor = ColorTranslator.FromHtml("#6B7280"); // neutral gray
                    break;
            }
        }

        // Helper to arrange buttons horizontally below a grid
        public static void AlinearBotonesInferiores(Control contenedor, int yPosition, params Button[] botones)
        {
            if (contenedor == null || botones == null || botones.Length == 0) return;
            int totalWidth = botones.Sum(b => b.Width + b.Margin.Left + b.Margin.Right);
            int spacing = 10;
            int xStart = Math.Max(10, (contenedor.ClientSize.Width - (botones.Length * botones[0].Width + (botones.Length - 1) * spacing)) / 2);
            int x = xStart;
            foreach (var btn in botones)
            {
                btn.Location = new Point(x, yPosition);
                btn.Anchor = AnchorStyles.Bottom;
                contenedor.Controls.Add(btn);
                x += btn.Width + spacing;
            }
        }
    }
}
