using System;

namespace PacuIbera.Dominio
{
    public class Proveedor
    {
        public int Id { get; set; }
        public string RazonSocial { get; set; }
        public string CUIT { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }
        public int ProvinciaId { get; set; }
        public int LocalidadId { get; set; }
        public bool Activo { get; set; }
    }
}