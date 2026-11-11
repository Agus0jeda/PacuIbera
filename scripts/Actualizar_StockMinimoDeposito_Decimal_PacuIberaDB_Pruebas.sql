/* Solo prepara la definicion del procedimiento en la base de pruebas.
   No modifica tablas ni valores existentes. Aplicar antes de guardar desde la nueva UI.
   No volver a aplicar definiciones antiguas con decimal(18,2).
   Los valores con mas de tres decimales deben rechazarse en el cliente ANTES del envio:
   SQL Server convierte el argumento al tipo del parametro antes de entrar al procedimiento.
*/
USE PacuIberaDB_Pruebas;

IF DB_NAME() <> N'PacuIberaDB_Pruebas'
    THROW 51500, 'Este script solo puede ejecutarse en PacuIberaDB_Pruebas.', 1;
IF OBJECT_ID(N'dbo.Producto', N'U') IS NULL
    THROW 51501, 'Falta dbo.Producto.', 1;
IF OBJECT_ID(N'dbo.DepositoStockMinimo', N'U') IS NULL
    THROW 51502, 'Falta dbo.DepositoStockMinimo.', 1;
IF COL_LENGTH(N'dbo.Producto', N'SeVendePorPeso') IS NULL
    THROW 51503, 'Falta Producto.SeVendePorPeso.', 1;

EXEC(N'
CREATE OR ALTER PROCEDURE dbo.sp_ActualizarStockMinimoDeposito
    @ProductoId int,
    @StockMinimo decimal(10,3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @TransaccionesEntrada int = @@TRANCOUNT;
    DECLARE @PorPeso bit;
    BEGIN TRY
        IF @TransaccionesEntrada = 0 BEGIN TRANSACTION;
        ELSE SAVE TRANSACTION StockMinimoSeguro;

        IF @StockMinimo IS NULL OR @StockMinimo < 0 OR @StockMinimo > 9999999.999
            THROW 51130, ''El minimo debe estar entre 0 y 9999999.999.'', 1;

        SELECT @PorPeso = SeVendePorPeso FROM dbo.Producto WITH (HOLDLOCK)
        WHERE Id = @ProductoId;
        IF @PorPeso IS NULL
            THROW 51131, ''El producto no existe o no tiene definida su unidad de venta.'', 1;
        IF @PorPeso = 0 AND @StockMinimo <> FLOOR(@StockMinimo)
            THROW 51510, ''Los productos por unidad requieren un minimo entero.'', 1;

        IF EXISTS (SELECT 1 FROM dbo.DepositoStockMinimo WITH (UPDLOCK, HOLDLOCK)
                   WHERE ProductoId = @ProductoId)
            UPDATE dbo.DepositoStockMinimo SET StockMinimo = @StockMinimo WHERE ProductoId = @ProductoId;
        ELSE
            INSERT INTO dbo.DepositoStockMinimo (ProductoId, StockMinimo) VALUES (@ProductoId, @StockMinimo);

        IF @TransaccionesEntrada = 0 COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @TransaccionesEntrada = 0
        BEGIN
            IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        END
        ELSE IF XACT_STATE() = 1 ROLLBACK TRANSACTION StockMinimoSeguro;
        THROW;
    END CATCH;
END;');
