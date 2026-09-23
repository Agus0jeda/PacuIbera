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
    


//  LISTAR TODOS 
public DataTable ObtenerTodos()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conexion = ObtenerConexion())
            {
                string query = @"SELECT u.Id, u.Nombre, u.Apellido, u.DNI, u.Telefono, u.Email, u.Direccion, 
                        r.Nombre AS Rol, 
                        p.Nombre AS Provincia, 
                        l.Nombre AS Localidad,
                        CASE WHEN u.Activo = 1 THEN 'Activo' ELSE 'Inactivo' END AS Estado,
                        u.ClaveHash AS Clave
                 FROM Usuario u 
                 INNER JOIN Rol r ON u.RolId = r.Id
                 INNER JOIN Provincia p ON u.ProvinciaId = p.Id
                 INNER JOIN Localidad l ON u.LocalidadId = l.Id";

                SqlCommand cmd = new SqlCommand(query, conexion);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        public void Insertar(Usuario usuario)
        {
            using (Microsoft.Data.SqlClient.SqlConnection conexion = ObtenerConexion())
            {
                string query = @"INSERT INTO Usuario (Nombre, Apellido, DNI, Telefono, Email, Direccion, RolId, ClaveHash, Activo, ProvinciaId, LocalidadId) 
                         VALUES (@Nombre, @Apellido, @DNI, @Telefono, @Email, @Direccion, @RolId, @ClaveHash, @Activo, @ProvinciaId, @LocalidadId)";

                Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre.Trim());
                cmd.Parameters.AddWithValue("@Apellido", usuario.Apellido.Trim());
                cmd.Parameters.AddWithValue("@DNI", usuario.DNI.Trim());
                cmd.Parameters.AddWithValue("@Telefono", string.IsNullOrWhiteSpace(usuario.Telefono) ? (object)DBNull.Value : usuario.Telefono.Trim());
                cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(usuario.Email) ? (object)DBNull.Value : usuario.Email.Trim());
                cmd.Parameters.AddWithValue("@Direccion", string.IsNullOrWhiteSpace(usuario.Direccion) ? (object)DBNull.Value : usuario.Direccion.Trim());

                // AHORA RECIBE EL ID DIRECTAMENTE (Ya no adivina el texto)
                cmd.Parameters.AddWithValue("@RolId", Convert.ToInt32(usuario.Rol));

                cmd.Parameters.AddWithValue("@ClaveHash", usuario.ClaveHash);
                cmd.Parameters.AddWithValue("@Activo", usuario.Activo ? 1 : 0);
                cmd.Parameters.AddWithValue("@ProvinciaId", usuario.ProvinciaId);
                cmd.Parameters.AddWithValue("@LocalidadId", usuario.LocalidadId);

                conexion.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Actualizar(Usuario usuario)
        {
            using (Microsoft.Data.SqlClient.SqlConnection conexion = ObtenerConexion())
            {
                // Agregamos ClaveHash para que se pueda actualizar
                string query = @"UPDATE Usuario SET Nombre=@Nombre, Apellido=@Apellido, DNI=@DNI, 
                         Telefono=@Telefono, Email=@Email, Direccion=@Direccion, RolId=@RolId, ClaveHash=@ClaveHash, 
                         Activo=@Activo, ProvinciaId=@ProvinciaId, LocalidadId=@LocalidadId 
                         WHERE Id=@Id";

                Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@Id", usuario.Id);
                cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre.Trim());
                cmd.Parameters.AddWithValue("@Apellido", usuario.Apellido.Trim());
                cmd.Parameters.AddWithValue("@DNI", usuario.DNI.Trim());
                cmd.Parameters.AddWithValue("@Telefono", string.IsNullOrWhiteSpace(usuario.Telefono) ? (object)DBNull.Value : usuario.Telefono.Trim());
                cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(usuario.Email) ? (object)DBNull.Value : usuario.Email.Trim());
                cmd.Parameters.AddWithValue("@Direccion", string.IsNullOrWhiteSpace(usuario.Direccion) ? (object)DBNull.Value : usuario.Direccion.Trim());

                // AHORA RECIBE EL ID DIRECTAMENTE
                cmd.Parameters.AddWithValue("@RolId", Convert.ToInt32(usuario.Rol));

                cmd.Parameters.AddWithValue("@ClaveHash", usuario.ClaveHash);
                cmd.Parameters.AddWithValue("@Activo", usuario.Activo ? 1 : 0);
                cmd.Parameters.AddWithValue("@ProvinciaId", usuario.ProvinciaId);
                cmd.Parameters.AddWithValue("@LocalidadId", usuario.LocalidadId);

                conexion.Open();
                cmd.ExecuteNonQuery();
            }
        }


        public void Eliminar(int idUsuario)
        {
            using (SqlConnection conexion = ObtenerConexion())
            {
                // En lugar de borrarlo, actualizamos su campo Activo a 0 (Falso/Inactivo)
                string query = "UPDATE Usuario SET Activo = 0 WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@Id", idUsuario);

                conexion.Open();
                cmd.ExecuteNonQuery();
            }
        }



    }
}