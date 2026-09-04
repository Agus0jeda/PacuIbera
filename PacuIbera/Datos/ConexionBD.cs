using System;
using Microsoft.Data.SqlClient; // La librería que instalaste en el Paso 1

namespace Datos 
{
    public abstract class ConexionBD
    {
         private readonly string cadenaConexion;

        public ConexionBD()
        {
            // Integrated Security=True usa la autenticación de Windows de tu PC.
            // TrustServerCertificate=True evita errores de certificados locales en .NET moderno.
            cadenaConexion = "Server=AGUS\\SQLEXPRESS; DataBase=PacuIberaDB; Integrated Security=True; TrustServerCertificate=True;";
        }

        // Este método lo van a usar tus repositorios para conectarse
        protected SqlConnection ObtenerConexion()
        {
            return new SqlConnection(cadenaConexion);
        }
    }
}