using Datos;
using Microsoft.Data.SqlClient;
using PacuIbera.Dominio;
using System;
using System.Data;

namespace PacuIbera.Datos
{
    public class ProveedorDatos : ConexionBD
    {
        public DataTable ObtenerTodos()
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = ObtenerConexion())
            {
                // Reemplazamos p.Activo por el CASE y quitamos el filtro WHERE
                string query = @"SELECT p.Id, p.RazonSocial, p.CUIT, p.Telefono, p.Email, p.Direccion, 
                                pr.Nombre AS Provincia, l.Nombre AS Localidad, 
                                CASE WHEN p.Activo = 1 THEN 'Activo' ELSE 'Inactivo' END AS Estado 
                         FROM Proveedor p
                         LEFT JOIN Provincia pr ON p.ProvinciaId = pr.Id
                         LEFT JOIN Localidad l ON p.LocalidadId = l.Id
                         ORDER BY p.RazonSocial ASC";

                SqlCommand cmd = new SqlCommand(query, conexion);
                conexion.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    tabla.Load(reader);
                }
            }
            return tabla;
        }

        public void Insertar(Proveedor prov)
        {
            using (SqlConnection conexion = ObtenerConexion())
            {
                string query = @"INSERT INTO Proveedor (RazonSocial, CUIT, Telefono, Email, Direccion, ProvinciaId, LocalidadId, Activo) 
                                 VALUES (@RazonSocial, @CUIT, @Telefono, @Email, @Direccion, @ProvinciaId, @LocalidadId, 1)";

                SqlCommand cmd = new SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@RazonSocial", prov.RazonSocial);
                cmd.Parameters.AddWithValue("@CUIT", prov.CUIT);
                cmd.Parameters.AddWithValue("@Telefono", string.IsNullOrEmpty(prov.Telefono) ? (object)DBNull.Value : prov.Telefono);
                cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(prov.Email) ? (object)DBNull.Value : prov.Email);
                cmd.Parameters.AddWithValue("@Direccion", string.IsNullOrEmpty(prov.Direccion) ? (object)DBNull.Value : prov.Direccion);
                cmd.Parameters.AddWithValue("@ProvinciaId", prov.ProvinciaId == 0 ? (object)DBNull.Value : prov.ProvinciaId);
                cmd.Parameters.AddWithValue("@LocalidadId", prov.LocalidadId == 0 ? (object)DBNull.Value : prov.LocalidadId);

                conexion.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Actualizar(Proveedor prov)
        {
            using (SqlConnection conexion = ObtenerConexion())
            {
                string query = @"UPDATE Proveedor 
                                 SET RazonSocial = @RazonSocial, CUIT = @CUIT, Telefono = @Telefono, 
                                     Email = @Email, Direccion = @Direccion, ProvinciaId = @ProvinciaId, 
                                     LocalidadId = @LocalidadId, Activo = @Activo 
                                 WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@Id", prov.Id);
                cmd.Parameters.AddWithValue("@RazonSocial", prov.RazonSocial);
                cmd.Parameters.AddWithValue("@CUIT", prov.CUIT);
                cmd.Parameters.AddWithValue("@Telefono", string.IsNullOrEmpty(prov.Telefono) ? (object)DBNull.Value : prov.Telefono);
                cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(prov.Email) ? (object)DBNull.Value : prov.Email);
                cmd.Parameters.AddWithValue("@Direccion", string.IsNullOrEmpty(prov.Direccion) ? (object)DBNull.Value : prov.Direccion);
                cmd.Parameters.AddWithValue("@ProvinciaId", prov.ProvinciaId == 0 ? (object)DBNull.Value : prov.ProvinciaId);
                cmd.Parameters.AddWithValue("@LocalidadId", prov.LocalidadId == 0 ? (object)DBNull.Value : prov.LocalidadId);
                cmd.Parameters.AddWithValue("@Activo", prov.Activo);

                conexion.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Eliminar(int id)
        {
            using (SqlConnection conexion = ObtenerConexion())
            {
                // Baja lógica cambiando el Activo a 0
                string query = "UPDATE Proveedor SET Activo = 0 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@Id", id);
                conexion.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}