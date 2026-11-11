/*
	Historial de recepciones de pedidos a proveedores.
	DESTINO EXCLUSIVO: PacuIberaDB_Pruebas.

	Este script:
	- No modifica las tablas existentes.
	- No inserta datos de prueba.
	- No modifica procedimientos ni stock.
	- Crea las tablas solo si no existen.
	- Puede ejecutarse nuevamente sin recrear las tablas existentes.
*/

USE PacuIberaDB_Pruebas;

IF DB_NAME() <> N'PacuIberaDB_Pruebas'
	THROW 51210, 'El script solo puede ejecutarse sobre PacuIberaDB_Pruebas.', 1;

IF OBJECT_ID(N'dbo.PedidoProveedor', N'U') IS NULL
	THROW 51211, 'Falta dbo.PedidoProveedor.', 1;
IF OBJECT_ID(N'dbo.DetallePedidoProveedor', N'U') IS NULL
	THROW 51212, 'Falta dbo.DetallePedidoProveedor.', 1;
IF OBJECT_ID(N'dbo.DepositoStock', N'U') IS NULL
	THROW 51213, 'Falta dbo.DepositoStock.', 1;
IF OBJECT_ID(N'dbo.Usuario', N'U') IS NULL
	THROW 51214, 'Falta dbo.Usuario.', 1;

IF COL_LENGTH(N'dbo.PedidoProveedor', N'Id') IS NULL
	THROW 51215, 'dbo.PedidoProveedor no contiene Id.', 1;
IF COL_LENGTH(N'dbo.DetallePedidoProveedor', N'Id') IS NULL
	THROW 51216, 'dbo.DetallePedidoProveedor no contiene Id.', 1;
IF COL_LENGTH(N'dbo.DepositoStock', N'Id') IS NULL
	THROW 51217, 'dbo.DepositoStock no contiene Id.', 1;
IF COL_LENGTH(N'dbo.Usuario', N'Id') IS NULL
	THROW 51218, 'dbo.Usuario no contiene Id.', 1;

SET XACT_ABORT ON;

BEGIN TRY
	BEGIN TRANSACTION;

	IF OBJECT_ID(N'dbo.RecepcionPedido', N'U') IS NULL
	BEGIN
		CREATE TABLE dbo.RecepcionPedido
		(
			Id int IDENTITY(1,1) NOT NULL
				CONSTRAINT PK_RecepcionPedido PRIMARY KEY,
			PedidoId int NOT NULL,
			UsuarioReceptorId int NOT NULL,
			FechaRecepcion datetime2(0) NOT NULL
				CONSTRAINT DF_RecepcionPedido_FechaRecepcion DEFAULT (SYSDATETIME()),
			EstadoResultado nvarchar(30) NOT NULL,
			CONSTRAINT UQ_RecepcionPedido_PedidoId UNIQUE (PedidoId),
			CONSTRAINT CK_RecepcionPedido_EstadoResultado
				CHECK (EstadoResultado IN (N'Completado', N'Cerrado con faltantes')),
			CONSTRAINT FK_RecepcionPedido_PedidoProveedor
				FOREIGN KEY (PedidoId) REFERENCES dbo.PedidoProveedor(Id),
			CONSTRAINT FK_RecepcionPedido_Usuario
				FOREIGN KEY (UsuarioReceptorId) REFERENCES dbo.Usuario(Id)
		);
	END
	ELSE IF
	(
		COL_LENGTH(N'dbo.RecepcionPedido', N'Id') IS NULL
		OR COL_LENGTH(N'dbo.RecepcionPedido', N'PedidoId') IS NULL
		OR COL_LENGTH(N'dbo.RecepcionPedido', N'UsuarioReceptorId') IS NULL
		OR COL_LENGTH(N'dbo.RecepcionPedido', N'FechaRecepcion') IS NULL
		OR COL_LENGTH(N'dbo.RecepcionPedido', N'EstadoResultado') IS NULL
	)
		THROW 51219, 'dbo.RecepcionPedido existente no cumple la estructura requerida.', 1;

	IF OBJECT_ID(N'dbo.RecepcionDetalle', N'U') IS NULL
	BEGIN
		CREATE TABLE dbo.RecepcionDetalle
		(
			Id int IDENTITY(1,1) NOT NULL
				CONSTRAINT PK_RecepcionDetalle PRIMARY KEY,
			RecepcionId int NOT NULL,
			DetallePedidoProveedorId int NOT NULL,
			CantidadSolicitada decimal(10,3) NOT NULL,
			CantidadRecibida decimal(10,3) NOT NULL,
			CantidadFaltante decimal(10,3) NOT NULL,
			CONSTRAINT UQ_RecepcionDetalle_Recepcion_DetallePedido
				UNIQUE (RecepcionId, DetallePedidoProveedorId),
			CONSTRAINT CK_RecepcionDetalle_Cantidades
				CHECK
				(
					CantidadSolicitada >= 0
					AND CantidadRecibida >= 0
					AND CantidadFaltante >= 0
					AND CantidadRecibida <= CantidadSolicitada
					AND CantidadFaltante = CantidadSolicitada - CantidadRecibida
				),
			CONSTRAINT FK_RecepcionDetalle_RecepcionPedido
				FOREIGN KEY (RecepcionId) REFERENCES dbo.RecepcionPedido(Id),
			CONSTRAINT FK_RecepcionDetalle_DetallePedido
				FOREIGN KEY (DetallePedidoProveedorId) REFERENCES dbo.DetallePedidoProveedor(Id)
		);
	END
	ELSE IF
	(
		COL_LENGTH(N'dbo.RecepcionDetalle', N'Id') IS NULL
		OR COL_LENGTH(N'dbo.RecepcionDetalle', N'RecepcionId') IS NULL
		OR COL_LENGTH(N'dbo.RecepcionDetalle', N'DetallePedidoProveedorId') IS NULL
		OR COL_LENGTH(N'dbo.RecepcionDetalle', N'CantidadSolicitada') IS NULL
		OR COL_LENGTH(N'dbo.RecepcionDetalle', N'CantidadRecibida') IS NULL
		OR COL_LENGTH(N'dbo.RecepcionDetalle', N'CantidadFaltante') IS NULL
	)
		THROW 51220, 'dbo.RecepcionDetalle existente no cumple la estructura requerida.', 1;

	IF OBJECT_ID(N'dbo.RecepcionLote', N'U') IS NULL
	BEGIN
		CREATE TABLE dbo.RecepcionLote
		(
			Id int IDENTITY(1,1) NOT NULL
				CONSTRAINT PK_RecepcionLote PRIMARY KEY,
			RecepcionDetalleId int NOT NULL,
			Cantidad decimal(10,3) NOT NULL,
			TieneVencimiento bit NOT NULL,
			FechaVencimiento datetime2(0) NULL,
			FechaIngreso datetime2(0) NOT NULL
				CONSTRAINT DF_RecepcionLote_FechaIngreso DEFAULT (SYSDATETIME()),
			DepositoStockId int NULL,
			CONSTRAINT CK_RecepcionLote_Cantidad
				CHECK (Cantidad > 0),
			CONSTRAINT CK_RecepcionLote_Vencimiento
				CHECK
				(
					(TieneVencimiento = 1 AND FechaVencimiento IS NOT NULL)
					OR
					(TieneVencimiento = 0 AND FechaVencimiento IS NULL)
				),
			CONSTRAINT FK_RecepcionLote_RecepcionDetalle
				FOREIGN KEY (RecepcionDetalleId) REFERENCES dbo.RecepcionDetalle(Id),
			CONSTRAINT FK_RecepcionLote_DepositoStock
				FOREIGN KEY (DepositoStockId) REFERENCES dbo.DepositoStock(Id)
		);
	END
	ELSE IF
	(
		COL_LENGTH(N'dbo.RecepcionLote', N'Id') IS NULL
		OR COL_LENGTH(N'dbo.RecepcionLote', N'RecepcionDetalleId') IS NULL
		OR COL_LENGTH(N'dbo.RecepcionLote', N'Cantidad') IS NULL
		OR COL_LENGTH(N'dbo.RecepcionLote', N'TieneVencimiento') IS NULL
		OR COL_LENGTH(N'dbo.RecepcionLote', N'FechaVencimiento') IS NULL
		OR COL_LENGTH(N'dbo.RecepcionLote', N'FechaIngreso') IS NULL
		OR COL_LENGTH(N'dbo.RecepcionLote', N'DepositoStockId') IS NULL
	)
		THROW 51221, 'dbo.RecepcionLote existente no cumple la estructura requerida.', 1;

	IF NOT EXISTS
	(
		SELECT 1
		FROM sys.indexes
		WHERE name = N'UX_RecepcionLote_DepositoStockId'
		  AND object_id = OBJECT_ID(N'dbo.RecepcionLote')
	)
	BEGIN
		CREATE UNIQUE INDEX UX_RecepcionLote_DepositoStockId
			ON dbo.RecepcionLote(DepositoStockId)
			WHERE DepositoStockId IS NOT NULL;
	END;

	COMMIT TRANSACTION;
END TRY
BEGIN CATCH
	IF XACT_STATE() <> 0
		ROLLBACK TRANSACTION;
	THROW;
END CATCH;
