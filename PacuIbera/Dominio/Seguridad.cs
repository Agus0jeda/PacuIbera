using System.Security.Cryptography;
using System.Text;
using System;

namespace Dominio
{
    public static class Seguridad
    {
        public static string GenerarHashSHA256(string textoPlano)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(textoPlano));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                // Convertimos a mayúsculas para que coincida con el formato de SQL Server
                return builder.ToString().ToUpper();
            }
        }
    }
}