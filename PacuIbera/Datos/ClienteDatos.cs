using Microsoft.Data.SqlClient;
using System;
using System.Data;
using PacuIbera.Dominio;
using PacuIbera.Datos;

namespace Datos
{
    public class ClienteDatos : ConexionBD
    {
        public void InsertarClienteRapido(string nombre, string telefono, string direccion, int provinciaId, int localidadId)
        {
            using (var conexion = ObtenerConexion())
            {
                conexion.Open();
                using (var cmd = new SqlCommand("sp_InsertarClienteRapido", conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Nombre", nombre);
                    cmd.Parameters.AddWithValue("@Telefono", string.IsNullOrWhiteSpace(telefono) ? (object)DBNull.Value : telefono);
                    cmd.Parameters.AddWithValue("@Direccion", string.IsNullOrWhiteSpace(direccion) ? (object)DBNull.Value : direccion);

                    // Pasamos los IDs del vendedor
                    cmd.Parameters.AddWithValue("@ProvinciaId", provinciaId);
                    cmd.Parameters.AddWithValue("@LocalidadId", localidadId);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public DataTable ObtenerClientesParaVenta()
        {
            DataTable tabla = new DataTable();
            using (var conexion = ObtenerConexion())
            {
                // Consulta SQL directa y cruda: traemos a TODOS sin usar INNER JOINS que nos oculten datos
                string query = "SELECT Id, Nombre, ISNULL(Apellido, '') AS Apellido FROM Cliente";

                // ¡Acá está la corrección! SqlCommand limpio sin la ruta larga.
                using (var cmd = new SqlCommand(query, conexion))
                {
                    conexion.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        tabla.Load(reader);
                    }
                }
            }
            return tabla;
        }

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

        // 1. Trae los clientes limpios para el CRM (Panel general)
        public DataTable ObtenerClientesCRM()
        {
            DataTable dt = new DataTable();
            using (Microsoft.Data.SqlClient.SqlConnection con = ObtenerConexion())
            {
                Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand("sp_ObtenerClientesCRM", con);
                cmd.CommandType = CommandType.StoredProcedure;
                Microsoft.Data.SqlClient.SqlDataAdapter da = new Microsoft.Data.SqlClient.SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        // 2. Trae el ranking de los mejores clientes (Panel de Top)
        public DataTable ObtenerTopClientes()
        {
            DataTable dt = new DataTable();
            using (Microsoft.Data.SqlClient.SqlConnection con = ObtenerConexion())
            {
                Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand("sp_ReporteTopClientes", con);
                cmd.CommandType = CommandType.StoredProcedure;
                Microsoft.Data.SqlClient.SqlDataAdapter da = new Microsoft.Data.SqlClient.SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        // 4. Modificar un cliente existente (CRM)
        public void ModificarClienteCRM(int id, string nombre, string apellido, string telefono, string direccion)
        {
            using (Microsoft.Data.SqlClient.SqlConnection conexion = ObtenerConexion())
            {
                Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand("sp_ActualizarClienteCRM", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Nombre", nombre.Trim());
                cmd.Parameters.AddWithValue("@Apellido", apellido.Trim());
                cmd.Parameters.AddWithValue("@Telefono", telefono.Trim());
                cmd.Parameters.AddWithValue("@Direccion", direccion.Trim());

                conexion.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}