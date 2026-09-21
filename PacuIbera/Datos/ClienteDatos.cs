using Microsoft.Data.SqlClient;
using System;
using System.Data;
using PacuIbera.Dominio;
using PacuIbera.Datos;

namespace Datos
{
    public class ClienteDatos : ConexionBD
    {
        

        // 3. Registrar un nuevo Cliente
        public void RegistrarCliente(Cliente cliente)
        {
            using (SqlConnection conexion = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_InsertarCliente", conexion);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Nombre", cliente.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", cliente.Apellido);

                // Usamos DBNull.Value por si el campo queda vacío (ya que no son obligatorios en tu DB)
                cmd.Parameters.AddWithValue("@DNI_CUIT", string.IsNullOrEmpty(cliente.DNI_CUIT) ? (object)DBNull.Value : cliente.DNI_CUIT);
                cmd.Parameters.AddWithValue("@Telefono", string.IsNullOrEmpty(cliente.Telefono) ? (object)DBNull.Value : cliente.Telefono);
                cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(cliente.Email) ? (object)DBNull.Value : cliente.Email);
                cmd.Parameters.AddWithValue("@Direccion", string.IsNullOrEmpty(cliente.Direccion) ? (object)DBNull.Value : cliente.Direccion);

                cmd.Parameters.AddWithValue("@ProvinciaId", cliente.ProvinciaId);
                cmd.Parameters.AddWithValue("@LocalidadId", cliente.LocalidadId);

                conexion.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable ObtenerClientes()
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("sp_ObtenerClientes", conexion);
                cmd.CommandType = CommandType.StoredProcedure;

                conexion.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    tabla.Load(reader);
                }
            }
            return tabla;
        }
    }
}