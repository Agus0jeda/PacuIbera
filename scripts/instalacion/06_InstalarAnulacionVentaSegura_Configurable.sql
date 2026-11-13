/*
    Instalacion configurable de la etapa 1 de anulacion segura de ventas.
    Requiere revisar la salida de auditorias duplicadas antes de autorizar la instalacion.
    No modifica VentaPago ni Caja. No ejecuta anulaciones.
    La aplicacion envia una decision por cada DetalleVenta/Lote original del FIFO.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @AutorizarInstalacion bit = 0;

IF N'$(DatabaseName)' LIKE N'%[^A-Za-z0-9_]%' OR N'$(DatabaseName)' IN
   (N'master',N'model',N'msdb',N'tempdb',N'PacuIberaDB',N'PacuIberaDB_Pruebas')
    THROW 54901, 'DatabaseName debe ser una base de entrega PacuIbera permitida.', 1;
IF DB_NAME() <> N'$(DatabaseName)'
    THROW 54926, 'Conectese a DatabaseName antes de instalar la anulacion segura.', 1;

IF OBJECT_ID(N'dbo.Venta',N'U') IS NULL OR OBJECT_ID(N'dbo.VentaAnulacion',N'U') IS NULL
   OR OBJECT_ID(N'dbo.DetalleVenta',N'U') IS NULL OR OBJECT_ID(N'dbo.Lote',N'U') IS NULL
   OR OBJECT_ID(N'dbo.Producto',N'U') IS NULL OR OBJECT_ID(N'dbo.Usuario',N'U') IS NULL
   OR OBJECT_ID(N'dbo.Rol',N'U') IS NULL
    THROW 54902, 'Faltan tablas requeridas para la anulacion segura.', 1;

IF COL_LENGTH(N'dbo.Venta',N'Estado') IS NULL OR COL_LENGTH(N'dbo.Venta',N'Id') IS NULL
   OR COL_LENGTH(N'dbo.VentaAnulacion',N'VentaId') IS NULL
   OR COL_LENGTH(N'dbo.VentaAnulacion',N'UsuarioId') IS NULL
   OR COL_LENGTH(N'dbo.VentaAnulacion',N'Motivo') IS NULL
   OR COL_LENGTH(N'dbo.DetalleVenta',N'LoteId') IS NULL
   OR COL_LENGTH(N'dbo.DetalleVenta',N'Cantidad') IS NULL
   OR COL_LENGTH(N'dbo.Lote',N'FechaVencimiento') IS NULL
   OR COL_LENGTH(N'dbo.Lote',N'StockActual') IS NULL
   OR COL_LENGTH(N'dbo.Lote',N'Activo') IS NULL
   OR COL_LENGTH(N'dbo.Producto',N'SeVendePorPeso') IS NULL
    THROW 54903, 'El esquema no coincide con las columnas esperadas por el circuito FIFO.', 1;

-- Inspeccion de auditorias repetidas heredadas. Este script no las borra ni las corrige.
SELECT VentaId, COUNT_BIG(*) AS CantidadAuditorias
FROM dbo.VentaAnulacion
GROUP BY VentaId
HAVING COUNT_BIG(*) > 1
ORDER BY VentaId;

-- No se instala una restriccion unica sobre VentaAnulacion.VentaId: primero deben revisarse
-- las filas anteriores. La nueva ruta se serializa bloqueando Venta y comprueba auditorias.
IF @AutorizarInstalacion <> 1
BEGIN
    PRINT N'Modo revision: no se crearon objetos ni se modifico ningun procedimiento.';
    RETURN;
END;

IF COL_LENGTH(N'dbo.DetalleVenta',N'Id') IS NULL OR COL_LENGTH(N'dbo.DetalleVenta',N'VentaId') IS NULL
   OR COL_LENGTH(N'dbo.DetalleVenta',N'ProductoId') IS NULL
   OR COL_LENGTH(N'dbo.Lote',N'Id') IS NULL OR COL_LENGTH(N'dbo.Lote',N'ProductoId') IS NULL
   OR COL_LENGTH(N'dbo.Lote',N'FechaVencimiento') IS NULL
   OR COL_LENGTH(N'dbo.Lote',N'StockActual') IS NULL OR COL_LENGTH(N'dbo.Lote',N'Activo') IS NULL
   OR COL_LENGTH(N'dbo.Producto',N'Id') IS NULL OR COL_LENGTH(N'dbo.Producto',N'StockActual') IS NULL
   OR COL_LENGTH(N'dbo.Usuario',N'Id') IS NULL OR COL_LENGTH(N'dbo.Usuario',N'Activo') IS NULL
   OR COL_LENGTH(N'dbo.Usuario',N'RolId') IS NULL OR COL_LENGTH(N'dbo.Rol',N'Id') IS NULL
   OR COL_LENGTH(N'dbo.Rol',N'Nombre') IS NULL
    THROW 54904, 'Faltan columnas requeridas para validar identidad, lote o cantidades.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    IF TYPE_ID(N'dbo.VentaAnulacionDetalleEntrada') IS NULL
    BEGIN
        EXEC sys.sp_executesql N'CREATE TYPE dbo.VentaAnulacionDetalleEntrada AS TABLE
        (
            DetalleVentaId int NOT NULL PRIMARY KEY,
            VolvioFisicamente bit NOT NULL,
            AptaParaVenta bit NOT NULL
        );';
    END
    ELSE
    BEGIN
        DECLARE @TipoEntradaObjectId int =
            (SELECT type_table_object_id FROM sys.table_types
             WHERE user_type_id = TYPE_ID(N'dbo.VentaAnulacionDetalleEntrada'));

        IF @TipoEntradaObjectId IS NULL
           OR (SELECT COUNT(*) FROM sys.columns WHERE object_id = @TipoEntradaObjectId) <> 3
           OR NOT EXISTS (SELECT 1 FROM sys.columns c JOIN sys.types t ON t.user_type_id=c.user_type_id
                          WHERE c.object_id=@TipoEntradaObjectId AND c.column_id=1
                            AND c.name=N'DetalleVentaId' AND t.name=N'int' AND c.is_nullable=0)
           OR NOT EXISTS (SELECT 1 FROM sys.columns c JOIN sys.types t ON t.user_type_id=c.user_type_id
                          WHERE c.object_id=@TipoEntradaObjectId AND c.column_id=2
                            AND c.name=N'VolvioFisicamente' AND t.name=N'bit' AND c.is_nullable=0)
           OR NOT EXISTS (SELECT 1 FROM sys.columns c JOIN sys.types t ON t.user_type_id=c.user_type_id
                          WHERE c.object_id=@TipoEntradaObjectId AND c.column_id=3
                            AND c.name=N'AptaParaVenta' AND t.name=N'bit' AND c.is_nullable=0)
           OR NOT EXISTS (SELECT 1 FROM sys.indexes i
                          INNER JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
                          INNER JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
                          WHERE i.object_id=@TipoEntradaObjectId AND i.is_primary_key=1
                            AND ic.key_ordinal=1 AND c.name=N'DetalleVentaId')
            THROW 54905, 'El tipo dbo.VentaAnulacionDetalleEntrada ya existe con una definicion incompatible; no se reemplazo.', 1;
    END;

    IF OBJECT_ID(N'dbo.VentaAnulacionDetalle',N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.VentaAnulacionDetalle
        (
            Id int IDENTITY(1,1) NOT NULL,
            VentaAnulacionId int NOT NULL,
            DetalleVentaId int NOT NULL,
            ProductoId int NOT NULL,
            LoteId int NOT NULL,
            CantidadVendida decimal(10,3) NOT NULL,
            VolvioFisicamente bit NOT NULL,
            AptaParaVenta bit NOT NULL,
            CantidadReincorporada decimal(10,3) NOT NULL,
            Resultado nvarchar(50) NOT NULL,
            CONSTRAINT PK_VentaAnulacionDetalle PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT UQ_VentaAnulacionDetalle_DetalleVenta UNIQUE (DetalleVentaId),
            CONSTRAINT FK_VentaAnulacionDetalle_Anulacion FOREIGN KEY (VentaAnulacionId) REFERENCES dbo.VentaAnulacion(Id),
            CONSTRAINT FK_VentaAnulacionDetalle_Detalle FOREIGN KEY (DetalleVentaId) REFERENCES dbo.DetalleVenta(Id),
            CONSTRAINT FK_VentaAnulacionDetalle_Producto FOREIGN KEY (ProductoId) REFERENCES dbo.Producto(Id),
            CONSTRAINT FK_VentaAnulacionDetalle_Lote FOREIGN KEY (LoteId) REFERENCES dbo.Lote(Id),
            CONSTRAINT CK_VentaAnulacionDetalle_Cantidades CHECK
                (CantidadVendida > 0 AND CantidadReincorporada >= 0 AND CantidadReincorporada <= CantidadVendida),
            CONSTRAINT CK_VentaAnulacionDetalle_Resultado CHECK
                (Resultado IN (N'No devuelta',N'Devuelta no apta',N'Devuelta, lote vencido',N'Devuelta, lote inactivo',N'Reincorporada')),
            CONSTRAINT CK_VentaAnulacionDetalle_Apta CHECK (AptaParaVenta=0 OR VolvioFisicamente=1)
        );
    END
    ELSE IF COL_LENGTH(N'dbo.VentaAnulacionDetalle',N'VentaAnulacionId') IS NULL
         OR COL_LENGTH(N'dbo.VentaAnulacionDetalle',N'DetalleVentaId') IS NULL
         OR COL_LENGTH(N'dbo.VentaAnulacionDetalle',N'ProductoId') IS NULL
         OR COL_LENGTH(N'dbo.VentaAnulacionDetalle',N'LoteId') IS NULL
         OR COL_LENGTH(N'dbo.VentaAnulacionDetalle',N'CantidadVendida') IS NULL
         OR COL_LENGTH(N'dbo.VentaAnulacionDetalle',N'VolvioFisicamente') IS NULL
         OR COL_LENGTH(N'dbo.VentaAnulacionDetalle',N'AptaParaVenta') IS NULL
         OR COL_LENGTH(N'dbo.VentaAnulacionDetalle',N'CantidadReincorporada') IS NULL
         OR COL_LENGTH(N'dbo.VentaAnulacionDetalle',N'Resultado') IS NULL
         OR NOT EXISTS (SELECT 1 FROM sys.key_constraints
                        WHERE parent_object_id=OBJECT_ID(N'dbo.VentaAnulacionDetalle')
                          AND name=N'UQ_VentaAnulacionDetalle_DetalleVenta' AND type=N'UQ')
         OR NOT EXISTS (SELECT 1 FROM sys.foreign_keys
                        WHERE parent_object_id=OBJECT_ID(N'dbo.VentaAnulacionDetalle')
                          AND name=N'FK_VentaAnulacionDetalle_Anulacion')
         OR NOT EXISTS (SELECT 1 FROM sys.foreign_keys
                        WHERE parent_object_id=OBJECT_ID(N'dbo.VentaAnulacionDetalle')
                          AND name=N'FK_VentaAnulacionDetalle_Detalle')
         OR NOT EXISTS (SELECT 1 FROM sys.foreign_keys
                        WHERE parent_object_id=OBJECT_ID(N'dbo.VentaAnulacionDetalle')
                          AND name=N'FK_VentaAnulacionDetalle_Producto')
         OR NOT EXISTS (SELECT 1 FROM sys.foreign_keys
                        WHERE parent_object_id=OBJECT_ID(N'dbo.VentaAnulacionDetalle')
                          AND name=N'FK_VentaAnulacionDetalle_Lote')
         OR NOT EXISTS (SELECT 1 FROM sys.check_constraints
                        WHERE parent_object_id=OBJECT_ID(N'dbo.VentaAnulacionDetalle')
                          AND name=N'CK_VentaAnulacionDetalle_Cantidades')
         OR NOT EXISTS (SELECT 1 FROM sys.check_constraints
                        WHERE parent_object_id=OBJECT_ID(N'dbo.VentaAnulacionDetalle')
                          AND name=N'CK_VentaAnulacionDetalle_Resultado')
         OR NOT EXISTS (SELECT 1 FROM sys.check_constraints
                        WHERE parent_object_id=OBJECT_ID(N'dbo.VentaAnulacionDetalle')
                          AND name=N'CK_VentaAnulacionDetalle_Apta')
        THROW 54906, 'dbo.VentaAnulacionDetalle ya existe pero su esquema no coincide; no se modifico.', 1;

    EXEC sys.sp_executesql N'
CREATE OR ALTER PROCEDURE dbo.sp_AnularVentaConMotivo
    @VentaId int,
    @UsuarioId int,
    @Motivo nvarchar(255),
    @Detalles dbo.VentaAnulacionDetalleEntrada READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @@TRANCOUNT <> 0
        THROW 54910, ''La anulacion debe ejecutarse sin una transaccion externa.'', 1;
    IF @VentaId IS NULL OR @VentaId <= 0 OR @UsuarioId IS NULL OR @UsuarioId <= 0
        THROW 54911, ''Seleccione una venta y un usuario validos.'', 1;
    IF @Motivo IS NULL OR LEN(LTRIM(RTRIM(@Motivo))) = 0
        THROW 54912, ''El motivo de anulacion es obligatorio.'', 1;
    IF EXISTS (SELECT 1 FROM @Detalles WHERE AptaParaVenta=1 AND VolvioFisicamente=0)
        THROW 54913, ''Un detalle solo puede marcarse apto si volvio fisicamente.'', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS
        (
            SELECT 1 FROM dbo.Usuario u WITH (HOLDLOCK)
            INNER JOIN dbo.Rol r WITH (HOLDLOCK) ON r.Id=u.RolId
            WHERE u.Id=@UsuarioId AND u.Activo=1 AND r.Nombre=N''Administrador''
        ) THROW 54914, ''Solo un Administrador activo puede anular ventas.'', 1;

        DECLARE @Estado varchar(30), @FechaAhora datetime=GETDATE(), @Hoy date=CONVERT(date,GETDATE());
        DECLARE @VentaAnulacionId int;
        SELECT @Estado=Estado FROM dbo.Venta WITH (UPDLOCK,HOLDLOCK) WHERE Id=@VentaId;
        IF @Estado IS NULL THROW 54915, ''La venta no existe.'', 1;
        IF @Estado<>''Completada'' THROW 54916, ''La venta ya no esta Completada y no puede anularse otra vez.'', 1;
        IF EXISTS (SELECT 1 FROM dbo.VentaAnulacion WITH (UPDLOCK,HOLDLOCK) WHERE VentaId=@VentaId)
            THROW 54917, ''La venta ya tiene una auditoria de anulacion; no se repetira la operacion.'', 1;

        CREATE TABLE #Trabajo
        (
            DetalleVentaId int NOT NULL PRIMARY KEY,
            ProductoId int NOT NULL,
            LoteId int NOT NULL,
            Cantidad decimal(10,3) NOT NULL,
            VolvioFisicamente bit NOT NULL,
            AptaParaVenta bit NOT NULL,
            ActivoActual bit NULL,
            VencimientoActual datetime NULL,
            StockLote decimal(10,3) NULL,
            CantidadReincorporada decimal(10,3) NOT NULL DEFAULT(0),
            Resultado nvarchar(50) NULL
        );

        INSERT #Trabajo (DetalleVentaId,ProductoId,LoteId,Cantidad,VolvioFisicamente,AptaParaVenta,ActivoActual,VencimientoActual,StockLote)
        SELECT dv.Id,dv.ProductoId,dv.LoteId,dv.Cantidad,e.VolvioFisicamente,e.AptaParaVenta,l.Activo,l.FechaVencimiento,l.StockActual
        FROM @Detalles e
        INNER JOIN dbo.DetalleVenta dv ON dv.Id=e.DetalleVentaId AND dv.VentaId=@VentaId
        INNER JOIN dbo.Producto p ON p.Id=dv.ProductoId
        INNER JOIN dbo.Lote l ON l.Id=dv.LoteId AND l.ProductoId=dv.ProductoId;

        IF NOT EXISTS (SELECT 1 FROM dbo.DetalleVenta WHERE VentaId=@VentaId)
           OR (SELECT COUNT_BIG(*) FROM #Trabajo)<>(SELECT COUNT_BIG(*) FROM dbo.DetalleVenta WHERE VentaId=@VentaId)
           OR (SELECT COUNT_BIG(*) FROM @Detalles)<>(SELECT COUNT_BIG(*) FROM dbo.DetalleVenta WHERE VentaId=@VentaId)
            THROW 54918, ''Indique una decision para cada detalle original de la venta.'', 1;
        IF EXISTS
        (
            SELECT 1 FROM #Trabajo t INNER JOIN dbo.Producto p ON p.Id=t.ProductoId
            WHERE p.SeVendePorPeso=0 AND t.Cantidad<>FLOOR(t.Cantidad)
        ) THROW 54919, ''La venta contiene un detalle fraccionario por unidad; requiere conciliacion antes de anular.'', 1;

        -- Bloquear los lotes originales en orden determinista y volver a leer su estado actual.
        DECLARE @LoteId int, @Activo bit, @Vencimiento datetime, @Saldo decimal(10,3);
        DECLARE lotes CURSOR LOCAL FAST_FORWARD FOR
            SELECT DISTINCT LoteId FROM #Trabajo ORDER BY LoteId;
        OPEN lotes;
        FETCH NEXT FROM lotes INTO @LoteId;
        WHILE @@FETCH_STATUS=0
        BEGIN
            SET @Activo=NULL;
            SET @Vencimiento=NULL;
            SET @Saldo=NULL;
            SELECT @Activo=Activo,@Vencimiento=FechaVencimiento,@Saldo=StockActual
            FROM dbo.Lote WITH (UPDLOCK,HOLDLOCK) WHERE Id=@LoteId;
            IF @Saldo IS NULL THROW 54920, ''No se encontro el lote original; no se anulo la venta.'', 1;
            UPDATE #Trabajo SET ActivoActual=@Activo,VencimientoActual=@Vencimiento,StockLote=@Saldo WHERE LoteId=@LoteId;
            FETCH NEXT FROM lotes INTO @LoteId;
        END;
        CLOSE lotes;
        DEALLOCATE lotes;

        -- Lote no persiste el motivo de Activo=0. Aunque el FIFO lo desactiva al
        -- llegar a cero, ese estado y saldo no prueban por si solos la causa.
        -- Por seguridad solo se reincorpora un lote que siga Activo y vigente;
        -- los inactivos quedan auditados con CantidadReincorporada=0.
        UPDATE #Trabajo
        SET CantidadReincorporada=CASE
                WHEN VolvioFisicamente=1 AND AptaParaVenta=1 AND ActivoActual=1
                 AND (VencimientoActual IS NULL OR CONVERT(date,VencimientoActual)=CONVERT(date,''20991231'',112)
                      OR VencimientoActual>=@Hoy)
                THEN Cantidad ELSE 0 END,
            Resultado=CASE
                WHEN VolvioFisicamente=0 THEN N''No devuelta''
                WHEN VencimientoActual IS NOT NULL AND CONVERT(date,VencimientoActual)<>CONVERT(date,''20991231'',112)
                     AND VencimientoActual<@Hoy THEN N''Devuelta, lote vencido''
                WHEN ActivoActual=0 THEN N''Devuelta, lote inactivo''
                WHEN AptaParaVenta=0 THEN N''Devuelta no apta''
                ELSE N''Reincorporada'' END;

        IF EXISTS
        (
            SELECT 1 FROM #Trabajo t
            INNER JOIN (SELECT LoteId,SUM(CONVERT(decimal(38,3),CantidadReincorporada)) AS AReincorporar
                        FROM #Trabajo GROUP BY LoteId) s ON s.LoteId=t.LoteId
            WHERE t.CantidadReincorporada>0
              AND CONVERT(decimal(38,3),t.StockLote)+s.AReincorporar>9999999.999
        ) THROW 54921, ''El saldo del lote superaria el limite permitido; no se anulo la venta.'', 1;

        INSERT dbo.VentaAnulacion (VentaId,UsuarioId,Motivo,Fecha)
        VALUES (@VentaId,@UsuarioId,LTRIM(RTRIM(@Motivo)),@FechaAhora);
        SET @VentaAnulacionId=CONVERT(int,SCOPE_IDENTITY());

        INSERT dbo.VentaAnulacionDetalle
            (VentaAnulacionId,DetalleVentaId,ProductoId,LoteId,CantidadVendida,VolvioFisicamente,AptaParaVenta,CantidadReincorporada,Resultado)
        SELECT @VentaAnulacionId,DetalleVentaId,ProductoId,LoteId,Cantidad,VolvioFisicamente,AptaParaVenta,CantidadReincorporada,Resultado
        FROM #Trabajo;

        -- Revalidacion defensiva: nunca reactivar un lote inactivo ni vencido.
        UPDATE l
        SET StockActual=CONVERT(decimal(10,3),CONVERT(decimal(38,3),l.StockActual)+s.Cantidad)
        FROM dbo.Lote l
        INNER JOIN (SELECT LoteId,SUM(CONVERT(decimal(38,3),CantidadReincorporada)) AS Cantidad
                    FROM #Trabajo WHERE CantidadReincorporada>0 GROUP BY LoteId) s ON s.LoteId=l.Id
        WHERE l.Activo=1
          AND (l.FechaVencimiento IS NULL
               OR CONVERT(date,l.FechaVencimiento)=CONVERT(date,''20991231'',112)
               OR l.FechaVencimiento>=@Hoy);

        -- Mantener el saldo de la ficha igual a los lotes activos, positivos y vigentes del FIFO.
        DECLARE @ProductoId int, @NuevoStock decimal(10,3);
        DECLARE productos CURSOR LOCAL FAST_FORWARD FOR
            SELECT DISTINCT ProductoId FROM #Trabajo ORDER BY ProductoId;
        OPEN productos;
        FETCH NEXT FROM productos INTO @ProductoId;
        WHILE @@FETCH_STATUS=0
        BEGIN
            SET @NuevoStock=NULL;
            SELECT @NuevoStock=TRY_CONVERT(decimal(10,3),COALESCE(SUM(CONVERT(decimal(38,3),l.StockActual)),0))
            FROM dbo.Lote l WITH (UPDLOCK,HOLDLOCK)
            WHERE l.ProductoId=@ProductoId AND l.Activo=1 AND l.StockActual>0
              AND (l.FechaVencimiento IS NULL
                   OR CONVERT(date,l.FechaVencimiento)=CONVERT(date,''20991231'',112)
                   OR l.FechaVencimiento>=@Hoy);
            IF @NuevoStock IS NULL THROW 54922, ''El stock utilizable supera el rango permitido; no se anulo la venta.'', 1;
            UPDATE dbo.Producto WITH (UPDLOCK,HOLDLOCK) SET StockActual=@NuevoStock WHERE Id=@ProductoId;
            IF @@ROWCOUNT<>1 THROW 54923, ''No se pudo recalcular el saldo del producto; no se anulo la venta.'', 1;
            FETCH NEXT FROM productos INTO @ProductoId;
        END;
        CLOSE productos;
        DEALLOCATE productos;

        UPDATE dbo.Venta SET Estado=''Anulada'' WHERE Id=@VentaId AND Estado=''Completada'';
        IF @@ROWCOUNT<>1 THROW 54924, ''La venta cambio de estado durante la anulacion.'', 1;

        IF (SELECT COUNT_BIG(*) FROM dbo.VentaAnulacionDetalle WHERE VentaAnulacionId=@VentaAnulacionId)
           <>(SELECT COUNT_BIG(*) FROM dbo.DetalleVenta WHERE VentaId=@VentaId)
            THROW 54925, ''No se completo la auditoria por detalle; se cancelo la anulacion.'', 1;

        COMMIT TRANSACTION;
        SELECT @VentaId AS VentaId,@VentaAnulacionId AS VentaAnulacionId,N''Anulada'' AS Estado;
    END TRY
    BEGIN CATCH
        IF CURSOR_STATUS(''local'',''lotes'')>=0 CLOSE lotes;
        IF CURSOR_STATUS(''local'',''lotes'')>=-1 DEALLOCATE lotes;
        IF CURSOR_STATUS(''local'',''productos'')>=0 CLOSE productos;
        IF CURSOR_STATUS(''local'',''productos'')>=-1 DEALLOCATE productos;
        IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

PRINT N'Anulacion segura instalada. No se modificaron ventas, lotes, pagos ni cajas.';
