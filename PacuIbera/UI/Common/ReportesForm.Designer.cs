using System.Drawing;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    partial class ReportesForm : Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dgvAlertasStock = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)dgvAlertasStock).BeginInit();
            SuspendLayout();
            // 
            // dgvAlertasStock
            // 
            dgvAlertasStock.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvAlertasStock.Location = new Point(364, 144);
            dgvAlertasStock.Name = "dgvAlertasStock";
            dgvAlertasStock.RowHeadersWidth = 51;
            dgvAlertasStock.Size = new Size(168, 102);
            dgvAlertasStock.TabIndex = 0;
            // 
            // ReportesForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(dgvAlertasStock);
            FormBorderStyle = FormBorderStyle.None;
            Name = "ReportesForm";
            Text = "ReporteForm";
            ((System.ComponentModel.ISupportInitialize)dgvAlertasStock).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dgvAlertasStock;
    }
}