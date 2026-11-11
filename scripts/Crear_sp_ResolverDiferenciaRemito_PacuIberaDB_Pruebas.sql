/*
    PREPARADO, NO EJECUTADO. Solo PacuIberaDB_Pruebas.
    Instala un tipo tabular y un SP nuevos. No cambia tablas ni SP existentes.
    Esquema revisado en Crear_EsquemaRemitosInternos_PacuIberaDB_Pruebas.sql y en
    la recepcion definitiva. No se consulto la base: se comprueba el contrato al instalar.

    Una llamada = un remito, un tipo, un motivo y una o mas diferencias parciales.
    Entrada: RecepcionRemitoLote.Id (NO RemitoLote.Id), Cantidad positiva decimal(10,3).
    Tipos canonicos exactos del CHECK existente:
      Recepción posterior: lote local NUEVO, solo vigente, suma Producto.StockActual.
      Devolución depósito: lote deposito NUEVO, sin sumar Producto.StockActual.
        Conserva vencimiento; Activo=0 si vencido o producto inactivo. El saldo fisico
        queda registrado sin habilitar su uso. No reactiva ni repone el lote original.
      Pérdida documentada: no crea lotes ni mueve existencias.
    Todos requieren Administrador activo y motivo; no se amplian permisos del menu.
    La confirmacion fisica/documental es responsabilidad del operador autorizado:
    estas tablas no contienen comprobantes ni un segundo aprobador.

    Una resolucion aplicada es inmutable. Solo se inserta en ResolucionDiferencia y
    su detalle. RecepcionRemito y RecepcionRemitoLote no se actualizan: su diferencia
    original permanece. Pendiente = diferencia original - SUM(resoluciones Aplicada).
    Si todos los pendientes del remito son cero, solo RemitoInterno pasa a Cerrado.
    Si queda saldo, sigue Con diferencias. No se hacen devoluciones automaticas.

    Idempotencia: clave + remito + responsable + tipo canonico + motivo EXACTO
    (comparacion binaria, sin recortar) + conjunto diferencia/cantidad. Replay identico
    devuelve ResolucionId/fecha/tipo originales y estado ACTUAL del remito, sin mover stock.
    Un UUID debe conservarse en reintentos. TVP convierte a decimal(10,3) ANTES del SP:
    el futuro cliente debe rechazar precision extra antes de enviar, sin redondear.

    Orden: app lock por clave -> Usuario/Rol estable -> Remito UPDLOCK -> historial
    estable -> Producto por Id ascendente -> nuevos lotes. La cabecera serializa
    resoluciones con claves distintas del mismo remito y evita sobrepasar pendientes.
    No garantiza ausencia de deadlocks contra ventas/emision/transferencia antigua:
    sus ordenes deben probarse juntos; rollback y consulta por MISMA clave ante errores.

    LIMITACION: no admite anular una resolucion aplicada ni retornar cantidades ya
    aceptadas por el local. Faltan vinculo al evento compensado y reglas de reversa.
    Cambio minimo A DISENAR/APROBAR: FK de compensacion al detalle original y una
    clasificacion/direccion compensatoria con limites acumulados y CHECK coherentes.
    No se agregan esas columnas ni se fuerzan cantidades negativas en este script.
    No hay estado persistido Parcialmente resuelta: es un resultado derivado del saldo.
*/
USE PacuIberaDB_Pruebas;
IF DB_NAME()<>N'PacuIberaDB_Pruebas' THROW 52000,'Instalacion exclusiva en PacuIberaDB_Pruebas.',1;
IF @@TRANCOUNT<>0
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
DECLARE @Instalar int;
EXEC @Instalar=sys.sp_getapplock @Resource=N'PacuIbera.InstalarResolucionRemito.v1',
    @LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=0;
IF @Instalar<0 THROW 52001,'Otra instalacion de resolucion esta en curso.',1;
DECLARE @Columnas TABLE (Tabla sysname,Columna sysname,Tipo sysname,P tinyint NULL,S tinyint NULL);
INSERT @Columnas VALUES
 (N'ResolucionDiferencia',N'Id',N'int',NULL,NULL),
 (N'ResolucionDiferencia',N'RemitoId',N'int',NULL,NULL),
 (N'ResolucionDiferencia',N'ClaveOperacion',N'uniqueidentifier',NULL,NULL),
 (N'ResolucionDiferencia',N'UsuarioResponsableId',N'int',NULL,NULL),
 (N'ResolucionDiferencia',N'FechaResolucion',N'datetime2',NULL,0),
 (N'ResolucionDiferencia',N'Tipo',N'nvarchar',NULL,NULL),
 (N'ResolucionDiferencia',N'Estado',N'nvarchar',NULL,NULL),
 (N'ResolucionDiferencia',N'Motivo',N'nvarchar',NULL,NULL),
 (N'ResolucionDiferenciaDetalle',N'ResolucionId',N'int',NULL,NULL),
 (N'ResolucionDiferenciaDetalle',N'RemitoId',N'int',NULL,NULL),
 (N'ResolucionDiferenciaDetalle',N'Tipo',N'nvarchar',NULL,NULL),
 (N'ResolucionDiferenciaDetalle',N'RecepcionRemitoLoteId',N'int',NULL,NULL),
 (N'ResolucionDiferenciaDetalle',N'Cantidad',N'decimal',10,3),
 (N'ResolucionDiferenciaDetalle',N'LoteLocalDestinoId',N'int',NULL,NULL),
 (N'ResolucionDiferenciaDetalle',N'DepositoStockDestinoId',N'int',NULL,NULL),
 (N'RecepcionRemito',N'Id',N'int',NULL,NULL),
 (N'RecepcionRemito',N'RemitoId',N'int',NULL,NULL),
 (N'RecepcionRemito',N'FechaRecepcion',N'datetime2',NULL,0),
 (N'RecepcionRemito',N'EstadoResultado',N'nvarchar',NULL,NULL),
 (N'RecepcionRemitoLote',N'Id',N'int',NULL,NULL),
 (N'RecepcionRemitoLote',N'RecepcionRemitoId',N'int',NULL,NULL),
 (N'RecepcionRemitoLote',N'RemitoId',N'int',NULL,NULL),
 (N'RecepcionRemitoLote',N'RemitoLoteId',N'int',NULL,NULL),
 (N'RecepcionRemitoLote',N'CantidadEnviada',N'decimal',10,3),
 (N'RecepcionRemitoLote',N'CantidadRecibida',N'decimal',10,3),
 (N'RecepcionRemitoLote',N'CantidadDiferencia',N'decimal',10,3),
 (N'RemitoInterno',N'Id',N'int',NULL,NULL),(N'RemitoInterno',N'Estado',N'nvarchar',NULL,NULL),
 (N'RemitoLote',N'Id',N'int',NULL,NULL),(N'RemitoLote',N'RemitoId',N'int',NULL,NULL),
 (N'RemitoLote',N'RemitoDetalleId',N'int',NULL,NULL),(N'RemitoLote',N'DepositoStockId',N'int',NULL,NULL),
 (N'RemitoLote',N'CantidadEnviada',N'decimal',10,3),(N'RemitoLote',N'FechaVencimiento',N'datetime2',NULL,0),
 (N'RemitoDetalle',N'Id',N'int',NULL,NULL),(N'RemitoDetalle',N'RemitoId',N'int',NULL,NULL),
 (N'RemitoDetalle',N'ProductoId',N'int',NULL,NULL),
 (N'Producto',N'Id',N'int',NULL,NULL),(N'Producto',N'Activo',N'bit',NULL,NULL),
 (N'Producto',N'SeVendePorPeso',N'bit',NULL,NULL),(N'Producto',N'StockActual',N'decimal',10,3),
 (N'Lote',N'Id',N'int',NULL,NULL),(N'Lote',N'ProductoId',N'int',NULL,NULL),
 (N'Lote',N'FechaIngreso',N'datetime',NULL,NULL),(N'Lote',N'FechaVencimiento',N'datetime',NULL,NULL),
 (N'Lote',N'StockInicial',N'decimal',10,3),(N'Lote',N'StockActual',N'decimal',10,3),(N'Lote',N'Activo',N'bit',NULL,NULL),
 (N'DepositoStock',N'Id',N'int',NULL,NULL),(N'DepositoStock',N'ProductoId',N'int',NULL,NULL),
 (N'DepositoStock',N'FechaIngreso',N'datetime2',NULL,0),(N'DepositoStock',N'FechaVencimiento',N'datetime2',NULL,0),
 (N'DepositoStock',N'StockInicial',N'decimal',10,3),(N'DepositoStock',N'StockActual',N'decimal',10,3),
 (N'DepositoStock',N'Activo',N'bit',NULL,NULL),
 (N'Usuario',N'Id',N'int',NULL,NULL),(N'Usuario',N'Activo',N'bit',NULL,NULL),
 (N'Usuario',N'RolId',N'int',NULL,NULL),(N'Rol',N'Id',N'int',NULL,NULL);
IF EXISTS
(
 SELECT 1 FROM @Columnas e LEFT JOIN sys.tables t ON t.schema_id=SCHEMA_ID(N'dbo') AND t.name=e.Tabla
 LEFT JOIN sys.columns c ON c.object_id=t.object_id AND c.name=e.Columna
 WHERE c.column_id IS NULL OR c.user_type_id<>TYPE_ID(e.Tipo)
    OR (e.P IS NOT NULL AND c.precision<>e.P) OR (e.S IS NOT NULL AND c.scale<>e.S)
) THROW 52002,'Las columnas o tipos no coinciden con el esquema esperado.',1;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.Rol') AND name=N'Nombre'
 AND user_type_id IN (TYPE_ID(N'varchar'),TYPE_ID(N'nvarchar')))
 THROW 52003,'Falta Rol.Nombre de texto.',1;
IF EXISTS (SELECT 1 FROM (VALUES(N'ResolucionDiferencia'),(N'Lote'),(N'DepositoStock')) e(Tabla)
 WHERE ISNULL(COLUMNPROPERTY(OBJECT_ID(N'dbo.'+e.Tabla),N'Id','IsIdentity'),0)<>1)
 THROW 52004,'Faltan claves IDENTITY de resolucion o lotes destino.',1;
IF NOT EXISTS
(
 SELECT 1 FROM sys.indexes i INNER JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
 INNER JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
 WHERE i.object_id=OBJECT_ID(N'dbo.ResolucionDiferencia') AND i.is_unique=1 AND i.has_filter=0 AND i.is_disabled=0
   AND ic.key_ordinal=1 AND c.name=N'ClaveOperacion'
   AND NOT EXISTS (SELECT 1 FROM sys.index_columns x WHERE x.object_id=i.object_id AND x.index_id=i.index_id AND x.key_ordinal>1)
) THROW 52005,'Falta unicidad de ClaveOperacion en resoluciones.',1;
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE parent_object_id IN
 (OBJECT_ID(N'dbo.ResolucionDiferencia'),OBJECT_ID(N'dbo.ResolucionDiferenciaDetalle'),OBJECT_ID(N'dbo.RecepcionRemitoLote'))
 AND (is_disabled=1 OR is_not_trusted=1))
 OR EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id IN
 (OBJECT_ID(N'dbo.ResolucionDiferencia'),OBJECT_ID(N'dbo.ResolucionDiferenciaDetalle'))
 AND (is_disabled=1 OR is_not_trusted=1))
 THROW 52006,'Hay restricciones deshabilitadas o no confiables.',1;
IF EXISTS (SELECT 1 FROM sys.triggers WHERE is_disabled=0 AND parent_class=1 AND parent_id IN
 (OBJECT_ID(N'dbo.ResolucionDiferencia'),OBJECT_ID(N'dbo.ResolucionDiferenciaDetalle'),
  OBJECT_ID(N'dbo.RemitoInterno'),OBJECT_ID(N'dbo.Producto'),OBJECT_ID(N'dbo.Lote'),OBJECT_ID(N'dbo.DepositoStock')))
 THROW 52007,'Hay triggers activos en destinos: revisar efectos antes de instalar; no se deshabilitan.',1;

IF TYPE_ID(N'dbo.ResolucionDiferenciaEntrada') IS NULL
 EXEC(N'CREATE TYPE dbo.ResolucionDiferenciaEntrada AS TABLE
 (RecepcionRemitoLoteId int NOT NULL PRIMARY KEY,Cantidad decimal(10,3) NOT NULL);');
DECLARE @TipoObjeto int=(SELECT type_table_object_id FROM sys.table_types
 WHERE user_type_id=TYPE_ID(N'dbo.ResolucionDiferenciaEntrada'));
IF @TipoObjeto IS NULL OR (SELECT COUNT(*) FROM sys.columns WHERE object_id=@TipoObjeto)<>2
 OR NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=@TipoObjeto AND column_id=1
    AND name=N'RecepcionRemitoLoteId' AND user_type_id=TYPE_ID(N'int') AND is_nullable=0)
 OR NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=@TipoObjeto AND column_id=2
    AND name=N'Cantidad' AND user_type_id=TYPE_ID(N'decimal') AND precision=10 AND scale=3 AND is_nullable=0)
 OR NOT EXISTS
 (
  SELECT 1 FROM sys.indexes i INNER JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
  WHERE i.object_id=@TipoObjeto AND i.is_primary_key=1 AND ic.column_id=1 AND ic.key_ordinal=1
   AND NOT EXISTS (SELECT 1 FROM sys.index_columns x WHERE x.object_id=i.object_id AND x.index_id=i.index_id AND x.key_ordinal>1)
 ) THROW 52008,'El tipo de entrada existente es incompatible; no se elimina ni recrea.',1;

EXEC(N'
CREATE OR ALTER PROCEDURE dbo.sp_ResolverDiferenciaRemito
 @RemitoId int,
 @ClaveOperacion uniqueidentifier,
 @UsuarioResponsableId int,
 @Tipo nvarchar(max),
 @Motivo nvarchar(max),
 @Lotes dbo.ResolucionDiferenciaEntrada READONLY
AS
BEGIN
 SET NOCOUNT ON;
 IF @@TRANCOUNT<>0
 BEGIN
  RAISERROR(''La resolucion requiere llamada sin transaccion externa.'',16,1);
  RETURN;
 END;
 SET IMPLICIT_TRANSACTIONS OFF;
 SET XACT_ABORT ON;
 SET ANSI_PADDING ON;
 SET ANSI_WARNINGS ON;
 SET ARITHABORT ON;
 SET CONCAT_NULL_YIELDS_NULL ON;
 SET NUMERIC_ROUNDABORT OFF;
 DECLARE @TipoCanon nvarchar(30),@Id int,@RemitoAnterior int,@UsuarioAnterior int,
  @TipoAnterior nvarchar(30),@MotivoAnterior nvarchar(500),@FechaAnterior datetime2(0),@EstadoAnterior nvarchar(20);
 DECLARE @EstadoRemito nvarchar(30),@RecepcionId int,@FechaRecepcion datetime2(0),@Ahora datetime2(0),@Hoy date;
 DECLARE @Lock int,@Recurso nvarchar(255),@ProductoId int,@PorPeso bit,@Activo bit,
  @SaldoLocal decimal(10,3),@TotalProducto decimal(38,3);
 DECLARE @DiferenciaId int,@Cantidad decimal(10,3),@Vence datetime2(0),@LocalId int,@DepositoId int;
 DECLARE @Pendientes TABLE
 (DiferenciaId int PRIMARY KEY,ProductoId int NOT NULL,RemitoLoteId int NOT NULL,DepositoOrigenId int NOT NULL,
  Enviada decimal(10,3) NOT NULL,Recibida decimal(10,3) NOT NULL,Diferencia decimal(10,3) NOT NULL,
  Resuelto decimal(38,3) NOT NULL,Vencimiento datetime2(0) NULL);
 DECLARE @Trabajo TABLE
 (DiferenciaId int PRIMARY KEY,ProductoId int NOT NULL,Cantidad decimal(10,3) NOT NULL,Vencimiento datetime2(0) NULL);
 BEGIN TRY
  IF @RemitoId IS NULL OR @RemitoId<=0 OR @UsuarioResponsableId IS NULL
   THROW 52010,''Remito y responsable son obligatorios.'',1;
  IF @ClaveOperacion IS NULL OR @ClaveOperacion=''00000000-0000-0000-0000-000000000000''
   THROW 52011,''La clave de operacion es obligatoria y no puede ser vacia.'',1;
  SET @TipoCanon=CASE @Tipo WHEN N''Recepción posterior'' THEN N''Recepción posterior''
   WHEN N''Devolución depósito'' THEN N''Devolución depósito'' WHEN N''Pérdida documentada'' THEN N''Pérdida documentada'' END;
  IF @TipoCanon IS NULL THROW 52012,''Tipo de resolucion no admitido.'',1;
  IF @Motivo IS NULL OR LEN(LTRIM(RTRIM(@Motivo)))=0 OR DATALENGTH(@Motivo)>1000
   THROW 52013,''El motivo es obligatorio y debe caber en nvarchar(500), sin truncarlo.'',1;
  IF NOT EXISTS (SELECT 1 FROM @Lotes) OR EXISTS (SELECT 1 FROM @Lotes WHERE RecepcionRemitoLoteId<=0 OR Cantidad<=0)
   THROW 52014,''Indique al menos una diferencia con cantidad positiva.'',1;
  BEGIN TRANSACTION;
  SET @Recurso=N''PacuIbera.ResolverDiferencia.''+CONVERT(nvarchar(36),@ClaveOperacion);
  EXEC @Lock=sys.sp_getapplock @Resource=@Recurso,@LockMode=N''Exclusive'',@LockOwner=N''Transaction'',@LockTimeout=15000;
  IF @Lock<0 THROW 52015,''Operacion ocupada: consultar la clave antes de reintentar.'',1;
  IF NOT EXISTS (SELECT 1 FROM dbo.Usuario u WITH (HOLDLOCK) INNER JOIN dbo.Rol r WITH (HOLDLOCK) ON r.Id=u.RolId
   WHERE u.Id=@UsuarioResponsableId AND u.Activo=1 AND r.Nombre=N''Administrador'')
   THROW 52016,''El responsable debe ser un Administrador activo.'',1;
  SELECT @EstadoRemito=Estado FROM dbo.RemitoInterno WITH (UPDLOCK,HOLDLOCK) WHERE Id=@RemitoId;
  IF @EstadoRemito IS NULL THROW 52017,''El remito no existe.'',1;

  SELECT @Id=Id,@RemitoAnterior=RemitoId,@UsuarioAnterior=UsuarioResponsableId,@TipoAnterior=Tipo,
   @MotivoAnterior=Motivo,@FechaAnterior=FechaResolucion,@EstadoAnterior=Estado
  FROM dbo.ResolucionDiferencia WITH (UPDLOCK,HOLDLOCK) WHERE ClaveOperacion=@ClaveOperacion;
  IF @Id IS NOT NULL
  BEGIN
   IF @RemitoAnterior<>@RemitoId OR @UsuarioAnterior<>@UsuarioResponsableId OR @TipoAnterior<>@TipoCanon
      OR CONVERT(varbinary(max),@MotivoAnterior)<>CONVERT(varbinary(max),@Motivo)
    THROW 52018,''La clave ya corresponde a otro remito, responsable, tipo o motivo.'',1;
   IF @EstadoAnterior<>N''Aplicada'' THROW 52019,''La resolucion original no esta Aplicada; requiere revision.'',1;
   IF EXISTS (SELECT RecepcionRemitoLoteId,Cantidad FROM @Lotes EXCEPT
              SELECT RecepcionRemitoLoteId,Cantidad FROM dbo.ResolucionDiferenciaDetalle WHERE ResolucionId=@Id)
      OR EXISTS (SELECT RecepcionRemitoLoteId,Cantidad FROM dbo.ResolucionDiferenciaDetalle WHERE ResolucionId=@Id
                 EXCEPT SELECT RecepcionRemitoLoteId,Cantidad FROM @Lotes)
    THROW 52020,''La clave ya fue usada con diferencias o cantidades distintas.'',1;
   COMMIT TRANSACTION;
   SELECT @Id AS ResolucionId,@RemitoId AS RemitoId,@TipoAnterior AS Tipo,@FechaAnterior AS FechaResolucion,
    @EstadoAnterior AS EstadoResolucion,@EstadoRemito AS EstadoRemitoActual,CONVERT(bit,1) AS Reutilizado;
   RETURN;
  END;
  IF @EstadoRemito<>N''Con diferencias'' THROW 52021,''Solo se resuelven remitos Con diferencias.'',1;
  SELECT @RecepcionId=Id,@FechaRecepcion=FechaRecepcion FROM dbo.RecepcionRemito WITH (HOLDLOCK)
   WHERE RemitoId=@RemitoId AND EstadoResultado=N''Con diferencias'';
  IF @RecepcionId IS NULL THROW 52022,''Falta una recepcion definitiva Con diferencias.'',1;
  IF EXISTS (SELECT 1 FROM dbo.ResolucionDiferencia WITH (HOLDLOCK) WHERE RemitoId=@RemitoId AND Estado<>N''Aplicada'')
   THROW 52023,''Existen resoluciones con estado no soportado.'',1;
  INSERT @Pendientes (DiferenciaId,ProductoId,RemitoLoteId,DepositoOrigenId,Enviada,Recibida,Diferencia,Resuelto,Vencimiento)
  SELECT rr.Id,rd.ProductoId,rl.Id,rl.DepositoStockId,rr.CantidadEnviada,rr.CantidadRecibida,rr.CantidadDiferencia,
   ISNULL(s.Cantidad,0),rl.FechaVencimiento
  FROM dbo.RecepcionRemitoLote rr WITH (HOLDLOCK)
  INNER JOIN dbo.RemitoLote rl WITH (HOLDLOCK) ON rl.Id=rr.RemitoLoteId AND rl.RemitoId=rr.RemitoId
  INNER JOIN dbo.RemitoDetalle rd WITH (HOLDLOCK) ON rd.Id=rl.RemitoDetalleId AND rd.RemitoId=rl.RemitoId
  OUTER APPLY
  (
   SELECT SUM(d.Cantidad) AS Cantidad FROM dbo.ResolucionDiferenciaDetalle d WITH (HOLDLOCK)
   INNER JOIN dbo.ResolucionDiferencia h WITH (HOLDLOCK) ON h.Id=d.ResolucionId AND h.RemitoId=d.RemitoId
   WHERE d.RecepcionRemitoLoteId=rr.Id AND d.RemitoId=@RemitoId AND h.Estado=N''Aplicada''
  ) s
  WHERE rr.RemitoId=@RemitoId AND rr.RecepcionRemitoId=@RecepcionId;
  IF NOT EXISTS (SELECT 1 FROM @Pendientes)
     OR (SELECT COUNT(*) FROM @Pendientes)<>(SELECT COUNT(*) FROM dbo.RecepcionRemitoLote WHERE RecepcionRemitoId=@RecepcionId)
   THROW 52024,''Recepcion sin lotes o con trazabilidad inconsistente.'',1;
  IF EXISTS (SELECT 1 FROM @Pendientes WHERE Diferencia<0 OR Recibida<0 OR Enviada<Recibida
     OR CONVERT(decimal(18,3),Enviada)-CONVERT(decimal(18,3),Recibida)<>Diferencia
     OR Resuelto<0 OR Resuelto>Diferencia)
   THROW 52025,''Las diferencias o resoluciones historicas no concilian; no se corrigen automaticamente.'',1;
  INSERT @Trabajo SELECT p.DiferenciaId,p.ProductoId,e.Cantidad,p.Vencimiento
   FROM @Lotes e INNER JOIN @Pendientes p ON p.DiferenciaId=e.RecepcionRemitoLoteId;
  IF (SELECT COUNT(*) FROM @Trabajo)<>(SELECT COUNT(*) FROM @Lotes)
   THROW 52026,''Se informaron diferencias ajenas a la recepcion del remito.'',1;
  -- Resuelto se compara antes de acotarlo: resta decimal(18,3), sin perder escala.
  IF EXISTS (SELECT 1 FROM @Trabajo t INNER JOIN @Pendientes p ON p.DiferenciaId=t.DiferenciaId
             WHERE t.Cantidad>CONVERT(decimal(18,3),p.Diferencia)-CONVERT(decimal(18,3),p.Resuelto))
   THROW 52027,''Una cantidad supera el saldo pendiente de la diferencia.'',1;

  DECLARE validarProductos CURSOR LOCAL FAST_FORWARD FOR
   SELECT ProductoId,SUM(Cantidad) FROM @Trabajo GROUP BY ProductoId ORDER BY ProductoId;
  OPEN validarProductos;
  FETCH NEXT FROM validarProductos INTO @ProductoId,@TotalProducto;
  WHILE @@FETCH_STATUS=0
  BEGIN
   SET @PorPeso=NULL;
   SELECT @PorPeso=SeVendePorPeso,@Activo=Activo,@SaldoLocal=StockActual
   FROM dbo.Producto WITH (UPDLOCK,HOLDLOCK) WHERE Id=@ProductoId;
   IF @PorPeso IS NULL THROW 52028,''Producto inexistente o sin unidad de venta definida.'',1;
   IF @PorPeso=0 AND
      (EXISTS (SELECT 1 FROM @Trabajo WHERE ProductoId=@ProductoId AND Cantidad<>FLOOR(Cantidad))
       OR EXISTS (SELECT 1 FROM @Pendientes WHERE ProductoId=@ProductoId
                  AND (Enviada<>FLOOR(Enviada) OR Recibida<>FLOOR(Recibida) OR Resuelto<>FLOOR(Resuelto))))
    THROW 52029,''Un producto por unidad tiene cantidades fraccionarias; requiere conciliacion.'',1;
   IF @TipoCanon=N''Recepción posterior''
   BEGIN
    IF ISNULL(@Activo,0)<>1 OR @SaldoLocal IS NULL OR @SaldoLocal<0
     THROW 52030,''La entrega posterior requiere producto activo y saldo local valido.'',1;
    IF @PorPeso=0 AND @SaldoLocal<>FLOOR(@SaldoLocal)
     THROW 52031,''El saldo local del producto por unidad es fraccionario.'',1;
    IF @TotalProducto>9999999.999-CONVERT(decimal(18,3),@SaldoLocal)
     THROW 52032,''El ingreso excederia decimal(10,3) del saldo local.'',1;
   END;
   FETCH NEXT FROM validarProductos INTO @ProductoId,@TotalProducto;
  END;
  CLOSE validarProductos;
  DEALLOCATE validarProductos;
  SET @Ahora=SYSDATETIME(); SET @Hoy=CONVERT(date,@Ahora);
  IF @Ahora<@FechaRecepcion THROW 52033,''La resolucion no puede ser anterior a la recepcion.'',1;
  IF @TipoCanon=N''Recepción posterior'' AND EXISTS (SELECT 1 FROM @Trabajo WHERE Vencimiento<@Hoy)
   THROW 52034,''No se acredita mercaderia vencida al stock vendible; corresponde otra resolucion justificada.'',1;
  INSERT dbo.ResolucionDiferencia (RemitoId,ClaveOperacion,UsuarioResponsableId,FechaResolucion,Tipo,Estado,Motivo)
   VALUES (@RemitoId,@ClaveOperacion,@UsuarioResponsableId,@Ahora,@TipoCanon,N''Aplicada'',CONVERT(nvarchar(500),@Motivo));
  SET @Id=CONVERT(int,SCOPE_IDENTITY());
  DECLARE aplicarLotes CURSOR LOCAL FAST_FORWARD FOR
   SELECT DiferenciaId,ProductoId,Cantidad,Vencimiento FROM @Trabajo ORDER BY ProductoId,DiferenciaId;
  OPEN aplicarLotes;
  FETCH NEXT FROM aplicarLotes INTO @DiferenciaId,@ProductoId,@Cantidad,@Vence;
  WHILE @@FETCH_STATUS=0
  BEGIN
   SET @LocalId=NULL; SET @DepositoId=NULL;
   IF @TipoCanon=N''Recepción posterior''
   BEGIN
    INSERT dbo.Lote (ProductoId,FechaIngreso,FechaVencimiento,StockInicial,StockActual,Activo)
     VALUES (@ProductoId,@Ahora,@Vence,@Cantidad,@Cantidad,1);
    SET @LocalId=CONVERT(int,SCOPE_IDENTITY());
   END;
   IF @TipoCanon=N''Devolución depósito''
   BEGIN
    SELECT @Activo=Activo FROM dbo.Producto WHERE Id=@ProductoId;
    INSERT dbo.DepositoStock (ProductoId,FechaIngreso,FechaVencimiento,StockInicial,StockActual,Activo)
     VALUES (@ProductoId,@Ahora,@Vence,@Cantidad,@Cantidad,
             CASE WHEN @Activo=1 AND (@Vence IS NULL OR @Vence>=@Hoy) THEN 1 ELSE 0 END);
    SET @DepositoId=CONVERT(int,SCOPE_IDENTITY());
   END;
   INSERT dbo.ResolucionDiferenciaDetalle
    (ResolucionId,RemitoId,Tipo,RecepcionRemitoLoteId,Cantidad,LoteLocalDestinoId,DepositoStockDestinoId)
    VALUES (@Id,@RemitoId,@TipoCanon,@DiferenciaId,@Cantidad,@LocalId,@DepositoId);
   FETCH NEXT FROM aplicarLotes INTO @DiferenciaId,@ProductoId,@Cantidad,@Vence;
  END;
  CLOSE aplicarLotes;
  DEALLOCATE aplicarLotes;
  IF @TipoCanon=N''Recepción posterior''
   UPDATE p SET StockActual=p.StockActual+s.Cantidad
   FROM dbo.Producto p INNER JOIN
    (SELECT ProductoId,CONVERT(decimal(18,3),SUM(Cantidad)) AS Cantidad FROM @Trabajo GROUP BY ProductoId) s
    ON s.ProductoId=p.Id;
  -- Las sumas son decimal(38,3). Comparar sin sumarlas al decimal(10,3) original.
  IF EXISTS
  (
   SELECT 1 FROM @Pendientes p OUTER APPLY
    (SELECT SUM(Cantidad) AS Cantidad FROM dbo.ResolucionDiferenciaDetalle
     WHERE RemitoId=@RemitoId AND RecepcionRemitoLoteId=p.DiferenciaId) s
   WHERE ISNULL(s.Cantidad,0)>p.Diferencia
  ) THROW 52035,''Se excedio una diferencia; se revierte toda la operacion.'',1;
  SET @EstadoRemito=CASE WHEN EXISTS
  (
   SELECT 1 FROM @Pendientes p OUTER APPLY
    (SELECT SUM(Cantidad) AS Cantidad FROM dbo.ResolucionDiferenciaDetalle
     WHERE RemitoId=@RemitoId AND RecepcionRemitoLoteId=p.DiferenciaId) s
   WHERE ISNULL(s.Cantidad,0)<p.Diferencia
  ) THEN N''Con diferencias'' ELSE N''Cerrado'' END;
  UPDATE dbo.RemitoInterno SET Estado=@EstadoRemito WHERE Id=@RemitoId AND Estado=N''Con diferencias'';
  IF @@ROWCOUNT<>1 THROW 52036,''Cambio inesperado del remito: se revierte la resolucion.'',1;
  COMMIT TRANSACTION;
  SELECT @Id AS ResolucionId,@RemitoId AS RemitoId,@TipoCanon AS Tipo,@Ahora AS FechaResolucion,
   N''Aplicada'' AS EstadoResolucion,@EstadoRemito AS EstadoRemitoActual,CONVERT(bit,0) AS Reutilizado;
 END TRY
 BEGIN CATCH
  IF CURSOR_STATUS(''local'',''validarProductos'')>=0 CLOSE validarProductos;
  IF CURSOR_STATUS(''local'',''validarProductos'')>=-1 DEALLOCATE validarProductos;
  IF CURSOR_STATUS(''local'',''aplicarLotes'')>=0 CLOSE aplicarLotes;
  IF CURSOR_STATUS(''local'',''aplicarLotes'')>=-1 DEALLOCATE aplicarLotes;
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
