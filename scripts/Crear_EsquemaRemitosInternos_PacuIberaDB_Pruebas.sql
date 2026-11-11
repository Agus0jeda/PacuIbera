/*
    DESTINO EXCLUSIVO: PacuIberaDB_Pruebas. Script preparado, NO ejecutado.
    Basado en los metadatos reales verificados previamente en esta sesion:
    Id int; cantidades decimal(10,3); DepositoStock.Fecha* datetime2(0);
    Lote.Fecha* datetime. La trazabilidad nueva conserva fechas datetime2(0).

    Solo crea siete tablas nuevas y sus indices. No cambia tablas, datos ni SP existentes.
    Ejecutar el archivo completo como un unico lote, sin introducir GO intermedios.
    Instalacion atomica y conservadora: si cualquier nombre de destino ya existe,
    se detiene SIN modificarlo ni completar instalaciones parciales desconocidas.
    Tras un fallo revertido puede reejecutarse. Tras una instalacion exitosa, una
    reejecucion se rechaza para exigir revision del esquema ya instalado.

    Id de RemitoInterno es el numero interno (IDENTITY admite saltos).
    Un deposito y un local: no se inventan sucursales/ubicaciones inexistentes.
    Las claves de operacion las genera el cliente antes del primer envio, sin DEFAULT.
    Reutilizar una clave exige comparar el contenido en los futuros procedimientos.

    IMPORTANTE: estas tablas no habilitan el circuito. Faltan SP que validen roles,
    FEFO/FIFO, enteros por unidad, transiciones, sumas entre filas y saldos pendientes.
    No usar INSERT directo como sustituto de esos SP. No deshabilitar aun la via antigua.
*/
USE PacuIberaDB_Pruebas;

IF DB_NAME() <> N'PacuIberaDB_Pruebas'
    THROW 51600, 'Este script solo puede ejecutarse en PacuIberaDB_Pruebas.', 1;

SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    -- Serializar instalaciones de este esquema dentro de la misma base.
    DECLARE @Bloqueo int;
    EXEC @Bloqueo = sys.sp_getapplock
        @Resource = N'PacuIbera.EsquemaRemitosInternos.v1',
        @LockMode = N'Exclusive', @LockOwner = N'Transaction', @LockTimeout = 0;
    IF @Bloqueo < 0
        THROW 51601, 'Otra instalacion del esquema de remitos esta en curso.', 1;

    IF EXISTS
    (
        SELECT 1 FROM sys.objects
        WHERE schema_id = SCHEMA_ID(N'dbo')
          AND name IN (N'RemitoInterno', N'RemitoDetalle', N'RemitoLote',
                       N'RecepcionRemito', N'RecepcionRemitoLote',
                       N'ResolucionDiferencia', N'ResolucionDiferenciaDetalle')
    )
        THROW 51602, 'Ya existe al menos un objeto de remitos. Revisar el esquema existente; no se modifico ni completo.', 1;

    -- Verificar el contrato externo sin modificar ninguna dependencia.
    DECLARE @Dependencias TABLE (Tabla sysname, Columna sysname, Tipo sysname,
                                PrecisionEsperada tinyint NULL, EscalaEsperada tinyint NULL);
    INSERT INTO @Dependencias VALUES
        (N'Producto', N'Id', N'int', NULL, NULL),
        (N'Producto', N'SeVendePorPeso', N'bit', NULL, NULL),
        (N'Usuario', N'Id', N'int', NULL, NULL),
        (N'DepositoStock', N'Id', N'int', NULL, NULL),
        (N'DepositoStock', N'ProductoId', N'int', NULL, NULL),
        (N'DepositoStock', N'StockInicial', N'decimal', 10, 3),
        (N'DepositoStock', N'StockActual', N'decimal', 10, 3),
        (N'DepositoStock', N'FechaIngreso', N'datetime2', NULL, 0),
        (N'DepositoStock', N'FechaVencimiento', N'datetime2', NULL, 0),
        (N'DepositoStock', N'Activo', N'bit', NULL, NULL),
        (N'Lote', N'Id', N'int', NULL, NULL),
        (N'Lote', N'ProductoId', N'int', NULL, NULL),
        (N'Lote', N'StockInicial', N'decimal', 10, 3),
        (N'Lote', N'StockActual', N'decimal', 10, 3),
        (N'Lote', N'FechaIngreso', N'datetime', NULL, NULL),
        (N'Lote', N'FechaVencimiento', N'datetime', NULL, NULL);
    IF EXISTS
    (
        SELECT 1 FROM @Dependencias e
        LEFT JOIN sys.tables t ON t.name = e.Tabla AND t.schema_id = SCHEMA_ID(N'dbo')
        LEFT JOIN sys.columns c ON c.object_id = t.object_id AND c.name = e.Columna
        WHERE c.column_id IS NULL OR c.user_type_id <> TYPE_ID(e.Tipo)
           OR (e.PrecisionEsperada IS NOT NULL AND c.precision <> e.PrecisionEsperada)
           OR (e.EscalaEsperada IS NOT NULL AND c.scale <> e.EscalaEsperada)
    )
        THROW 51603, 'Las dependencias no coinciden con el esquema esperado. No se modificaron.', 1;

    IF EXISTS
    (
        SELECT 1 FROM (VALUES (N'Producto'), (N'Usuario'), (N'DepositoStock'), (N'Lote')) e(Tabla)
        WHERE NOT EXISTS
        (
            SELECT 1 FROM sys.indexes i
            INNER JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.key_ordinal = 1
            INNER JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
            WHERE i.object_id = OBJECT_ID(N'dbo.' + e.Tabla, N'U') AND i.is_unique = 1
              AND i.is_disabled = 0 AND i.has_filter = 0 AND c.name = N'Id'
              AND NOT EXISTS (SELECT 1 FROM sys.index_columns x
                              WHERE x.object_id = i.object_id AND x.index_id = i.index_id AND x.key_ordinal > 1)
        )
    )
        THROW 51604, 'Falta una clave unica Id en las dependencias. No se modificaron.', 1;

    CREATE TABLE dbo.RemitoInterno
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_RemitoInterno PRIMARY KEY,
        ClaveOperacion uniqueidentifier NOT NULL CONSTRAINT UQ_RemitoInterno_Operacion UNIQUE,
        Estado nvarchar(30) NOT NULL CONSTRAINT DF_RemitoInterno_Estado DEFAULT (N'Borrador'),
        UsuarioCreadorId int NOT NULL REFERENCES dbo.Usuario(Id),
        FechaCreacion datetime2(0) NOT NULL CONSTRAINT DF_RemitoInterno_Fecha DEFAULT (SYSDATETIME()),
        UsuarioEmisorId int NULL REFERENCES dbo.Usuario(Id),
        FechaEmision datetime2(0) NULL,
        UsuarioCanceladorId int NULL REFERENCES dbo.Usuario(Id),
        FechaCancelacion datetime2(0) NULL,
        MotivoCancelacion nvarchar(500) NULL,
        CONSTRAINT CK_RemitoInterno_Estado CHECK (Estado IN
            (N'Borrador', N'Cancelado', N'En tránsito', N'Recibido', N'Con diferencias', N'Cerrado')),
        CONSTRAINT CK_RemitoInterno_Operacion CHECK (ClaveOperacion <> '00000000-0000-0000-0000-000000000000'),
        CONSTRAINT CK_RemitoInterno_Fechas CHECK
            ((FechaEmision IS NULL OR FechaEmision >= FechaCreacion)
             AND (FechaCancelacion IS NULL OR FechaCancelacion >= FechaCreacion)),
        CONSTRAINT CK_RemitoInterno_Ciclo CHECK
        (
            (Estado = N'Borrador' AND UsuarioEmisorId IS NULL AND FechaEmision IS NULL
                AND UsuarioCanceladorId IS NULL AND FechaCancelacion IS NULL AND MotivoCancelacion IS NULL)
            OR (Estado = N'Cancelado' AND UsuarioEmisorId IS NULL AND FechaEmision IS NULL
                AND UsuarioCanceladorId IS NOT NULL AND FechaCancelacion IS NOT NULL
                AND MotivoCancelacion IS NOT NULL AND LEN(LTRIM(RTRIM(MotivoCancelacion))) > 0)
            OR (Estado IN (N'En tránsito', N'Recibido', N'Con diferencias', N'Cerrado')
                AND UsuarioEmisorId IS NOT NULL AND FechaEmision IS NOT NULL
                AND UsuarioCanceladorId IS NULL AND FechaCancelacion IS NULL AND MotivoCancelacion IS NULL)
        )
    );

    CREATE TABLE dbo.RemitoDetalle
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_RemitoDetalle PRIMARY KEY,
        RemitoId int NOT NULL REFERENCES dbo.RemitoInterno(Id),
        ProductoId int NOT NULL REFERENCES dbo.Producto(Id),
        CantidadEnviada decimal(10,3) NOT NULL,
        CONSTRAINT CK_RemitoDetalle_Cantidad CHECK (CantidadEnviada > 0),
        CONSTRAINT UQ_RemitoDetalle_Producto UNIQUE (RemitoId, ProductoId),
        CONSTRAINT UQ_RemitoDetalle_Remito UNIQUE (Id, RemitoId)
    );

    CREATE TABLE dbo.RemitoLote
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_RemitoLote PRIMARY KEY,
        RemitoId int NOT NULL,
        RemitoDetalleId int NOT NULL,
        DepositoStockId int NOT NULL REFERENCES dbo.DepositoStock(Id),
        CantidadEnviada decimal(10,3) NOT NULL,
        FechaIngresoOrigen datetime2(0) NULL,
        FechaVencimiento datetime2(0) NULL,
        CONSTRAINT FK_RemitoLote_Detalle FOREIGN KEY (RemitoDetalleId, RemitoId) REFERENCES dbo.RemitoDetalle(Id, RemitoId),
        CONSTRAINT CK_RemitoLote_Cantidad CHECK (CantidadEnviada > 0),
        CONSTRAINT UQ_RemitoLote_Origen UNIQUE (RemitoId, DepositoStockId),
        CONSTRAINT UQ_RemitoLote_Cantidad UNIQUE (Id, RemitoId, CantidadEnviada)
    );

    CREATE TABLE dbo.RecepcionRemito
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_RecepcionRemito PRIMARY KEY,
        RemitoId int NOT NULL CONSTRAINT UQ_RecepcionRemito_Remito UNIQUE REFERENCES dbo.RemitoInterno(Id),
        ClaveOperacion uniqueidentifier NOT NULL CONSTRAINT UQ_RecepcionRemito_Operacion UNIQUE,
        UsuarioReceptorId int NOT NULL REFERENCES dbo.Usuario(Id),
        FechaRecepcion datetime2(0) NOT NULL CONSTRAINT DF_RecepcionRemito_Fecha DEFAULT (SYSDATETIME()),
        EstadoResultado nvarchar(30) NOT NULL,
        CONSTRAINT CK_RecepcionRemito_Estado CHECK (EstadoResultado IN (N'Recibido', N'Con diferencias')),
        CONSTRAINT CK_RecepcionRemito_Operacion CHECK (ClaveOperacion <> '00000000-0000-0000-0000-000000000000'),
        CONSTRAINT UQ_RecepcionRemito_RemitoId UNIQUE (Id, RemitoId)
    );

    CREATE TABLE dbo.RecepcionRemitoLote
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_RecepcionRemitoLote PRIMARY KEY,
        RecepcionRemitoId int NOT NULL,
        RemitoId int NOT NULL,
        RemitoLoteId int NOT NULL,
        -- Copia vinculada por FK al valor enviado original, para validar la diferencia.
        CantidadEnviada decimal(10,3) NOT NULL,
        -- Recibida significa aceptada y acreditada en stock local vendible.
        CantidadRecibida decimal(10,3) NOT NULL,
        CantidadDiferencia AS (CONVERT(decimal(10,3), CantidadEnviada - CantidadRecibida)) PERSISTED,
        LoteLocalId int NULL REFERENCES dbo.Lote(Id),
        Observacion nvarchar(500) NULL,
        CONSTRAINT FK_RecepcionRemitoLote_Recepcion FOREIGN KEY (RecepcionRemitoId, RemitoId) REFERENCES dbo.RecepcionRemito(Id, RemitoId),
        CONSTRAINT FK_RecepcionRemitoLote_Enviado FOREIGN KEY (RemitoLoteId, RemitoId, CantidadEnviada) REFERENCES dbo.RemitoLote(Id, RemitoId, CantidadEnviada),
        CONSTRAINT UQ_RecepcionRemitoLote_Enviado UNIQUE (RemitoLoteId),
        CONSTRAINT UQ_RecepcionRemitoLote_Remito UNIQUE (Id, RemitoId),
        CONSTRAINT CK_RecepcionRemitoLote_Cantidad CHECK
            (CantidadEnviada > 0 AND CantidadRecibida >= 0 AND CantidadRecibida <= CantidadEnviada),
        CONSTRAINT CK_RecepcionRemitoLote_Destino CHECK
            ((CantidadRecibida = 0 AND LoteLocalId IS NULL) OR (CantidadRecibida > 0 AND LoteLocalId IS NOT NULL))
    );

    CREATE TABLE dbo.ResolucionDiferencia
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ResolucionDiferencia PRIMARY KEY,
        RemitoId int NOT NULL REFERENCES dbo.RemitoInterno(Id),
        ClaveOperacion uniqueidentifier NOT NULL CONSTRAINT UQ_ResolucionDiferencia_Operacion UNIQUE,
        UsuarioResponsableId int NOT NULL REFERENCES dbo.Usuario(Id),
        FechaResolucion datetime2(0) NOT NULL CONSTRAINT DF_ResolucionDiferencia_Fecha DEFAULT (SYSDATETIME()),
        Tipo nvarchar(30) NOT NULL,
        -- Solo operaciones definitivas. No se editan/anulan: se compensan con otro evento.
        Estado nvarchar(20) NOT NULL CONSTRAINT DF_ResolucionDiferencia_Estado DEFAULT (N'Aplicada'),
        Motivo nvarchar(500) NOT NULL,
        CONSTRAINT UQ_ResolucionDiferencia_Remito UNIQUE (Id, RemitoId, Tipo),
        CONSTRAINT CK_ResolucionDiferencia_Tipo CHECK (Tipo IN (N'Recepción posterior', N'Devolución depósito', N'Pérdida documentada')),
        CONSTRAINT CK_ResolucionDiferencia_Estado CHECK (Estado = N'Aplicada'),
        CONSTRAINT CK_ResolucionDiferencia_Motivo CHECK (LEN(LTRIM(RTRIM(Motivo))) > 0),
        CONSTRAINT CK_ResolucionDiferencia_Operacion CHECK (ClaveOperacion <> '00000000-0000-0000-0000-000000000000')
    );

    CREATE TABLE dbo.ResolucionDiferenciaDetalle
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ResolucionDiferenciaDetalle PRIMARY KEY,
        ResolucionId int NOT NULL,
        RemitoId int NOT NULL,
        Tipo nvarchar(30) NOT NULL,
        RecepcionRemitoLoteId int NOT NULL,
        Cantidad decimal(10,3) NOT NULL,
        LoteLocalDestinoId int NULL REFERENCES dbo.Lote(Id),
        DepositoStockDestinoId int NULL REFERENCES dbo.DepositoStock(Id),
        CONSTRAINT FK_ResolucionDetalle_Cabecera FOREIGN KEY (ResolucionId, RemitoId, Tipo) REFERENCES dbo.ResolucionDiferencia(Id, RemitoId, Tipo),
        CONSTRAINT FK_ResolucionDetalle_Diferencia FOREIGN KEY (RecepcionRemitoLoteId, RemitoId) REFERENCES dbo.RecepcionRemitoLote(Id, RemitoId),
        CONSTRAINT UQ_ResolucionDetalle_Lote UNIQUE (ResolucionId, RecepcionRemitoLoteId),
        CONSTRAINT CK_ResolucionDetalle_Cantidad CHECK (Cantidad > 0),
        CONSTRAINT CK_ResolucionDetalle_Destino CHECK
        (
            (Tipo = N'Recepción posterior' AND LoteLocalDestinoId IS NOT NULL AND DepositoStockDestinoId IS NULL)
            OR (Tipo = N'Devolución depósito' AND LoteLocalDestinoId IS NULL AND DepositoStockDestinoId IS NOT NULL)
            OR (Tipo = N'Pérdida documentada' AND LoteLocalDestinoId IS NULL AND DepositoStockDestinoId IS NULL)
        )
    );

    -- Cada ingreso nuevo conserva su lote propio y origen; no mezclar destinos.
    CREATE UNIQUE INDEX UX_RecepcionRemitoLote_Local ON dbo.RecepcionRemitoLote(LoteLocalId) WHERE LoteLocalId IS NOT NULL;
    CREATE UNIQUE INDEX UX_ResolucionDetalle_Local ON dbo.ResolucionDiferenciaDetalle(LoteLocalDestinoId) WHERE LoteLocalDestinoId IS NOT NULL;
    CREATE UNIQUE INDEX UX_ResolucionDetalle_Deposito ON dbo.ResolucionDiferenciaDetalle(DepositoStockDestinoId) WHERE DepositoStockDestinoId IS NOT NULL;
    CREATE INDEX IX_RemitoInterno_Pendientes ON dbo.RemitoInterno(Estado, FechaEmision, Id);
    CREATE INDEX IX_RemitoDetalle_Producto ON dbo.RemitoDetalle(ProductoId, RemitoId);
    CREATE INDEX IX_RemitoLote_Origen ON dbo.RemitoLote(DepositoStockId);
    CREATE INDEX IX_RecepcionRemito_Fecha ON dbo.RecepcionRemito(FechaRecepcion, Id);
    CREATE INDEX IX_ResolucionDiferencia_Remito ON dbo.ResolucionDiferencia(RemitoId, FechaResolucion);
    CREATE INDEX IX_ResolucionDetalle_Pendiente ON dbo.ResolucionDiferenciaDetalle(RecepcionRemitoLoteId) INCLUDE (Cantidad, ResolucionId);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

/*
    CONTRATO PENDIENTE PARA LOS FUTUROS PROCEDIMIENTOS (no implementados aqui):
    - Mismo producto en detalle, lote origen y destino; vencimiento/origen inmutables.
    - Suma de lotes = detalle; detalles no vacios al emitir; todos los lotes en recepcion.
    - No resolver mas que CantidadDiferencia menos SUM(resoluciones aplicadas).
    - Estados Pendiente/Parcialmente resuelta/Resuelta se derivan de esas cantidades.
    - No acreditar vencidos al local vendible. Resolver su incidencia sin ocultar saldos.
    - Verificar usuarios activos y roles; CHECK no autentica al operador.
    - Impedir UPDATE/DELETE directos y transiciones hacia atras al habilitar el circuito.
    - Las UNIQUE de destino no cubren reutilizacion cruzada entre recepcion y resolucion:
      crear siempre lotes nuevos y comprobarlo transaccionalmente en los SP.
    - Una anulacion posterior exige eventos compensatorios con vinculo al original.
      Este esquema cubre diferencias pendientes, NO autoriza revertir recepciones ya
      aceptadas; esa extension debe disenarse antes de habilitar anulaciones posteriores.
*/
