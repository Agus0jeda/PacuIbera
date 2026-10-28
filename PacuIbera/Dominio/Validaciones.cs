using System;
using System.Net.Mail;
using System.Windows.Forms;

namespace PacuIbera.Dominio
{
    /// <summary>
    /// Clase compartida para validaciones comunes de formularios
    /// </summary>
    public static class Validaciones
    {
        /// <summary>
        /// Valida que un correo electrónico tenga formato válido
        /// </summary>
        public static bool EsEmailValido(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true; // Permitir vacío si no es obligatorio

            try
            {
                var correo = new MailAddress(email);
                return correo.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Valida que un texto contenga solo números
        /// </summary>
        public static bool EsNumeroValido(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return false;

            foreach (char c in texto)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Valida que un texto sea un decimal válido
        /// </summary>
        public static bool EsDecimalValido(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return false;

            return decimal.TryParse(texto, out _);
        }

        /// <summary>
        /// Valida que un decimal esté dentro de un rango
        /// </summary>
        public static bool EstaEnRango(decimal valor, decimal minimo, decimal maximo)
        {
            return valor >= minimo && valor <= maximo;
        }

        /// <summary>
        /// Valida que una fecha no sea pasada
        /// </summary>
        public static bool EsFechaFutura(DateTime fecha)
        {
            return fecha.Date >= DateTime.Today;
        }

        /// <summary>
        /// Valida que una fecha de nacimiento sea válida (mayor de edad)
        /// </summary>
        public static bool EsMayorDeEdad(DateTime fechaNacimiento)
        {
            int edad = DateTime.Today.Year - fechaNacimiento.Year;
            if (fechaNacimiento.Date > DateTime.Today.AddYears(-edad))
                edad--;
            return edad >= 18;
        }

        /// <summary>
        /// Configura un TextBox para aceptar solo números
        /// </summary>
        public static void ConfigurarSoloNumeros(TextBox textBox)
        {
            textBox.KeyPress += (sender, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                {
                    e.Handled = true;
                }
            };
        }

        /// <summary>
        /// Configura un TextBox para aceptar solo números decimales
        /// </summary>
        public static void ConfigurarSoloDecimales(TextBox textBox)
        {
            textBox.KeyPress += (sender, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != ',')
                {
                    e.Handled = true;
                }

                // Solo permitir un punto o coma decimal
                if ((e.KeyChar == '.' || e.KeyChar == ',') && 
                    (textBox.Text.Contains(".") || textBox.Text.Contains(",")))
                {
                    e.Handled = true;
                }
            };
        }

        /// <summary>
        /// Muestra un mensaje de validación
        /// </summary>
        public static void MostrarError(string mensaje, string titulo = "Validación")
        {
            MessageBox.Show(mensaje, titulo, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
