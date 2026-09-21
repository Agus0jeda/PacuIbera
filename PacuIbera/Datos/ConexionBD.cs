using System;
using Microsoft.Data.SqlClient;

namespace Datos
{
    public abstract class ConexionBD
    {
        // Propiedad estática accesible desde cualquier lado del proyecto sin instanciar
        public static readonly string CadenaGlobal;

        static ConexionBD()
        {

            // Detecta automáticamente en qué computadora está corriendo el proyecto
            if (Environment.MachineName == "AGUS") 
            {
                CadenaGlobal = "Server=AGUS\\SQLEXPRESS; DataBase=PacuIberaDB; Integrated Security=True; TrustServerCertificate=True;";
            }
            else
            {
                // Si la PC no se llama AGUS, asume automáticamente que es la máquina de tu compañero
                CadenaGlobal = "Server=(localdb)\\MSSQLLocalDB; DataBase=PacuIberaDB; Integrated Security=True; TrustServerCertificate=True;";
            }

            
        }

        // Este método lo siguen usando tus repositorios internos (CajaDatos, ClienteDatos, etc.)
        protected SqlConnection ObtenerConexion()
        {
            return new SqlConnection(CadenaGlobal);
        }
    }
}