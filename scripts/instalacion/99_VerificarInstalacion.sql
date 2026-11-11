/* Verificación final de solo lectura. Ejecutar conectado a la base destino. */
:on error exit
:setvar DatabaseName "PacuIbera_Entrega"
SET NOCOUNT ON;
DECLARE @BaseEsperada sysname=N'$(DatabaseName)';
IF DB_NAME()<>@BaseEsperada
    THROW 54020, 'La conexión no corresponde a la base destino indicada en DatabaseName.', 1;

DECLARE @Tablas TABLE(Nombre sysname PRIMARY KEY);
INSERT @Tablas VALUES
 (N'Rol'),(N'Usuario'),(N'Provincia'),(N'Localidad'),(N'Cliente'),(N'Categoria'),
 (N'Producto'),(N'Proveedor'),(N'ProveedorProducto'),(N'Lote'),(N'Caja'),(N'Venta'),
 (N'DetalleVenta'),(N'MetodoPago'),(N'VentaPago'),(N'PedidoProveedor'),
 (N'DetallePedidoProveedor'),(N'DepositoStock'),(N'DepositoStockMinimo'),
 (N'RecepcionPedido'),(N'RecepcionDetalle'),(N'RecepcionLote'),(N'RemitoInterno'),
 (N'RemitoDetalle'),(N'RemitoLote'),(N'RecepcionRemito'),(N'RecepcionRemitoLote'),
 (N'ResolucionDiferencia'),(N'ResolucionDiferenciaDetalle'),(N'MovimientoCaja'),(N'VentaAnulacion');

DECLARE @Procedimientos TABLE(Nombre sysname PRIMARY KEY);
INSERT @Procedimientos VALUES
 (N'sp_AbrirCaja'),(N'sp_ActualizarClienteCRM'),(N'sp_ActualizarStockMinimoDeposito'),(N'sp_ActualizarUsuario'),
 (N'sp_AgregarItemPedidoProveedor'),(N'sp_AnalisisCurvaVentas'),(N'sp_AnalisisPerdidaVencimientos'),
 (N'sp_AnularVenta'),(N'sp_AnularVentaConMotivo'),(N'sp_BajaUsuario'),(N'sp_BuscarProductosParaVenta'),
 (N'sp_CerrarCaja'),(N'sp_ConfirmarRecepcionPedido'),(N'sp_ConfirmarRecepcionRemito'),
 (N'sp_CrearPedidoProveedor'),(N'sp_EmitirRemitoInterno'),(N'sp_InsertarCliente'),
 (N'sp_InsertarClienteRapido'),(N'sp_InsertarUsuario'),(N'sp_LoginUsuario'),(N'sp_ModificarProducto'),
 (N'sp_ObtenerClientes'),(N'sp_ObtenerClientesCRM'),(N'sp_ObtenerDetallePedidoProveedor'),
 (N'sp_ObtenerDetalleVenta'),(N'sp_ObtenerHistorialVentas'),(N'sp_ObtenerLocalidadesPorProvincia'),
 (N'sp_ObtenerPedidosProveedor'),(N'sp_ObtenerProvincias'),(N'sp_ObtenerRoles'),
 (N'sp_ObtenerStockDeposito'),(N'sp_ObtenerUsuariosDetalle'),(N'sp_RecepcionarItemPedido'),
 (N'sp_RegistrarDetalleVentaFIFO'),(N'sp_RegistrarMovimientoCaja'),(N'sp_RegistrarPagoVenta'),
 (N'sp_RegistrarProducto'),(N'sp_RegistrarVenta'),(N'sp_ReporteAuditoria'),
 (N'sp_ReporteLotesVencidos'),(N'sp_ReporteMetodosPago'),(N'sp_ReporteRankingVendedores'),
 (N'sp_ReporteResumenGlobal'),(N'sp_ReporteStockCritico'),(N'sp_ReporteTopClientes'),
 (N'sp_ReporteTopProductos'),(N'sp_ResolverDiferenciaRemito'),(N'sp_ResumenCajas'),
 (N'sp_TransferirDepositoALocal'),(N'sp_VerificarCajaAbierta');

DECLARE @Tipos TABLE(Nombre sysname PRIMARY KEY);
INSERT @Tipos VALUES
 (N'RecepcionDetalleEntrada'),(N'RecepcionLoteEntrada'),(N'RemitoProductoEntrada'),
 (N'RecepcionRemitoLoteEntrada'),(N'ResolucionDiferenciaEntrada');

SELECT N'Tabla' AS Clase,x.Nombre,
       CASE WHEN t.object_id IS NULL THEN N'Falta' ELSE N'Existe' END AS Estado
FROM @Tablas x LEFT JOIN sys.tables t ON t.schema_id=SCHEMA_ID(N'dbo') AND t.name=x.Nombre
UNION ALL
SELECT N'Procedimiento',x.Nombre,
       CASE WHEN p.object_id IS NULL THEN N'Falta' ELSE N'Existe' END
FROM @Procedimientos x LEFT JOIN sys.procedures p ON p.schema_id=SCHEMA_ID(N'dbo') AND p.name=x.Nombre
UNION ALL
SELECT N'Tipo tabular',x.Nombre,
       CASE WHEN t.user_type_id IS NULL THEN N'Falta' ELSE N'Existe' END
FROM @Tipos x LEFT JOIN sys.table_types t ON t.schema_id=SCHEMA_ID(N'dbo') AND t.name=x.Nombre
ORDER BY Clase,Nombre;

SELECT r.Nombre AS Rol,COUNT_BIG(CASE WHEN u.Activo=1 THEN 1 END) AS UsuariosActivos
FROM dbo.Rol r LEFT JOIN dbo.Usuario u ON u.RolId=r.Id
GROUP BY r.Nombre ORDER BY r.Nombre;

SELECT esperado.Nombre AS RolEsperado,
       CASE WHEN r.Id IS NULL THEN N'Falta' ELSE N'Existe' END AS Estado
FROM (VALUES (N'Administrador'),(N'Gerente'),(N'Vendedor')) esperado(Nombre)
LEFT JOIN dbo.Rol r ON r.Nombre=esperado.Nombre
ORDER BY esperado.Nombre;

SELECT (SELECT COUNT_BIG(*) FROM dbo.Categoria) AS Categorias,
       (SELECT COUNT_BIG(*) FROM dbo.Proveedor WHERE Activo=1) AS ProveedoresActivos,
       (SELECT COUNT_BIG(*) FROM dbo.Producto WHERE Activo=1) AS ProductosActivos;

SELECT (SELECT COUNT_BIG(*) FROM sys.foreign_keys WHERE is_disabled=0 AND is_not_trusted=0) AS ClavesForaneasConfiables,
       (SELECT COUNT_BIG(*) FROM sys.check_constraints WHERE is_disabled=0 AND is_not_trusted=0) AS ChecksConfiables,
       (SELECT COUNT_BIG(*) FROM sys.indexes WHERE is_unique=1 AND is_disabled=0 AND is_hypothetical=0) AS IndicesUnicosActivos;

SELECT N'Usuarios demo deben probarse por DNI desde Login; no mostrar hashes ni secretos en esta verificación.' AS Nota;
