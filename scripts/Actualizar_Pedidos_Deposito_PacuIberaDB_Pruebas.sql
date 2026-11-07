/*
	Actualización de pedidos a proveedores y depósito.

	DESTINO EXCLUSIVO: PacuIberaDB_Pruebas
	Este archivo NO debe ejecutarse sobre PacuIberaDB.

	Características de seguridad:
	- No elimina bases, tablas, columnas, datos ni procedimientos.
	- Solo crea las cuatro tablas si no existen.
	- Solo crea los ocho procedimientos si no existen; no reemplaza procedimientos existentes.
	- No inserta datos ficticios.
	- Valida las dependencias conocidas antes de crear objetos.

	Decisión de diseño:
	DepositoStock se modela por lotes para conservar FechaIngreso, FechaVencimiento,
	StockInicial y StockActual. sp_ObtenerStockDeposito agrega esos lotes por producto
	para devolver las columnas que consume FormDeposito.

	Limitación del código C# actual:
	@Cantidad, @CantidadIngresada y el parámetro Cantidad de transferencia llegan como
	int. Las tablas usan decimal(10,3), pero el C# actual no puede enviar fracciones.
	Este script conserva exactamente los parámetros del código y permite valores
	decimales internos; el soporte de cantidades fraccionarias requiere cambiar C# a
	decimal antes de usar fracciones para productos vendidos por peso.
*/

USE PacuIberaDB_Pruebas;

IF DB_NAME() <> N'PacuIberaDB_Pruebas'
	THROW 51000, 'El script solo puede ejecutarse sobre PacuIberaDB_Pruebas.', 1;

/* Dependencias verificadas por el código C#. */
IF OBJECT_ID(N'dbo.Producto', N'U') IS NULL
	THROW 51001, 'Falta la tabla dbo.Producto.', 1;
IF OBJECT_ID(N'dbo.Lote', N'U') IS NULL
	THROW 51002, 'Falta la tabla dbo.Lote.', 1;
IF OBJECT_ID(N'dbo.Proveedor', N'U') IS NULL
	THROW 51003, 'Falta la tabla dbo.Proveedor.', 1;
IF OBJECT_ID(N'dbo.Usuario', N'U') IS NULL
	THROW 51004, 'Falta la tabla dbo.Usuario.', 1;
IF OBJECT_ID(N'dbo.Categoria', N'U') IS NULL
	THROW 51005, 'Falta la tabla dbo.Categoria.', 1;

/* Las siguientes columnas son utilizadas directamente por el código actual. */
IF COL_LENGTH(N'dbo.Producto', N'Id') IS NULL
	THROW 51010, 'dbo.Producto no contiene Id.', 1;
IF COL_LENGTH(N'dbo.Producto', N'StockActual') IS NULL
	THROW 51011, 'dbo.Producto no contiene StockActual.', 1;
IF COL_LENGTH(N'dbo.Producto', N'SeVendePorPeso') IS NULL
	THROW 51012, 'dbo.Producto no contiene SeVendePorPeso.', 1;
IF COL_LENGTH(N'dbo.Lote', N'Id') IS NULL
	THROW 51013, 'dbo.Lote no contiene Id.', 1;
IF COL_LENGTH(N'dbo.Lote', N'ProductoId') IS NULL
	THROW 51014, 'dbo.Lote no contiene ProductoId.', 1;
IF COL_LENGTH(N'dbo.Lote', N'FechaIngreso') IS NULL
	THROW 51015, 'dbo.Lote no contiene FechaIngreso.', 1;
IF COL_LENGTH(N'dbo.Lote', N'FechaVencimiento') IS NULL
	THROW 51016, 'dbo.Lote no contiene FechaVencimiento.', 1;
IF COL_LENGTH(N'dbo.Lote', N'StockInicial') IS NULL
	THROW 51017, 'dbo.Lote no contiene StockInicial.', 1;
IF COL_LENGTH(N'dbo.Lote', N'StockActual') IS NULL
	THROW 51018, 'dbo.Lote no contiene StockActual.', 1;
IF COL_LENGTH(N'dbo.Lote', N'Activo') IS NULL
	THROW 51019, 'dbo.Lote no contiene Activo.', 1;
IF COL_LENGTH(N'dbo.Producto', N'CategoriaId') IS NULL
	THROW 51006, 'dbo.Producto no contiene CategoriaId.', 1;
IF COL_LENGTH(N'dbo.Producto', N'Nombre') IS NULL
	THROW 51007, 'dbo.Producto no contiene Nombre.', 1;
IF COL_LENGTH(N'dbo.Producto', N'Descripcion') IS NULL
	THROW 51008, 'dbo.Producto no contiene Descripcion.', 1;
IF COL_LENGTH(N'dbo.Proveedor', N'RazonSocial') IS NULL
	THROW 51009, 'dbo.Proveedor no contiene RazonSocial.', 1;
IF COL_LENGTH(N'dbo.Categoria', N'Id') IS NULL
	THROW 51041, 'dbo.Categoria no contiene Id.', 1;
IF COL_LENGTH(N'dbo.Categoria', N'Nombre') IS NULL
	THROW 51042, 'dbo.Categoria no contiene Nombre.', 1;

/* Si los objetos que administra este script ya existen, se valida su contrato
   antes de crear cualquier otro objeto. */
IF OBJECT_ID(N'dbo.PedidoProveedor', N'U') IS NOT NULL
   AND
   (
	   COL_LENGTH(N'dbo.PedidoProveedor', N'Id') IS NULL
	   OR COL_LENGTH(N'dbo.PedidoProveedor', N'ProveedorId') IS NULL
	   OR COL_LENGTH(N'dbo.PedidoProveedor', N'UsuarioId') IS NULL
	   OR COL_LENGTH(N'dbo.PedidoProveedor', N'FechaPedido') IS NULL
	   OR COL_LENGTH(N'dbo.PedidoProveedor', N'Estado') IS NULL
   )
	THROW 51043, 'dbo.PedidoProveedor existente no cumple el contrato requerido.', 1;
IF OBJECT_ID(N'dbo.DetallePedidoProveedor', N'U') IS NOT NULL
   AND
   (
	   COL_LENGTH(N'dbo.DetallePedidoProveedor', N'Id') IS NULL
	   OR COL_LENGTH(N'dbo.DetallePedidoProveedor', N'PedidoId') IS NULL
	   OR COL_LENGTH(N'dbo.DetallePedidoProveedor', N'ProductoId') IS NULL
	   OR COL_LENGTH(N'dbo.DetallePedidoProveedor', N'Cantidad') IS NULL
	   OR COL_LENGTH(N'dbo.DetallePedidoProveedor', N'CantidadRecibida') IS NULL
	   OR COL_LENGTH(N'dbo.DetallePedidoProveedor', N'PrecioCosto') IS NULL
	   )
	THROW 51044, 'dbo.DetallePedidoProveedor existente no cumple el contrato requerido.', 1;
IF OBJECT_ID(N'dbo.DepositoStock', N'U') IS NOT NULL
   AND
   (
	   COL_LENGTH(N'dbo.DepositoStock', N'Id') IS NULL
	   OR COL_LENGTH(N'dbo.DepositoStock', N'ProductoId') IS NULL
	   OR COL_LENGTH(N'dbo.DepositoStock', N'FechaIngreso') IS NULL
	   OR COL_LENGTH(N'dbo.DepositoStock', N'FechaVencimiento') IS NULL
	   OR COL_LENGTH(N'dbo.DepositoStock', N'StockInicial') IS NULL
	   OR COL_LENGTH(N'dbo.DepositoStock', N'StockActual') IS NULL
	   OR COL_LENGTH(N'dbo.DepositoStock', N'Activo') IS NULL
	   )
	THROW 51045, 'dbo.DepositoStock existente no cumple el contrato requerido.', 1;
IF OBJECT_ID(N'dbo.DepositoStockMinimo', N'U') IS NOT NULL
   AND
   (
	   COL_LENGTH(N'dbo.DepositoStockMinimo', N'ProductoId') IS NULL
	   OR COL_LENGTH(N'dbo.DepositoStockMinimo', N'StockMinimo') IS NULL
	   )
	THROW 51046, 'dbo.DepositoStockMinimo existente no cumple el contrato requerido.', 1;

SET XACT_ABORT ON;
BEGIN TRY
BEGIN TRANSACTION;

/* PedidoProveedor: cabecera del pedido a un proveedor. */
IF OBJECT_ID(N'dbo.PedidoProveedor', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.PedidoProveedor
	(
		Id int IDENTITY(1,1) NOT NULL
			CONSTRAINT PK_PedidoProveedor PRIMARY KEY,
		ProveedorId int NOT NULL,
		UsuarioId int NOT NULL,
		FechaPedido datetime2(0) NOT NULL
			CONSTRAINT DF_PedidoProveedor_FechaPedido DEFAULT (SYSDATETIME()),
		Estado nvarchar(30) NOT NULL
			CONSTRAINT DF_PedidoProveedor_Estado DEFAULT (N'Pendiente'),
		CONSTRAINT CK_PedidoProveedor_Estado
			CHECK (Estado IN (N'Pendiente', N'Parcialmente Recibido', N'Completado')),
		CONSTRAINT FK_PedidoProveedor_Proveedor
			FOREIGN KEY (ProveedorId) REFERENCES dbo.Proveedor(Id),
		CONSTRAINT FK_PedidoProveedor_Usuario
			FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuario(Id)
	);
END;

/* DetallePedidoProveedor: productos y cantidades solicitadas/recibidas. */
IF OBJECT_ID(N'dbo.DetallePedidoProveedor', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.DetallePedidoProveedor
	(
		Id int IDENTITY(1,1) NOT NULL
			CONSTRAINT PK_DetallePedidoProveedor PRIMARY KEY,
		PedidoId int NOT NULL,
		ProductoId int NOT NULL,
		Cantidad decimal(10,3) NOT NULL,
		CantidadRecibida decimal(10,3) NOT NULL
			CONSTRAINT DF_DetallePedidoProveedor_CantidadRecibida DEFAULT (0),
		PrecioCosto decimal(18,2) NOT NULL,
		CONSTRAINT CK_DetallePedidoProveedor_Cantidad
			CHECK (Cantidad > 0),
		CONSTRAINT CK_DetallePedidoProveedor_CantidadRecibida
			CHECK (CantidadRecibida >= 0 AND CantidadRecibida <= Cantidad),
		CONSTRAINT CK_DetallePedidoProveedor_PrecioCosto
			CHECK (PrecioCosto >= 0),
		CONSTRAINT FK_DetallePedidoProveedor_Pedido
			FOREIGN KEY (PedidoId) REFERENCES dbo.PedidoProveedor(Id),
		CONSTRAINT FK_DetallePedidoProveedor_Producto
			FOREIGN KEY (ProductoId) REFERENCES dbo.Producto(Id)
	);
END;

/*
	DepositoStock representa lotes que todavía están en el depósito central.
	Una fila es un lote de un producto. Las filas con StockActual = 0 se conservan
	para no perder historial operativo y no participan del stock disponible.
*/
IF OBJECT_ID(N'dbo.DepositoStock', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.DepositoStock
	(
		Id int IDENTITY(1,1) NOT NULL
			CONSTRAINT PK_DepositoStock PRIMARY KEY,
		ProductoId int NOT NULL,
		FechaIngreso datetime2(0) NULL,
		FechaVencimiento datetime2(0) NULL,
		StockInicial decimal(10,3) NOT NULL
			CONSTRAINT DF_DepositoStock_StockInicial DEFAULT (0),
		StockActual decimal(10,3) NOT NULL
			CONSTRAINT DF_DepositoStock_StockActual DEFAULT (0),
		Activo bit NOT NULL
			CONSTRAINT DF_DepositoStock_Activo DEFAULT (1),
		CONSTRAINT CK_DepositoStock_StockInicial
			CHECK (StockInicial >= 0),
		CONSTRAINT CK_DepositoStock_StockActual
			CHECK (StockActual >= 0),
		CONSTRAINT FK_DepositoStock_Producto
			FOREIGN KEY (ProductoId) REFERENCES dbo.Producto(Id)
	);
END;

IF OBJECT_ID(N'dbo.DepositoStockMinimo', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.DepositoStockMinimo
	(
		ProductoId int NOT NULL
			CONSTRAINT PK_DepositoStockMinimo PRIMARY KEY,
		StockMinimo decimal(10,3) NOT NULL
			CONSTRAINT DF_DepositoStockMinimo_StockMinimo DEFAULT (0),
		CONSTRAINT CK_DepositoStockMinimo_StockMinimo
			CHECK (StockMinimo >= 0),
		CONSTRAINT FK_DepositoStockMinimo_Producto
			FOREIGN KEY (ProductoId) REFERENCES dbo.Producto(Id)
	);
END;

/* Si alguno de estos objetos ya existía, no se modifica: se valida su contrato. */
IF COL_LENGTH(N'dbo.PedidoProveedor', N'Id') IS NULL
	THROW 51020, 'dbo.PedidoProveedor no contiene Id.', 1;
IF COL_LENGTH(N'dbo.PedidoProveedor', N'ProveedorId') IS NULL
	THROW 51021, 'dbo.PedidoProveedor no contiene ProveedorId.', 1;
IF COL_LENGTH(N'dbo.PedidoProveedor', N'UsuarioId') IS NULL
	THROW 51022, 'dbo.PedidoProveedor no contiene UsuarioId.', 1;
IF COL_LENGTH(N'dbo.PedidoProveedor', N'FechaPedido') IS NULL
	THROW 51023, 'dbo.PedidoProveedor no contiene FechaPedido.', 1;
IF COL_LENGTH(N'dbo.PedidoProveedor', N'Estado') IS NULL
	THROW 51024, 'dbo.PedidoProveedor no contiene Estado.', 1;

IF COL_LENGTH(N'dbo.DetallePedidoProveedor', N'Id') IS NULL
	THROW 51025, 'dbo.DetallePedidoProveedor no contiene Id.', 1;
IF COL_LENGTH(N'dbo.DetallePedidoProveedor', N'PedidoId') IS NULL
	THROW 51026, 'dbo.DetallePedidoProveedor no contiene PedidoId.', 1;
IF COL_LENGTH(N'dbo.DetallePedidoProveedor', N'ProductoId') IS NULL
	THROW 51027, 'dbo.DetallePedidoProveedor no contiene ProductoId.', 1;
IF COL_LENGTH(N'dbo.DetallePedidoProveedor', N'Cantidad') IS NULL
	THROW 51028, 'dbo.DetallePedidoProveedor no contiene Cantidad.', 1;
IF COL_LENGTH(N'dbo.DetallePedidoProveedor', N'CantidadRecibida') IS NULL
	THROW 51029, 'dbo.DetallePedidoProveedor no contiene CantidadRecibida.', 1;
IF COL_LENGTH(N'dbo.DetallePedidoProveedor', N'PrecioCosto') IS NULL
	THROW 51030, 'dbo.DetallePedidoProveedor no contiene PrecioCosto.', 1;

IF COL_LENGTH(N'dbo.DepositoStock', N'Id') IS NULL
	THROW 51031, 'dbo.DepositoStock no contiene Id.', 1;
IF COL_LENGTH(N'dbo.DepositoStock', N'ProductoId') IS NULL
	THROW 51032, 'dbo.DepositoStock no contiene ProductoId.', 1;
IF COL_LENGTH(N'dbo.DepositoStock', N'FechaIngreso') IS NULL
	THROW 51033, 'dbo.DepositoStock no contiene FechaIngreso.', 1;
IF COL_LENGTH(N'dbo.DepositoStock', N'FechaVencimiento') IS NULL
	THROW 51034, 'dbo.DepositoStock no contiene FechaVencimiento.', 1;
IF COL_LENGTH(N'dbo.DepositoStock', N'StockInicial') IS NULL
	THROW 51035, 'dbo.DepositoStock no contiene StockInicial.', 1;
IF COL_LENGTH(N'dbo.DepositoStock', N'StockActual') IS NULL
	THROW 51036, 'dbo.DepositoStock no contiene StockActual.', 1;
IF COL_LENGTH(N'dbo.DepositoStock', N'Activo') IS NULL
	THROW 51037, 'dbo.DepositoStock no contiene Activo.', 1;
IF COL_LENGTH(N'dbo.DepositoStockMinimo', N'ProductoId') IS NULL
	THROW 51038, 'dbo.DepositoStockMinimo no contiene ProductoId.', 1;
IF COL_LENGTH(N'dbo.DepositoStockMinimo', N'StockMinimo') IS NULL
	THROW 51039, 'dbo.DepositoStockMinimo no contiene StockMinimo.', 1;

IF NOT EXISTS
(
	SELECT 1
	FROM sys.indexes
	WHERE name = N'IX_DetallePedidoProveedor_PedidoId'
	  AND object_id = OBJECT_ID(N'dbo.DetallePedidoProveedor')
)
BEGIN
	CREATE INDEX IX_DetallePedidoProveedor_PedidoId
		ON dbo.DetallePedidoProveedor(PedidoId);
END;

IF NOT EXISTS
(
	SELECT 1
	FROM sys.indexes
	WHERE name = N'IX_DetallePedidoProveedor_ProductoId'
	  AND object_id = OBJECT_ID(N'dbo.DetallePedidoProveedor')
)
BEGIN
	CREATE INDEX IX_DetallePedidoProveedor_ProductoId
		ON dbo.DetallePedidoProveedor(ProductoId);
END;

IF NOT EXISTS
(
	SELECT 1
	FROM sys.indexes
	WHERE name = N'IX_DepositoStock_Producto_Activo'
	  AND object_id = OBJECT_ID(N'dbo.DepositoStock')
)
BEGIN
	CREATE INDEX IX_DepositoStock_Producto_Activo
		ON dbo.DepositoStock(ProductoId, Activo, FechaVencimiento, Id)
		INCLUDE (StockActual);
END;

/* Crea el procedimiento solo si no existe; no reemplaza una versión existente. */
IF OBJECT_ID(N'dbo.sp_CrearPedidoProveedor', N'P') IS NULL
BEGIN
	EXEC(N'
CREATE PROCEDURE dbo.sp_CrearPedidoProveedor
	@ProveedorId int,
	@UsuarioId int,
	@FechaPedido datetime2(0),
	@NuevoPedidoId int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	BEGIN TRY
		IF @ProveedorId IS NULL OR @UsuarioId IS NULL OR @FechaPedido IS NULL
			THROW 51100, ''Proveedor, usuario y fecha son obligatorios.'', 1;

		IF NOT EXISTS (SELECT 1 FROM dbo.Proveedor WHERE Id = @ProveedorId)
			THROW 51101, ''El proveedor indicado no existe.'', 1;

		IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE Id = @UsuarioId)
			THROW 51102, ''El usuario indicado no existe.'', 1;

		BEGIN TRANSACTION;
			INSERT INTO dbo.PedidoProveedor (ProveedorId, UsuarioId, FechaPedido, Estado)
			VALUES (@ProveedorId, @UsuarioId, @FechaPedido, N''Pendiente'');

			SET @NuevoPedidoId = CONVERT(int, SCOPE_IDENTITY());
		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END');
END;

IF OBJECT_ID(N'dbo.sp_AgregarItemPedidoProveedor', N'P') IS NULL
BEGIN
	EXEC(N'
CREATE PROCEDURE dbo.sp_AgregarItemPedidoProveedor
	@PedidoId int,
	@ProductoId int,
	@Cantidad int,
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
		(@PedidoId, @ProductoId, CONVERT(decimal(10,3), @Cantidad), 0, @PrecioCosto);
END');
END;

IF OBJECT_ID(N'dbo.sp_ObtenerPedidosProveedor', N'P') IS NULL
BEGIN
	EXEC(N'
CREATE PROCEDURE dbo.sp_ObtenerPedidosProveedor
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		pp.Id,
		pp.ProveedorId,
		p.RazonSocial AS Proveedor,
		pp.UsuarioId,
		pp.FechaPedido,
		pp.Estado,
		COUNT(dpp.Id) AS CantidadItems,
		ISNULL(SUM(dpp.Cantidad * dpp.PrecioCosto), 0) AS TotalEstimado
	FROM dbo.PedidoProveedor pp
	INNER JOIN dbo.Proveedor p ON p.Id = pp.ProveedorId
	LEFT JOIN dbo.DetallePedidoProveedor dpp ON dpp.PedidoId = pp.Id
	GROUP BY
		pp.Id,
		pp.ProveedorId,
		p.RazonSocial,
		pp.UsuarioId,
		pp.FechaPedido,
		pp.Estado
	ORDER BY pp.FechaPedido DESC, pp.Id DESC;
END');
END;

IF OBJECT_ID(N'dbo.sp_ObtenerDetallePedidoProveedor', N'P') IS NULL
BEGIN
	EXEC(N'
CREATE PROCEDURE dbo.sp_ObtenerDetallePedidoProveedor
	@PedidoId int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		dpp.Id AS DetalleId,
		dpp.ProductoId,
		pr.Nombre AS Producto,
		dpp.Cantidad,
		dpp.CantidadRecibida,
		dpp.Cantidad - dpp.CantidadRecibida AS Pendiente,
		dpp.PrecioCosto
	FROM dbo.DetallePedidoProveedor dpp
	INNER JOIN dbo.Producto pr ON pr.Id = dpp.ProductoId
	WHERE dpp.PedidoId = @PedidoId
	ORDER BY dpp.Id;
END');
END;

IF OBJECT_ID(N'dbo.sp_RecepcionarItemPedido', N'P') IS NULL
BEGIN
	EXEC(N'
CREATE PROCEDURE dbo.sp_RecepcionarItemPedido
	@DetalleId int,
	@CantidadIngresada int,
	@FechaVencimiento datetime2(0) = NULL
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;
	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

	BEGIN TRY
		IF @CantidadIngresada IS NULL OR @CantidadIngresada <= 0
			THROW 51120, ''La cantidad recibida debe ser mayor que cero.'', 1;

		DECLARE @PedidoId int,
				@ProductoId int,
				@Cantidad decimal(10,3),
				@CantidadRecibida decimal(10,3),
				@Pendiente decimal(10,3);

		BEGIN TRANSACTION;

			SELECT
				@PedidoId = dpp.PedidoId,
				@ProductoId = dpp.ProductoId,
				@Cantidad = dpp.Cantidad,
				@CantidadRecibida = dpp.CantidadRecibida
			FROM dbo.DetallePedidoProveedor dpp WITH (UPDLOCK, HOLDLOCK)
			WHERE dpp.Id = @DetalleId;

			IF @PedidoId IS NULL
				THROW 51121, ''El detalle del pedido no existe.'', 1;

			SET @Pendiente = @Cantidad - @CantidadRecibida;

			IF CONVERT(decimal(10,3), @CantidadIngresada) > @Pendiente
				THROW 51122, ''La recepción supera la cantidad pendiente.'', 1;

			INSERT INTO dbo.DepositoStock
				(ProductoId, FechaIngreso, FechaVencimiento, StockInicial,
				 StockActual, Activo)
			VALUES
				(@ProductoId, SYSDATETIME(), @FechaVencimiento,
				 CONVERT(decimal(10,3), @CantidadIngresada),
				 CONVERT(decimal(10,3), @CantidadIngresada),
				 1);

			UPDATE dbo.DetallePedidoProveedor
			SET CantidadRecibida = CantidadRecibida + CONVERT(decimal(10,3), @CantidadIngresada)
			WHERE Id = @DetalleId;

			UPDATE pp
			SET Estado = CASE
				WHEN NOT EXISTS
				(
					SELECT 1
					FROM dbo.DetallePedidoProveedor x
					WHERE x.PedidoId = pp.Id
					  AND x.CantidadRecibida < x.Cantidad
				) THEN N''Completado''
				WHEN EXISTS
				(
					SELECT 1
					FROM dbo.DetallePedidoProveedor x
					WHERE x.PedidoId = pp.Id
					  AND x.CantidadRecibida > 0
				) THEN N''Parcialmente Recibido''
				ELSE N''Pendiente''
			END
			FROM dbo.PedidoProveedor pp
			WHERE pp.Id = @PedidoId;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END');
END;

IF OBJECT_ID(N'dbo.sp_ObtenerStockDeposito', N'P') IS NULL
BEGIN
	EXEC(N'
CREATE PROCEDURE dbo.sp_ObtenerStockDeposito
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		pr.Id AS ProductoId,
		pr.Nombre,
		pr.Descripcion,
		ca.Nombre AS Categoria,
		ISNULL(SUM(ds.StockActual), 0) AS StockDeposito,
		ISNULL(dsm.StockMinimo, 0) AS StockMinimoDeposito,
		CASE
			WHEN ISNULL(SUM(ds.StockActual), 0)
				 < ISNULL(dsm.StockMinimo, 0)
			THEN N''ALERTA''
			ELSE N''OK''
		END AS EstadoStock
	FROM dbo.Producto pr
	LEFT JOIN dbo.Categoria ca ON ca.Id = pr.CategoriaId
	LEFT JOIN dbo.DepositoStock ds
		ON ds.ProductoId = pr.Id
		AND ds.Activo = 1
		AND (ds.FechaVencimiento IS NULL
			 OR ds.FechaVencimiento >= CONVERT(date, SYSDATETIME()))
	LEFT JOIN dbo.DepositoStockMinimo dsm ON dsm.ProductoId = pr.Id
	WHERE pr.Activo = 1
	GROUP BY pr.Id, pr.Nombre, pr.Descripcion, ca.Nombre, dsm.StockMinimo
	ORDER BY pr.Nombre;
END');
END;

IF OBJECT_ID(N'dbo.sp_ActualizarStockMinimoDeposito', N'P') IS NULL
BEGIN
	EXEC(N'
CREATE PROCEDURE dbo.sp_ActualizarStockMinimoDeposito
	@ProductoId int,
	@StockMinimo decimal(18,2)
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	IF @StockMinimo IS NULL OR @StockMinimo < 0
		THROW 51130, ''El stock mínimo no puede ser negativo.'', 1;

	IF NOT EXISTS (SELECT 1 FROM dbo.Producto WHERE Id = @ProductoId)
		THROW 51131, ''El producto indicado no existe.'', 1;

	BEGIN TRANSACTION;

		IF EXISTS
		(
			SELECT 1
			FROM dbo.DepositoStockMinimo WITH (UPDLOCK, HOLDLOCK)
			WHERE ProductoId = @ProductoId
		)
		BEGIN
			UPDATE dbo.DepositoStockMinimo
			SET StockMinimo = CONVERT(decimal(10,3), @StockMinimo)
			WHERE ProductoId = @ProductoId;
		END
		ELSE
		BEGIN
			INSERT INTO dbo.DepositoStockMinimo (ProductoId, StockMinimo)
			VALUES (@ProductoId, CONVERT(decimal(10,3), @StockMinimo));
		END;

	COMMIT TRANSACTION;
END');
END;

IF OBJECT_ID(N'dbo.sp_TransferirDepositoALocal', N'P') IS NULL
BEGIN
	EXEC(N'
CREATE PROCEDURE dbo.sp_TransferirDepositoALocal
	@UsuarioId int,
	@ProductoId int,
	@Cantidad int,
	@FechaVencimiento datetime2(0) = NULL
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;
	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

	BEGIN TRY
		IF @UsuarioId IS NULL OR @ProductoId IS NULL
			THROW 51140, ''Usuario y producto son obligatorios.'', 1;

		IF @Cantidad IS NULL OR @Cantidad <= 0
			THROW 51141, ''La cantidad a transferir debe ser mayor que cero.'', 1;

		IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE Id = @UsuarioId)
			THROW 51142, ''El usuario indicado no existe.'', 1;

		IF NOT EXISTS (SELECT 1 FROM dbo.Producto WHERE Id = @ProductoId)
			THROW 51143, ''El producto indicado no existe.'', 1;

		DECLARE @Restante decimal(10,3) = CONVERT(decimal(10,3), @Cantidad),
				@LoteDepositoId int,
				@StockLote decimal(10,3),
				@VencimientoLote datetime2(0),
				@Tomar decimal(10,3);

		BEGIN TRANSACTION;

			IF
			(
				SELECT ISNULL(SUM(StockActual), 0)
				FROM dbo.DepositoStock WITH (UPDLOCK, HOLDLOCK)
				WHERE ProductoId = @ProductoId
				  AND Activo = 1
				  AND (FechaVencimiento IS NULL
					   OR FechaVencimiento >= CONVERT(date, SYSDATETIME()))
			) < @Restante
				THROW 51144, ''No hay stock suficiente en el depósito.'', 1;

			WHILE @Restante > 0
			BEGIN
				SET @LoteDepositoId = NULL;

				SELECT TOP (1)
					@LoteDepositoId = Id,
					@StockLote = StockActual,
					@VencimientoLote = FechaVencimiento
				FROM dbo.DepositoStock WITH (UPDLOCK, HOLDLOCK)
				WHERE ProductoId = @ProductoId
				  AND Activo = 1
				  AND StockActual > 0
				  AND (FechaVencimiento IS NULL
					   OR FechaVencimiento >= CONVERT(date, SYSDATETIME()))
				ORDER BY
					CASE WHEN FechaVencimiento IS NULL THEN 1 ELSE 0 END,
					FechaVencimiento,
					Id;

				IF @LoteDepositoId IS NULL
					THROW 51145, ''No se pudo localizar un lote disponible en el depósito.'', 1;

				IF @VencimientoLote IS NULL
				   AND @FechaVencimiento IS NOT NULL
				   AND @FechaVencimiento < CONVERT(date, SYSDATETIME())
					THROW 51146, ''La fecha de vencimiento manual no puede ser anterior al día actual.'', 1;

				SET @Tomar = CASE
					WHEN @StockLote < @Restante THEN @StockLote
					ELSE @Restante
				END;

				UPDATE dbo.DepositoStock
				SET StockActual = StockActual - @Tomar
				WHERE Id = @LoteDepositoId;

			/*
				La fecha original del lote tiene prioridad. La fecha enviada por el
				formulario solo completa un lote cuyo vencimiento original era NULL;
				nunca puede convertir un lote vencido en vigente.
			*/
				INSERT INTO dbo.Lote
					(ProductoId, FechaIngreso, FechaVencimiento,
					 StockInicial, StockActual, Activo)
				VALUES
					(@ProductoId, SYSDATETIME(),
					  CASE
						 WHEN @VencimientoLote IS NULL THEN @FechaVencimiento
						 ELSE @VencimientoLote
					  END,
					 @Tomar, @Tomar, 1);

				UPDATE dbo.Producto
				SET StockActual = ISNULL(StockActual, 0) + @Tomar
				WHERE Id = @ProductoId;

				SET @Restante = @Restante - @Tomar;
			END;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END');
END;

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
	IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
	THROW;
END CATCH;

/*
	Dependencia funcional pendiente de decisión:
	el C# actual envía cantidades enteras para pedidos, recepción y transferencia.
	Las tablas admiten decimal(10,3), pero para operar con fracciones de productos
	vendidos por peso será necesario cambiar esos parámetros y controles C# a decimal.
*/
PRINT 'Script preparado para PacuIberaDB_Pruebas. No se ejecutaron operaciones desde este archivo.';
