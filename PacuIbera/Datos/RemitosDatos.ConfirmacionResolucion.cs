using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using PacuIbera.Dominio;

namespace PacuIbera.Datos;

public sealed partial class RemitosDatos
{
    public sealed record ResultadoResolucion(int ResolucionId, int RemitoId, string Tipo,
        DateTime FechaResolucion, string EstadoResolucion, string EstadoRemitoActual, bool Reutilizado);
    public sealed record IntentoResolucion(Guid ClaveOperacion, int RemitoId, int UsuarioResponsableId,
        string Tipo, string Motivo, string Servidor, string Base, ResolucionRemitoBorrador[] Lotes);

    public static bool DestinoResolucionPermitido => DestinoAplicacionPermitido;

    public static string ArchivoResolucionPendiente(int remitoId)
    {
        var configuracion = new SqlConnectionStringBuilder(CadenaGlobal);
        string destino = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            configuracion.DataSource.ToUpperInvariant() + "|" + configuracion.InitialCatalog.ToUpperInvariant())));
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PacuIbera", "ResolucionesRemitos", destino, $"remito-{remitoId}", "pendiente.json");
    }

    public static IReadOnlyList<int> RemitosConResolucionPendiente()
    {
        string raiz = Path.GetDirectoryName(Path.GetDirectoryName(ArchivoResolucionPendiente(0)))!;
        if (!Directory.Exists(raiz)) return Array.Empty<int>();
        return Directory.GetDirectories(raiz, "remito-*")
            .Where(d => File.Exists(Path.Combine(d, "pendiente.json")))
            .Select(d => int.TryParse(Path.GetFileName(d)[7..], out int id) ? id : 0)
            .Where(id => id > 0).ToArray();
    }

    private async Task<SqlConnection> AbrirResolucionAsync()
    {
        if (!PuedePrepararResolucion || !DestinoResolucionPermitido)
            throw new InvalidOperationException("Esta operación requiere un Administrador y una configuración PacuIbera permitida.");
        var conexion = ObtenerConexion();
        try
        {
            await conexion.OpenAsync();
            using var validar = new SqlCommand(@"
                SELECT DB_NAME() WHERE DB_NAME()=@BaseConfigurada AND EXISTS
                (SELECT 1 FROM dbo.Usuario u JOIN dbo.Rol r ON r.Id=u.RolId
                 WHERE u.Id=@Usuario AND u.Activo=1 AND r.Nombre=N'Administrador');", conexion);
            validar.Parameters.Add("@Usuario", SqlDbType.Int).Value = SesionActiva.IdUsuario;
            validar.Parameters.Add("@BaseConfigurada", SqlDbType.NVarChar, 128).Value = BaseConfigurada;
            if (!string.Equals(await validar.ExecuteScalarAsync() as string, BaseConfigurada, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("No se verificó la base configurada o el permiso de Administrador activo.");
            return conexion;
        }
        catch { conexion.Dispose(); throw; }
    }

    public async Task ValidarDestinoResolucionAsync()
    {
        using var conexion = await AbrirResolucionAsync();
    }

    private static ResultadoResolucion LeerResultadoResolucion(SqlDataReader lector) => new(
        lector.GetInt32(lector.GetOrdinal("ResolucionId")), lector.GetInt32(lector.GetOrdinal("RemitoId")),
        lector.GetString(lector.GetOrdinal("Tipo")), lector.GetDateTime(lector.GetOrdinal("FechaResolucion")),
        lector.GetString(lector.GetOrdinal("EstadoResolucion")), lector.GetString(lector.GetOrdinal("EstadoRemitoActual")),
        lector.GetBoolean(lector.GetOrdinal("Reutilizado")));

    private static void ArchivarResolucionConfirmada(string archivo, Guid clave)
    {
        // Mover en el mismo directorio conserva el contenido original. Si falla, sigue bloqueado.
        File.Move(archivo, Path.Combine(Path.GetDirectoryName(archivo)!, $"confirmada-{clave:D}.json"));
    }

    // Solo SELECT en SQL. Nunca reejecuta el SP. Una ausencia no demuestra rollback.
    // Un archivo corrupto o que no concilie se conserva y bloquea la siguiente operación.
    public async Task<ResultadoResolucion?> ConciliarResolucionPendienteAsync(int remitoId)
    {
        string archivo = ArchivoResolucionPendiente(remitoId);
        if (!File.Exists(archivo)) return null;
        var intento = JsonSerializer.Deserialize<IntentoResolucion>(File.ReadAllBytes(archivo))
            ?? throw new InvalidOperationException("No se pudo leer el intento anterior. Se mantiene bloqueado.");
        if (intento.RemitoId != remitoId || intento.ClaveOperacion == Guid.Empty || intento.Lotes == null
            || intento.Lotes.Length == 0 || intento.Lotes.Select(l => l.RecepcionRemitoLoteId).Distinct().Count() != intento.Lotes.Length)
            throw new InvalidOperationException("El registro del intento anterior requiere revisión. No lo borre.");
        using var conexion = await AbrirResolucionAsync();
        if (!string.Equals(intento.Servidor, conexion.DataSource, StringComparison.OrdinalIgnoreCase)
            || intento.Base != conexion.Database)
            throw new InvalidOperationException("El destino del intento anterior no coincide. Se mantiene bloqueado.");
        using var cmd = new SqlCommand(@"
            SELECT rd.Id AS ResolucionId,rd.RemitoId,rd.Tipo,rd.FechaResolucion,
                   rd.Estado AS EstadoResolucion,ri.Estado AS EstadoRemitoActual,
                   CONVERT(bit,1) AS Reutilizado,rd.UsuarioResponsableId,rd.Motivo
            FROM dbo.ResolucionDiferencia rd JOIN dbo.RemitoInterno ri ON ri.Id=rd.RemitoId
            WHERE rd.ClaveOperacion=@Clave;
            SELECT d.RecepcionRemitoLoteId,d.Cantidad
            FROM dbo.ResolucionDiferenciaDetalle d JOIN dbo.ResolucionDiferencia rd ON rd.Id=d.ResolucionId
            WHERE rd.ClaveOperacion=@Clave;", conexion);
        cmd.Parameters.Add("@Clave", SqlDbType.UniqueIdentifier).Value = intento.ClaveOperacion;
        ResultadoResolucion resultado;
        var detalles = new Dictionary<int, decimal>();
        using (var lector = await cmd.ExecuteReaderAsync())
        {
            if (!await lector.ReadAsync()) return null;
            resultado = LeerResultadoResolucion(lector);
            if (resultado.RemitoId != remitoId || resultado.EstadoResolucion != "Aplicada"
                || resultado.Tipo != intento.Tipo || lector.GetInt32(7) != intento.UsuarioResponsableId
                || !string.Equals(lector.GetString(8), intento.Motivo, StringComparison.Ordinal))
                throw new InvalidOperationException("El resultado anterior no coincide con lo enviado. Se mantiene bloqueado.");
            if (!await lector.NextResultAsync()) throw new InvalidOperationException("Falta comprobar el detalle del intento anterior.");
            while (await lector.ReadAsync()) detalles.Add(lector.GetInt32(0), lector.GetDecimal(1));
        }
        if (detalles.Count != intento.Lotes.Length || intento.Lotes.Any(l =>
            !detalles.TryGetValue(l.RecepcionRemitoLoteId, out decimal cantidad) || cantidad != l.Cantidad))
            throw new InvalidOperationException("Las cantidades del resultado anterior no coinciden. Se mantiene bloqueado.");
        ArchivarResolucionConfirmada(archivo, intento.ClaveOperacion);
        return resultado;
    }

    public async Task<ResultadoResolucion> ConfirmarResolucionRemitoAsync(int remitoId,
        IReadOnlyList<ResolucionRemitoBorrador> cantidades, string tipo, string motivo)
    {
        if (!PuedePrepararResolucion || !DestinoResolucionPermitido)
            throw new InvalidOperationException("Configure una base PacuIbera permitida e ingrese como Administrador.");
        string archivo = ArchivoResolucionPendiente(remitoId);
        if (File.Exists(archivo)) throw new InvalidOperationException("Hay un intento anterior pendiente de comprobar. No se volverá a enviar.");
        int usuario = SesionActiva.IdUsuario;
        var filas = cantidades.ToArray();
        var actual = await ObtenerRemitoParaResolucionAsync(remitoId);
        using var conexion = await AbrirResolucionAsync();
        if (usuario != SesionActiva.IdUsuario) throw new UnauthorizedAccessException("Cambió el usuario conectado. Abra nuevamente la pantalla.");
        Guid clave = Guid.NewGuid();
        using var comando = PrepararComandoResolucion(conexion, clave, actual.Remito, filas, tipo, motivo, actual.FechaServidor);
        using var tabla = (DataTable)comando.Parameters["@Lotes"].Value;
        var intento = new IntentoResolucion(clave, remitoId, usuario, tipo, motivo, conexion.DataSource, conexion.Database, filas);
        Directory.CreateDirectory(Path.GetDirectoryName(archivo)!);
        byte[] contenido = JsonSerializer.SerializeToUtf8Bytes(intento);
        // Creacion exclusiva y vaciado a disco ANTES de la unica llamada de escritura.
        // Si falla, no ejecutar. Nunca sobrescribir ni borrar un pendiente.
        using (var registro = new FileStream(archivo, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                   4096, FileOptions.WriteThrough))
        {
            registro.Write(contenido);
            registro.Flush(flushToDisk: true);
        }
        ResultadoResolucion resultado;
        using (var lector = await comando.ExecuteReaderAsync())
        {
            if (!await lector.ReadAsync()) throw new InvalidOperationException("No se recibió el resultado de SQL Server.");
            resultado = LeerResultadoResolucion(lector);
            if (resultado.ResolucionId <= 0 || resultado.RemitoId != remitoId || resultado.Tipo != tipo
                || resultado.EstadoResolucion != "Aplicada" || resultado.EstadoRemitoActual is not ("Con diferencias" or "Cerrado"))
                throw new InvalidOperationException("El resultado recibido requiere comprobación.");
            while (await lector.ReadAsync()) { }
            while (await lector.NextResultAsync()) { while (await lector.ReadAsync()) { } }
        }
        // Solo liberar otra resolucion parcial tras recibir una respuesta completa de exito.
        ArchivarResolucionConfirmada(archivo, clave);
        return resultado;
    }
}
