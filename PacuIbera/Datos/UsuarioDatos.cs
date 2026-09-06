using Microsoft.Data.SqlClient;
using System;
using System.Data;
using PacuIbera.Dominio;

namespace Datos
{
    public class UsuarioDatos : ConexionBD
    {
        public Usuario ObtenerPorDNI(string dni)
         {
            Usuario usuario = null;
            using (SqlConnection conexion = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_LoginUsuario", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@DNI", dni);

                conexion.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        usuario = new Usuario
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Nombre = reader["Nombre"].ToString(),
                            Apellido = reader["Apellido"].ToString(),
                            DNI = reader["DNI"].ToString(),
                            ClaveHash = reader["ClaveHash"].ToString(),
                            Rol = reader["Rol"].ToString()
                        };
                    }
                }
            }
            return usuario;
        }
    }
}