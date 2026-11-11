/*
    PREPARADO SOLAMENTE. NO EJECUTADO. Destino exclusivo PacuIberaDB_Pruebas.
    Aplicar DESPUES de Agregar_Producto_ControlaVencimiento_PacuIberaDB_Pruebas.sql.
    Seleccionar Pruebas y ejecutar el archivo completo sin transaccion externa.
    No usa USE ni GO. No ejecuta recepciones ni modifica registros al instalar.

    Reemplaza UNICAMENTE dbo.sp_ConfirmarRecepcionPedido, conservando sus cuatro
    parametros y TVP. No crea/elimina tipos ni toca los procedimientos de remitos.
    Base: Crear_sp_ConfirmarRecepcionPedido_PacuIberaDB_Pruebas.sql del repositorio,
    cotejada con la definicion instalada mediante SELECT antes de preparar esto.
    Conserva rechazos por pedido vacio, cantidades previamente recibidas, doble
    confirmacion, pertenencia de detalles, sumas, cantidades y fechas invalidas;
    tambien la transaccion, historial, decimal(10,3) y cierre del pedido.

    Nuevo: leer ControlaVencimiento bajo HOLDLOCK, por Producto.Id ascendente,
    y rechazar cada lote sin fecha si la regla actual exige vencimiento. Un detalle
    recibido en cero, sin lotes, sigue siendo valido. No elimina fechas si la regla
    es No. Elegir 'No' permite omitir la fecha, pero no ignora fechas informadas.
    No altera recepciones, vencimientos o existencias historicas.

    Instala una huella SHA2_256 como propiedad extendida DEL MISMO procedimiento,
    dentro de la transaccion de instalacion. C# comprueba esta huella antes de
    confirmar. Si se reinstala la definicion anterior, la huella deja de coincidir
    y la aplicacion bloquea el envio. No editar esta propiedad manualmente.
    Ejecucion futura del SP: los bloqueos pueden entrar en deadlock con circuitos
    que usen otro orden; ante error se revierte la recepcion completa. No reintentar
    automaticamente; consultar primero el estado existente del pedido.
*/
SET NOCOUNT ON;
IF DB_NAME()<>N'PacuIberaDB_Pruebas'
    THROW 52400,'El script solo puede ejecutarse en PacuIberaDB_Pruebas.',1;
IF @@TRANCOUNT<>0
BEGIN
    RAISERROR('Ejecute la instalacion sin una transaccion externa abierta.',16,1);
    RETURN;
END;
SET IMPLICIT_TRANSACTIONS OFF;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @Bloqueo int;
    EXEC @Bloqueo=sys.sp_getapplock @Resource=N'PacuIbera.InstalarRecepcionControlVencimiento.v1',
        @LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=15000;
    IF @Bloqueo<0 THROW 52401,'Otra instalacion de esta proteccion esta en curso.',1;
    IF OBJECT_ID(N'dbo.sp_ConfirmarRecepcionPedido',N'P') IS NULL
        THROW 52402,'Falta el procedimiento original de recepcion. Instalar primero sus dependencias.',1;
    IF EXISTS
    (
        SELECT 1 FROM (VALUES(N'PedidoProveedor'),(N'DetallePedidoProveedor'),(N'DepositoStock'),
            (N'Usuario'),(N'Producto'),(N'RecepcionPedido'),(N'RecepcionDetalle'),(N'RecepcionLote')) t(Nombre)
        WHERE OBJECT_ID(N'dbo.'+t.Nombre,N'U') IS NULL
    ) THROW 52403,'Faltan tablas del circuito de recepcion.',1;
    IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.Producto')
        AND name=N'ControlaVencimiento' AND user_type_id=TYPE_ID(N'bit') AND is_nullable=0 AND is_computed=0)
        THROW 52404,'Debe instalarse primero Producto.ControlaVencimiento bit NOT NULL.',1;
    IF TYPE_ID(N'dbo.RecepcionDetalleEntrada') IS NULL OR TYPE_ID(N'dbo.RecepcionLoteEntrada') IS NULL
        THROW 52405,'Faltan los tipos instalados de recepcion. No se crean ni reemplazan en este archivo.',1;
    IF (SELECT COUNT(*) FROM sys.parameters WHERE object_id=OBJECT_ID(N'dbo.sp_ConfirmarRecepcionPedido'))<>4
       OR EXISTS
       (
           SELECT 1 FROM (VALUES(N'@PedidoId',TYPE_ID(N'int'),0),
               (N'@UsuarioReceptorId',TYPE_ID(N'int'),0),
               (N'@Detalles',TYPE_ID(N'dbo.RecepcionDetalleEntrada'),1),
               (N'@Lotes',TYPE_ID(N'dbo.RecepcionLoteEntrada'),1)) e(Nombre,Tipo,EsTabla)
           LEFT JOIN sys.parameters p ON p.object_id=OBJECT_ID(N'dbo.sp_ConfirmarRecepcionPedido') AND p.name=e.Nombre
           WHERE p.parameter_id IS NULL OR p.user_type_id<>e.Tipo OR p.is_output<>0 OR p.is_readonly<>e.EsTabla
       ) THROW 52406,'La firma instalada de recepcion no coincide con la revisada.',1;
-- Validar tipos preexistentes antes de reemplazar el procedimiento.
-- Incluye orden, nulabilidad, tipos base, precision/escala y clave primaria.
DECLARE @ColumnasEsperadas TABLE
(
	Tipo sysname, Posicion int, Columna sysname, TipoBase sysname,
	PrecisionEsperada tinyint, EscalaEsperada tinyint, Nullable bit, OrdenClave int
);
INSERT INTO @ColumnasEsperadas VALUES
	(N'RecepcionDetalleEntrada', 1, N'DetallePedidoProveedorId', N'int', 10, 0, 0, 1),
	(N'RecepcionDetalleEntrada', 2, N'CantidadRecibida', N'decimal', 10, 3, 0, 0),
	(N'RecepcionLoteEntrada', 1, N'DetallePedidoProveedorId', N'int', 10, 0, 0, 1),
	(N'RecepcionLoteEntrada', 2, N'LoteNumero', N'int', 10, 0, 0, 2),
	(N'RecepcionLoteEntrada', 3, N'Cantidad', N'decimal', 10, 3, 0, 0),
	(N'RecepcionLoteEntrada', 4, N'TieneVencimiento', N'bit', 1, 0, 0, 0),
	(N'RecepcionLoteEntrada', 5, N'FechaVencimiento', N'datetime2', 19, 0, 1, 0);

IF EXISTS
(
	SELECT 1
	FROM @ColumnasEsperadas e
	LEFT JOIN sys.table_types tt
		ON tt.name = e.Tipo AND tt.schema_id = SCHEMA_ID(N'dbo')
	LEFT JOIN sys.columns c
		ON c.object_id = tt.type_table_object_id AND c.column_id = e.Posicion
	LEFT JOIN sys.indexes pk
		ON pk.object_id = tt.type_table_object_id AND pk.is_primary_key = 1
	LEFT JOIN sys.index_columns ic
		ON ic.object_id = pk.object_id AND ic.index_id = pk.index_id
		AND ic.column_id = c.column_id
	WHERE c.name IS NULL OR c.name <> e.Columna
		OR c.user_type_id <> TYPE_ID(e.TipoBase)
		OR (e.TipoBase <> N'datetime2' AND c.precision <> e.PrecisionEsperada)
		OR c.scale <> e.EscalaEsperada
		OR c.is_nullable <> e.Nullable
		OR ISNULL(ic.key_ordinal, 0) <> e.OrdenClave
)
OR EXISTS
(
	SELECT 1
	FROM sys.table_types tt
	INNER JOIN sys.columns c ON c.object_id = tt.type_table_object_id
	WHERE tt.schema_id = SCHEMA_ID(N'dbo')
		AND tt.name IN (N'RecepcionDetalleEntrada', N'RecepcionLoteEntrada')
		AND NOT EXISTS
		(
			SELECT 1 FROM @ColumnasEsperadas e
			WHERE e.Tipo = tt.name AND e.Posicion = c.column_id
		)
)
	THROW 51308, 'Los tipos de recepcion existentes no tienen la estructura esperada. Revisar dependencias sin eliminar los tipos.', 1;


EXEC(N'
CREATE OR ALTER PROCEDURE dbo.sp_ConfirmarRecepcionPedido
	@PedidoId int,
	@UsuarioReceptorId int,
	@Detalles dbo.RecepcionDetalleEntrada READONLY,
	@Lotes dbo.RecepcionLoteEntrada READONLY
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;
	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

	DECLARE @Ahora datetime2(0) = SYSDATETIME();
	DECLARE @RecepcionId int;
	DECLARE @EstadoResultado nvarchar(30);
	DECLARE @CantidadDetallesPedido int;
	DECLARE @CantidadDetallesEntrada int;
	DECLARE @DetalleId int;
	DECLARE @LoteNumero int;
	DECLARE @ProductoId int;
	DECLARE @CantidadLote decimal(10,3);
	DECLARE @TieneVencimiento bit;
	DECLARE @FechaVencimiento datetime2(0);
	DECLARE @DepositoStockId int;
	DECLARE @ControlaVencimiento bit;

	BEGIN TRY
		BEGIN TRANSACTION;

		IF NOT EXISTS
		(
			SELECT 1
			FROM dbo.Usuario WITH (UPDLOCK, HOLDLOCK)
			WHERE Id = @UsuarioReceptorId
		)
			THROW 51310, ''El usuario receptor no existe.'', 1;

		IF NOT EXISTS
		(
			SELECT 1
			FROM dbo.PedidoProveedor WITH (UPDLOCK, HOLDLOCK)
			WHERE Id = @PedidoId
			  AND Estado = N''Pendiente''
		)
			THROW 51311, ''El pedido no existe o no está pendiente.'', 1;

		IF EXISTS
		(
			SELECT 1
			FROM dbo.RecepcionPedido WITH (UPDLOCK, HOLDLOCK)
			WHERE PedidoId = @PedidoId
		)
			THROW 51312, ''El pedido ya tiene una recepción definitiva.'', 1;

		SELECT @CantidadDetallesPedido = COUNT(*)
		FROM dbo.DetallePedidoProveedor WITH (UPDLOCK, HOLDLOCK)
		WHERE PedidoId = @PedidoId;

		IF @CantidadDetallesPedido = 0
			THROW 51321, ''No se puede confirmar un pedido sin detalles.'', 1;

		-- No mezclar ingresos anteriores con una confirmacion definitiva.
		-- La cabecera y todos sus detalles ya permanecen bloqueados.
		IF EXISTS
		(
			SELECT 1
			FROM dbo.DetallePedidoProveedor WITH (UPDLOCK, HOLDLOCK)
			WHERE PedidoId = @PedidoId
			  AND CantidadRecibida <> 0
		)
			THROW 51320, ''El pedido tiene cantidades recibidas anteriormente; requiere conciliacion antes de confirmar.'', 1;

		SELECT @CantidadDetallesEntrada = COUNT(*)
		FROM @Detalles;

		IF @CantidadDetallesEntrada <> @CantidadDetallesPedido
			THROW 51313, ''La recepción debe representar todos los detalles del pedido una sola vez.'', 1;

		IF EXISTS
		(
			SELECT 1
			FROM @Detalles e
			LEFT JOIN dbo.DetallePedidoProveedor d
				ON d.Id = e.DetallePedidoProveedorId
			   AND d.PedidoId = @PedidoId
			WHERE d.Id IS NULL
		)
			THROW 51314, ''Existe un detalle que no pertenece al pedido indicado.'', 1;

		IF EXISTS
		(
			SELECT 1
			FROM dbo.DetallePedidoProveedor d
			LEFT JOIN @Detalles e ON e.DetallePedidoProveedorId = d.Id
			WHERE d.PedidoId = @PedidoId
			  AND e.DetallePedidoProveedorId IS NULL
		)
			THROW 51315, ''Falta representar un detalle del pedido.'', 1;

		IF EXISTS
		(
			SELECT 1
			FROM @Detalles e
			INNER JOIN dbo.DetallePedidoProveedor d ON d.Id = e.DetallePedidoProveedorId
			WHERE e.CantidadRecibida < 0
			   OR e.CantidadRecibida > d.Cantidad
		)
			THROW 51316, ''La cantidad recibida es negativa o supera la cantidad solicitada.'', 1;

		IF EXISTS
		(
			SELECT 1
			FROM @Lotes l
			LEFT JOIN @Detalles e ON e.DetallePedidoProveedorId = l.DetallePedidoProveedorId
			WHERE e.DetallePedidoProveedorId IS NULL
		)
			THROW 51317, ''Existe un lote para un detalle no incluido en la recepción.'', 1;

        -- Leer y mantener estable la configuracion actual del producto hasta COMMIT.
        -- Orden: usuario -> pedido -> detalles -> productos por Id -> lotes de deposito.
        -- La edicion de ficha necesita X; estos HOLDLOCK impiden cambiar la regla
        -- entre la validacion y el ingreso. No se modifican fichas ni lotes previos.
        DECLARE curProductosControl CURSOR LOCAL FAST_FORWARD FOR
            SELECT DISTINCT ProductoId FROM dbo.DetallePedidoProveedor
            WHERE PedidoId=@PedidoId ORDER BY ProductoId;
        OPEN curProductosControl;
        FETCH NEXT FROM curProductosControl INTO @ProductoId;
        WHILE @@FETCH_STATUS=0
        BEGIN
            SET @ControlaVencimiento=NULL;
            SELECT @ControlaVencimiento=ControlaVencimiento
            FROM dbo.Producto WITH (HOLDLOCK) WHERE Id=@ProductoId;
            IF @ControlaVencimiento IS NULL
                THROW 52410,''No se pudo consultar la configuracion de vencimiento de un producto.'',1;
            IF @ControlaVencimiento=1 AND EXISTS
            (
                SELECT 1 FROM @Lotes l JOIN dbo.DetallePedidoProveedor d
                    ON d.Id=l.DetallePedidoProveedorId AND d.PedidoId=@PedidoId
                WHERE d.ProductoId=@ProductoId
                    AND(l.TieneVencimiento=0 OR l.FechaVencimiento IS NULL)
            )
                THROW 52411,''Un producto requiere fecha de vencimiento en cada lote recibido. Revise la recepcion.'',1;
            FETCH NEXT FROM curProductosControl INTO @ProductoId;
        END;
        CLOSE curProductosControl;
        DEALLOCATE curProductosControl;
        -- Usar el dia real despues de las esperas por bloqueos (incluye hoy como vigente).
        SET @Ahora=SYSDATETIME();

		IF EXISTS
		(
			SELECT 1
			FROM @Lotes l
			WHERE l.Cantidad <= 0
			   OR (l.TieneVencimiento = 1 AND l.FechaVencimiento IS NULL)
			   OR (l.TieneVencimiento = 0 AND l.FechaVencimiento IS NOT NULL)
			   OR (l.TieneVencimiento = 1 AND l.FechaVencimiento < CONVERT(date, @Ahora))
		)
			THROW 51318, ''Un lote tiene cantidad, vencimiento o fecha inválidos.'', 1;

		IF EXISTS
		(
			SELECT 1
			FROM @Detalles e
			OUTER APPLY
			(
				SELECT SUM(l.Cantidad) AS CantidadLotes
				FROM @Lotes l
				WHERE l.DetallePedidoProveedorId = e.DetallePedidoProveedorId
			) s
			WHERE (e.CantidadRecibida = 0 AND ISNULL(s.CantidadLotes, 0) <> 0)
			   OR (e.CantidadRecibida > 0 AND ISNULL(s.CantidadLotes, 0) <> e.CantidadRecibida)
		)
			THROW 51319, ''La suma de los lotes no coincide con la cantidad recibida del detalle.'', 1;

		SET @EstadoResultado = CASE
			WHEN EXISTS
			(
				SELECT 1
				FROM @Detalles e
				INNER JOIN dbo.DetallePedidoProveedor d ON d.Id = e.DetallePedidoProveedorId
				WHERE e.CantidadRecibida < d.Cantidad
			) THEN N''Cerrado con faltantes''
			ELSE N''Completado''
		END;

		INSERT INTO dbo.RecepcionPedido
			(PedidoId, UsuarioReceptorId, FechaRecepcion, EstadoResultado)
		VALUES
			(@PedidoId, @UsuarioReceptorId, @Ahora, @EstadoResultado);

		SET @RecepcionId = CONVERT(int, SCOPE_IDENTITY());

		INSERT INTO dbo.RecepcionDetalle
			(RecepcionId, DetallePedidoProveedorId, CantidadSolicitada, CantidadRecibida, CantidadFaltante)
		SELECT
			@RecepcionId,
			e.DetallePedidoProveedorId,
			d.Cantidad,
			e.CantidadRecibida,
			d.Cantidad - e.CantidadRecibida
		FROM @Detalles e
		INNER JOIN dbo.DetallePedidoProveedor d ON d.Id = e.DetallePedidoProveedorId;

		DECLARE curLotes CURSOR LOCAL FAST_FORWARD FOR
			SELECT
				l.DetallePedidoProveedorId,
				l.LoteNumero,
				d.ProductoId,
				l.Cantidad,
				l.TieneVencimiento,
				l.FechaVencimiento
			FROM @Lotes l
			INNER JOIN dbo.DetallePedidoProveedor d
				ON d.Id = l.DetallePedidoProveedorId
			   AND d.PedidoId = @PedidoId
			ORDER BY l.DetallePedidoProveedorId, l.LoteNumero;

		OPEN curLotes;
		FETCH NEXT FROM curLotes
			INTO @DetalleId, @LoteNumero, @ProductoId, @CantidadLote, @TieneVencimiento, @FechaVencimiento;

		WHILE @@FETCH_STATUS = 0
		BEGIN
			INSERT INTO dbo.DepositoStock
				(ProductoId, FechaIngreso, FechaVencimiento, StockInicial, StockActual, Activo)
			VALUES
				(@ProductoId, @Ahora, @FechaVencimiento, @CantidadLote, @CantidadLote, 1);

			SET @DepositoStockId = CONVERT(int, SCOPE_IDENTITY());

			INSERT INTO dbo.RecepcionLote
				(RecepcionDetalleId, Cantidad, TieneVencimiento, FechaVencimiento, FechaIngreso, DepositoStockId)
			SELECT
				rd.Id,
				@CantidadLote,
				@TieneVencimiento,
				@FechaVencimiento,
				@Ahora,
				@DepositoStockId
			FROM dbo.RecepcionDetalle rd
			WHERE rd.RecepcionId = @RecepcionId
			  AND rd.DetallePedidoProveedorId = @DetalleId;

			FETCH NEXT FROM curLotes
				INTO @DetalleId, @LoteNumero, @ProductoId, @CantidadLote, @TieneVencimiento, @FechaVencimiento;
		END;

		CLOSE curLotes;
		DEALLOCATE curLotes;

		UPDATE d
		SET d.CantidadRecibida = e.CantidadRecibida
		FROM dbo.DetallePedidoProveedor d
		INNER JOIN @Detalles e ON e.DetallePedidoProveedorId = d.Id
		WHERE d.PedidoId = @PedidoId;

		UPDATE dbo.PedidoProveedor
		SET Estado = @EstadoResultado
		WHERE Id = @PedidoId;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
        IF CURSOR_STATUS(''local'',''curProductosControl'')>=0 CLOSE curProductosControl;
        IF CURSOR_STATUS(''local'',''curProductosControl'')>=-1 DEALLOCATE curProductosControl;
		IF CURSOR_STATUS(''local'', ''curLotes'') >= 0
			CLOSE curLotes;
		IF CURSOR_STATUS(''local'', ''curLotes'') >= -1
			DEALLOCATE curLotes;
		IF XACT_STATE() <> 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;');
    DECLARE @Huella nvarchar(64)=CONVERT(nvarchar(64),HASHBYTES('SHA2_256',
        OBJECT_DEFINITION(OBJECT_ID(N'dbo.sp_ConfirmarRecepcionPedido'))),2);
    IF @Huella IS NULL THROW 52407,'No se pudo verificar la definicion instalada. Se revierte la instalacion.',1;
    IF EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=1
        AND major_id=OBJECT_ID(N'dbo.sp_ConfirmarRecepcionPedido') AND minor_id=0
        AND name=N'PacuIbera.RecepcionPedido.ControlVencimiento')
        EXEC sys.sp_updateextendedproperty @name=N'PacuIbera.RecepcionPedido.ControlVencimiento',@value=@Huella,
            @level0type=N'SCHEMA',@level0name=N'dbo',@level1type=N'PROCEDURE',@level1name=N'sp_ConfirmarRecepcionPedido';
    ELSE
        EXEC sys.sp_addextendedproperty @name=N'PacuIbera.RecepcionPedido.ControlVencimiento',@value=@Huella,
            @level0type=N'SCHEMA',@level0name=N'dbo',@level1type=N'PROCEDURE',@level1name=N'sp_ConfirmarRecepcionPedido';
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
-- Solo verificacion de la instalacion; NO ejecuta una recepcion.
SELECT DB_NAME() AS BaseActual,e.name AS Proteccion,
    CASE WHEN CONVERT(nvarchar(64),e.value)=CONVERT(nvarchar(64),HASHBYTES('SHA2_256',
        OBJECT_DEFINITION(OBJECT_ID(N'dbo.sp_ConfirmarRecepcionPedido'))),2)
        THEN N'Proteccion instalada y verificada' ELSE N'REVISAR' END AS Resultado
FROM sys.extended_properties e WHERE e.class=1 AND e.major_id=OBJECT_ID(N'dbo.sp_ConfirmarRecepcionPedido')
    AND e.minor_id=0 AND e.name=N'PacuIbera.RecepcionPedido.ControlVencimiento';
