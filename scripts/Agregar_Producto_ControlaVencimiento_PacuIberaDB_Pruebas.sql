/*
    PREPARADO SOLAMENTE. NO EJECUTADO.
    Destino: PacuIberaDB_Pruebas. Seleccionar esta base ANTES de ejecutar todo
    el archivo en una sola sesion, sin transaccion externa. No usa USE ni GO:
    un destino incorrecto detiene el lote completo, incluso master/PacuIberaDB.

    Ejecutar con la aplicacion cerrada y sin otros escritores. La migracion
    toma bloqueos de tablas para mantener estable la evidencia de clasificacion;
    un timeout/deadlock provoca rollback. No promete ausencia de deadlocks con
    otros circuitos que adquieren bloqueos en distinto orden.

    CAMBIO UNICO PERSISTENTE:
      dbo.Producto.ControlaVencimiento bit NOT NULL, DEFAULT (1).
      Se inicializa SOLO la columna nueva, nunca otras columnas de Producto.
      No cambia Lote, DepositoStock, fechas, cantidades ni movimientos.

    REGLA INICIAL (sin inferir por nombre/categoria/rol/stock):
      1 / Si: cualquier fecha no NULL en lotes locales O del deposito.
      1 / Si: producto sin lotes (desconocido => opcion conservadora).
      0 / No PROVISIONAL: tiene lotes, pero TODOS tienen fecha NULL.
    Se incluyen lotes inactivos y agotados: tambien aportan evidencia historica.
    La fecha 2099-12-31 NO se convierte a NULL ni se descarta como evidencia.
    El codigo antiguo la utilizaba al desmarcar Tiene vencimiento, pero no se
    puede demostrar que todas las fechas de ese dia sean artificiales.

    REVISION MANUAL: todos los No provisionales, productos sin lotes,
    cualquier fecha 2099-12-31 y cualquier mezcla de fechas y NULL.
    No significa que una fecha sea incorrecta ni autoriza cambiarla.
    Observado mediante SELECT al preparar este archivo (no IDs codificados):
      - Sin lotes: 2 EMPANADAS, 1002 FILET DESPINADO, 2005 VINO MALBEC.
      - Solo NULL: 2003 CARBON, 2004 VINO MALBEC, 3007 CARBON.
      - Con 2099: 3009 HIELO 10 KG, 3010 VINO BLANCO,
                   3011 VINO TINTO, 3012 HIELO 2KG.
    El informe final se calcula otra vez con los datos reales al ejecutar;
    no actualiza productos segun esta lista estatica.

    DEFAULT (1) protege nuevas altas de clientes antiguos que omitan la columna.
    Esta migracion NO activa la exigencia de fecha en recepciones: faltan las
    etapas de C# y validacion SQL de recepcion. No confundir con su instalacion.

    REEJECUCION: si la columna ya existe como bit NOT NULL no calculado,
    se conservan valores Y default existentes. Si es incompatible, rechaza sin
    intentar corregirla. No reutilizar la regla inicial para pisar decisiones.
    La creacion de columna/default y su inicializacion son una unica transaccion;
    cualquier fallo previo al COMMIT revierte la migracion completa.
    Los SELECT posteriores al COMMIT son informes: si fallan, la migracion ya
    esta confirmada; reejecutar para verificarlos sin reinicializar la columna.
*/
SET NOCOUNT ON;
IF DB_NAME() <> N'PacuIberaDB_Pruebas'
    THROW 52300, 'Este archivo solo puede ejecutarse en PacuIberaDB_Pruebas. No se cambio ninguna base.', 1;
IF @@TRANCOUNT <> 0
BEGIN
    RAISERROR('Ejecute el archivo sin una transaccion externa abierta.', 16, 1);
    RETURN;
END;
SET IMPLICIT_TRANSACTIONS OFF;
SET XACT_ABORT ON;
SET LOCK_TIMEOUT 15000;

DECLARE @Nueva bit = 0;
DECLARE @ConsultaBase nvarchar(max) = N'
;WITH Fuentes AS
(
    SELECT ProductoId, FechaVencimiento FROM dbo.Lote
    UNION ALL
    SELECT ProductoId, FechaVencimiento FROM dbo.DepositoStock
), Evidencia AS
(
    SELECT p.Id,p.Nombre,p.Activo,COUNT_BIG(f.ProductoId) AS TotalLotes,
        SUM(CONVERT(bigint,CASE WHEN f.FechaVencimiento IS NOT NULL THEN 1 ELSE 0 END)) AS ConFecha,
        SUM(CONVERT(bigint,CASE WHEN CONVERT(date,f.FechaVencimiento)=''20991231'' THEN 1 ELSE 0 END)) AS Fecha2099
    FROM dbo.Producto p LEFT JOIN Fuentes f ON f.ProductoId=p.Id
    GROUP BY p.Id,p.Nombre,p.Activo
), Clasificacion AS
(
    SELECT *,TotalLotes-ConFecha AS SinFecha,
        CONVERT(bit,CASE WHEN TotalLotes>0 AND ConFecha=0 THEN 0 ELSE 1 END) AS ValorInicial,
        CONVERT(bit,CASE WHEN TotalLotes=0 OR ConFecha=0 OR Fecha2099>0 OR ConFecha<TotalLotes THEN 1 ELSE 0 END) AS Revisar,
        CASE WHEN TotalLotes=0 THEN N''Sin historial de lotes: Si conservador; confirmar configuracion.''
             WHEN ConFecha=0 THEN N''Solo fechas NULL: No provisional; confirmar si las futuras cargas requieren fecha.''
             ELSE
                 CASE WHEN Fecha2099>0 THEN N''Existe 31/12/2099: posible fecha artificial del sistema antiguo; no se altera. '' ELSE N'''' END
                 + CASE WHEN ConFecha<TotalLotes THEN N''Hay lotes con fecha y sin fecha: confirmar politica para futuras cargas.'' ELSE N'''' END
        END AS MotivoRevision
    FROM Evidencia
)
';

BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @Bloqueo int;
    EXEC @Bloqueo=sys.sp_getapplock
        @Resource=N'PacuIbera.MigrarProductoControlaVencimiento.v1',
        @LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=15000;
    IF @Bloqueo<0 THROW 52301, 'Otra migracion de ControlaVencimiento esta en curso.', 1;

    IF OBJECT_ID(N'dbo.Producto',N'U') IS NULL
       OR OBJECT_ID(N'dbo.Lote',N'U') IS NULL
       OR OBJECT_ID(N'dbo.DepositoStock',N'U') IS NULL
        THROW 52302, 'Falta Producto, Lote o DepositoStock. No se crean dependencias nuevas.', 1;

    DECLARE @Esperadas TABLE(Tabla sysname,Columna sysname,Tipo sysname);
    INSERT INTO @Esperadas VALUES
        (N'Producto',N'Id',N'int'),(N'Producto',N'Nombre',N'varchar'),(N'Producto',N'Activo',N'bit'),
        (N'Lote',N'ProductoId',N'int'),(N'Lote',N'FechaVencimiento',N'datetime'),
        (N'DepositoStock',N'ProductoId',N'int'),(N'DepositoStock',N'FechaVencimiento',N'datetime2');
    IF EXISTS
    (
        SELECT 1 FROM @Esperadas e LEFT JOIN sys.columns c
            ON c.object_id=OBJECT_ID(N'dbo.'+e.Tabla) AND c.name=e.Columna
        WHERE c.column_id IS NULL OR c.user_type_id<>TYPE_ID(e.Tipo) OR c.is_computed=1
    ) THROW 52303, 'Las columnas de las dependencias no coinciden con el esquema revisado.', 1;

    DECLARE @Filas bigint;
    SELECT @Filas=COUNT_BIG(*) FROM dbo.DepositoStock WITH(TABLOCK,HOLDLOCK);
    SELECT @Filas=COUNT_BIG(*) FROM dbo.Lote WITH(TABLOCK,HOLDLOCK);
    SELECT @Filas=COUNT_BIG(*) FROM dbo.Producto WITH(TABLOCKX,HOLDLOCK);

    -- Comprobar despues de bloquear Producto para impedir un cambio de triggers
    -- entre esta verificacion y el UPDATE de la columna nueva.
    IF EXISTS(SELECT 1 FROM sys.triggers WHERE parent_id=OBJECT_ID(N'dbo.Producto') AND is_disabled=0)
        THROW 52304, 'Producto tiene triggers habilitados. Revisarlos antes de aplicar esta migracion.', 1;

    IF EXISTS(SELECT 1 FROM dbo.Lote l LEFT JOIN dbo.Producto p ON p.Id=l.ProductoId WHERE p.Id IS NULL)
       OR EXISTS(SELECT 1 FROM dbo.DepositoStock l LEFT JOIN dbo.Producto p ON p.Id=l.ProductoId WHERE p.Id IS NULL)
        THROW 52305, 'Hay lotes sin producto valido. Se requiere revisar las dependencias, sin reparar datos automaticamente.', 1;

    IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.Producto') AND name=N'ControlaVencimiento')
    BEGIN
        IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.Producto')
            AND name=N'ControlaVencimiento' AND user_type_id=TYPE_ID(N'bit')
            AND is_nullable=0 AND is_computed=0 AND generated_always_type=0)
            THROW 52306, 'ControlaVencimiento existe con una definicion incompatible. No se modifica.', 1;
    END
    ELSE
    BEGIN
        IF OBJECT_ID(N'dbo.DF_Producto_ControlaVencimiento') IS NOT NULL
            THROW 52307, 'El nombre DF_Producto_ControlaVencimiento ya esta ocupado. Revisar antes de instalar.', 1;
        -- SQL dinamico tambien en los usos posteriores: evita errores de compilacion
        -- al referenciar en el mismo lote una columna que aun no existia.
        EXEC(N'ALTER TABLE dbo.Producto ADD ControlaVencimiento bit NOT NULL
            CONSTRAINT DF_Producto_ControlaVencimiento DEFAULT (1) WITH VALUES;');
        SET @Nueva=1;

        DECLARE @Inicializar nvarchar(max)=@ConsultaBase+N'
            UPDATE p SET ControlaVencimiento=c.ValorInicial
            FROM dbo.Producto p JOIN Clasificacion c ON c.Id=p.Id
            WHERE p.ControlaVencimiento<>c.ValorInicial;';
        EXEC sys.sp_executesql @Inicializar;
        DECLARE @Comprobar nvarchar(max)=@ConsultaBase+N'
            SELECT @Incorrectas=COUNT_BIG(*) FROM Clasificacion c JOIN dbo.Producto p ON p.Id=c.Id
            WHERE p.ControlaVencimiento<>c.ValorInicial;';
        DECLARE @Incorrectas bigint;
        EXEC sys.sp_executesql @Comprobar,N'@Incorrectas bigint OUTPUT',@Incorrectas=@Incorrectas OUTPUT;
        IF @Incorrectas<>0 THROW 52308, 'No coincide la inicializacion. Se revertira toda la migracion.', 1;
    END;

    IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.Producto')
        AND name=N'ControlaVencimiento' AND user_type_id=TYPE_ID(N'bit') AND is_nullable=0 AND is_computed=0)
        THROW 52309, 'No se verifico la columna bit NOT NULL. Se revertira la migracion.', 1;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

-- VERIFICACION POSTERIOR, exclusivamente SELECT. En una reejecucion no cambia valores.
SELECT DB_NAME() AS BaseVerificada,
    CASE WHEN @Nueva=1 THEN N'Columna creada e inicializada; COMMIT completado.'
         ELSE N'Columna compatible ya instalada: valores y default conservados.' END AS Resultado;
SELECT c.name AS Columna,TYPE_NAME(c.user_type_id) AS Tipo,c.is_nullable AS PermiteNull,
       d.name AS RestriccionDefault,d.definition AS DefaultActual
FROM sys.columns c LEFT JOIN sys.default_constraints d ON d.object_id=c.default_object_id
WHERE c.object_id=OBJECT_ID(N'dbo.Producto') AND c.name=N'ControlaVencimiento';
DECLARE @Informe nvarchar(max)=@ConsultaBase+N'
    SELECT c.Id,c.Nombre,c.Activo,
        p.ControlaVencimiento AS ValorGuardado,
        CASE WHEN p.ControlaVencimiento=1 THEN N''Si'' ELSE N''No'' END AS RequiereVencimiento,
        c.TotalLotes,c.ConFecha,c.SinFecha,c.Fecha2099,c.ValorInicial AS InferenciaInicial,
        CONVERT(bit,CASE WHEN c.Revisar=1 OR p.ControlaVencimiento<>c.ValorInicial THEN 1 ELSE 0 END) AS RevisarManualmente,
        CASE WHEN p.ControlaVencimiento<>c.ValorInicial
             THEN N''La configuracion guardada difiere de la inferencia inicial; se conserva. '' ELSE N'''' END
             +c.MotivoRevision AS MotivoRevision
    FROM Clasificacion c JOIN dbo.Producto p ON p.Id=c.Id ORDER BY c.Id;';
EXEC sys.sp_executesql @Informe;
-- No hay updates de confirmacion/reparacion ni cambios de interfaz en este archivo.
