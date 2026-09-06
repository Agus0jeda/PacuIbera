using System;
using System.Collections.Generic;
using System.Text;

namespace PacuIbera.Dominio
{
    public class Usuario
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public required string Apellido { get; set; }
        public required string DNI { get; set; }
        public required string ClaveHash { get; set; }

        public int RolId { get; set; }
        public string Direccion { get; set; }
        public int? ProvinciaId { get; set; }
        public int? LocalidadId { get; set; }
        public bool Activo { get; set; }

        public string NombreCompleto
        {
            get { return $"{Nombre} {Apellido}"; }
        }
    }       
}
