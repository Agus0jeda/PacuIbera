/*
	Relación entre proveedores y productos.
	Destino exclusivo: PacuIberaDB_Pruebas.
	Este script no inserta datos y puede ejecutarse nuevamente sin borrar objetos.
*/

USE PacuIberaDB_Pruebas;

IF DB_NAME() <> N'PacuIberaDB_Pruebas'
	THROW 52000, 'El script solo puede ejecutarse sobre PacuIberaDB_Pruebas.', 1;

IF OBJECT_ID(N'dbo.Proveedor', N'U') IS NULL
	THROW 52001, 'Falta la tabla dbo.Proveedor.', 1;

IF COL_LENGTH(N'dbo.Proveedor', N'Id') IS NULL
	THROW 52002, 'dbo.Proveedor no contiene la columna Id.', 1;

IF OBJECT_ID(N'dbo.Producto', N'U') IS NULL
	THROW 52003, 'Falta la tabla dbo.Producto.', 1;

IF COL_LENGTH(N'dbo.Producto', N'Id') IS NULL
	THROW 52004, 'dbo.Producto no contiene la columna Id.', 1;

IF OBJECT_ID(N'dbo.ProveedorProducto', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.ProveedorProducto
	(
		ProveedorId int NOT NULL,
		ProductoId int NOT NULL,
		Activo bit NOT NULL
			CONSTRAINT DF_ProveedorProducto_Activo DEFAULT (1),
		CONSTRAINT PK_ProveedorProducto
			PRIMARY KEY (ProveedorId, ProductoId),
		CONSTRAINT FK_ProveedorProducto_Proveedor
			FOREIGN KEY (ProveedorId) REFERENCES dbo.Proveedor(Id),
		CONSTRAINT FK_ProveedorProducto_Producto
			FOREIGN KEY (ProductoId) REFERENCES dbo.Producto(Id)
	);
END;
