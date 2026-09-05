using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PacuIbera.UI.Common
{
    public partial class PacuIbera_IniciarSesion : Form
    {
        public PacuIbera_IniciarSesion()
        {
            InitializeComponent();


            this.BackColor = ColorTranslator.FromHtml("#88E788");

            // fecha actual con formato local (día/mes/año)
            lblFecha.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            if (txtUsuario.Text == "admin" && txtClave.Text == "123456")
            {
                this.Hide(); // Escondemos el login rápidamente

                // TODO: Más adelante Agustín conectará esto a la BD para saber si ya hay caja abierta
                bool cajaAbiertaHoy = false;

                if (cajaAbiertaHoy)
                {
                    // Si ya abrieron la caja hoy, vamos directo al sistema
                    MainForm ventanaPrincipal = new MainForm();
                    ventanaPrincipal.FormClosed += (s, args) => this.Close();
                    ventanaPrincipal.Show();
                }
                else
                {
                    // Es el primer ingreso, forzamos la apertura de caja
                    AperturaCajaForm formCaja = new AperturaCajaForm();

                    // Si el usuario cierra la apertura de caja con la X, matamos la app
                    formCaja.FormClosed += (s, args) => this.Close();
                    formCaja.Show();
                }
            }

        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void PacuIbera_IniciarSesion_Load(object sender, EventArgs e)
        {

        }
    }
}
