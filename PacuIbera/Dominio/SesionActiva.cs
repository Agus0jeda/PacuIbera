namespace PacuIbera.Dominio
{
    public static class SesionActiva
    {
        public static int IdUsuario { get; set; }
        public static string Nombre { get; set; }
        public static string Apellido { get; set; }
        public static string Rol { get; set; } // Guardará "Administrador", "Vendedor", etc.

        public static int IdCaja { get; set; }
    }
}