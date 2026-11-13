/*
  Consultas de solo lectura para elegir valores iniciales reales.
  No exportan usuarios, claves, DNI, contactos ni existencias.
*/
SET NOCOUNT ON;
IF DB_NAME()<>N'PacuIberaDB_Pruebas'
    THROW 54050, 'Estas consultas fuente solo se ejecutan en PacuIberaDB_Pruebas.', 1;

SELECT Id,Nombre FROM dbo.Rol ORDER BY Id;
SELECT Id,Nombre FROM dbo.Provincia ORDER BY Id;
SELECT Id,ProvinciaId,Nombre FROM dbo.Localidad ORDER BY ProvinciaId,Id;
SELECT Id,Nombre FROM dbo.MetodoPago ORDER BY Id;
SELECT Id,Nombre FROM dbo.Categoria ORDER BY Id;
SELECT Id,RazonSocial,CUIT,Activo FROM dbo.Proveedor ORDER BY Id;
/* Dependencia para habilitar la carga de pedidos: el reporte no exporta estas relaciones. */
SELECT pp.ProveedorId,p.RazonSocial,pp.ProductoId,pr.Nombre AS Producto,pp.Activo
FROM dbo.ProveedorProducto pp
JOIN dbo.Proveedor p ON p.Id=pp.ProveedorId
JOIN dbo.Producto pr ON pr.Id=pp.ProductoId
ORDER BY pp.ProveedorId,pp.ProductoId;
SELECT Id,Nombre,CategoriaId,PrecioVenta,StockMinimo,SeVendePorPeso,ControlaVencimiento,Activo
FROM dbo.Producto ORDER BY Id;
