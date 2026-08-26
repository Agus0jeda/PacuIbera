using System;
using System.Collections.Generic;
using System.Text;

namespace PacuIbera.Dominio
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string DNI { get; set; }
        public string ClaveHash { get; set; }

        public int RolId { get; set; }
        public int? ProvinciaId { get; set; }
        public int? LocalidadId { get; set; }
        public bool Activo { get; set; }

        public string NombreCompleto
        {
            get { return $"{Nombre} {Apellido}"; }
        }
    }       
}
