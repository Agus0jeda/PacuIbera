/*
  Actualiza el reporte de mercaderia vencida para la instalacion configurable.
  Requiere SQLCMD Mode y el mismo DatabaseName del resto del instalador.
  No modifica lotes, productos, existencias ni movimientos.
*/
:on error exit
:setvar DatabaseName "PacuIbera_Entrega"
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @AutorizarInstalacion bit = 0;

IF N'$(DatabaseName)' LIKE N'%[^A-Za-z0-9_]%' OR N'$(DatabaseName)' IN
   (N'master',N'model',N'msdb',N'tempdb',N'PacuIberaDB',N'PacuIberaDB_Pruebas')
    THROW 54801, 'DatabaseName debe ser una base de entrega permitida.', 1;
IF DB_NAME() <> N'$(DatabaseName)'
    THROW 54802, 'Conectese a DatabaseName antes de actualizar el analisis de vencimientos.', 1;
IF OBJECT_ID(N'dbo.sp_AnalisisPerdidaVencimientos',N'P') IS NULL
    THROW 54803, 'No existe dbo.sp_AnalisisPerdidaVencimientos; instale primero el esquema.', 1;
IF OBJECT_ID(N'dbo.Lote',N'U') IS NULL OR OBJECT_ID(N'dbo.Producto',N'U') IS NULL
    THROW 54804, 'Faltan las tablas requeridas por el reporte.', 1;
IF COL_LENGTH(N'dbo.Lote',N'Id') IS NULL OR COL_LENGTH(N'dbo.Lote',N'ProductoId') IS NULL
   OR COL_LENGTH(N'dbo.Lote',N'FechaVencimiento') IS NULL
   OR COL_LENGTH(N'dbo.Lote',N'StockActual') IS NULL OR COL_LENGTH(N'dbo.Lote',N'Activo') IS NULL
   OR COL_LENGTH(N'dbo.Producto',N'Id') IS NULL OR COL_LENGTH(N'dbo.Producto',N'Nombre') IS NULL
   OR COL_LENGTH(N'dbo.Producto',N'PrecioVenta') IS NULL
   OR COL_LENGTH(N'dbo.Producto',N'SeVendePorPeso') IS NULL
    THROW 54805, 'El esquema de Producto o Lote no contiene las columnas requeridas.', 1;
IF @AutorizarInstalacion <> 1
    THROW 54806, 'Instalacion bloqueada: revise el destino y autorice manualmente el script.', 1;

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRY
    BEGIN TRANSACTION;

    EXEC sys.sp_executesql N'
CREATE OR ALTER PROCEDURE dbo.sp_AnalisisPerdidaVencimientos
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.Nombre AS Producto,
        l.Id AS Lote,
        l.FechaVencimiento,
        l.StockActual AS [Cantidad vencida],
        CONVERT(bit,p.SeVendePorPeso) AS SeVendePorPeso,
        (l.StockActual*p.PrecioVenta) AS [Valor estimado]
    FROM dbo.Lote l
    INNER JOIN dbo.Producto p ON p.Id=l.ProductoId
    WHERE l.Activo=1
      AND l.FechaVencimiento<CONVERT(date,GETDATE())
      AND l.StockActual>0
    ORDER BY [Valor estimado] DESC,p.Nombre,l.Id;
END;';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

PRINT N'El reporte de mercaderia vencida fue actualizado; no se modificaron datos de inventario.';
