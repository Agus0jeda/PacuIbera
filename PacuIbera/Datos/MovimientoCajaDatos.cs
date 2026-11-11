using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using PacuIbera.Dominio;

namespace Datos;

public sealed class MovimientoCajaDatos : ConexionBD
{
    public sealed record CajaAbiertaMovimiento(int CajaId, DateTime Apertura, string Responsable,
        decimal? EfectivoDisponible, string MotivoNoDisponible)
    {
        public string Descripcion
        {
            get => $"Caja N.º {CajaId} · {Responsable} · apertura {Apertura:dd/MM/yyyy HH:mm}";
        }
    }

    public sealed record IntentoMovimiento(Guid ClaveOperacion, int CajaId, int UsuarioId,
        string TipoMovimiento, decimal Importe, string Motivo, string Servidor, string Base);

    public sealed record ResultadoMovimiento(int MovimientoCajaId, int CajaId, int UsuarioId,
        string TipoMovimiento, decimal Importe, string Motivo, DateTime FechaHora,
        Guid ClaveOperacion, bool Reutilizado, bool PendienteLocal);

    public sealed record MovimientoCajaHistorial(int MovimientoCajaId, DateTime FechaHora,
        int CajaId, string ResponsableCaja, string TipoMovimiento, decimal ImporteFirmado,
        string Administrador, string Motivo)
    {
        public string ImporteConSigno =>
            $"{(ImporteFirmado < 0 ? "−" : "+")} $ {Math.Abs(ImporteFirmado).ToString("N2", Argentina)}";
    }

    public sealed record CajaHistorial(int CajaId, string Responsable);

    public sealed class RechazoMovimientoException(string mensaje) : Exception(mensaje);

    private static readonly CultureInfo Argentina = CultureInfo.GetCultureInfo("es-AR");
    private static readonly SemaphoreSlim OperacionLocal = new(1, 1);

    public static bool DestinoPermitido => DestinoAplicacionPermitido;

    public static bool PuedeConsultarHistorial => SesionActiva.IdUsuario > 0
        && (SesionActiva.Rol is "Administrador" or "Gerente");

    private static string ArchivoPendiente
    {
        get
        {
            var cs = new SqlConnectionStringBuilder(CadenaGlobal);
            string destino = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
                cs.DataSource.ToUpperInvariant() + "|" + cs.InitialCatalog.ToUpperInvariant())));
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PacuIbera", "MovimientosCaja", destino, $"administrador-{SesionActiva.IdUsuario}.json");
        }
    }

    private static void ValidarSesionLocal()
    {
        if (SesionActiva.IdUsuario <= 0 || SesionActiva.Rol != "Administrador")
            throw new UnauthorizedAccessException("Solo un Administrador puede registrar movimientos de efectivo.");
        if (!DestinoPermitido)
            throw new InvalidOperationException("La configuración no habilita movimientos de caja para este destino.");
    }

    private async Task<SqlConnection> AbrirConexionCajaAsync()
    {
        ValidarSesionLocal();
        var conexion = ObtenerConexion();
        try
        {
            await conexion.OpenAsync();
            using var comando = new SqlCommand(@"
SELECT DB_NAME(),
       CASE WHEN EXISTS
       (
           SELECT 1 FROM dbo.Usuario u
           INNER JOIN dbo.Rol r ON r.Id = u.RolId
           WHERE u.Id = @UsuarioId AND u.Activo = 1 AND r.Nombre = N'Administrador'
       ) THEN 1 ELSE 0 END,
       CASE WHEN OBJECT_ID(N'dbo.MovimientoCaja', N'U') IS NOT NULL
                  AND OBJECT_ID(N'dbo.sp_RegistrarMovimientoCaja', N'P') IS NOT NULL
            THEN 1 ELSE 0 END;", conexion);
            comando.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = SesionActiva.IdUsuario;
            using var lector = await comando.ExecuteReaderAsync();
            if (!await lector.ReadAsync() || !string.Equals(lector.GetString(0), BaseConfigurada, StringComparison.OrdinalIgnoreCase)
                || lector.GetInt32(1) != 1 || lector.GetInt32(2) != 1)
                throw new InvalidOperationException("No se pudo verificar la base configurada, el Administrador activo o el procedimiento instalado.");
            return conexion;
        }
        catch
        {
            await conexion.DisposeAsync();
            throw;
        }
    }

    public async Task<IReadOnlyList<CajaAbiertaMovimiento>> ObtenerCajasAbiertasAsync()
    {
        using var conexion = await AbrirConexionCajaAsync();
        using var comando = new SqlCommand(ConsultaCajasAbiertas, conexion);
        using var lector = await comando.ExecuteReaderAsync();
        var resultado = new List<CajaAbiertaMovimiento>();
        while (await lector.ReadAsync())
        {
            resultado.Add(new CajaAbiertaMovimiento(
                lector.GetInt32(0), lector.GetDateTime(1), lector.GetString(2),
                lector.IsDBNull(3) ? null : lector.GetDecimal(3),
                lector.IsDBNull(4) ? "" : lector.GetString(4)));
        }
        return resultado;
    }

    public async Task ValidarAdministradorActivoAsync()
    {
        using var conexion = await AbrirConexionCajaAsync();
    }

    private async Task<SqlConnection> AbrirConexionHistorialAsync()
    {
        if (SesionActiva.IdUsuario <= 0 || SesionActiva.Rol is not ("Administrador" or "Gerente"))
            throw new UnauthorizedAccessException("Solo un Administrador o Gerente puede consultar este historial.");
        if (!DestinoPermitido)
            throw new InvalidOperationException("La configuración no habilita el historial para este destino.");

        var conexion = ObtenerConexion();
        try
        {
            await conexion.OpenAsync();
            using var comando = new SqlCommand(@"
SELECT DB_NAME(), CASE WHEN EXISTS
(
    SELECT 1 FROM dbo.Usuario u
    INNER JOIN dbo.Rol r ON r.Id=u.RolId
    WHERE u.Id=@UsuarioId AND u.Activo=1 AND r.Nombre=@Rol
) THEN 1 ELSE 0 END,
CASE WHEN OBJECT_ID(N'dbo.MovimientoCaja',N'U') IS NOT NULL THEN 1 ELSE 0 END;", conexion);
            comando.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = SesionActiva.IdUsuario;
            comando.Parameters.Add("@Rol", SqlDbType.NVarChar, 50).Value = SesionActiva.Rol;
            using var lector = await comando.ExecuteReaderAsync();
            if (!await lector.ReadAsync() || !string.Equals(lector.GetString(0), BaseConfigurada, StringComparison.OrdinalIgnoreCase)
                || lector.GetInt32(1) != 1 || lector.GetInt32(2) != 1)
                throw new UnauthorizedAccessException("No se pudo validar el usuario activo o la base configurada.");
            return conexion;
        }
        catch
        {
            await conexion.DisposeAsync();
            throw;
        }
    }

    public async Task ValidarAccesoHistorialAsync()
    {
        using var conexion = await AbrirConexionHistorialAsync();
    }

    public async Task<IReadOnlyList<CajaHistorial>> ObtenerCajasConMovimientosAsync()
    {
        using var conexion = await AbrirConexionHistorialAsync();
        using var comando = new SqlCommand(@"
SELECT DISTINCT c.Id,
       LTRIM(RTRIM(COALESCE(v.Nombre,N'')+N' '+COALESCE(v.Apellido,N''))) AS Responsable
FROM dbo.MovimientoCaja m
INNER JOIN dbo.Caja c ON c.Id=m.CajaId
INNER JOIN dbo.Usuario v ON v.Id=c.UsuarioId
ORDER BY c.Id;", conexion);
        using var lector = await comando.ExecuteReaderAsync();
        var resultado = new List<CajaHistorial>();
        while (await lector.ReadAsync())
            resultado.Add(new CajaHistorial(lector.GetInt32(0), lector.GetString(1)));
        return resultado;
    }

    public async Task<IReadOnlyList<MovimientoCajaHistorial>> ObtenerHistorialAsync(
        DateTime? desde, DateTime? hasta, int? cajaId, string? tipoMovimiento)
    {
        if (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date)
            throw new ArgumentException("La fecha inicial no puede ser posterior a la fecha final.");
        if (cajaId is <= 0) throw new ArgumentException("El filtro de caja no es válido.");
        if (tipoMovimiento is not (null or "Ingreso" or "Retiro"))
            throw new ArgumentException("El filtro de tipo de movimiento no es válido.");

        using var conexion = await AbrirConexionHistorialAsync();
        using var comando = new SqlCommand(@"
SELECT m.Id, m.FechaHora, c.Id AS CajaId,
       LTRIM(RTRIM(COALESCE(v.Nombre,N'')+N' '+COALESCE(v.Apellido,N''))) AS ResponsableCaja,
       m.TipoMovimiento,
       CASE WHEN m.TipoMovimiento=N'Ingreso' THEN m.Importe ELSE -m.Importe END AS ImporteFirmado,
       LTRIM(RTRIM(COALESCE(a.Nombre,N'')+N' '+COALESCE(a.Apellido,N''))) AS Administrador,
       m.Motivo
FROM dbo.MovimientoCaja m
INNER JOIN dbo.Caja c ON c.Id=m.CajaId
INNER JOIN dbo.Usuario v ON v.Id=c.UsuarioId
INNER JOIN dbo.Usuario a ON a.Id=m.UsuarioId
WHERE (@Desde IS NULL OR m.FechaHora>=@Desde)
  AND (@HastaExclusivo IS NULL OR m.FechaHora<@HastaExclusivo)
  AND (@CajaId IS NULL OR m.CajaId=@CajaId)
  AND (@TipoMovimiento IS NULL OR m.TipoMovimiento=@TipoMovimiento)
ORDER BY m.FechaHora DESC,m.Id DESC;", conexion);
        comando.Parameters.Add("@Desde", SqlDbType.DateTime2).Value = desde.HasValue
            ? desde.Value.Date : DBNull.Value;
        comando.Parameters.Add("@HastaExclusivo", SqlDbType.DateTime2).Value = hasta.HasValue
            ? hasta.Value.Date.AddDays(1) : DBNull.Value;
        comando.Parameters.Add("@CajaId", SqlDbType.Int).Value = cajaId.HasValue
            ? cajaId.Value : DBNull.Value;
        comando.Parameters.Add("@TipoMovimiento", SqlDbType.NVarChar, 10).Value = tipoMovimiento is null
            ? DBNull.Value : tipoMovimiento;

        using var lector = await comando.ExecuteReaderAsync();
        var resultado = new List<MovimientoCajaHistorial>();
        while (await lector.ReadAsync())
        {
            resultado.Add(new MovimientoCajaHistorial(
                lector.GetInt32(0), lector.GetDateTime(1), lector.GetInt32(2),
                lector.GetString(3), lector.GetString(4), lector.GetDecimal(5),
                lector.GetString(6), lector.GetString(7)));
        }
        return resultado;
    }

    public IntentoMovimiento CrearIntento(int cajaId, string tipo, decimal importe, string motivo)
    {
        ValidarSesionLocal();
        if (cajaId <= 0 || tipo is not ("Ingreso" or "Retiro") || importe <= 0
            || importe > 9999999999999999.99m || decimal.Round(importe, 2) != importe
            || string.IsNullOrWhiteSpace(motivo) || motivo.Trim().Length > 500)
            throw new ArgumentException("Revise el tipo, el importe y el motivo del movimiento.");

        var cs = new SqlConnectionStringBuilder(CadenaGlobal);
        var intento = new IntentoMovimiento(Guid.NewGuid(), cajaId, SesionActiva.IdUsuario,
            tipo, importe, motivo.Trim(), cs.DataSource, cs.InitialCatalog);
        string ruta = ArchivoPendiente;
        Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
        byte[] contenido = JsonSerializer.SerializeToUtf8Bytes(intento);
        using var archivo = new FileStream(ruta, FileMode.CreateNew, FileAccess.Write, FileShare.None,
            4096, FileOptions.WriteThrough);
        archivo.Write(contenido);
        archivo.Flush(flushToDisk: true);
        return intento;
    }

    public IntentoMovimiento? LeerIntentoPendiente()
    {
        if (!File.Exists(ArchivoPendiente)) return null;
        var intento = JsonSerializer.Deserialize<IntentoMovimiento>(File.ReadAllBytes(ArchivoPendiente))
            ?? throw new InvalidOperationException("Hay un movimiento anterior que requiere revisión. No se iniciará otro.");
        if (intento.ClaveOperacion == Guid.Empty || intento.CajaId <= 0 || intento.UsuarioId != SesionActiva.IdUsuario
            || intento.TipoMovimiento is not ("Ingreso" or "Retiro") || intento.Importe <= 0
            || string.IsNullOrWhiteSpace(intento.Motivo) || intento.Motivo.Length > 500)
            throw new InvalidOperationException("El movimiento anterior no se puede verificar. Se mantiene bloqueado.");
        return intento;
    }

    public async Task<ResultadoMovimiento> RegistrarPendienteAsync(IntentoMovimiento intento)
    {
        await OperacionLocal.WaitAsync();
        try
        {
            ValidarSesionLocal();
            if (intento.UsuarioId != SesionActiva.IdUsuario)
                throw new UnauthorizedAccessException("El usuario conectado no coincide con el intento pendiente.");
            var guardado = LeerIntentoPendiente();
            if (guardado != intento)
                throw new InvalidOperationException("El movimiento pendiente cambió. No se enviará una operación distinta.");

            using var conexion = await AbrirConexionCajaAsync();
            if (!string.Equals(intento.Servidor, conexion.DataSource, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(intento.Base, conexion.Database, StringComparison.Ordinal))
                throw new InvalidOperationException("El destino del intento anterior no coincide con la base configurada.");

            using var comando = new SqlCommand("dbo.sp_RegistrarMovimientoCaja", conexion)
            { CommandType = CommandType.StoredProcedure, CommandTimeout = 30 };
            comando.Parameters.Add("@CajaId", SqlDbType.Int).Value = intento.CajaId;
            comando.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = intento.UsuarioId;
            comando.Parameters.Add("@TipoMovimiento", SqlDbType.NVarChar, 10).Value = intento.TipoMovimiento;
            var importe = comando.Parameters.Add("@Importe", SqlDbType.Decimal);
            importe.Precision = 18; importe.Scale = 2; importe.Value = intento.Importe;
            comando.Parameters.Add("@Motivo", SqlDbType.NVarChar, 500).Value = intento.Motivo;
            comando.Parameters.Add("@ClaveOperacion", SqlDbType.UniqueIdentifier).Value = intento.ClaveOperacion;

            try
            {
                using var lector = await comando.ExecuteReaderAsync();
                if (!await lector.ReadAsync())
                    throw new InvalidOperationException("El servidor no devolvió la confirmación del movimiento.");
                var resultado = new ResultadoMovimiento(
                    lector.GetInt32(lector.GetOrdinal("MovimientoCajaId")),
                    lector.GetInt32(lector.GetOrdinal("CajaId")),
                    lector.GetInt32(lector.GetOrdinal("UsuarioId")),
                    lector.GetString(lector.GetOrdinal("TipoMovimiento")),
                    lector.GetDecimal(lector.GetOrdinal("Importe")),
                    lector.GetString(lector.GetOrdinal("Motivo")),
                    lector.GetDateTime(lector.GetOrdinal("FechaHora")),
                    lector.GetGuid(lector.GetOrdinal("ClaveOperacion")),
                    lector.GetBoolean(lector.GetOrdinal("Reutilizado")), false);
                if (resultado.MovimientoCajaId <= 0 || resultado.ClaveOperacion != intento.ClaveOperacion
                    || resultado.CajaId != intento.CajaId || resultado.UsuarioId != intento.UsuarioId
                    || resultado.TipoMovimiento != intento.TipoMovimiento || resultado.Importe != intento.Importe
                    || resultado.Motivo != intento.Motivo)
                    throw new InvalidOperationException("La respuesta recibida no coincide con el movimiento pendiente.");

                bool archivado = ArchivarPendiente(intento.ClaveOperacion);
                return resultado with { PendienteLocal = !archivado };
            }
            catch (SqlException ex) when (EsRechazoDefinitivo(ex.Number))
            {
                EliminarPendiente(intento.ClaveOperacion);
                throw new RechazoMovimientoException(MensajeRechazo(ex.Number));
            }
        }
        finally { OperacionLocal.Release(); }
    }

    private static bool EsRechazoDefinitivo(int numero) =>
        numero is >= 52910 and <= 52929 && numero != 52917;

    private static string MensajeRechazo(int numero) => numero switch
    {
        52916 => "Solo un Administrador activo puede registrar el movimiento.",
        52919 or 52920 => "La caja ya no está abierta. Actualice la lista y seleccione una caja abierta.",
        52922 or 52923 or 52927 or 52928 or 52929 => "No se pudo calcular el efectivo de esta caja con seguridad. No se registró el movimiento.",
        52925 => "El retiro supera el efectivo disponible. No se registró el movimiento.",
        52917 => "La clave de operación ya está asociada a otros datos. Se mantiene bloqueada para revisión.",
        _ => "No se registró el movimiento. Revise los datos e inténtelo nuevamente."
    };

    private static bool ArchivarPendiente(Guid clave)
    {
        try
        {
            File.Move(ArchivoPendiente,
                Path.Combine(Path.GetDirectoryName(ArchivoPendiente)!, $"confirmado-{clave:D}.json"), false);
            return true;
        }
        catch { return false; }
    }

    private static void EliminarPendiente(Guid clave)
    {
        var intento = JsonSerializer.Deserialize<IntentoMovimiento>(File.ReadAllBytes(ArchivoPendiente));
        if (intento?.ClaveOperacion == clave) File.Delete(ArchivoPendiente);
    }

    private const string ConsultaCajasAbiertas = @"
;WITH MetodosEsperados AS
(
    SELECT * FROM (VALUES
        (1, N'Efectivo'), (1002, N'Efectivo'), (1006, N'Efectivo'), (2006, N'Efectivo'),
        (2, N'Tarjeta'), (1003, N'Tarjeta'), (1007, N'Tarjeta'), (2007, N'Tarjeta'),
        (3, N'Tarjeta'), (1004, N'Tarjeta'), (1008, N'Tarjeta'), (2008, N'Tarjeta'),
        (4, N'Transferencia'), (1005, N'Transferencia'), (1009, N'Transferencia'), (2009, N'Transferencia')
    ) v(Id, Categoria)
), ConfiguracionPagos AS
(
    SELECT CONVERT(bit, CASE WHEN EXISTS
    (
        SELECT 1 FROM MetodosEsperados e LEFT JOIN dbo.MetodoPago mp ON mp.Id=e.Id
        WHERE mp.Id IS NULL
           OR (e.Categoria=N'Efectivo' AND LOWER(mp.Nombre) NOT LIKE N'%efectivo%')
           OR (e.Categoria=N'Tarjeta' AND mp.Nombre COLLATE Latin1_General_100_CI_AI NOT LIKE N'%debito%'
                                        AND mp.Nombre COLLATE Latin1_General_100_CI_AI NOT LIKE N'%credito%')
           OR (e.Categoria=N'Transferencia' AND LOWER(mp.Nombre) NOT LIKE N'%transfer%'
                                               AND LOWER(mp.Nombre) NOT LIKE N'%mercado pago%')
    ) THEN 0 ELSE 1 END) AS Valida
), Cajas AS
(
    SELECT c.Id, c.FechaHoraApertura, c.SaldoInicial,
           LTRIM(RTRIM(COALESCE(u.Nombre, N'') + N' ' + COALESCE(u.Apellido, N''))) AS Responsable
    FROM dbo.Caja c
    INNER JOIN dbo.Usuario u ON u.Id=c.UsuarioId
    WHERE c.Estado=N'Abierta'
), PagosPorVenta AS
(
    SELECT c.Id AS CajaId, v.Id AS VentaId, v.Estado, v.Total,
           COALESCE(SUM(CASE WHEN mp.Id IN (1,1002,1006,2006)
                             THEN CONVERT(decimal(38,2),vp.MontoCobrado) ELSE CONVERT(decimal(38,2),0) END),0) AS EfectivoEntregado,
           COALESCE(SUM(CASE WHEN mp.Id IN (2,1003,1007,2007,3,1004,1008,2008,4,1005,1009,2009)
                             THEN CONVERT(decimal(38,2),vp.MontoCobrado) ELSE CONVERT(decimal(38,2),0) END),0) AS OtrosMedios,
           MAX(CASE WHEN vp.VentaId IS NOT NULL AND (mp.Id IS NULL OR mp.Id NOT IN
               (1,1002,1006,2006,2,1003,1007,2007,3,1004,1008,2008,4,1005,1009,2009)) THEN 1 ELSE 0 END) AS MedioDesconocido
    FROM Cajas c
    LEFT JOIN dbo.Venta v ON v.CajaId=c.Id
    LEFT JOIN dbo.VentaPago vp ON vp.VentaId=v.Id
    LEFT JOIN dbo.MetodoPago mp ON mp.Id=vp.MetodoPagoId
    GROUP BY c.Id,v.Id,v.Estado,v.Total
), TotalesVenta AS
(
    SELECT CajaId,
           COALESCE(SUM(CASE WHEN Estado<>N'Anulada' AND Estado IS NOT NULL
               THEN CASE WHEN EfectivoEntregado>Total-OtrosMedios THEN Total-OtrosMedios ELSE EfectivoEntregado END
               ELSE CONVERT(decimal(38,2),0) END),0) AS EfectivoConservado,
           MAX(CASE WHEN MedioDesconocido=1 OR Estado IS NULL
               OR (Estado=N'Anulada' AND EfectivoEntregado>0)
               OR (Estado<>N'Anulada' AND (Total IS NULL OR OtrosMedios>Total OR EfectivoEntregado+OtrosMedios<Total))
               THEN 1 ELSE 0 END) AS CalculoInseguro
    FROM PagosPorVenta WHERE VentaId IS NOT NULL
    GROUP BY CajaId
), TotalesMovimiento AS
(
    SELECT CajaId,
           SUM(CASE WHEN TipoMovimiento=N'Ingreso' THEN CONVERT(decimal(38,2),Importe) ELSE CONVERT(decimal(38,2),0) END) AS Ingresos,
           SUM(CASE WHEN TipoMovimiento=N'Retiro' THEN CONVERT(decimal(38,2),Importe) ELSE CONVERT(decimal(38,2),0) END) AS Retiros
    FROM dbo.MovimientoCaja GROUP BY CajaId
), Saldos AS
(
    SELECT c.*, p.Valida AS MetodosValidos,
           COALESCE(v.EfectivoConservado,0) AS EfectivoConservado,
           COALESCE(v.CalculoInseguro,0) AS CalculoInseguro,
           COALESCE(m.Ingresos,0) AS Ingresos, COALESCE(m.Retiros,0) AS Retiros
    FROM Cajas c CROSS JOIN ConfiguracionPagos p
    LEFT JOIN TotalesVenta v ON v.CajaId=c.Id
    LEFT JOIN TotalesMovimiento m ON m.CajaId=c.Id
)
SELECT s.Id, s.FechaHoraApertura, s.Responsable,
       CASE WHEN s.SaldoInicial IS NULL OR s.MetodosValidos=0 OR s.CalculoInseguro=1 THEN NULL
            ELSE TRY_CONVERT(decimal(18,2),CONVERT(decimal(38,2),s.SaldoInicial)+s.EfectivoConservado+s.Ingresos-s.Retiros) END AS EfectivoDisponible,
       CASE WHEN s.SaldoInicial IS NULL THEN N'La caja no tiene fondo inicial registrado.'
            WHEN s.MetodosValidos=0 THEN N'Los medios de pago registrados requieren revisión.'
            WHEN s.CalculoInseguro=1 THEN N'Hay ventas o pagos que no permiten calcular el efectivo con seguridad.'
            WHEN TRY_CONVERT(decimal(18,2),CONVERT(decimal(38,2),s.SaldoInicial)+s.EfectivoConservado+s.Ingresos-s.Retiros) IS NULL
                 THEN N'El saldo supera el rango que puede mostrarse con seguridad.'
            WHEN CONVERT(decimal(38,2),s.SaldoInicial)+s.EfectivoConservado+s.Ingresos-s.Retiros<0
                 THEN N'El efectivo calculado es inconsistente.'
            ELSE N'' END AS MotivoNoDisponible
FROM Saldos s ORDER BY s.FechaHoraApertura,s.Id;";
}
