using Datos;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace Datos
{
    public class UsuarioDatos : ConexionBD
    {
        public void RegistrarUsuario(string nombre, string apellido, string dni, string claveHasheada, int rolId, int provinciaId, int localidadId)
        {
            using (SqlConnection conexion = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_InsertarUsuario", conexion);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Nombre", nombre);
                cmd.Parameters.AddWithValue("@Apellido", apellido);
                cmd.Parameters.AddWithValue("@DNI", dni);
                cmd.Parameters.AddWithValue("@ClaveHash", claveHasheada);

                // Estos IDs vendrán de los ComboBox de tu Formulario
                cmd.Parameters.AddWithValue("@RolId", rolId);
                cmd.Parameters.AddWithValue("@ProvinciaId", provinciaId);
                cmd.Parameters.AddWithValue("@LocalidadId", localidadId);

                conexion.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}