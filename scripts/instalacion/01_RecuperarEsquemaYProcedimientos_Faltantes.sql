/*
  Recuperación de solo lectura desde PacuIberaDB_Pruebas.
  No instala objetos ni lee/modifica filas de negocio; consulta catálogos y definiciones.
*/
SET NOCOUNT ON;

IF DB_NAME() <> N'PacuIberaDB_Pruebas'
    THROW 54010, 'Conectese a PacuIberaDB_Pruebas para recuperar el esquema fuente.', 1;

DECLARE @Procedimientos TABLE (Nombre sysname NOT NULL PRIMARY KEY);
INSERT @Procedimientos(Nombre) VALUES
 (N'sp_LoginUsuario'),
 (N'sp_InsertarClienteRapido'),(N'sp_InsertarCliente'),(N'sp_ObtenerClientes'),
 (N'sp_ObtenerClientesCRM'),(N'sp_ReporteTopClientes'),(N'sp_ActualizarClienteCRM'),
 (N'sp_ObtenerProvincias'),(N'sp_ObtenerLocalidadesPorProvincia'),(N'sp_ObtenerRoles'),
 (N'sp_RegistrarProducto'),
 (N'sp_RegistrarPagoVenta'),(N'sp_AnularVentaConMotivo'),(N'sp_ObtenerDetalleVenta'),
 (N'sp_ReporteTopProductos'),(N'sp_ReporteMetodosPago'),(N'sp_ReporteStockCritico'),
 (N'sp_ReporteLotesVencidos'),(N'sp_ReporteResumenGlobal'),(N'sp_ReporteRankingVendedores'),
 (N'sp_ReporteAuditoria'),(N'sp_AnalisisCurvaVentas'),(N'sp_AnalisisPerdidaVencimientos'),
 (N'sp_CrearPedidoProveedor'),(N'sp_AgregarItemPedidoProveedor'),(N'sp_ObtenerPedidosProveedor'),
 (N'sp_ObtenerDetallePedidoProveedor'),(N'sp_ObtenerStockDeposito'),(N'sp_ActualizarStockMinimoDeposito'),
 (N'sp_RecepcionarItemPedido'),(N'sp_TransferirDepositoALocal'),(N'sp_ConfirmarRecepcionPedido'),
 (N'sp_EmitirRemitoInterno'),(N'sp_ConfirmarRecepcionRemito'),(N'sp_ResolverDiferenciaRemito'),
 (N'sp_RegistrarVenta'),(N'sp_RegistrarDetalleVentaFIFO'),(N'sp_AbrirCaja'),(N'sp_CerrarCaja'),
 (N'sp_ResumenCajas'),(N'sp_RegistrarMovimientoCaja');

-- Definiciones SQL usadas por la aplicación: recuperar las faltantes y comparar
-- las instaladas con sus scripts fuente. NULL significa ausente, cifrada o sin VIEW DEFINITION.
SELECT p.Nombre,
       OBJECT_ID(N'dbo.' + p.Nombre, N'P') AS ObjectId,
       OBJECT_DEFINITION(OBJECT_ID(N'dbo.' + p.Nombre, N'P')) AS Definicion
FROM @Procedimientos p
ORDER BY p.Nombre;

-- Esquema de tablas, tipos tabulares y columnas, incluidos identidad/default/computed.
SELECT s.name AS Esquema,t.name AS Objeto,
       CASE WHEN t.type='U' THEN N'USER_TABLE' WHEN t.type='TT' THEN N'TABLE_TYPE' ELSE t.type_desc END AS TipoObjeto,
       c.column_id,c.name AS Columna,TYPE_NAME(c.user_type_id) AS Tipo,
       c.max_length,c.precision,c.scale,c.is_nullable,c.is_identity,c.is_computed,
       dc.definition AS DefaultSQL
FROM sys.objects t
INNER JOIN sys.schemas s ON s.schema_id=t.schema_id
LEFT JOIN sys.columns c ON c.object_id=t.object_id
LEFT JOIN sys.default_constraints dc ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
WHERE t.type IN ('U','TT') AND s.name=N'dbo'
ORDER BY t.name,c.column_id;

-- Claves primarias y únicas.
SELECT s.name AS Esquema,t.name AS Tabla,k.name AS Restriccion,k.type_desc,
       i.name AS Indice,i.is_disabled
FROM sys.key_constraints k
INNER JOIN sys.tables t ON t.object_id=k.parent_object_id
INNER JOIN sys.schemas s ON s.schema_id=t.schema_id
LEFT JOIN sys.indexes i ON i.object_id=k.parent_object_id AND i.index_id=k.unique_index_id
WHERE s.name=N'dbo'
ORDER BY t.name,k.type_desc,k.name;

-- CHECK y DEFAULT con definiciones completas.
SELECT s.name AS Esquema,t.name AS Tabla,cc.name AS Restriccion,N'CHECK' AS Tipo,
       cc.is_disabled,cc.is_not_trusted,cc.definition
FROM sys.check_constraints cc
INNER JOIN sys.tables t ON t.object_id=cc.parent_object_id
INNER JOIN sys.schemas s ON s.schema_id=t.schema_id
WHERE s.name=N'dbo'
UNION ALL
SELECT s.name,t.name,dc.name,N'DEFAULT',CONVERT(bit,0),CONVERT(bit,0),dc.definition
FROM sys.default_constraints dc
INNER JOIN sys.tables t ON t.object_id=dc.parent_object_id
INNER JOIN sys.schemas s ON s.schema_id=t.schema_id
WHERE s.name=N'dbo'
ORDER BY Tabla,Tipo,Restriccion;

SELECT ps.name AS EsquemaTabla,pt.name AS Tabla,fk.name AS FK,
       pc.name AS Columna,rs.name AS EsquemaReferenciado,rt.name AS TablaReferenciada,
       rc.name AS ColumnaReferenciada,fkc.constraint_column_id AS OrdenColumna
FROM sys.foreign_key_columns fkc
INNER JOIN sys.foreign_keys fk ON fk.object_id=fkc.constraint_object_id
INNER JOIN sys.tables pt ON pt.object_id=fkc.parent_object_id
INNER JOIN sys.schemas ps ON ps.schema_id=pt.schema_id
INNER JOIN sys.columns pc ON pc.object_id=fkc.parent_object_id AND pc.column_id=fkc.parent_column_id
INNER JOIN sys.tables rt ON rt.object_id=fkc.referenced_object_id
INNER JOIN sys.schemas rs ON rs.schema_id=rt.schema_id
INNER JOIN sys.columns rc ON rc.object_id=fkc.referenced_object_id AND rc.column_id=fkc.referenced_column_id
WHERE ps.name=N'dbo'
ORDER BY pt.name,fk.name,fkc.constraint_column_id;

SELECT s.name AS Esquema,t.name AS Tabla,i.name AS Indice,
       i.is_unique,i.is_primary_key,i.type_desc,i.filter_definition,
       ic.key_ordinal,ic.is_included_column,c.name AS Columna
FROM sys.indexes i
INNER JOIN sys.tables t ON t.object_id=i.object_id
INNER JOIN sys.schemas s ON s.schema_id=t.schema_id
INNER JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
INNER JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
WHERE s.name=N'dbo' AND i.index_id>0 AND i.is_hypothetical=0
ORDER BY t.name,i.name,ic.is_included_column,ic.key_ordinal,ic.index_column_id;

-- Definiciones de los tipos TVP relevantes para la aplicación.
SELECT s.name AS Esquema,t.name AS TipoTabla,
       c.column_id,c.name AS Columna,TYPE_NAME(c.user_type_id) AS Tipo,
       c.max_length,c.precision,c.scale,c.is_nullable
FROM sys.table_types t
INNER JOIN sys.schemas s ON s.schema_id=t.schema_id
INNER JOIN sys.columns c ON c.object_id=t.type_table_object_id
WHERE s.name=N'dbo'
ORDER BY t.name,c.column_id;

SELECT e.class_desc,e.major_id,e.minor_id,e.name AS Propiedad,e.value AS Valor
FROM sys.extended_properties e
WHERE e.name=N'PacuIbera.ControlaVencimientoRecepcion.v1'
   OR (e.class=1 AND e.major_id=OBJECT_ID(N'dbo.sp_ConfirmarRecepcionPedido') AND e.minor_id=0);

-- Roles y usuarios demo existentes: solo conteos, sin nombres, DNI ni hashes.
SELECT r.Nombre AS Rol,COUNT_BIG(CASE WHEN u.Activo=1 THEN 1 END) AS UsuariosActivos
FROM dbo.Rol r LEFT JOIN dbo.Usuario u ON u.RolId=r.Id
GROUP BY r.Nombre ORDER BY r.Nombre;
