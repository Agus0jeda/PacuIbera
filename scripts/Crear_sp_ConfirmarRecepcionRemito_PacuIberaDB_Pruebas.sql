/*
    PREPARADO, NO EJECUTADO. Solo PacuIberaDB_Pruebas.
    Instala dbo.RecepcionRemitoLoteEntrada y dbo.sp_ConfirmarRecepcionRemito.
    No cambia tablas, otros SP, transferencia antigua ni emision.
    Contrato revisado contra Crear_EsquemaRemitosInternos_PacuIberaDB_Pruebas.sql
    y los tipos reales previamente verificados. No se consulto la BD en esta etapa.

    Entrada: una fila por CADA RemitoLote.Id, incluso con recibido cero.
    CantidadRecibida = aceptada para stock vendible; NO es conteo fisico de mercaderia
    rechazada. Para vencidos enviar cero; se registra toda la diferencia y observacion.
    Una cantidad positiva vencida se rechaza, no se transforma silenciosamente.
    Sin vencimiento sigue siendo NULL; se conserva la fecha guardada al emitir.
    Un lote que vence hoy sigue vigente segun el dia del servidor, no el cliente.
    Rol autorizado: exclusivamente Vendedor activo. No se amplian otros roles.

    TVP decimal(10,3): el futuro cliente debe validar precision ANTES del envio;
    SQL convierte valores al tipo del TVP antes de ejecutar el cuerpo del SP.
    Una clave identifica RemitoId + UsuarioReceptorId + conjunto exacto lote/cantidad.
    Replay identico devuelve la recepcion original incluso si ya cambio el estado
    del remito o vencieron despues los lotes; no vuelve a acreditar stock.

    Bloqueos: app lock por clave -> Usuario/Rol (lectura estable) -> cabecera remito
    -> recepcion -> detalle/lotes enviados -> Producto por Id ascendente -> Lote nuevo.
    No se toma bloqueo exclusivo ni se escribe en DepositoStock.
    Validar concurrencia con ventas y transferencia antigua antes de habilitar:
    no se conocen aqui todos sus ordenes internos; un deadlock debe revertirse y
    consultarse por clave antes de reintentar, nunca reenviar con otra clave.

    Se crean lotes locales NUEVOS por cada lote enviado con recibido > 0, y se suma
    el total aceptado a Producto.StockActual. No se fusionan lotes ni se corrigen
    saldos locales historicos. Las diferencias quedan pendientes, sin devolucion.
    Para productos por unidad, el saldo local previo tambien debe ser entero;
    si tiene fracciones se rechaza toda la recepcion, incluso con recibido cero.
    Cambios historicos de unidad de venta/producto pueden exigir conciliacion.
    Los roles comprobados desde un Id de usuario no sustituyen autenticacion SQL.
    Falta integrar UI, permisos EXECUTE y resolucion de diferencias en otras etapas.
*/
USE PacuIberaDB_Pruebas;
IF DB_NAME() <> N'PacuIberaDB_Pruebas'
    THROW 51900, 'Este script solo puede instalarse en PacuIberaDB_Pruebas.', 1;
IF @@TRANCOUNT <> 0
BEGIN
    RAISERROR('Ejecute la instalacion sin transaccion externa.',16,1);
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
    EXEC @Instalacion = sys.sp_getapplock @Resource=N'PacuIbera.InstalarRecepcionRemito.v1',
        @LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=0;
    IF @Instalacion<0 THROW 51901,'Otra instalacion de la recepcion esta en curso.',1;

    DECLARE @Columnas TABLE (Tabla sysname,Columna sysname,Tipo sysname,PrecisionEsperada tinyint NULL,EscalaEsperada tinyint NULL);
    INSERT @Columnas VALUES
      (N'RemitoInterno',N'Id',N'int',NULL,NULL),
      (N'RemitoInterno',N'Estado',N'nvarchar',NULL,NULL),
      (N'RemitoInterno',N'FechaEmision',N'datetime2',NULL,0),
      (N'RemitoDetalle',N'Id',N'int',NULL,NULL),
      (N'RemitoDetalle',N'RemitoId',N'int',NULL,NULL),
      (N'RemitoDetalle',N'ProductoId',N'int',NULL,NULL),
      (N'RemitoDetalle',N'CantidadEnviada',N'decimal',10,3),
      (N'RemitoLote',N'Id',N'int',NULL,NULL),
      (N'RemitoLote',N'RemitoId',N'int',NULL,NULL),
      (N'RemitoLote',N'RemitoDetalleId',N'int',NULL,NULL),
      (N'RemitoLote',N'DepositoStockId',N'int',NULL,NULL),
      (N'RemitoLote',N'CantidadEnviada',N'decimal',10,3),
      (N'RemitoLote',N'FechaVencimiento',N'datetime2',NULL,0),
      (N'RecepcionRemito',N'Id',N'int',NULL,NULL),
      (N'RecepcionRemito',N'RemitoId',N'int',NULL,NULL),
      (N'RecepcionRemito',N'ClaveOperacion',N'uniqueidentifier',NULL,NULL),
      (N'RecepcionRemito',N'UsuarioReceptorId',N'int',NULL,NULL),
      (N'RecepcionRemito',N'FechaRecepcion',N'datetime2',NULL,0),
      (N'RecepcionRemito',N'EstadoResultado',N'nvarchar',NULL,NULL),
      (N'RecepcionRemitoLote',N'RecepcionRemitoId',N'int',NULL,NULL),
      (N'RecepcionRemitoLote',N'RemitoId',N'int',NULL,NULL),
      (N'RecepcionRemitoLote',N'RemitoLoteId',N'int',NULL,NULL),
      (N'RecepcionRemitoLote',N'CantidadEnviada',N'decimal',10,3),
      (N'RecepcionRemitoLote',N'CantidadRecibida',N'decimal',10,3),
      (N'RecepcionRemitoLote',N'CantidadDiferencia',N'decimal',10,3),
      (N'RecepcionRemitoLote',N'LoteLocalId',N'int',NULL,NULL),
      (N'RecepcionRemitoLote',N'Observacion',N'nvarchar',NULL,NULL),
      (N'Producto',N'Id',N'int',NULL,NULL),
      (N'Producto',N'Activo',N'bit',NULL,NULL),
      (N'Producto',N'SeVendePorPeso',N'bit',NULL,NULL),
      (N'Producto',N'StockActual',N'decimal',10,3),
      (N'Lote',N'Id',N'int',NULL,NULL),
      (N'Lote',N'ProductoId',N'int',NULL,NULL),
      (N'Lote',N'FechaIngreso',N'datetime',NULL,NULL),
      (N'Lote',N'FechaVencimiento',N'datetime',NULL,NULL),
      (N'Lote',N'StockInicial',N'decimal',10,3),
      (N'Lote',N'StockActual',N'decimal',10,3),
      (N'Lote',N'Activo',N'bit',NULL,NULL),
      (N'DepositoStock',N'Id',N'int',NULL,NULL),
      (N'DepositoStock',N'ProductoId',N'int',NULL,NULL),
      (N'Usuario',N'Id',N'int',NULL,NULL),
      (N'Usuario',N'Activo',N'bit',NULL,NULL),
      (N'Usuario',N'RolId',N'int',NULL,NULL),
      (N'Rol',N'Id',N'int',NULL,NULL);
    IF EXISTS
    (
        SELECT 1 FROM @Columnas e
        LEFT JOIN sys.tables t ON t.schema_id=SCHEMA_ID(N'dbo') AND t.name=e.Tabla
        LEFT JOIN sys.columns c ON c.object_id=t.object_id AND c.name=e.Columna
        WHERE c.column_id IS NULL OR c.user_type_id<>TYPE_ID(e.Tipo)
           OR (e.PrecisionEsperada IS NOT NULL AND c.precision<>e.PrecisionEsperada)
           OR (e.EscalaEsperada IS NOT NULL AND c.scale<>e.EscalaEsperada)
    ) THROW 51902,'Las tablas o columnas no coinciden con el esquema esperado.',1;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.Rol')
        AND name=N'Nombre' AND user_type_id IN (TYPE_ID(N'varchar'),TYPE_ID(N'nvarchar')))
        THROW 51903,'Falta Rol.Nombre de tipo texto.',1;
    IF COLUMNPROPERTY(OBJECT_ID(N'dbo.RecepcionRemito'),N'Id','IsIdentity')<>1
       OR COLUMNPROPERTY(OBJECT_ID(N'dbo.Lote'),N'Id','IsIdentity')<>1
       OR NOT EXISTS (SELECT 1 FROM sys.computed_columns WHERE object_id=OBJECT_ID(N'dbo.RecepcionRemitoLote')
                      AND name=N'CantidadDiferencia' AND is_persisted=1)
        THROW 51904,'Faltan IDENTITY o CantidadDiferencia calculada persistida.',1;

    -- Unicidad necesaria, independientemente del nombre de los indices instalados.
    DECLARE @Unicas TABLE (Tabla sysname,Columna sysname);
    INSERT @Unicas VALUES (N'RecepcionRemito',N'RemitoId'),(N'RecepcionRemito',N'ClaveOperacion'),
                          (N'RecepcionRemitoLote',N'RemitoLoteId');
    IF EXISTS
    (
        SELECT 1 FROM @Unicas e WHERE NOT EXISTS
        (
            SELECT 1 FROM sys.indexes i INNER JOIN sys.index_columns ic
            ON ic.object_id=i.object_id AND ic.index_id=i.index_id
            INNER JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
            WHERE i.object_id=OBJECT_ID(N'dbo.'+e.Tabla) AND i.is_unique=1
              AND i.is_disabled=0 AND i.has_filter=0 AND ic.key_ordinal=1 AND c.name=e.Columna
              AND NOT EXISTS (SELECT 1 FROM sys.index_columns x WHERE x.object_id=i.object_id
                              AND x.index_id=i.index_id AND x.key_ordinal>1)
        )
    ) THROW 51905,'Faltan restricciones unicas para recepcion definitiva e idempotencia.',1;
    IF EXISTS (SELECT 1 FROM sys.triggers WHERE is_disabled=0 AND parent_class=1
        AND parent_id IN (OBJECT_ID(N'dbo.RecepcionRemito'),OBJECT_ID(N'dbo.RecepcionRemitoLote'),
                          OBJECT_ID(N'dbo.Lote'),OBJECT_ID(N'dbo.Producto'),OBJECT_ID(N'dbo.RemitoInterno')))
        THROW 51908,'Hay triggers activos en las tablas de destino. Revisar sus efectos antes de instalar; no se deshabilitan.',1;
    IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE parent_object_id IN
        (OBJECT_ID(N'dbo.RemitoInterno'),OBJECT_ID(N'dbo.RecepcionRemito'),OBJECT_ID(N'dbo.RecepcionRemitoLote'))
        AND (is_disabled=1 OR is_not_trusted=1))
       OR EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id IN
        (OBJECT_ID(N'dbo.RemitoLote'),OBJECT_ID(N'dbo.RecepcionRemito'),OBJECT_ID(N'dbo.RecepcionRemitoLote'))
        AND (is_disabled=1 OR is_not_trusted=1))
        THROW 51906,'Hay restricciones deshabilitadas o no confiables en el circuito.',1;

    IF TYPE_ID(N'dbo.RecepcionRemitoLoteEntrada') IS NULL
        EXEC(N'CREATE TYPE dbo.RecepcionRemitoLoteEntrada AS TABLE
            (RemitoLoteId int NOT NULL PRIMARY KEY, CantidadRecibida decimal(10,3) NOT NULL);');
    DECLARE @Tipo int=(SELECT type_table_object_id FROM sys.table_types
                      WHERE user_type_id=TYPE_ID(N'dbo.RecepcionRemitoLoteEntrada'));
    IF @Tipo IS NULL OR (SELECT COUNT(*) FROM sys.columns WHERE object_id=@Tipo)<>2
       OR NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=@Tipo AND column_id=1
           AND name=N'RemitoLoteId' AND user_type_id=TYPE_ID(N'int') AND is_nullable=0)
       OR NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=@Tipo AND column_id=2
           AND name=N'CantidadRecibida' AND user_type_id=TYPE_ID(N'decimal') AND precision=10 AND scale=3 AND is_nullable=0)
       OR NOT EXISTS
       (
           SELECT 1 FROM sys.indexes i INNER JOIN sys.index_columns ic
             ON ic.object_id=i.object_id AND ic.index_id=i.index_id
           WHERE i.object_id=@Tipo AND i.is_primary_key=1 AND ic.column_id=1 AND ic.key_ordinal=1
             AND NOT EXISTS (SELECT 1 FROM sys.index_columns x WHERE x.object_id=i.object_id
                             AND x.index_id=i.index_id AND x.key_ordinal>1)
       ) THROW 51907,'El tipo de entrada existente es incompatible. No se elimina ni recrea.',1;

    EXEC(N'
CREATE OR ALTER PROCEDURE dbo.sp_ConfirmarRecepcionRemito
    @RemitoId int,
    @ClaveOperacion uniqueidentifier,
    @UsuarioReceptorId int,
    @Lotes dbo.RecepcionRemitoLoteEntrada READONLY
AS
BEGIN
    SET NOCOUNT ON;
    IF @@TRANCOUNT<>0
    BEGIN
        RAISERROR(''La recepcion requiere una llamada sin transaccion externa.'',16,1);
        RETURN;
    END;
    SET IMPLICIT_TRANSACTIONS OFF;
    SET XACT_ABORT ON;
    SET ANSI_PADDING ON;
    SET ANSI_WARNINGS ON;
    SET ARITHABORT ON;
    SET CONCAT_NULL_YIELDS_NULL ON;
    SET NUMERIC_ROUNDABORT OFF;
    DECLARE @RecepcionId int,@RemitoOriginal int,@UsuarioOriginal int,@FechaOriginal datetime2(0);
    DECLARE @Estado nvarchar(30),@Resultado nvarchar(30),@Emision datetime2(0),@Ahora datetime2(0),@Hoy date;
    DECLARE @Bloqueo int,@Recurso nvarchar(255),@NumeroLotes int;
    DECLARE @ProductoId int,@PorPeso bit,@SaldoLocal decimal(10,3),@TotalProducto decimal(38,3);
    DECLARE @RemitoLoteId int,@Enviada decimal(10,3),@Recibida decimal(10,3),@Vence datetime2(0),@LoteLocalId int;
    DECLARE @Trabajo TABLE
        (RemitoLoteId int PRIMARY KEY,ProductoId int NOT NULL,Enviada decimal(10,3) NOT NULL,
         Recibida decimal(10,3) NOT NULL,Vencimiento datetime2(0) NULL);
    BEGIN TRY
        IF @RemitoId IS NULL OR @RemitoId<=0 OR @UsuarioReceptorId IS NULL
            THROW 51910,''Remito y usuario receptor son obligatorios.'',1;
        IF @ClaveOperacion IS NULL OR @ClaveOperacion=''00000000-0000-0000-0000-000000000000''
            THROW 51911,''La clave de operacion es obligatoria y no puede ser vacia.'',1;
        IF NOT EXISTS (SELECT 1 FROM @Lotes)
            THROW 51912,''Debe informar todos los lotes enviados, incluso con cantidad recibida cero.'',1;
        IF EXISTS (SELECT 1 FROM @Lotes WHERE RemitoLoteId<=0 OR CantidadRecibida<0)
            THROW 51913,''Los lotes deben ser validos y las cantidades no pueden ser negativas.'',1;
        BEGIN TRANSACTION;
        SET @Recurso=N''PacuIbera.RecepcionRemito.''+CONVERT(nvarchar(36),@ClaveOperacion);
        EXEC @Bloqueo=sys.sp_getapplock @Resource=@Recurso,@LockMode=N''Exclusive'',
            @LockOwner=N''Transaction'',@LockTimeout=15000;
        IF @Bloqueo<0 THROW 51914,''Operacion ocupada. Consulte la clave antes de reintentar.'',1;
        IF NOT EXISTS
        (
            SELECT 1 FROM dbo.Usuario u WITH (HOLDLOCK)
            INNER JOIN dbo.Rol r WITH (HOLDLOCK) ON r.Id=u.RolId
            WHERE u.Id=@UsuarioReceptorId AND u.Activo=1 AND r.Nombre=N''Vendedor''
        ) THROW 51915,''El receptor debe existir, estar activo y tener rol Vendedor.'',1;
        SELECT @Estado=Estado,@Emision=FechaEmision
        FROM dbo.RemitoInterno WITH (UPDLOCK,HOLDLOCK) WHERE Id=@RemitoId;
        IF @Estado IS NULL THROW 51916,''El remito no existe.'',1;

        SELECT @RecepcionId=Id,@RemitoOriginal=RemitoId,@UsuarioOriginal=UsuarioReceptorId,
               @FechaOriginal=FechaRecepcion,@Resultado=EstadoResultado
        FROM dbo.RecepcionRemito WITH (UPDLOCK,HOLDLOCK) WHERE ClaveOperacion=@ClaveOperacion;
        IF @RecepcionId IS NOT NULL
        BEGIN
            IF @RemitoOriginal<>@RemitoId OR @UsuarioOriginal<>@UsuarioReceptorId
                THROW 51917,''La clave pertenece a otro remito o receptor.'',1;
            IF EXISTS (SELECT RemitoLoteId,CantidadRecibida FROM @Lotes
                       EXCEPT SELECT RemitoLoteId,CantidadRecibida FROM dbo.RecepcionRemitoLote WHERE RecepcionRemitoId=@RecepcionId)
               OR EXISTS (SELECT RemitoLoteId,CantidadRecibida FROM dbo.RecepcionRemitoLote WHERE RecepcionRemitoId=@RecepcionId
                          EXCEPT SELECT RemitoLoteId,CantidadRecibida FROM @Lotes)
                THROW 51918,''La clave ya fue usada con lotes o cantidades diferentes.'',1;
            COMMIT TRANSACTION;
            SELECT @RecepcionId AS RecepcionRemitoId,@RemitoId AS RemitoId,@Resultado AS Estado,
                   @FechaOriginal AS FechaRecepcion,CONVERT(bit,1) AS Reutilizado;
            RETURN;
        END;
        IF EXISTS (SELECT 1 FROM dbo.RecepcionRemito WITH (UPDLOCK,HOLDLOCK) WHERE RemitoId=@RemitoId)
            THROW 51919,''El remito ya tiene una recepcion definitiva con otra clave.'',1;
        IF @Estado<>N''En tránsito'' OR @Emision IS NULL
            THROW 51920,''Solo se pueden recibir remitos emitidos En transito.'',1;

        SELECT @NumeroLotes=COUNT(*) FROM dbo.RemitoLote WITH (HOLDLOCK) WHERE RemitoId=@RemitoId;
        IF @NumeroLotes=0 OR @NumeroLotes<>(SELECT COUNT(*) FROM @Lotes)
            THROW 51921,''La recepcion debe contener exactamente todos los lotes del remito.'',1;
        INSERT @Trabajo (RemitoLoteId,ProductoId,Enviada,Recibida,Vencimiento)
        SELECT rl.Id,rd.ProductoId,rl.CantidadEnviada,e.CantidadRecibida,rl.FechaVencimiento
        FROM @Lotes e INNER JOIN dbo.RemitoLote rl WITH (HOLDLOCK) ON rl.Id=e.RemitoLoteId AND rl.RemitoId=@RemitoId
        INNER JOIN dbo.RemitoDetalle rd WITH (HOLDLOCK) ON rd.Id=rl.RemitoDetalleId AND rd.RemitoId=rl.RemitoId;
        IF (SELECT COUNT(*) FROM @Trabajo)<>@NumeroLotes
            THROW 51922,''Hay lotes ajenos al remito o detalles inconsistentes.'',1;
        IF EXISTS (SELECT 1 FROM @Trabajo WHERE Enviada<=0 OR Recibida>Enviada)
            THROW 51923,''La cantidad recibida no puede superar la enviada.'',1;
        IF EXISTS
        (
            SELECT 1 FROM dbo.RemitoLote rl INNER JOIN dbo.RemitoDetalle rd ON rd.Id=rl.RemitoDetalleId
            LEFT JOIN dbo.DepositoStock ds ON ds.Id=rl.DepositoStockId
            WHERE rl.RemitoId=@RemitoId AND (ds.Id IS NULL OR ds.ProductoId<>rd.ProductoId)
        ) THROW 51924,''El producto no coincide con su lote original de deposito.'',1;
        IF EXISTS
        (
            SELECT rd.Id FROM dbo.RemitoDetalle rd WITH (HOLDLOCK)
            LEFT JOIN dbo.RemitoLote rl WITH (HOLDLOCK) ON rl.RemitoDetalleId=rd.Id AND rl.RemitoId=rd.RemitoId
            WHERE rd.RemitoId=@RemitoId GROUP BY rd.Id,rd.CantidadEnviada
            HAVING rd.CantidadEnviada<=0 OR ISNULL(SUM(rl.CantidadEnviada),0)<>rd.CantidadEnviada
        ) THROW 51925,''Las cantidades de los detalles no coinciden con los lotes enviados.'',1;

        DECLARE validarProductos CURSOR LOCAL FAST_FORWARD FOR
            SELECT ProductoId,SUM(Recibida) FROM @Trabajo GROUP BY ProductoId ORDER BY ProductoId;
        OPEN validarProductos;
        FETCH NEXT FROM validarProductos INTO @ProductoId,@TotalProducto;
        WHILE @@FETCH_STATUS=0
        BEGIN
            SET @PorPeso=NULL;
            SELECT @PorPeso=SeVendePorPeso,@SaldoLocal=StockActual FROM dbo.Producto WITH (UPDLOCK,HOLDLOCK)
            WHERE Id=@ProductoId AND Activo=1;
            IF @PorPeso IS NULL OR @SaldoLocal IS NULL
                THROW 51926,''Un producto no existe, esta inactivo o tiene configuracion invalida.'',1;
            IF @PorPeso=0 AND @SaldoLocal<>FLOOR(@SaldoLocal)
                THROW 51933,''Un producto por unidad tiene saldo local fraccionario. Requiere conciliacion antes de recibir.'',1;
            IF @PorPeso=0 AND EXISTS (SELECT 1 FROM @Trabajo WHERE ProductoId=@ProductoId
                                     AND (Recibida<>FLOOR(Recibida) OR Enviada<>FLOOR(Enviada)))
                THROW 51927,''Los productos por unidad requieren cantidades enteras.'',1;
            IF @SaldoLocal<0 OR @TotalProducto>9999999.999-CONVERT(decimal(18,3),@SaldoLocal)
                THROW 51928,''El saldo local es invalido o excederia decimal(10,3).'',1;
            FETCH NEXT FROM validarProductos INTO @ProductoId,@TotalProducto;
        END;
        CLOSE validarProductos;
        DEALLOCATE validarProductos;

        SET @Ahora=SYSDATETIME();
        SET @Hoy=CONVERT(date,@Ahora);
        IF @Ahora<@Emision THROW 51929,''La fecha de recepcion no puede ser anterior a la emision.'',1;
        IF EXISTS (SELECT 1 FROM @Trabajo WHERE Recibida>0 AND Vencimiento<@Hoy)
            THROW 51930,''Hay lotes vencidos: informe recibido cero para registrarlos como diferencia, sin acreditar stock vendible.'',1;
        SET @Resultado=CASE WHEN EXISTS (SELECT 1 FROM @Trabajo WHERE Recibida<Enviada)
                             THEN N''Con diferencias'' ELSE N''Recibido'' END;
        INSERT dbo.RecepcionRemito (RemitoId,ClaveOperacion,UsuarioReceptorId,FechaRecepcion,EstadoResultado)
        VALUES (@RemitoId,@ClaveOperacion,@UsuarioReceptorId,@Ahora,@Resultado);
        SET @RecepcionId=CONVERT(int,SCOPE_IDENTITY());

        DECLARE acreditarLotes CURSOR LOCAL FAST_FORWARD FOR
            SELECT RemitoLoteId,ProductoId,Enviada,Recibida,Vencimiento FROM @Trabajo ORDER BY ProductoId,RemitoLoteId;
        OPEN acreditarLotes;
        FETCH NEXT FROM acreditarLotes INTO @RemitoLoteId,@ProductoId,@Enviada,@Recibida,@Vence;
        WHILE @@FETCH_STATUS=0
        BEGIN
            SET @LoteLocalId=NULL;
            IF @Recibida>0
            BEGIN
                INSERT dbo.Lote (ProductoId,FechaIngreso,FechaVencimiento,StockInicial,StockActual,Activo)
                VALUES (@ProductoId,@Ahora,@Vence,@Recibida,@Recibida,1);
                SET @LoteLocalId=CONVERT(int,SCOPE_IDENTITY());
            END;
            INSERT dbo.RecepcionRemitoLote
                (RecepcionRemitoId,RemitoId,RemitoLoteId,CantidadEnviada,CantidadRecibida,LoteLocalId,Observacion)
            VALUES (@RecepcionId,@RemitoId,@RemitoLoteId,@Enviada,@Recibida,@LoteLocalId,
                CASE WHEN @Vence<@Hoy THEN N''Lote vencido al recibir: no acreditado; diferencia pendiente.''
                     WHEN @Recibida<@Enviada THEN N''Diferencia pendiente de resolucion.'' ELSE NULL END);
            FETCH NEXT FROM acreditarLotes INTO @RemitoLoteId,@ProductoId,@Enviada,@Recibida,@Vence;
        END;
        CLOSE acreditarLotes;
        DEALLOCATE acreditarLotes;
        UPDATE p SET StockActual=p.StockActual+s.Recibido
        FROM dbo.Producto p INNER JOIN
            -- SUM(decimal) devuelve decimal(38,3). Acotar antes de sumar al saldo
            -- evita que la aritmetica reduzca escala y redondee la tercera posicion.
            (SELECT ProductoId,CONVERT(decimal(18,3),SUM(Recibida)) AS Recibido
             FROM @Trabajo GROUP BY ProductoId) s ON s.ProductoId=p.Id
        WHERE s.Recibido>0;

        IF (SELECT COUNT(*) FROM dbo.RecepcionRemitoLote WHERE RecepcionRemitoId=@RecepcionId)<>@NumeroLotes
           OR (SELECT SUM(CantidadRecibida+CantidadDiferencia) FROM dbo.RecepcionRemitoLote WHERE RecepcionRemitoId=@RecepcionId)
              <> (SELECT SUM(Enviada) FROM @Trabajo)
            THROW 51931,''La suma recibida mas diferencias no coincide con el envio. Se revierte toda la recepcion.'',1;
        UPDATE dbo.RemitoInterno SET Estado=@Resultado WHERE Id=@RemitoId AND Estado=N''En tránsito'';
        IF @@ROWCOUNT<>1 THROW 51932,''El remito cambio de estado. Se revierte toda la recepcion.'',1;
        COMMIT TRANSACTION;
        SELECT @RecepcionId AS RecepcionRemitoId,@RemitoId AS RemitoId,@Resultado AS Estado,
               @Ahora AS FechaRecepcion,CONVERT(bit,0) AS Reutilizado;
    END TRY
    BEGIN CATCH
        IF CURSOR_STATUS(''local'',''validarProductos'')>=0 CLOSE validarProductos;
        IF CURSOR_STATUS(''local'',''validarProductos'')>=-1 DEALLOCATE validarProductos;
        IF CURSOR_STATUS(''local'',''acreditarLotes'')>=0 CLOSE acreditarLotes;
        IF CURSOR_STATUS(''local'',''acreditarLotes'')>=-1 DEALLOCATE acreditarLotes;
        IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;');
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
