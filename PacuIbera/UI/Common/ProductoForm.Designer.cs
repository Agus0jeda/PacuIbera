#nullable enable
namespace PacuIbera.UI.Common
{
    partial class ProductoForm
    {
        private System.ComponentModel.IContainer? components;
        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            SuspendLayout();
            Name = "ProductoForm";
            Text = "Productos";
            Font = new Font("Segoe UI", 9F);
            BackColor = Color.White;
            FormBorderStyle = FormBorderStyle.None;
            ClientSize = new Size(1100, 650);
            ResumeLayout(false);
        }
    }
}
