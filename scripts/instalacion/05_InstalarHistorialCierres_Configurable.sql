/* Procedimiento de historial de cierres para la instalación configurable.
   Ejecutar en SQLCMD Mode con el mismo DatabaseName usado por el instalador.
   La consulta es de solo lectura y no altera caja, ventas ni movimientos. */
:on error exit
:setvar DatabaseName "PacuIbera_Entrega"
IF N'$(DatabaseName)' LIKE N'%[^A-Za-z0-9_]%' OR N'$(DatabaseName)' IN (N'master',N'model',N'msdb',N'tempdb',N'PacuIberaDB',N'PacuIberaDB_Pruebas')
    THROW 53510, 'DatabaseName debe ser una base PacuIbera de entrega permitida.', 1;
IF DB_NAME() <> N'$(DatabaseName)'
    THROW 53511, 'Conectese a DatabaseName antes de instalar el historial de cierres.', 1;
IF OBJECT_ID(N'dbo.Caja',N'U') IS NULL OR OBJECT_ID(N'dbo.Usuario',N'U') IS NULL
   OR OBJECT_ID(N'dbo.Rol',N'U') IS NULL OR OBJECT_ID(N'dbo.Venta',N'U') IS NULL
   OR OBJECT_ID(N'dbo.VentaPago',N'U') IS NULL OR OBJECT_ID(N'dbo.MetodoPago',N'U') IS NULL
   OR OBJECT_ID(N'dbo.MovimientoCaja',N'U') IS NULL
    THROW 53512, 'Faltan dependencias del esquema de caja; ejecute primero el instalador completo.', 1;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.sp_ConsultarHistorialCierresCaja
    @FechaDesde date,
    @FechaHasta date,
    @UsuarioId int
AS
BEGIN
    SET NOCOUNT ON;

    IF @FechaDesde IS NULL OR @FechaHasta IS NULL OR @FechaDesde > @FechaHasta
        THROW 53502, ''Indique un intervalo de fechas válido.'', 1;
    IF @UsuarioId IS NULL OR @UsuarioId <= 0 OR NOT EXISTS
    (
        SELECT 1
        FROM dbo.Usuario u
        INNER JOIN dbo.Rol r ON r.Id = u.RolId
        WHERE u.Id = @UsuarioId AND u.Activo = 1
          AND r.Nombre IN (N''Administrador'',N''Gerente'')
    )
        THROW 53503, ''Solo un Administrador o Gerente activo puede consultar el historial de cierres.'', 1;

    DECLARE @Inicio datetime2(0) = CONVERT(datetime2(0), @FechaDesde);
    DECLARE @Metodos TABLE (Id int NOT NULL PRIMARY KEY, Categoria nvarchar(20) NOT NULL);
    INSERT INTO @Metodos(Id, Categoria)
    VALUES (1,N''Efectivo''),(1002,N''Efectivo''),(1006,N''Efectivo''),(2006,N''Efectivo''),
           (2,N''Debito''),(1003,N''Debito''),(1007,N''Debito''),(2007,N''Debito''),
           (3,N''Credito''),(1004,N''Credito''),(1008,N''Credito''),(2008,N''Credito''),
           (4,N''Transferencia''),(1005,N''Transferencia''),(1009,N''Transferencia''),(2009,N''Transferencia'');

    DECLARE @MetodosValidos bit = 1;
    IF EXISTS
    (
        SELECT 1
        FROM @Metodos e
        LEFT JOIN dbo.MetodoPago mp ON mp.Id = e.Id
        WHERE mp.Id IS NULL
           OR (e.Categoria = N''Efectivo'' AND LOWER(mp.Nombre) NOT LIKE N''%efectivo%'')
           OR (e.Categoria = N''Debito'' AND mp.Nombre COLLATE Latin1_General_100_CI_AI NOT LIKE N''%debito%'')
           OR (e.Categoria = N''Credito'' AND mp.Nombre COLLATE Latin1_General_100_CI_AI NOT LIKE N''%credito%'')
           OR (e.Categoria = N''Transferencia'' AND LOWER(mp.Nombre) NOT LIKE N''%transfer%''
               AND LOWER(mp.Nombre) NOT LIKE N''%mercado pago%'')
    ) SET @MetodosValidos = 0;

    ;WITH CajasDelPeriodo AS
    (
        SELECT c.Id, c.UsuarioId, c.FechaHoraApertura, c.FechaHoraCierre,
               c.SaldoInicial, c.SaldoFinal, c.Estado,
               LTRIM(RTRIM(CONCAT(u.Nombre,N'' '',u.Apellido))) AS Cajero
        FROM dbo.Caja c
        INNER JOIN dbo.Usuario u ON u.Id = c.UsuarioId
        WHERE c.Estado = N''Cerrada''
          AND c.FechaHoraCierre >= @Inicio
          AND CONVERT(date,c.FechaHoraCierre) <= @FechaHasta
    ), VentasPorCaja AS
    (
        SELECT v.CajaId,
               SUM(CASE WHEN v.FechaHora >= cd.FechaHoraApertura AND v.FechaHora <= cd.FechaHoraCierre
                              AND v.Estado <> N''Anulada''
                        THEN v.Total ELSE CONVERT(decimal(18,2),0) END) AS VentasSistema,
               SUM(CASE WHEN v.FechaHora >= cd.FechaHoraApertura AND v.FechaHora <= cd.FechaHoraCierre
                              AND v.Estado = N''Anulada'' THEN 1 ELSE 0 END) AS CantidadVentasAnuladas,
               MAX(CASE WHEN v.FechaHora IS NULL OR v.FechaHora < cd.FechaHoraApertura
                             OR v.FechaHora > cd.FechaHoraCierre
                             OR v.Estado IS NULL OR v.Estado NOT IN (N''Completada'',N''Anulada'')
                        THEN 1 ELSE 0 END) AS VentaInconsistente
        FROM dbo.Venta v
        INNER JOIN CajasDelPeriodo cd ON cd.Id = v.CajaId
        GROUP BY v.CajaId
    ), PagosPorVenta AS
    (
        SELECT v.CajaId, v.Id AS VentaId, v.Estado, v.Total,
               COALESCE(SUM(CASE WHEN m.Categoria = N''Efectivo'' THEN CONVERT(decimal(38,2),vp.MontoCobrado)
                                 ELSE CONVERT(decimal(38,2),0) END),0) AS EfectivoEntregado,
               COALESCE(SUM(CASE WHEN m.Categoria IN (N''Debito'',N''Credito'',N''Transferencia'')
                                 THEN CONVERT(decimal(38,2),vp.MontoCobrado) ELSE CONVERT(decimal(38,2),0) END),0) AS OtrosMedios,
               COALESCE(SUM(CASE WHEN m.Categoria IN (N''Debito'',N''Credito'')
                                 THEN CONVERT(decimal(38,2),vp.MontoCobrado) ELSE CONVERT(decimal(38,2),0) END),0) AS CobrosTarjeta,
               COALESCE(SUM(CASE WHEN m.Categoria = N''Transferencia'' THEN CONVERT(decimal(38,2),vp.MontoCobrado)
                                 ELSE CONVERT(decimal(38,2),0) END),0) AS CobrosTransferencia,
               COALESCE(SUM(CASE WHEN v.Estado = N''Anulada'' AND m.Categoria = N''Efectivo''
                                 THEN CONVERT(decimal(38,2),vp.MontoCobrado) ELSE CONVERT(decimal(38,2),0) END),0) AS EfectivoAnulado,
               MAX(CASE WHEN vp.VentaId IS NOT NULL AND
                             (m.Id IS NULL OR vp.MontoCobrado IS NULL OR vp.MontoCobrado <= 0)
                        THEN 1 ELSE 0 END) AS PagoDesconocido
        FROM dbo.Venta v
        INNER JOIN CajasDelPeriodo cd ON cd.Id = v.CajaId
        LEFT JOIN dbo.VentaPago vp ON vp.VentaId = v.Id
        LEFT JOIN @Metodos m ON m.Id = vp.MetodoPagoId
        WHERE v.FechaHora >= cd.FechaHoraApertura AND v.FechaHora <= cd.FechaHoraCierre
        GROUP BY v.CajaId,v.Id,v.Estado,v.Total
    ), TotalesPago AS
    (
        SELECT CajaId,
               COALESCE(SUM(CASE WHEN Estado <> N''Anulada'' AND Estado IS NOT NULL
                    THEN CASE WHEN EfectivoEntregado > Total-OtrosMedios THEN Total-OtrosMedios ELSE EfectivoEntregado END
                    ELSE CONVERT(decimal(38,2),0) END),0) AS EfectivoConservado,
               COALESCE(SUM(EfectivoEntregado),0) AS EfectivoEntregado,
               COALESCE(SUM(CobrosTarjeta),0) AS CobrosTarjeta,
               COALESCE(SUM(CobrosTransferencia),0) AS CobrosTransferencia,
               COALESCE(SUM(EfectivoAnulado),0) AS EfectivoAnulado,
               MAX(CASE WHEN Estado IS NULL OR Estado NOT IN (N''Anulada'',N''Completada'')
                     OR PagoDesconocido = 1 OR Total IS NULL OR Total <= 0 OR OtrosMedios > Total
                     OR EfectivoEntregado + OtrosMedios < Total THEN 1 ELSE 0 END) AS PagoInconsistente
        FROM PagosPorVenta GROUP BY CajaId
    ), MovimientosCaja AS
    (
        SELECT CajaId,
               SUM(CASE WHEN TipoMovimiento = N''Ingreso'' THEN CONVERT(decimal(38,2),Importe) ELSE CONVERT(decimal(38,2),0) END) AS IngresosCaja,
               SUM(CASE WHEN TipoMovimiento = N''Retiro'' THEN CONVERT(decimal(38,2),Importe) ELSE CONVERT(decimal(38,2),0) END) AS RetirosCaja,
               MAX(CASE WHEN TipoMovimiento IS NULL OR TipoMovimiento NOT IN (N''Ingreso'',N''Retiro'')
                             OR Importe IS NULL OR Importe <= 0 THEN 1 ELSE 0 END) AS MovimientoInconsistente
        FROM dbo.MovimientoCaja GROUP BY CajaId
    ), Base AS
    (
        SELECT cd.*,
               COALESCE(v.VentasSistema,CONVERT(decimal(18,2),0)) AS VentasSistema,
               COALESCE(v.CantidadVentasAnuladas,0) AS CantidadVentasAnuladas,
               COALESCE(p.EfectivoConservado,0) AS EfectivoConservado,
               COALESCE(p.EfectivoEntregado,0) AS EfectivoEntregado,
               COALESCE(p.CobrosTarjeta,0) AS CobrosTarjeta,
               COALESCE(p.CobrosTransferencia,0) AS CobrosTransferencia,
               COALESCE(p.EfectivoAnulado,0) AS EfectivoAnulado,
               COALESCE(p.PagoInconsistente,0) AS PagoInconsistente,
               COALESCE(m.IngresosCaja,0) AS IngresosCaja,
               COALESCE(m.RetirosCaja,0) AS RetirosCaja,
               COALESCE(m.MovimientoInconsistente,0) AS MovimientoInconsistente,
               COALESCE(v.VentaInconsistente,0) AS VentaInconsistente
        FROM CajasDelPeriodo cd
        LEFT JOIN VentasPorCaja v ON v.CajaId = cd.Id
        LEFT JOIN TotalesPago p ON p.CajaId = cd.Id
        LEFT JOIN MovimientosCaja m ON m.CajaId = cd.Id
    ), Calculado AS
    (
        SELECT b.*,
               CASE WHEN @MetodosValidos = 0 OR b.EfectivoAnulado > 0 OR b.PagoInconsistente = 1
                          OR b.MovimientoInconsistente = 1 OR b.VentaInconsistente = 1
                          OR b.SaldoInicial IS NULL OR b.SaldoInicial < 0
                          OR TRY_CONVERT(decimal(18,2),b.EfectivoConservado) IS NULL
                          OR TRY_CONVERT(decimal(18,2),b.EfectivoConservado) < 0
                          OR TRY_CONVERT(decimal(18,2),b.IngresosCaja) IS NULL
                          OR TRY_CONVERT(decimal(18,2),b.RetirosCaja) IS NULL
                          OR CONVERT(decimal(38,2),b.SaldoInicial)+b.EfectivoConservado+b.IngresosCaja-b.RetirosCaja < 0
                     THEN CONVERT(decimal(18,2),NULL)
                     ELSE TRY_CONVERT(decimal(18,2),CONVERT(decimal(38,2),b.SaldoInicial)+b.EfectivoConservado+b.IngresosCaja-b.RetirosCaja)
                END AS MontoEsperado,
               CASE WHEN @MetodosValidos = 0 OR b.EfectivoAnulado > 0 OR b.PagoInconsistente = 1
                          OR b.MovimientoInconsistente = 1 OR b.VentaInconsistente = 1
                          OR b.SaldoInicial IS NULL OR b.SaldoInicial < 0
                          OR TRY_CONVERT(decimal(18,2),b.EfectivoConservado) IS NULL
                          OR TRY_CONVERT(decimal(18,2),b.EfectivoConservado) < 0
                          OR TRY_CONVERT(decimal(18,2),b.IngresosCaja) IS NULL
                          OR TRY_CONVERT(decimal(18,2),b.RetirosCaja) IS NULL
                          OR TRY_CONVERT(decimal(18,2),CONVERT(decimal(38,2),b.SaldoInicial)+b.EfectivoConservado+b.IngresosCaja-b.RetirosCaja) IS NULL
                          OR CONVERT(decimal(38,2),b.SaldoInicial)+b.EfectivoConservado+b.IngresosCaja-b.RetirosCaja < 0
                     THEN CONVERT(bit,1) ELSE CONVERT(bit,0) END AS CajaBloqueadaCierre
        FROM Base b
    )
    SELECT e.Id AS CajaId,
           e.Cajero,
           e.FechaHoraApertura AS Apertura,
           e.FechaHoraCierre AS Cierre,
           e.SaldoInicial AS FondoCaja,
           e.VentasSistema,
           e.SaldoFinal AS EfectivoDeclarado,
           e.MontoEsperado,
           CASE WHEN e.CajaBloqueadaCierre = 1 THEN CONVERT(decimal(18,2),NULL)
                ELSE e.SaldoFinal-e.MontoEsperado END AS Diferencia,
           e.Estado,
           TRY_CONVERT(decimal(18,2),e.EfectivoConservado) AS CobrosEfectivo,
           TRY_CONVERT(decimal(18,2),e.CobrosTarjeta) AS CobrosTarjeta,
           TRY_CONVERT(decimal(18,2),e.CobrosTransferencia) AS CobrosTransferencia,
           TRY_CONVERT(decimal(18,2),e.EfectivoAnulado) AS EfectivoAnuladoPendienteVerificar,
           CASE WHEN @MetodosValidos = 0 THEN N''Revisar clasificación de medios de pago''
                WHEN e.EfectivoAnulado > 0 THEN N''Bloqueado: efectivo de ventas anuladas sin devolución verificada''
                WHEN e.PagoInconsistente = 1 THEN N''Bloqueado: pagos desconocidos o que no concilian''
                WHEN e.MovimientoInconsistente = 1 THEN N''Bloqueado: movimientos de efectivo inválidos''
                WHEN e.VentaInconsistente = 1 THEN N''Bloqueado: ventas fuera del turno o con estado no válido''
                WHEN e.SaldoInicial IS NULL OR e.SaldoInicial < 0 THEN N''Bloqueado: fondo inicial inválido''
                WHEN TRY_CONVERT(decimal(18,2),e.EfectivoConservado) < 0 THEN N''Bloqueado: cobros conservados negativos''
                WHEN CONVERT(decimal(38,2),e.SaldoInicial)+e.EfectivoConservado+e.IngresosCaja-e.RetirosCaja < 0
                    THEN N''Bloqueado: efectivo esperado negativo''
                WHEN e.MontoEsperado IS NULL THEN N''Bloqueado: efectivo esperado fuera del rango admitido''
                ELSE N''Cierre histórico calculado con pagos y movimientos registrados'' END AS ObservacionEfectivo,
           e.CantidadVentasAnuladas,
           TRY_CONVERT(decimal(18,2),e.EfectivoEntregado) AS EfectivoEntregado,
           TRY_CONVERT(decimal(18,2),e.IngresosCaja) AS IngresosCaja,
           TRY_CONVERT(decimal(18,2),e.RetirosCaja) AS RetirosCaja,
           e.CajaBloqueadaCierre
    FROM Calculado e
    ORDER BY e.FechaHoraCierre DESC, e.Id DESC;
END;';
PRINT N'Historial de cierres instalado en la base DatabaseName configurada.';
