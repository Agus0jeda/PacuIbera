
/*
    Actualiza únicamente dbo.sp_RegistrarDetalleVentaFIFO en PacuIberaDB_Pruebas.
    No ejecutar en producción. Compatible con VentaDatos, que invoca este SP
    dentro de una transacción externa que también contiene venta y pagos.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
BEGIN TRANSACTION;

IF DB_NAME() <> N'PacuIberaDB_Pruebas'
    THROW 52400, 'Este script solo se puede ejecutar en PacuIberaDB_Pruebas.', 1;

IF OBJECT_ID(N'dbo.Lote', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Producto', N'U') IS NULL
   OR OBJECT_ID(N'dbo.DetalleVenta', N'U') IS NULL
    THROW 52401, 'Faltan tablas requeridas por dbo.sp_RegistrarDetalleVentaFIFO.', 1;

IF OBJECT_ID(N'dbo.sp_RegistrarDetalleVentaFIFO', N'P') IS NULL
    THROW 52402, 'No existe dbo.sp_RegistrarDetalleVentaFIFO.', 1;

IF COL_LENGTH(N'dbo.Lote', N'ProductoId') IS NULL
   OR COL_LENGTH(N'dbo.Lote', N'FechaVencimiento') IS NULL
   OR COL_LENGTH(N'dbo.Lote', N'StockActual') IS NULL
   OR COL_LENGTH(N'dbo.Lote', N'Activo') IS NULL
   OR COL_LENGTH(N'dbo.Producto', N'StockActual') IS NULL
    THROW 52403, 'El esquema de lotes o productos no coincide con el esperado.', 1;

IF EXISTS
(
    SELECT 1
    FROM (VALUES
        (N'Cantidad', N'decimal', 10, 3),
        (N'PrecioUnitario', N'decimal', 10, 2),
        (N'Subtotal', N'decimal', 10, 2)
    ) e(Nombre, Tipo, PrecisionEsperada, EscalaEsperada)
    LEFT JOIN
    (
        SELECT c.name Nombre, TYPE_NAME(c.user_type_id) Tipo,
               c.precision PrecisionEsperada, c.scale EscalaEsperada
        FROM sys.columns c
        WHERE c.object_id = OBJECT_ID(N'dbo.DetalleVenta')
    ) a ON a.Nombre = e.Nombre
    WHERE a.Nombre IS NULL
       OR e.Tipo <> a.Tipo
       OR e.PrecisionEsperada <> a.PrecisionEsperada
       OR e.EscalaEsperada <> a.EscalaEsperada
)
    THROW 52405, 'Los tipos de DetalleVenta cambiaron; no se aplicó la actualización.', 1;

IF EXISTS
(
    SELECT 1
    FROM (VALUES
        (N'@VentaId', N'int', 10, 0),
        (N'@ProductoId', N'int', 10, 0),
        (N'@CantidadRequerida', N'decimal', 18, 3),
        (N'@PrecioUnitario', N'decimal', 18, 2)
    ) e(Nombre, Tipo, PrecisionEsperada, EscalaEsperada)
    FULL JOIN
    (
        SELECT p.name Nombre, TYPE_NAME(p.user_type_id) Tipo,
               p.precision PrecisionEsperada, p.scale EscalaEsperada
        FROM sys.parameters p
        WHERE p.object_id = OBJECT_ID(N'dbo.sp_RegistrarDetalleVentaFIFO')
          AND p.parameter_id > 0
    ) a ON a.Nombre = e.Nombre
    WHERE e.Nombre IS NULL
       OR a.Nombre IS NULL
       OR e.Tipo <> a.Tipo
       OR e.PrecisionEsperada <> a.PrecisionEsperada
       OR (e.Nombre = N'@CantidadRequerida'
           AND a.EscalaEsperada NOT IN (2, 3))
       OR (e.Nombre <> N'@CantidadRequerida'
           AND e.EscalaEsperada <> a.EscalaEsperada)
)
    THROW 52404, 'La firma instalada del procedimiento cambió; no se aplicó la actualización.', 1;

EXEC sys.sp_executesql N'
CREATE OR ALTER PROCEDURE dbo.sp_RegistrarDetalleVentaFIFO
    @VentaId INT,
    @ProductoId INT,
    @CantidadRequerida DECIMAL(18,3),
    @PrecioUnitario DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @VentaId IS NULL OR @ProductoId IS NULL
       OR @CantidadRequerida IS NULL OR @CantidadRequerida <= 0
       OR @PrecioUnitario IS NULL OR @PrecioUnitario < 0
        THROW 52410, ''La venta, el producto y las cantidades deben ser válidos.'', 1;

    DECLARE @FechaHoy DATE = CONVERT(date, SYSDATETIME());
    DECLARE @StockUtilizable DECIMAL(38,3);
    DECLARE @CantidadRestante DECIMAL(18,3) = @CantidadRequerida;
    DECLARE @LoteId INT, @StockLote DECIMAL(10,3);
    DECLARE @Tomar DECIMAL(10,3);

    IF @@TRANCOUNT = 0
        THROW 52415, ''La venta debe ejecutarse dentro de su transacción de aplicación.'', 1;

    /*
      La aplicación ya tiene una transacción abierta. Estos bloqueos se conservan
      hasta que ella confirme o revierta venta, pagos y todos los detalles.
    */
    SELECT @StockUtilizable =
        COALESCE(SUM(CONVERT(DECIMAL(38,3), l.StockActual)), 0)
    FROM dbo.Lote l WITH (UPDLOCK, HOLDLOCK)
    WHERE l.ProductoId = @ProductoId
      AND l.Activo = 1
      AND l.StockActual > 0
      AND (
          l.FechaVencimiento IS NULL
          OR CONVERT(date, l.FechaVencimiento) =
             CONVERT(date, ''20991231'', 112)
          OR l.FechaVencimiento >= @FechaHoy
      );

    IF @StockUtilizable < @CantidadRequerida
        THROW 52411, ''No hay suficiente mercadería vigente disponible para completar la venta.'', 1;

    WHILE @CantidadRestante > 0
    BEGIN
        SET @LoteId = NULL;
        SET @StockLote = NULL;

        SELECT TOP (1)
            @LoteId = l.Id,
            @StockLote = l.StockActual
        FROM dbo.Lote l WITH (UPDLOCK, HOLDLOCK)
        WHERE l.ProductoId = @ProductoId
          AND l.Activo = 1
          AND l.StockActual > 0
          AND (
              l.FechaVencimiento IS NULL
              OR CONVERT(date, l.FechaVencimiento) =
                 CONVERT(date, ''20991231'', 112)
              OR l.FechaVencimiento >= @FechaHoy
          )
        ORDER BY
            CASE
                WHEN l.FechaVencimiento IS NULL
                  OR CONVERT(date, l.FechaVencimiento) =
                     CONVERT(date, ''20991231'', 112)
                THEN 1
                ELSE 0
            END,
            CASE
                WHEN CONVERT(date, l.FechaVencimiento) =
                     CONVERT(date, ''20991231'', 112)
                THEN NULL
                ELSE l.FechaVencimiento
            END,
            l.Id;

        IF @LoteId IS NULL OR @StockLote IS NULL
            THROW 52412, ''No se pudo reservar un lote vigente para completar la venta.'', 1;

        SET @Tomar =
            CASE
                WHEN @StockLote >= @CantidadRestante
                THEN @CantidadRestante
                ELSE @StockLote
            END;

        UPDATE dbo.Lote
        SET StockActual = StockActual - @Tomar,
            Activo =
                CASE
                    WHEN StockActual - @Tomar = 0
                    THEN 0
                    ELSE Activo
                END
        WHERE Id = @LoteId
          AND Activo = 1
          AND StockActual >= @Tomar
          AND (
              FechaVencimiento IS NULL
              OR CONVERT(date, FechaVencimiento) =
                 CONVERT(date, ''20991231'', 112)
              OR FechaVencimiento >= @FechaHoy
          );

        IF @@ROWCOUNT <> 1
            THROW 52413, ''El lote cambió mientras se procesaba la venta. Intente nuevamente.'', 1;

        INSERT dbo.DetalleVenta
            (VentaId, ProductoId, LoteId, Cantidad, PrecioUnitario, Subtotal)
        VALUES
            (@VentaId, @ProductoId, @LoteId, @Tomar,
             @PrecioUnitario, @Tomar * @PrecioUnitario);

        SET @CantidadRestante -= @Tomar;
    END;

    /*
      Producto.StockActual representa el saldo utilizable del local.
      Recalcular con los mismos criterios evita contar lotes vencidos.
    */
    UPDATE p
    SET StockActual =
        (
            SELECT COALESCE(
                SUM(CONVERT(DECIMAL(10,3), l.StockActual)), 0
            )
            FROM dbo.Lote l WITH (UPDLOCK, HOLDLOCK)
            WHERE l.ProductoId = @ProductoId
              AND l.Activo = 1
              AND l.StockActual > 0
              AND (
                  l.FechaVencimiento IS NULL
                  OR CONVERT(date, l.FechaVencimiento) =
                     CONVERT(date, ''20991231'', 112)
                  OR l.FechaVencimiento >= @FechaHoy
              )
        )
    FROM dbo.Producto p WITH (UPDLOCK, HOLDLOCK)
    WHERE p.Id = @ProductoId;

    IF @@ROWCOUNT <> 1
        THROW 52414, ''No se encontró el producto para actualizar su saldo disponible.'', 1;
END;
';

COMMIT TRANSACTION;

SELECT
    DB_NAME() AS BaseActual,
    N'Procedimiento FIFO instalado y verificado' AS Resultado;

END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
