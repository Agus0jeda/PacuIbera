using System.Data;
using Datos;
using PacuIbera.Datos;
using PacuIbera.Dominio;

namespace Negocio
{
    public class ClienteNegocio
    {


        private OtrosDatos datos = new OtrosDatos();
        private ClienteDatos clientes = new ClienteDatos();

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

        // Método para registrar el cliente validando campos obligatorios
        public void Registrar(Cliente cliente)
        {
            if (string.IsNullOrWhiteSpace(cliente.Nombre) || string.IsNullOrWhiteSpace(cliente.Apellido))
            {
                throw new System.Exception("El nombre y el apellido son obligatorios.");
            }

            clientes.RegistrarCliente(cliente);
        }
        public DataTable ListarClientes()
        {
            return clientes.ObtenerClientes();
        }
    }
}