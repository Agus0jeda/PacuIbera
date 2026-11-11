using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using PacuIbera.Dominio;

namespace PacuIbera.Datos;

public sealed partial class RemitosDatos
{
    public sealed record ResultadoRecepcion(int RecepcionRemitoId, int RemitoId, string Estado,
        DateTime FechaRecepcion, bool Reutilizado);
    public sealed record IntentoRecepcion(Guid ClaveOperacion, int RemitoId, int UsuarioReceptorId,
        LoteRecepcionRemitoBorrador[] Lotes);

    public static IntentoRecepcion LeerIntentoRecepcion(int remitoId) =>
        JsonSerializer.Deserialize<IntentoRecepcion>(File.ReadAllBytes(ArchivoIntentoRecepcion(remitoId)))
        ?? throw new InvalidOperationException("No se pudo leer el registro del intento anterior.");

    public async Task ValidarDestinoRecepcionPruebasAsync()
    {
        using var conexion = await AbrirPruebasRecepcionAsync();
    }

    public static bool PerfilRecepcionPruebas =>
        Environment.GetEnvironmentVariable("PACUIBERA_DB_PRUEBAS") == "1"
        && new SqlConnectionStringBuilder(CadenaGlobal).InitialCatalog == "PacuIberaDB_Pruebas";

    // Registro persistente por destino y remito, compartido por las ventanas de este usuario
    // de Windows. CreateNew impide dos envios locales. SQL garantiza la exclusividad global.
    public static string ArchivoIntentoRecepcion(int remitoId)
    {
        var configuracion = new SqlConnectionStringBuilder(CadenaGlobal);
        string destino = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            configuracion.DataSource.ToUpperInvariant() + "|" + configuracion.InitialCatalog.ToUpperInvariant())));
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PacuIbera", "RecepcionesRemitos", destino, $"remito-{remitoId}.json");
    }

    private async Task<SqlConnection> AbrirPruebasRecepcionAsync()
    {
        if (!PuedePrepararRecepcion || !PerfilRecepcionPruebas)
            throw new InvalidOperationException("La recepción requiere un Vendedor y el perfil PacuIbera - Pruebas.");
        var conexion = ObtenerConexion();
        try
        {
            await conexion.OpenAsync();
            using var comprobar = new SqlCommand("SELECT DB_NAME();", conexion);
            if (!string.Equals(await comprobar.ExecuteScalarAsync() as string, "PacuIberaDB_Pruebas", StringComparison.Ordinal))
                throw new InvalidOperationException("La base conectada no es PacuIberaDB_Pruebas. No se enviará la recepción.");
            return conexion;
        }
        catch { conexion.Dispose(); throw; }
    }

    public async Task<string> ConsultarEstadoConfirmacionAsync(int remitoId)
    {
        using var conexion = await AbrirPruebasRecepcionAsync();
        using var cmd = new SqlCommand(@"
            SELECT ri.Estado, rr.Id, rr.ClaveOperacion, rr.UsuarioReceptorId
            FROM dbo.RemitoInterno ri LEFT JOIN dbo.RecepcionRemito rr ON rr.RemitoId=ri.Id
            WHERE ri.Id=@Id AND EXISTS
              (SELECT 1 FROM dbo.Usuario u JOIN dbo.Rol r ON r.Id=u.RolId
               WHERE u.Id=@Usuario AND u.Activo=1 AND r.Nombre=N'Vendedor');", conexion);
        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = remitoId;
        cmd.Parameters.Add("@Usuario", SqlDbType.Int).Value = SesionActiva.IdUsuario;
        using var lector = await cmd.ExecuteReaderAsync();
        if (!await lector.ReadAsync()) return "No se pudo comprobar el remito o los permisos del usuario.";
        return lector.IsDBNull(1)
            ? $"Estado consultado: {lector.GetString(0)}. Aún no aparece una recepción; esto no autoriza un reintento si la llamada anterior sigue en curso."
            : $"Estado consultado: {lector.GetString(0)}. Recepción N.º {lector.GetInt32(1)}, clave {lector.GetGuid(2)}, usuario {lector.GetInt32(3)}.";
    }

    public async Task<ResultadoRecepcion> ConfirmarRecepcionRemitoAsync(DetalleRemitoInterno origen,
        IReadOnlyList<LoteRecepcionRemitoBorrador> cantidades)
    {
        int usuario = SesionActiva.IdUsuario;
        var filas = cantidades.ToArray();
        string archivo = ArchivoIntentoRecepcion(origen.Cabecera.Id);
        if (File.Exists(archivo))
            throw new InvalidOperationException("Ya existe un intento registrado. Consulte el estado; no se generará otra clave ni se reenviará la recepción.");
        // Relectura: usuario activo, remito En tránsito, todos los lotes y fecha del servidor.
        // La validacion final bajo bloqueo pertenece al SP, no a esta lectura previa.
        if (!PerfilRecepcionPruebas) throw new InvalidOperationException("Seleccione el perfil PacuIbera - Pruebas.");
        var actual = await ObtenerRemitoParaRecepcionAsync(origen.Cabecera.Id);
        using var entrada = ConstruirLotesRecepcionEntrada(actual.Remito, filas, actual.FechaServidor);
        using var conexion = await AbrirPruebasRecepcionAsync();
        if (usuario != SesionActiva.IdUsuario) throw new UnauthorizedAccessException("El usuario conectado cambió. Vuelva a abrir la recepción.");
        Guid clave = Guid.NewGuid();
        using var comando = PrepararComandoRecepcionRemito(conexion, clave, actual.Remito, filas, actual.FechaServidor);
        // El DataTable preparado tambien se libera, sin introducir transacciones externas.
        using var tablaComando = (DataTable)comando.Parameters["@Lotes"].Value;
        Directory.CreateDirectory(Path.GetDirectoryName(archivo)!);
        byte[] contenido = JsonSerializer.SerializeToUtf8Bytes(new
        {
            Version = 1, ClaveOperacion = clave, RemitoId = origen.Cabecera.Id,
            UsuarioReceptorId = usuario, Servidor = conexion.DataSource, Base = conexion.Database,
            FechaIntentoUtc = DateTime.UtcNow, Lotes = filas
        });
        // Si falla la persistencia, NO ejecutar SQL. Nunca sobreescribir una clave previa.
        using (var registro = new FileStream(archivo, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                   4096, FileOptions.WriteThrough))
        {
            registro.Write(contenido);
            registro.Flush(flushToDisk: true);
        }
        // Desde este punto, cualquier error se trata conservadoramente como incierto.
        // El registro se conserva incluso tras exito. No borrar para forzar otro intento.
        using var lector = await comando.ExecuteReaderAsync();
        if (!await lector.ReadAsync()) throw new InvalidOperationException("SQL Server no devolvió el resultado de la recepción.");
        var resultado = new ResultadoRecepcion(lector.GetInt32(lector.GetOrdinal("RecepcionRemitoId")),
            lector.GetInt32(lector.GetOrdinal("RemitoId")), lector.GetString(lector.GetOrdinal("Estado")),
            lector.GetDateTime(lector.GetOrdinal("FechaRecepcion")), lector.GetBoolean(lector.GetOrdinal("Reutilizado")));
        if (resultado.RemitoId != origen.Cabecera.Id || resultado.RecepcionRemitoId <= 0
            || resultado.Estado is not ("Recibido" or "Con diferencias"))
            throw new InvalidOperationException("SQL Server devolvió un resultado inesperado. Consulte el estado antes de continuar.");
        // Consumir la respuesta completa: un fallo posterior no debe presentarse como exito.
        while (await lector.ReadAsync()) { }
        while (await lector.NextResultAsync()) { while (await lector.ReadAsync()) { } }
        return resultado;
    }
}
