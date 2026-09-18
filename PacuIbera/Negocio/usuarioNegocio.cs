using Datos;
using PacuIbera.Datos;
using PacuIbera.Dominio;
using System;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Negocio
{
    public class UsuarioNegocio
    {
        private UsuarioDatos usuarios = new UsuarioDatos();
        private OtrosDatos datos = new OtrosDatos();

        public Usuario ValidarLogin(string dni, string claveIngresada)
        {
            if (string.IsNullOrWhiteSpace(dni) || string.IsNullOrWhiteSpace(claveIngresada))
            {
                throw new Exception("El DNI y la contraseña no pueden estar vacíos.");
            }

            Usuario usuario = usuarios.ObtenerPorDNI(dni);

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
        // Método para traer las provincias al ComboBox
        public DataTable ListarProvincias()
        {
            return datos.ObtenerProvincias();
        }

        // Método para traer las localidades según la provincia elegida
        public DataTable ListarLocalidades(int provinciaId)
        {
            return datos.ObtenerLocalidadesPorProvincia(provinciaId);
        }
        public DataTable ListarRoles()
        {
            return datos.ObtenerRoles();
        }
    }
}