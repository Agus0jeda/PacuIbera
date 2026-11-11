using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using PacuIbera.Dominio;

namespace PacuIbera.Datos;

public sealed partial class RemitosDatos
{
    public sealed record ResultadoEmision(int RemitoId, string Estado, DateTime FechaEmision, bool Reutilizado);
    public sealed record IntentoEmision(Guid ClaveOperacion, int UsuarioEmisorId, string Servidor,
        string Base, ProductoEmisionRemito[] Productos);

    public static bool PerfilEmisionPruebas =>
        Environment.GetEnvironmentVariable("PACUIBERA_DB_PRUEBAS") == "1"
        && new SqlConnectionStringBuilder(CadenaGlobal).InitialCatalog == "PacuIberaDB_Pruebas";

    // Un pendiente por destino y usuario de Windows bloquea nuevas emisiones locales.
    // La unicidad global y la seleccion/descuento de lotes pertenecen al procedimiento SQL.
    public static string ArchivoEmisionPendiente
    {
        get
        {
            var configuracion = new SqlConnectionStringBuilder(CadenaGlobal);
            string destino = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
                configuracion.DataSource.ToUpperInvariant() + "|" + configuracion.InitialCatalog.ToUpperInvariant())));
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PacuIbera", "EmisionesRemitos", destino, "pendiente.json");
        }
    }

    public static IntentoEmision LeerEmisionPendiente() =>
        JsonSerializer.Deserialize<IntentoEmision>(File.ReadAllBytes(ArchivoEmisionPendiente))
        ?? throw new InvalidOperationException("No se pudo leer el intento anterior. Se mantiene bloqueado.");

    private async Task<SqlConnection> AbrirPruebasEmisionAsync()
    {
        if (!PuedePrepararEmision || !PerfilEmisionPruebas)
            throw new InvalidOperationException("Ingrese como Administrador con el perfil PacuIbera - Pruebas.");
        var conexion = ObtenerConexion();
        try
        {
            await conexion.OpenAsync();
            using var validar = new SqlCommand(@"
                SELECT DB_NAME() WHERE EXISTS
                (SELECT 1 FROM dbo.Usuario u JOIN dbo.Rol r ON r.Id=u.RolId
                 WHERE u.Id=@Usuario AND u.Activo=1 AND r.Nombre=N'Administrador');", conexion);
            validar.Parameters.Add("@Usuario", SqlDbType.Int).Value = SesionActiva.IdUsuario;
            if (!string.Equals(await validar.ExecuteScalarAsync() as string, "PacuIberaDB_Pruebas", StringComparison.Ordinal))
                throw new InvalidOperationException("No se verificó la base de pruebas o el Administrador activo. No se emitirá el remito.");
            return conexion;
        }
        catch { conexion.Dispose(); throw; }
    }

    public async Task ValidarDestinoEmisionPruebasAsync()
    {
        using var conexion = await AbrirPruebasEmisionAsync();
    }

    public async Task<ProductoEmisionRemito[]> ValidarProductosEmisionActualAsync(IEnumerable<ProductoEmisionRemito> productos)
    {
        using var conexion = await AbrirPruebasEmisionAsync();
        var filas = productos.ToArray();
        var disponibles = await ObtenerProductosEmisionAsync();
        var actualizadas = filas.Select(p =>
        {
            var actual = disponibles.FirstOrDefault(d => d.ProductoId == p.ProductoId);
            if (actual == null || actual.SeVendePorPeso != p.SeVendePorPeso)
                throw new InvalidOperationException($"{p.Producto}: el producto cambió o ya no está disponible. Actualice la lista.");
            return p with { StockDisponible = actual.StockDisponible };
        }).ToArray();
        using var entrada = ConstruirProductosEntrada(actualizadas);
        return actualizadas;
    }

    private static ResultadoEmision LeerResultadoEmision(SqlDataReader lector) => new(
        lector.GetInt32(lector.GetOrdinal("RemitoId")), lector.GetString(lector.GetOrdinal("Estado")),
        lector.GetDateTime(lector.GetOrdinal("FechaEmision")), lector.GetBoolean(lector.GetOrdinal("Reutilizado")));

    private static void ArchivarEmisionConfirmada(Guid clave) =>
        File.Move(ArchivoEmisionPendiente,
            Path.Combine(Path.GetDirectoryName(ArchivoEmisionPendiente)!, $"confirmada-{clave:D}.json"));

    // Consulta por la misma clave; nunca reenvia el SP. Ausencia NO prueba un rollback.
    public async Task<ResultadoEmision?> ConciliarEmisionPendienteAsync(Guid? claveEsperada = null)
    {
        if (!File.Exists(ArchivoEmisionPendiente)) return null;
        var intento = LeerEmisionPendiente();
        if (claveEsperada.HasValue && intento.ClaveOperacion != claveEsperada.Value)
            throw new InvalidOperationException("El registro pendiente corresponde a otra operación. No se repetirá el envío.");
        if (intento.ClaveOperacion == Guid.Empty || intento.Productos == null || intento.Productos.Length == 0
            || intento.Productos.Select(p => p.ProductoId).Distinct().Count() != intento.Productos.Length)
            throw new InvalidOperationException("El intento anterior requiere revisión. No borre su registro.");
        using var conexion = await AbrirPruebasEmisionAsync();
        if (!string.Equals(intento.Servidor, conexion.DataSource, StringComparison.OrdinalIgnoreCase)
            || intento.Base != conexion.Database)
            throw new InvalidOperationException("El destino del intento anterior no coincide. No se emitirá otro remito.");
        using var cmd = new SqlCommand(@"
            SELECT Id AS RemitoId,Estado,FechaEmision,CONVERT(bit,1) AS Reutilizado,UsuarioEmisorId
            FROM dbo.RemitoInterno WHERE ClaveOperacion=@Clave;
            SELECT d.ProductoId,d.CantidadEnviada FROM dbo.RemitoDetalle d
            JOIN dbo.RemitoInterno r ON r.Id=d.RemitoId WHERE r.ClaveOperacion=@Clave;", conexion);
        cmd.Parameters.Add("@Clave", SqlDbType.UniqueIdentifier).Value = intento.ClaveOperacion;
        ResultadoEmision resultado;
        var cantidades = new Dictionary<int, decimal>();
        using (var lector = await cmd.ExecuteReaderAsync())
        {
            if (!await lector.ReadAsync()) return null;
            if (lector.IsDBNull(2) || lector.IsDBNull(4) || lector.GetInt32(4) != intento.UsuarioEmisorId)
                throw new InvalidOperationException("La emisión encontrada no coincide con el intento anterior.");
            resultado = LeerResultadoEmision(lector);
            if (resultado.RemitoId <= 0 || resultado.Estado is not ("En tránsito" or "Recibido" or "Con diferencias" or "Cerrado"))
                throw new InvalidOperationException("El estado de la emisión anterior requiere revisión.");
            if (!await lector.NextResultAsync()) throw new InvalidOperationException("Falta comprobar los productos del remito anterior.");
            while (await lector.ReadAsync()) cantidades.Add(lector.GetInt32(0), lector.GetDecimal(1));
        }
        if (cantidades.Count != intento.Productos.Length || intento.Productos.Any(p =>
            !cantidades.TryGetValue(p.ProductoId, out decimal cantidad) || cantidad != p.Cantidad))
            throw new InvalidOperationException("Los productos del remito anterior no coinciden con lo enviado. Se mantiene bloqueado.");
        ArchivarEmisionConfirmada(intento.ClaveOperacion);
        return resultado;
    }

    public async Task<ResultadoEmision> EmitirRemitoAsync(Guid clave, IReadOnlyList<ProductoEmisionRemito> productos)
    {
        if (clave == Guid.Empty) throw new ArgumentException("La clave de operación es obligatoria.");
        if (File.Exists(ArchivoEmisionPendiente))
            throw new InvalidOperationException("Hay una emisión anterior sin comprobar. No se generará otra clave.");
        int usuario = SesionActiva.IdUsuario;
        var filas = await ValidarProductosEmisionActualAsync(productos);
        using var conexion = await AbrirPruebasEmisionAsync();
        if (usuario != SesionActiva.IdUsuario) throw new UnauthorizedAccessException("El usuario conectado cambió. Abra nuevamente la pantalla.");
        using var comando = PrepararComandoEmision(conexion, clave, filas);
        using var tabla = (DataTable)comando.Parameters["@Productos"].Value;
        var intento = new IntentoEmision(clave, usuario, conexion.DataSource, conexion.Database, filas);
        Directory.CreateDirectory(Path.GetDirectoryName(ArchivoEmisionPendiente)!);
        byte[] contenido = JsonSerializer.SerializeToUtf8Bytes(intento);
        // CreateNew arbitra ventanas/procesos locales. Persistir y vaciar antes de ejecutar.
        // Si falla el guardado, NO enviar. Nunca reemplazar ni borrar un pendiente.
        using (var registro = new FileStream(ArchivoEmisionPendiente, FileMode.CreateNew, FileAccess.Write,
                   FileShare.None, 4096, FileOptions.WriteThrough))
        {
            registro.Write(contenido);
            registro.Flush(flushToDisk: true);
        }
        ResultadoEmision resultado;
        // Unica escritura: sin transaccion externa ni actualizaciones de stock desde C#.
        using (var lector = await comando.ExecuteReaderAsync())
        {
            if (!await lector.ReadAsync()) throw new InvalidOperationException("SQL Server no devolvió el resultado de la emisión.");
            resultado = LeerResultadoEmision(lector);
            if (resultado.RemitoId <= 0 || resultado.Estado != "En tránsito")
                throw new InvalidOperationException("La respuesta de la emisión requiere comprobación.");
            while (await lector.ReadAsync()) { }
            while (await lector.NextResultAsync()) { while (await lector.ReadAsync()) { } }
        }
        // Solo una respuesta completa permite preparar otra operacion.
        ArchivarEmisionConfirmada(clave);
        return resultado;
    }
}
