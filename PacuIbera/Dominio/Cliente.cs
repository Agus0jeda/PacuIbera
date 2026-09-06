using System;

namespace PacuIbera.Dominio
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string DNI_CUIT { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }
        public int ProvinciaId { get; set; }
        public int LocalidadId { get; set; }
    }
}