/*
    Top 5 por identidad de producto, con unidad de venta.
    Preparado, NO ejecutado. No modifica datos de negocio.
    Revisar la definicion propuesta y cambiar @AutorizarInstalacion a 1
    para instalar exclusivamente en PacuIberaDB_Pruebas.
    Ejecutar como un solo lote, sin agregar GO entre las guardas y el ALTER.
*/
SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF DB_NAME() <> N'PacuIberaDB_Pruebas'
    THROW 53300, 'Este script solo puede instalarse en PacuIberaDB_Pruebas.', 1;

IF @@TRANCOUNT <> 0
    THROW 53301, 'Ejecute sin una transaccion externa abierta.', 1;

DECLARE @AutorizarInstalacion bit = 0;

IF OBJECT_ID(N'dbo.sp_ReporteTopProductos', N'P') IS NULL
   OR OBJECT_ID(N'dbo.Venta', N'U') IS NULL
   OR OBJECT_ID(N'dbo.DetalleVenta', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Producto', N'U') IS NULL
    THROW 53302, 'Falta el procedimiento o alguna de sus tablas.', 1;

IF COL_LENGTH(N'dbo.Producto', N'Id') IS NULL
   OR COL_LENGTH(N'dbo.Producto', N'Nombre') IS NULL
   OR COL_LENGTH(N'dbo.Producto', N'SeVendePorPeso') IS NULL
   OR COL_LENGTH(N'dbo.DetalleVenta', N'VentaId') IS NULL
   OR COL_LENGTH(N'dbo.DetalleVenta', N'ProductoId') IS NULL
   OR COL_LENGTH(N'dbo.DetalleVenta', N'Cantidad') IS NULL
   OR COL_LENGTH(N'dbo.Venta', N'Id') IS NULL
   OR COL_LENGTH(N'dbo.Venta', N'Estado') IS NULL
   OR COL_LENGTH(N'dbo.Venta', N'FechaHora') IS NULL
    THROW 53303, 'El esquema no contiene las columnas requeridas.', 1;

DECLARE @DefinicionActual nvarchar(max) = OBJECT_DEFINITION(OBJECT_ID(N'dbo.sp_ReporteTopProductos'));
IF @DefinicionActual IS NULL
    THROW 53304, 'No se puede leer la definicion actual; no se reemplazo.', 1;

DECLARE @DefinicionNueva nvarchar(max) = N'ALTER PROCEDURE [dbo].[sp_ReporteTopProductos]
    @FechaDesde DATE,
    @FechaHasta DATE
AS BEGIN
    SET NOCOUNT ON;
    SELECT TOP (5)
        P.Id AS ProductoId,
        P.Nombre,
        P.SeVendePorPeso,
        SUM(D.Cantidad) AS TotalVendido
    FROM dbo.DetalleVenta D
    INNER JOIN dbo.Producto P ON D.ProductoId = P.Id
    INNER JOIN dbo.Venta V ON D.VentaId = V.Id
    WHERE V.Estado != ''Anulada''
      AND CAST(V.FechaHora AS DATE) >= @FechaDesde
      AND CAST(V.FechaHora AS DATE) <= @FechaHasta
    GROUP BY P.Id, P.Nombre, P.SeVendePorPeso
    ORDER BY TotalVendido DESC, P.Id;
END;';

SELECT DB_NAME() AS BaseActual, @DefinicionActual AS DefinicionActual,
       @DefinicionNueva AS DefinicionPropuesta;

IF @AutorizarInstalacion <> 1
BEGIN
    PRINT 'Modo revision: no se modifico ningun procedimiento ni dato.';
    RETURN;
END;

SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;
    EXEC sys.sp_executesql @DefinicionNueva;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
