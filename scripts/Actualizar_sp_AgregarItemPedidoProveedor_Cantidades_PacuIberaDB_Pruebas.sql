USE PacuIberaDB_Pruebas;
GO

IF DB_NAME() <> N'PacuIberaDB_Pruebas'
	THROW 51200, 'La base de datos seleccionada no es PacuIberaDB_Pruebas. El procedimiento no será modificado.', 1;

/*
	Actualiza exclusivamente el procedimiento de pedidos de la base de pruebas.
	No modifica tablas, stock, recepción ni cantidades recibidas.
*/
EXEC(N'
CREATE OR ALTER PROCEDURE dbo.sp_AgregarItemPedidoProveedor
	@PedidoId int,
	@ProductoId int,
	@Cantidad decimal(10,3),
	@PrecioCosto decimal(18,2)
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	IF @Cantidad IS NULL OR @Cantidad <= 0
		THROW 51110, ''La cantidad del pedido debe ser mayor que cero.'', 1;

	IF @PrecioCosto IS NULL OR @PrecioCosto < 0
		THROW 51111, ''El precio de costo no puede ser negativo.'', 1;

	IF NOT EXISTS (SELECT 1 FROM dbo.PedidoProveedor WHERE Id = @PedidoId)
		THROW 51112, ''El pedido indicado no existe.'', 1;

	IF EXISTS
	(
		SELECT 1
		FROM dbo.PedidoProveedor
		WHERE Id = @PedidoId
			AND Estado = N''Completado''
	)
		THROW 51113, ''No se pueden agregar items a un pedido completado.'', 1;

	IF NOT EXISTS (SELECT 1 FROM dbo.Producto WHERE Id = @ProductoId)
		THROW 51114, ''El producto indicado no existe.'', 1;

	INSERT INTO dbo.DetallePedidoProveedor
		(PedidoId, ProductoId, Cantidad, CantidadRecibida, PrecioCosto)
	VALUES
		(@PedidoId, @ProductoId, @Cantidad, 0, @PrecioCosto);
END;');
