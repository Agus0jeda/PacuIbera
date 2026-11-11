using System;
using System.IO;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace Datos
{
    public abstract class ConexionBD
    {
        public const string BasePruebas = "PacuIberaDB_Pruebas";
        public const string BaseEntrega = "PacuIbera_Entrega";

        public static readonly string CadenaGlobal;
        public static readonly string PerfilConfigurado;
        public static readonly string BaseConfigurada;

        // La aplicación solo habilita escrituras de negocio en estos dos destinos.
        // Los scripts administrativos SQL conservan sus guardas literales propias.
        public static bool EsPerfilPruebas =>
            PerfilConfigurado.Equals("Pruebas", StringComparison.OrdinalIgnoreCase)
            && BaseConfigurada.Equals(BasePruebas, StringComparison.OrdinalIgnoreCase);

        public static bool EsPerfilEntrega =>
            PerfilConfigurado.Equals("Entrega", StringComparison.OrdinalIgnoreCase)
            && BaseConfigurada.Equals(BaseEntrega, StringComparison.OrdinalIgnoreCase);

        public static bool DestinoAplicacionPermitido => EsPerfilPruebas || EsPerfilEntrega;

        static ConexionBD()
        {
            string? perfil = null;
            string? servidor = null;
            string? baseDatos = null;
            bool seguridadIntegrada = true;
            bool confiarCertificado = true;

            string archivoConfiguracion = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (File.Exists(archivoConfiguracion))
            {
                using JsonDocument documento = JsonDocument.Parse(File.ReadAllText(archivoConfiguracion));
                if (!documento.RootElement.TryGetProperty("Database", out JsonElement database)
                    || database.ValueKind != JsonValueKind.Object)
                    throw new InvalidOperationException("appsettings.json debe contener la sección Database.");

                perfil = LeerTexto(database, "Profile");
                servidor = LeerTexto(database, "Server");
                baseDatos = LeerTexto(database, "Name");
                if (database.TryGetProperty("IntegratedSecurity", out JsonElement integrated)
                    && integrated.ValueKind is JsonValueKind.True or JsonValueKind.False)
                    seguridadIntegrada = integrated.GetBoolean();
                if (database.TryGetProperty("TrustServerCertificate", out JsonElement trust)
                    && trust.ValueKind is JsonValueKind.True or JsonValueKind.False)
                    confiarCertificado = trust.GetBoolean();
            }

            // Compatibilidad para ejecuciones de desarrollo que aún no copian appsettings.json.
            if (string.IsNullOrWhiteSpace(perfil)
                && Environment.GetEnvironmentVariable("PACUIBERA_DB_PRUEBAS") == "1")
            {
                perfil = "Pruebas";
                baseDatos = BasePruebas;
            }

            if (string.IsNullOrWhiteSpace(perfil))
                throw new InvalidOperationException("Configure appsettings.json junto a PacuIbera.exe.");

            perfil = perfil.Trim();
            if (perfil.Equals("Pruebas", StringComparison.OrdinalIgnoreCase))
            {
                perfil = "Pruebas";
                baseDatos = string.IsNullOrWhiteSpace(baseDatos) ? BasePruebas : baseDatos.Trim();
                if (!baseDatos.Equals(BasePruebas, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("El perfil Pruebas solo puede usar PacuIberaDB_Pruebas.");
            }
            else if (perfil.Equals("Entrega", StringComparison.OrdinalIgnoreCase))
            {
                perfil = "Entrega";
                baseDatos = string.IsNullOrWhiteSpace(baseDatos) ? BaseEntrega : baseDatos.Trim();
                if (!baseDatos.Equals(BaseEntrega, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("El perfil Entrega solo puede usar PacuIbera_Entrega.");
            }
            else
            {
                throw new InvalidOperationException("Database.Profile debe ser Pruebas o Entrega.");
            }

            if (string.IsNullOrWhiteSpace(servidor))
                servidor = Environment.MachineName.Equals("AGUS", StringComparison.OrdinalIgnoreCase)
                    ? @"AGUS\SQLEXPRESS"
                    : @"(localdb)\MSSQLLocalDB";

            if (!seguridadIntegrada)
                throw new InvalidOperationException("PacuIbera requiere autenticación integrada de Windows; no se admiten credenciales SQL en appsettings.json.");

            PerfilConfigurado = perfil;
            BaseConfigurada = baseDatos;
            var cadena = new SqlConnectionStringBuilder
            {
                DataSource = servidor.Trim(),
                InitialCatalog = baseDatos,
                IntegratedSecurity = true,
                TrustServerCertificate = confiarCertificado,
                ConnectTimeout = 15
            };
            CadenaGlobal = cadena.ConnectionString;
        }

        private static string? LeerTexto(JsonElement objeto, string propiedad)
        {
            return objeto.TryGetProperty(propiedad, out JsonElement valor)
                && valor.ValueKind == JsonValueKind.String
                ? valor.GetString()
                : null;
        }

        protected SqlConnection ObtenerConexion()
        {
            if (!DestinoAplicacionPermitido)
                throw new InvalidOperationException("El destino configurado no está habilitado para PacuIbera.");
            return new SqlConnection(CadenaGlobal);
        }

        protected async System.Threading.Tasks.Task VerificarBaseConfiguradaAsync(SqlConnection conexion)
        {
            if (!DestinoAplicacionPermitido)
                throw new InvalidOperationException("El perfil o la base configurada no están permitidos.");
            using var comando = new SqlCommand("SELECT DB_NAME();", conexion);
            string? baseReal = await comando.ExecuteScalarAsync() as string;
            if (!string.Equals(baseReal, BaseConfigurada, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("SQL Server conectó a una base distinta de la configurada. Operación cancelada.");
        }
    }
}
