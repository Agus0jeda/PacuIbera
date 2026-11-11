/*
	Confirmación definitiva de recepción de pedidos a proveedores.
	DESTINO EXCLUSIVO: PacuIberaDB_Pruebas.

	Crea únicamente dos tipos tabulares y dbo.sp_ConfirmarRecepcionPedido.
	No ejecuta el procedimiento ni inserta datos de prueba.
	No modifica sp_RecepcionarItemPedido ni sp_TransferirDepositoALocal.
	Orden: historial y restriccion de estados existentes; luego
	Proteger_EscriturasRecepcionYPedidos_PacuIberaDB_Pruebas.sql; finalmente este script.
	La instalacion de tipos y procedimiento es atomica. Los tipos existentes
	se validan y reutilizan: nunca se eliminan ni se recrean.
*/

USE PacuIberaDB_Pruebas;

IF DB_NAME() <> N'PacuIberaDB_Pruebas'
	THROW 51300, 'El script solo puede ejecutarse sobre PacuIberaDB_Pruebas.', 1;

IF OBJECT_ID(N'dbo.PedidoProveedor', N'U') IS NULL
	THROW 51301, 'Falta dbo.PedidoProveedor.', 1;
IF OBJECT_ID(N'dbo.DetallePedidoProveedor', N'U') IS NULL
	THROW 51302, 'Falta dbo.DetallePedidoProveedor.', 1;
IF OBJECT_ID(N'dbo.DepositoStock', N'U') IS NULL
	THROW 51303, 'Falta dbo.DepositoStock.', 1;
IF OBJECT_ID(N'dbo.Usuario', N'U') IS NULL
	THROW 51304, 'Falta dbo.Usuario.', 1;
IF OBJECT_ID(N'dbo.RecepcionPedido', N'U') IS NULL
	THROW 51305, 'Falta dbo.RecepcionPedido.', 1;
IF OBJECT_ID(N'dbo.RecepcionDetalle', N'U') IS NULL
	THROW 51306, 'Falta dbo.RecepcionDetalle.', 1;
IF OBJECT_ID(N'dbo.RecepcionLote', N'U') IS NULL
	THROW 51307, 'Falta dbo.RecepcionLote.', 1;

SET XACT_ABORT ON;

BEGIN TRY
BEGIN TRANSACTION;

IF TYPE_ID(N'dbo.RecepcionDetalleEntrada') IS NULL
BEGIN
	EXEC(N'
		CREATE TYPE dbo.RecepcionDetalleEntrada AS TABLE
		(
			DetallePedidoProveedorId int NOT NULL PRIMARY KEY,
			CantidadRecibida decimal(10,3) NOT NULL
		);');
END;

IF TYPE_ID(N'dbo.RecepcionLoteEntrada') IS NULL
BEGIN
	EXEC(N'
		CREATE TYPE dbo.RecepcionLoteEntrada AS TABLE
		(
			DetallePedidoProveedorId int NOT NULL,
			LoteNumero int NOT NULL,
			Cantidad decimal(10,3) NOT NULL,
			TieneVencimiento bit NOT NULL,
			FechaVencimiento datetime2(0) NULL,
			PRIMARY KEY (DetallePedidoProveedorId, LoteNumero)
		);');
END;

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
		IF CURSOR_STATUS(''local'', ''curLotes'') >= 0
			CLOSE curLotes;
		IF CURSOR_STATUS(''local'', ''curLotes'') >= -1
			DEALLOCATE curLotes;
		IF XACT_STATE() <> 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;');

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
	IF XACT_STATE() <> 0
		ROLLBACK TRANSACTION;
	THROW;
END CATCH;
