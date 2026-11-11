/*
    SOLO PREPARADO: NO ejecutado. Destino exclusivo PacuIberaDB_Pruebas.
    Instala dbo.RemitoProductoEntrada y dbo.sp_EmitirRemitoInterno en una transaccion.
    No ejecuta la emision ni cambia otros SP/tablas. No toca Lote ni Producto.StockActual.
    Contrato basado en el esquema de remitos del repositorio y los metadatos reales
    verificados previamente para Producto, DepositoStock, Usuario y Rol.
    Las dependencias se comprueban al instalar: no se presupone que la BD siga igual.

    Entrada: una fila por producto, Cantidad decimal(10,3), clave UUID generada por
    el llamador ANTES del primer intento. Reutilizar esa clave en reintentos.
    SQL convierte al tipo del TVP antes de entrar al SP: el llamador debe rechazar
    valores con mas de tres decimales ANTES de construir el TVP, sin redondearlos.

    Resultado (solo despues de COMMIT): RemitoId, Estado, FechaEmision, Reutilizado.
    Una operacion ya emitida se devuelve aunque luego se haya recibido/cerrado;
    cambios de stock, vencimiento o activacion de productos no invalidan el replay.
    El usuario debe seguir activo y ser Administrador para acceder al resultado.

    Bloqueos: app lock por clave -> DepositoStock TABLOCKX -> Usuario/Rol -> remito
    -> productos por Id ascendente -> seleccion FEFO/FIFO e inserciones.
    TABLOCKX es deliberadamente conservador y compartido con escritores antiguos
    mediante los bloqueos SQL normales (no depende de que usen sp_getapplock).
    Serializa todas las emisiones y puede demorar recepciones/consultas de deposito.
    No elimina todo deadlock posible frente a otros circuitos: reintentar con la
    MISMA clave tras consultar el resultado. No combinar con transacciones externas.
    No se implementan recepcion del vendedor ni resoluciones en este archivo.
*/
USE PacuIberaDB_Pruebas;

IF DB_NAME() <> N'PacuIberaDB_Pruebas'
    THROW 51700, 'El script solo puede instalarse en PacuIberaDB_Pruebas.', 1;
IF @@TRANCOUNT <> 0
BEGIN
    RAISERROR('Ejecute la instalacion sin una transaccion previa abierta.', 16, 1);
    RETURN;
END;
SET IMPLICIT_TRANSACTIONS OFF;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @Instalacion int;
    EXEC @Instalacion = sys.sp_getapplock
        @Resource = N'PacuIbera.InstalarEmitirRemito.v1', @LockMode = N'Exclusive',
        @LockOwner = N'Transaction', @LockTimeout = 0;
    IF @Instalacion < 0
        THROW 51701, 'Otra instalacion del procedimiento esta en curso.', 1;

    DECLARE @Esperadas TABLE
        (Tabla sysname, Columna sysname, Tipo sysname, PrecisionEsperada tinyint NULL,
         EscalaEsperada tinyint NULL, LongitudMinima int NULL);
    INSERT INTO @Esperadas VALUES
        (N'RemitoInterno', N'Id', N'int', NULL, NULL, NULL),
        (N'RemitoInterno', N'ClaveOperacion', N'uniqueidentifier', NULL, NULL, NULL),
        (N'RemitoInterno', N'Estado', N'nvarchar', NULL, NULL, 60),
        (N'RemitoInterno', N'UsuarioCreadorId', N'int', NULL, NULL, NULL),
        (N'RemitoInterno', N'FechaCreacion', N'datetime2', NULL, 0, NULL),
        (N'RemitoInterno', N'UsuarioEmisorId', N'int', NULL, NULL, NULL),
        (N'RemitoInterno', N'FechaEmision', N'datetime2', NULL, 0, NULL),
        (N'RemitoDetalle', N'Id', N'int', NULL, NULL, NULL),
        (N'RemitoDetalle', N'RemitoId', N'int', NULL, NULL, NULL),
        (N'RemitoDetalle', N'ProductoId', N'int', NULL, NULL, NULL),
        (N'RemitoDetalle', N'CantidadEnviada', N'decimal', 10, 3, NULL),
        (N'RemitoLote', N'RemitoId', N'int', NULL, NULL, NULL),
        (N'RemitoLote', N'RemitoDetalleId', N'int', NULL, NULL, NULL),
        (N'RemitoLote', N'DepositoStockId', N'int', NULL, NULL, NULL),
        (N'RemitoLote', N'CantidadEnviada', N'decimal', 10, 3, NULL),
        (N'RemitoLote', N'FechaIngresoOrigen', N'datetime2', NULL, 0, NULL),
        (N'RemitoLote', N'FechaVencimiento', N'datetime2', NULL, 0, NULL),
        (N'DepositoStock', N'Id', N'int', NULL, NULL, NULL),
        (N'DepositoStock', N'ProductoId', N'int', NULL, NULL, NULL),
        (N'DepositoStock', N'StockActual', N'decimal', 10, 3, NULL),
        (N'DepositoStock', N'FechaIngreso', N'datetime2', NULL, 0, NULL),
        (N'DepositoStock', N'FechaVencimiento', N'datetime2', NULL, 0, NULL),
        (N'DepositoStock', N'Activo', N'bit', NULL, NULL, NULL),
        (N'Producto', N'Id', N'int', NULL, NULL, NULL),
        (N'Producto', N'Activo', N'bit', NULL, NULL, NULL),
        (N'Producto', N'SeVendePorPeso', N'bit', NULL, NULL, NULL),
        (N'Usuario', N'Id', N'int', NULL, NULL, NULL),
        (N'Usuario', N'Activo', N'bit', NULL, NULL, NULL),
        (N'Usuario', N'RolId', N'int', NULL, NULL, NULL),
        (N'Rol', N'Id', N'int', NULL, NULL, NULL),
        (N'Rol', N'Nombre', N'varchar', NULL, NULL, 13);
    IF EXISTS
    (
        SELECT 1 FROM @Esperadas e
        LEFT JOIN sys.tables t ON t.name = e.Tabla AND t.schema_id = SCHEMA_ID(N'dbo')
        LEFT JOIN sys.columns c ON c.object_id = t.object_id AND c.name = e.Columna
        WHERE c.column_id IS NULL
           OR (c.user_type_id <> TYPE_ID(e.Tipo)
               AND NOT (e.Tabla = N'Rol' AND e.Columna = N'Nombre'
                        AND c.user_type_id IN (TYPE_ID(N'varchar'), TYPE_ID(N'nvarchar'))))
           OR (e.PrecisionEsperada IS NOT NULL AND c.precision <> e.PrecisionEsperada)
           OR (e.EscalaEsperada IS NOT NULL AND c.scale <> e.EscalaEsperada)
           OR (e.LongitudMinima IS NOT NULL AND c.max_length <> -1 AND c.max_length < e.LongitudMinima)
    )
        THROW 51702, 'Las columnas/tipos de las dependencias no coinciden con el contrato esperado.', 1;
    IF COLUMNPROPERTY(OBJECT_ID(N'dbo.RemitoInterno'), N'Id', 'IsIdentity') <> 1
       OR COLUMNPROPERTY(OBJECT_ID(N'dbo.RemitoDetalle'), N'Id', 'IsIdentity') <> 1
        THROW 51703, 'Los Id de remito y detalle deben ser IDENTITY.', 1;

    -- Exigir las claves unicas usadas para idempotencia e integridad; sin depender del nombre.
    DECLARE @Claves TABLE (Tabla sysname, Grupo int, Posicion int, Columna sysname);
    INSERT INTO @Claves VALUES
        (N'RemitoInterno', 1, 1, N'ClaveOperacion'),
        (N'RemitoDetalle', 2, 1, N'RemitoId'), (N'RemitoDetalle', 2, 2, N'ProductoId'),
        (N'RemitoLote', 3, 1, N'RemitoId'), (N'RemitoLote', 3, 2, N'DepositoStockId');
    IF EXISTS
    (
        SELECT 1 FROM @Claves k
        WHERE NOT EXISTS
        (
            SELECT 1 FROM sys.indexes i
            WHERE i.object_id = OBJECT_ID(N'dbo.' + k.Tabla) AND i.is_unique = 1
              AND i.is_disabled = 0 AND i.has_filter = 0
              AND (SELECT COUNT(*) FROM sys.index_columns ic WHERE ic.object_id = i.object_id
                   AND ic.index_id = i.index_id AND ic.key_ordinal > 0)
                    = (SELECT COUNT(*) FROM @Claves e WHERE e.Grupo = k.Grupo)
              AND NOT EXISTS
              (
                  SELECT 1 FROM @Claves e WHERE e.Grupo = k.Grupo
                  AND NOT EXISTS
                  (
                      SELECT 1 FROM sys.index_columns ic
                      INNER JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
                      WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id
                        AND ic.key_ordinal = e.Posicion AND c.name = e.Columna
                  )
              )
        )
    )
        THROW 51704, 'Faltan claves unicas requeridas en las tablas del circuito.', 1;

    IF TYPE_ID(N'dbo.RemitoProductoEntrada') IS NULL
        EXEC(N'CREATE TYPE dbo.RemitoProductoEntrada AS TABLE
            (ProductoId int NOT NULL PRIMARY KEY, Cantidad decimal(10,3) NOT NULL);');
    DECLARE @TipoObjeto int = (SELECT type_table_object_id FROM sys.table_types
                               WHERE user_type_id = TYPE_ID(N'dbo.RemitoProductoEntrada'));
    IF @TipoObjeto IS NULL
       OR (SELECT COUNT(*) FROM sys.columns WHERE object_id = @TipoObjeto) <> 2
       OR NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = @TipoObjeto AND column_id = 1
                      AND name = N'ProductoId' AND user_type_id = TYPE_ID(N'int') AND is_nullable = 0)
       OR NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = @TipoObjeto AND column_id = 2
                      AND name = N'Cantidad' AND user_type_id = TYPE_ID(N'decimal')
                      AND precision = 10 AND scale = 3 AND is_nullable = 0)
       OR NOT EXISTS
       (
           SELECT 1 FROM sys.indexes i INNER JOIN sys.index_columns ic
             ON ic.object_id = i.object_id AND ic.index_id = i.index_id
           WHERE i.object_id = @TipoObjeto AND i.is_primary_key = 1 AND ic.column_id = 1 AND ic.key_ordinal = 1
             AND NOT EXISTS (SELECT 1 FROM sys.index_columns x WHERE x.object_id = i.object_id
                             AND x.index_id = i.index_id AND x.key_ordinal > 1)
       )
        THROW 51705, 'RemitoProductoEntrada existe con estructura incompatible. No se elimina ni recrea.', 1;

    EXEC(N'
CREATE OR ALTER PROCEDURE dbo.sp_EmitirRemitoInterno
    @ClaveOperacion uniqueidentifier,
    @UsuarioEmisorId int,
    @Productos dbo.RemitoProductoEntrada READONLY
AS
BEGIN
    SET NOCOUNT ON;
    -- El SP es la unidad atomica; no confirmar ni revertir transacciones ajenas.
    IF @@TRANCOUNT <> 0
    BEGIN
        RAISERROR(''La emision requiere una llamada sin transaccion externa.'', 16, 1);
        RETURN;
    END;
    SET IMPLICIT_TRANSACTIONS OFF;
    SET XACT_ABORT ON;
    SET ANSI_PADDING ON;
    SET ANSI_WARNINGS ON;
    SET ARITHABORT ON;
    SET CONCAT_NULL_YIELDS_NULL ON;
    SET NUMERIC_ROUNDABORT OFF;

    DECLARE @RemitoId int, @UsuarioOriginal int, @FechaOriginal datetime2(0), @Estado nvarchar(30);
    DECLARE @ProductoId int, @PorPeso bit, @Cantidad decimal(10,3), @Restante decimal(10,3);
    DECLARE @DetalleId int, @LoteId int, @Saldo decimal(10,3), @Tomar decimal(10,3);
    DECLARE @Ingreso datetime2(0), @Vencimiento datetime2(0), @Ahora datetime2(0), @Hoy date;
    DECLARE @ResultadoBloqueo int, @Recurso nvarchar(255), @LotesBloqueados bigint;
    BEGIN TRY
        IF @ClaveOperacion IS NULL OR @ClaveOperacion = ''00000000-0000-0000-0000-000000000000''
            THROW 51710, ''La clave de operacion es obligatoria y no puede ser vacia.'', 1;
        IF @UsuarioEmisorId IS NULL
            THROW 51711, ''El usuario emisor es obligatorio.'', 1;
        IF NOT EXISTS (SELECT 1 FROM @Productos)
            THROW 51712, ''El remito debe contener al menos un producto.'', 1;
        IF EXISTS (SELECT 1 FROM @Productos WHERE ProductoId <= 0 OR Cantidad <= 0)
            THROW 51713, ''Los productos y cantidades deben ser positivos.'', 1;

        BEGIN TRANSACTION;
        SET @Recurso = N''PacuIbera.EmitirRemito.'' + CONVERT(nvarchar(36), @ClaveOperacion);
        EXEC @ResultadoBloqueo = sys.sp_getapplock @Resource = @Recurso,
            @LockMode = N''Exclusive'', @LockOwner = N''Transaction'', @LockTimeout = 15000;
        IF @ResultadoBloqueo < 0
            THROW 51714, ''No se pudo bloquear la operacion. Consulte su resultado y reintente con la misma clave.'', 1;

        SELECT @LotesBloqueados = COUNT_BIG(*) FROM dbo.DepositoStock WITH (TABLOCKX, HOLDLOCK);
        -- Calcular el dia despues de esperar los bloqueos; no usar una fecha anterior a la espera.
        SET @Ahora = SYSDATETIME();
        SET @Hoy = CONVERT(date, @Ahora);

        IF NOT EXISTS
        (
            SELECT 1 FROM dbo.Usuario u WITH (HOLDLOCK)
            INNER JOIN dbo.Rol r WITH (HOLDLOCK) ON r.Id = u.RolId
            WHERE u.Id = @UsuarioEmisorId AND u.Activo = 1 AND r.Nombre = N''Administrador''
        )
            THROW 51715, ''El emisor debe existir, estar activo y tener rol Administrador.'', 1;

        SELECT @RemitoId = Id, @UsuarioOriginal = UsuarioEmisorId,
               @FechaOriginal = FechaEmision, @Estado = Estado
        FROM dbo.RemitoInterno WITH (UPDLOCK, HOLDLOCK) WHERE ClaveOperacion = @ClaveOperacion;
        IF @RemitoId IS NOT NULL
        BEGIN
            IF @FechaOriginal IS NULL OR @UsuarioOriginal IS NULL OR @UsuarioOriginal <> @UsuarioEmisorId
               OR @Estado NOT IN (N''En tránsito'', N''Recibido'', N''Con diferencias'', N''Cerrado'')
                THROW 51716, ''La clave corresponde a otro emisor o a una operacion no emitida.'', 1;
            IF EXISTS
                (SELECT ProductoId, Cantidad FROM @Productos
                 EXCEPT SELECT ProductoId, CantidadEnviada FROM dbo.RemitoDetalle WHERE RemitoId = @RemitoId)
               OR EXISTS
                (SELECT ProductoId, CantidadEnviada FROM dbo.RemitoDetalle WHERE RemitoId = @RemitoId
                 EXCEPT SELECT ProductoId, Cantidad FROM @Productos)
                THROW 51717, ''La clave de operacion ya se utilizo con productos o cantidades diferentes.'', 1;
            COMMIT TRANSACTION;
            SELECT @RemitoId AS RemitoId, @Estado AS Estado, @FechaOriginal AS FechaEmision,
                   CONVERT(bit, 1) AS Reutilizado;
            RETURN;
        END;

        INSERT INTO dbo.RemitoInterno
            (ClaveOperacion, Estado, UsuarioCreadorId, FechaCreacion, UsuarioEmisorId, FechaEmision)
        VALUES (@ClaveOperacion, N''En tránsito'', @UsuarioEmisorId, @Ahora, @UsuarioEmisorId, @Ahora);
        SET @RemitoId = CONVERT(int, SCOPE_IDENTITY());

        DECLARE productos CURSOR LOCAL FAST_FORWARD FOR
            SELECT ProductoId, Cantidad FROM @Productos ORDER BY ProductoId;
        OPEN productos;
        FETCH NEXT FROM productos INTO @ProductoId, @Cantidad;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            SET @PorPeso = NULL;
            SELECT @PorPeso = SeVendePorPeso FROM dbo.Producto WITH (HOLDLOCK)
            WHERE Id = @ProductoId AND Activo = 1;
            IF @PorPeso IS NULL
                THROW 51718, ''Un producto no existe, esta inactivo o no tiene unidad de venta definida.'', 1;
            IF @PorPeso = 0 AND @Cantidad <> FLOOR(@Cantidad)
                THROW 51719, ''Los productos por unidad requieren cantidades enteras.'', 1;
            IF (SELECT ISNULL(SUM(StockActual), 0) FROM dbo.DepositoStock
                WHERE ProductoId = @ProductoId AND Activo = 1 AND StockActual > 0
                  AND (FechaVencimiento IS NULL OR FechaVencimiento >= @Hoy)) < @Cantidad
                THROW 51720, ''No hay stock utilizable suficiente para uno de los productos.'', 1;

            INSERT INTO dbo.RemitoDetalle (RemitoId, ProductoId, CantidadEnviada)
            VALUES (@RemitoId, @ProductoId, @Cantidad);
            SET @DetalleId = CONVERT(int, SCOPE_IDENTITY());
            SET @Restante = @Cantidad;
            WHILE @Restante > 0
            BEGIN
                SET @LoteId = NULL;
                SELECT TOP (1) @LoteId = Id, @Saldo = StockActual,
                               @Ingreso = FechaIngreso, @Vencimiento = FechaVencimiento
                FROM dbo.DepositoStock
                WHERE ProductoId = @ProductoId AND Activo = 1 AND StockActual > 0
                  AND (FechaVencimiento IS NULL OR FechaVencimiento >= @Hoy)
                ORDER BY CASE WHEN FechaVencimiento IS NULL THEN 1 ELSE 0 END,
                         FechaVencimiento,
                         CASE WHEN FechaVencimiento IS NULL THEN FechaIngreso END, Id;
                IF @LoteId IS NULL
                    THROW 51721, ''No se encontro un lote elegible. Se revertira toda la emision.'', 1;
                -- Un saldo historico fraccionario por unidad no se corrige ni se saltea silenciosamente.
                IF @PorPeso = 0 AND @Saldo <> FLOOR(@Saldo)
                    THROW 51722, ''Un lote por unidad tiene saldo fraccionario; requiere conciliacion.'', 1;
                SET @Tomar = CASE WHEN @Saldo < @Restante THEN @Saldo ELSE @Restante END;
                INSERT INTO dbo.RemitoLote
                    (RemitoId, RemitoDetalleId, DepositoStockId, CantidadEnviada, FechaIngresoOrigen, FechaVencimiento)
                VALUES (@RemitoId, @DetalleId, @LoteId, @Tomar, @Ingreso, @Vencimiento);
                UPDATE dbo.DepositoStock SET StockActual = StockActual - @Tomar
                WHERE Id = @LoteId AND StockActual >= @Tomar;
                IF @@ROWCOUNT <> 1
                    THROW 51723, ''No se pudo descontar el lote. Se revertira toda la emision.'', 1;
                SET @Restante = @Restante - @Tomar;
            END;
            FETCH NEXT FROM productos INTO @ProductoId, @Cantidad;
        END;
        CLOSE productos;
        DEALLOCATE productos;
        COMMIT TRANSACTION;
        SELECT @RemitoId AS RemitoId, N''En tránsito'' AS Estado, @Ahora AS FechaEmision,
               CONVERT(bit, 0) AS Reutilizado;
    END TRY
    BEGIN CATCH
        IF CURSOR_STATUS(''local'', ''productos'') >= 0 CLOSE productos;
        IF CURSOR_STATUS(''local'', ''productos'') >= -1 DEALLOCATE productos;
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;');
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
