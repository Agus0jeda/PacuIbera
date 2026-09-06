using System;
using Datos;
using PacuIbera.Dominio;

namespace Negocio
{
    public class UsuarioNegocio
    {
        private UsuarioDatos datos = new UsuarioDatos();

        public Usuario ValidarLogin(string dni, string claveIngresada)
        {
            if (string.IsNullOrWhiteSpace(dni) || string.IsNullOrWhiteSpace(claveIngresada))
            {
                throw new Exception("El DNI y la contraseña no pueden estar vacíos.");
            }

            Usuario usuario = datos.ObtenerPorDNI(dni);

            if (usuario == null)
            {
                throw new Exception("El usuario no existe o se encuentra inactivo.");
            }

            // Aquí puedes aplicar tu lógica de hashing si usas encriptación (ej. SHA256)
            // Si por ahora guardas la clave en texto plano en la BD:
            if (usuario.ClaveHash != claveIngresada)
            {
                throw new Exception("Contraseña incorrecta.");
            }

            return usuario; // Devuelve el usuario completo con su rol
        }
    }
}