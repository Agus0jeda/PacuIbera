/*
  Datos iniciales de PacuIbera. Fuente: pacutxt2.rpt (recuperación de
  PacuIberaDB_Pruebas). No contiene stock/lotes ni movimientos históricos.

  Los CUIT y las relaciones ProveedorProducto se tomaron de
  PacuIbera_Proveedores.rpt. Los hashes de las cuentas demo corresponden a la
  contraseña 1234: SHA-256 hexadecimal mayúsculo de bytes UTF-8, compatible con
  Seguridad.GenerarHashSHA256. Las claves están en CredencialesDemo.local.txt,
  ignorado por Git.
  Las cuentas usan DNI sintéticos DEMO-... y datos de nombre demo, no personales.

  La ejecución se bloquea deliberadamente con @AutorizarSeed=0. Cambiar la
  autorización solo al instalar en la base aprobada y después de revisar el seed.
  Ejecutar en SSMS SQLCMD Mode en DatabaseName.
*/
:on error exit
:setvar DatabaseName "PacuIbera_Entrega"
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @BaseEsperada sysname=N'$(DatabaseName)';
DECLARE @AutorizarSeed bit=0; -- CAMBIAR A 1 SOLO LUEGO DE COMPLETAR LOS VALORES PENDIENTES

/* Hashes SHA-256 UTF-8 compatibles; no sustituir por contraseñas en texto plano. */
DECLARE @HashAdministrador varchar(255)='03AC674216F3E15C761EE1A5E255F067953623C8B388B4459E13F978D7C846F4';
DECLARE @HashVendedor varchar(255)='03AC674216F3E15C761EE1A5E255F067953623C8B388B4459E13F978D7C846F4';
DECLARE @HashGerente varchar(255)='03AC674216F3E15C761EE1A5E255F067953623C8B388B4459E13F978D7C846F4';

IF DB_NAME()<>@BaseEsperada
    THROW 54100, 'Conectese a la base indicada en DatabaseName.', 1;
IF DB_NAME() IN (N'master',N'model',N'msdb',N'tempdb',N'PacuIberaDB',N'PacuIberaDB_Pruebas')
    THROW 54101, 'El seed no se permite en bases del sistema ni en las bases actuales de PacuIbera.', 1;
IF @AutorizarSeed<>1
    THROW 54102, 'Seed desautorizado. Revise los valores pendientes; no se insertó nada.', 1;

IF OBJECT_ID(N'dbo.Provincia',N'U') IS NULL OR OBJECT_ID(N'dbo.Localidad',N'U') IS NULL
 OR OBJECT_ID(N'dbo.Rol',N'U') IS NULL OR OBJECT_ID(N'dbo.MetodoPago',N'U') IS NULL
 OR OBJECT_ID(N'dbo.Categoria',N'U') IS NULL OR OBJECT_ID(N'dbo.Proveedor',N'U') IS NULL
 OR OBJECT_ID(N'dbo.Producto',N'U') IS NULL OR OBJECT_ID(N'dbo.Usuario',N'U') IS NULL
    THROW 54103, 'Faltan tablas del esquema. Instale primero el esquema completo.', 1;
IF COL_LENGTH(N'dbo.Producto',N'ControlaVencimiento') IS NULL
 OR COL_LENGTH(N'dbo.Proveedor',N'CUIT') IS NULL
 OR COL_LENGTH(N'dbo.Usuario',N'ClaveHash') IS NULL
    THROW 54104, 'El esquema no coincide con las columnas requeridas por este seed.', 1;

DECLARE @Provincias TABLE(Id int PRIMARY KEY,Nombre varchar(100) NOT NULL UNIQUE);
INSERT @Provincias VALUES
 (1,'Buenos Aires'),(2,'Ciudad Autónoma de Buenos Aires'),(3,'Catamarca'),(4,'Chaco'),
 (5,'Chubut'),(6,'Córdoba'),(7,'Corrientes'),(8,'Entre Ríos'),(9,'Formosa'),
 (10,'Jujuy'),(11,'La Pampa'),(12,'La Rioja'),(13,'Mendoza'),(14,'Misiones'),
 (15,'Neuquén'),(16,'Río Negro'),(17,'Salta'),(18,'San Juan'),(19,'San Luis'),
 (20,'Santa Cruz'),(21,'Santa Fe'),(22,'Santiago del Estero'),
 (23,'Tierra del Fuego, Antártida e Islas del Atlántico Sur'),(24,'Tucumán');

DECLARE @Localidades TABLE(Id int PRIMARY KEY,ProvinciaId int NOT NULL,Nombre varchar(100) NOT NULL,
                          UNIQUE(ProvinciaId,Nombre));
INSERT @Localidades VALUES
    (1,1,N'La Plata'),
    (2,1,N'Mar del Plata'),
    (3,1,N'Bahía Blanca'),
    (4,1,N'Tandil'),
    (5,1,N'San Nicolás de los Arroyos'),
    (6,1,N'Pergamino'),
    (7,2,N'CABA'),
    (8,3,N'San Fernando del Valle de Catamarca'),
    (9,3,N'Andalgalá'),
    (10,3,N'Tinogasta'),
    (11,4,N'Resistencia'),
    (12,4,N'Presidencia Roque Sáenz Peña'),
    (13,4,N'Barranqueras'),
    (14,4,N'Fontana'),
    (15,4,N'Villa Ángela'),
    (16,5,N'Rawson'),
    (17,5,N'Comodoro Rivadavia'),
    (18,5,N'Trelew'),
    (19,5,N'Puerto Madryn'),
    (20,6,N'Córdoba'),
    (21,6,N'Río Cuarto'),
    (22,6,N'Villa Carlos Paz'),
    (23,6,N'Villa María'),
    (24,6,N'San Francisco'),
    (25,7,N'Corrientes Capital'),
    (26,7,N'Goya'),
    (27,7,N'Paso de los Libres'),
    (28,7,N'Bella Vista'),
    (29,7,N'Santo Tomé'),
    (30,7,N'Esquina'),
    (31,7,N'Mercedes'),
    (32,7,N'Curuzú Cuatiá'),
    (33,8,N'Paraná'),
    (34,8,N'Concordia'),
    (35,8,N'Gualeguaychú'),
    (36,8,N'Concepción del Uruguay'),
    (37,9,N'Formosa Capital'),
    (38,9,N'Clorinda'),
    (39,9,N'Pirané'),
    (40,10,N'San Salvador de Jujuy'),
    (41,10,N'San Pedro de Jujuy'),
    (42,10,N'Libertador General San Martín'),
    (43,11,N'Santa Rosa'),
    (44,11,N'General Pico'),
    (45,12,N'La Rioja Capital'),
    (46,12,N'Chilecito'),
    (47,13,N'Mendoza Capital'),
    (48,13,N'Godoy Cruz'),
    (49,13,N'Guaymallén'),
    (50,13,N'San Rafael'),
    (51,14,N'Posadas'),
    (52,14,N'Oberá'),
    (53,14,N'Eldorado'),
    (54,14,N'Puerto Iguazú'),
    (55,15,N'Neuquén Capital'),
    (56,15,N'San Martín de los Andes'),
    (57,15,N'Cutral Có'),
    (58,15,N'Plottier'),
    (59,16,N'Viedma'),
    (60,16,N'San Carlos de Bariloche'),
    (61,16,N'General Roca'),
    (62,16,N'Cipolletti'),
    (63,17,N'Salta Capital'),
    (64,17,N'San Ramón de la Nueva Orán'),
    (65,17,N'Tartagal'),
    (66,17,N'Cafayate'),
    (67,18,N'San Juan Capital'),
    (68,18,N'Rawson'),
    (69,18,N'Rivadavia'),
    (70,19,N'San Luis Capital'),
    (71,19,N'Villa Mercedes'),
    (72,19,N'Merlo'),
    (73,20,N'Río Gallegos'),
    (74,20,N'Caleta Olivia'),
    (75,20,N'El Calafate'),
    (76,21,N'Santa Fe Capital'),
    (77,21,N'Rosario'),
    (78,21,N'Venado Tuerto'),
    (79,21,N'Rafaela'),
    (80,21,N'Reconquista'),
    (81,22,N'Santiago del Estero Capital'),
    (82,22,N'La Banda'),
    (83,22,N'Termas de Río Hondo'),
    (84,23,N'Ushuaia'),
    (85,23,N'Río Grande'),
    (86,24,N'San Miguel de Tucumán'),
    (87,24,N'Tafí Viejo'),
    (88,24,N'Concepción');

DECLARE @Metodos TABLE(Id int PRIMARY KEY,Nombre varchar(50) NOT NULL);
/* Los 16 IDs están requeridos por los procedimientos de Caja exportados; son alias
   del mismo medio y no filas innecesarias. C# crea pagos nuevos con los IDs 1..4. */
INSERT @Metodos VALUES
 (1,'Efectivo'),(2,'Tarjeta de Débito'),(3,'Tarjeta de Crédito'),(4,'Mercado Pago / Transferencia'),
 (1002,'Efectivo'),(1003,'Tarjeta de Débito'),(1004,'Tarjeta de Crédito'),(1005,'Mercado Pago / Transferencia'),
 (1006,'Efectivo'),(1007,'Tarjeta de Débito'),(1008,'Tarjeta de Crédito'),(1009,'Mercado Pago / Transferencia'),
 (2006,'Efectivo'),(2007,'Tarjeta de Débito'),(2008,'Tarjeta de Crédito'),(2009,'Mercado Pago / Transferencia');

DECLARE @Categorias TABLE(Id int PRIMARY KEY,Nombre varchar(100) NOT NULL);
INSERT @Categorias VALUES (1,'CONGELADOS '),(2,'ELABORADOS'),(1002,'LEÑAS'),
                         (2002,'BEBIDAS'),(2003,'HIELO');

DECLARE @Proveedores TABLE(Id int PRIMARY KEY,RazonSocial varchar(100) NOT NULL,CUIT varchar(20) NOT NULL,Activo bit NOT NULL);
INSERT @Proveedores VALUES
 (1,'Piscicultura NEA S.A.','30-71112223-4',1),
 (2,'Frigorífico Pacú Porá','30-72223334-5',1),
 (3,'Acuícola Chaco S.R.L.','30-73334445-6',1),
 (4,'Cooperativa Río Paraná','33-74445556-9',1),
 (5,'Carbones El Quebracho','20-25556667-8',1),
 (6,'Hielos Frozen SRL','99-23334337-4',1);

DECLARE @Productos TABLE(Id int PRIMARY KEY,Nombre varchar(100) NOT NULL,CategoriaId int NOT NULL,
    PrecioVenta decimal(10,2) NOT NULL,StockMinimo decimal(10,3) NOT NULL,
    SeVendePorPeso bit NOT NULL,ControlaVencimiento bit NOT NULL);
INSERT @Productos VALUES
 (2006,'FILET DESPINADO',1,12500.00,3.500,1,1),
 (3006,'MEDALLONES',1,5000.00,3.000,0,1),
 (3007,'CARBON',1002,2500.00,3.000,0,0),
 (3008,'MILANESA',1,6000.00,3.000,1,1),
 (3009,'HIELO 10 KG',2003,5000.00,3.000,0,0),
 (3011,'VINO TINTO',2002,7000.00,2.000,0,1),
 (3012,'HIELO 2KG',2003,2500.00,3.000,0,1),
 (3013,'EMPANADAS',2,12000.00,3.000,0,1);

DECLARE @ProveedorProducto TABLE(ProveedorId int NOT NULL,ProductoId int NOT NULL,Activo bit NOT NULL,
                                  PRIMARY KEY(ProveedorId,ProductoId));
INSERT @ProveedorProducto VALUES (1,2006,1),(6,3009,1),(6,3012,1);

DECLARE @UsuariosDemo TABLE(Nombre varchar(100),Apellido varchar(100),DNI varchar(20) PRIMARY KEY,
                            ClaveHash varchar(255),RolId int,ProvinciaId int,LocalidadId int);
INSERT @UsuariosDemo VALUES
 ('Demo','Administrador','11111111',@HashAdministrador,0,7,25),
 ('Demo','Vendedor','22222222',@HashVendedor,1,7,25),
 ('Demo','Gerente','33333333',@HashGerente,2,7,25);
IF (SELECT COUNT(DISTINCT DNI) FROM @UsuariosDemo)<>3
    THROW 54120, 'Los DNI demo deben ser únicos.', 1;

IF EXISTS (SELECT 1 FROM @Proveedores WHERE LEN(REPLACE(CUIT,'-',''))<>11
           OR REPLACE(CUIT,'-','') LIKE '%[^0-9]%')
    THROW 54105, 'Un CUIT del reporte no tiene el formato esperado de 11 dígitos.', 1;
IF (SELECT COUNT(DISTINCT REPLACE(CUIT,'-','')) FROM @Proveedores)<>6
    THROW 54106, 'Los CUIT de proveedores deben ser distintos.', 1;
IF EXISTS (SELECT 1 FROM (VALUES(@HashAdministrador),(@HashVendedor),(@HashGerente)) h(Valor)
           WHERE Valor IS NULL OR LEN(Valor)<>64 OR Valor LIKE '%[^0-9A-F]%')
    THROW 54107, 'Faltan hashes demo SHA-256 hexadecimales. Genere hashes fuera de SQL y complete las variables.', 1;
IF EXISTS (SELECT 1 FROM @Provincias p WHERE EXISTS
           (SELECT 1 FROM dbo.Provincia x WHERE x.Id=p.Id AND x.Nombre<>p.Nombre)
           OR EXISTS (SELECT 1 FROM dbo.Provincia x WHERE x.Nombre=p.Nombre AND x.Id<>p.Id))
    THROW 54108, 'Hay provincias existentes en conflicto; no se modificará el destino.', 1;
IF EXISTS (SELECT 1 FROM @Localidades l WHERE NOT EXISTS
           (SELECT 1 FROM @Provincias p WHERE p.Id=l.ProvinciaId))
    THROW 54109, 'El seed contiene una localidad sin provincia.', 1;
IF EXISTS (SELECT 1 FROM @Localidades l WHERE EXISTS
           (SELECT 1 FROM dbo.Localidad x WHERE x.Id=l.Id AND (x.Nombre<>l.Nombre OR x.ProvinciaId<>l.ProvinciaId))
           OR EXISTS (SELECT 1 FROM dbo.Localidad x WHERE x.Nombre=l.Nombre AND x.ProvinciaId=l.ProvinciaId AND x.Id<>l.Id))
    THROW 54110, 'Hay localidades existentes en conflicto; no se modificará el destino.', 1;
IF EXISTS (SELECT 1 FROM @Metodos m WHERE EXISTS
           (SELECT 1 FROM dbo.MetodoPago x WHERE x.Id=m.Id AND
             (x.Nombre COLLATE Latin1_General_100_CI_AI NOT LIKE
               CASE WHEN m.Id IN (1,1002,1006,2006) THEN '%efectivo%'
                    WHEN m.Id IN (2,1003,1007,2007) THEN '%debito%'
                    WHEN m.Id IN (3,1004,1008,2008) THEN '%credito%'
                    ELSE '%transfer%' END
              AND NOT (m.Id IN (4,1005,1009,2009) AND x.Nombre COLLATE Latin1_General_100_CI_AI LIKE '%mercado pago%'))))
    THROW 54111, 'Un ID de método de pago ya existe con una clasificación incompatible.', 1;
IF EXISTS (SELECT 1 FROM @Categorias c WHERE EXISTS
           (SELECT 1 FROM dbo.Categoria x WHERE x.Id=c.Id AND RTRIM(x.Nombre)<>RTRIM(c.Nombre))
           OR EXISTS (SELECT 1 FROM dbo.Categoria x WHERE RTRIM(x.Nombre)=RTRIM(c.Nombre) AND x.Id<>c.Id))
    THROW 54112, 'Hay categorías existentes en conflicto; no se modificará el destino.', 1;
IF EXISTS (SELECT 1 FROM @Proveedores p WHERE EXISTS
           (SELECT 1 FROM dbo.Proveedor x WHERE x.Id=p.Id AND
             (x.RazonSocial<>p.RazonSocial OR x.CUIT<>p.CUIT OR ISNULL(x.Activo,0)<>p.Activo))
           OR EXISTS (SELECT 1 FROM dbo.Proveedor x
                      WHERE REPLACE(x.CUIT,'-','')=REPLACE(p.CUIT,'-','') AND x.Id<>p.Id))
    THROW 54113, 'Hay proveedores existentes en conflicto; no se modificará el destino.', 1;
IF EXISTS (SELECT 1 FROM @Productos p WHERE NOT EXISTS
           (SELECT 1 FROM @Categorias c WHERE c.Id=p.CategoriaId))
    THROW 54114, 'El seed contiene productos sin categoría.', 1;
IF EXISTS (SELECT 1 FROM @Productos p WHERE EXISTS
           (SELECT 1 FROM dbo.Producto x WHERE x.Id=p.Id AND
             (x.Nombre<>p.Nombre OR x.CategoriaId<>p.CategoriaId OR x.PrecioVenta<>p.PrecioVenta
              OR x.StockMinimo<>p.StockMinimo OR x.SeVendePorPeso<>p.SeVendePorPeso
              OR x.ControlaVencimiento<>p.ControlaVencimiento OR ISNULL(x.Activo,0)<>1
              OR x.StockActual<>0))
           OR EXISTS (SELECT 1 FROM dbo.Producto x WHERE x.Nombre=p.Nombre AND x.Id<>p.Id))
    THROW 54115, 'Hay productos existentes en conflicto; no se modificarán fichas ni existencias.', 1;
IF EXISTS (SELECT 1 FROM @ProveedorProducto pp
           WHERE NOT EXISTS (SELECT 1 FROM @Proveedores p WHERE p.Id=pp.ProveedorId)
              OR NOT EXISTS (SELECT 1 FROM @Productos p WHERE p.Id=pp.ProductoId))
    THROW 54118, 'Una relación proveedor-producto no tiene sus registros principales en el seed.', 1;
IF EXISTS (SELECT 1 FROM @ProveedorProducto pp JOIN dbo.ProveedorProducto x
           ON x.ProveedorId=pp.ProveedorId AND x.ProductoId=pp.ProductoId
           WHERE x.Activo<>pp.Activo)
    THROW 54119, 'Una relación proveedor-producto existente tiene estado diferente.', 1;
IF EXISTS (SELECT 1 FROM dbo.Rol WHERE (Id=0 AND Nombre<>'Administrador')
           OR (Id=1 AND Nombre<>'Vendedor') OR (Id=2 AND Nombre<>'Gerente'))
 OR EXISTS (SELECT 1 FROM (VALUES(0,'Administrador'),(1,'Vendedor'),(2,'Gerente')) e(Id,Nombre)
            WHERE NOT EXISTS (SELECT 1 FROM dbo.Rol r WHERE r.Id=e.Id AND r.Nombre=e.Nombre))
 OR EXISTS (SELECT Nombre FROM dbo.Rol WHERE Nombre IN ('Administrador','Vendedor','Gerente')
            GROUP BY Nombre HAVING COUNT_BIG(*)<>1)
    THROW 54116, 'Instale primero los roles con IDs 0, 1 y 2 mediante el paso 02.', 1;
IF EXISTS
(
    SELECT 1 FROM @UsuariosDemo d JOIN dbo.Usuario u ON u.DNI=d.DNI
    WHERE u.Nombre<>d.Nombre OR u.Apellido<>d.Apellido OR u.ClaveHash<>d.ClaveHash
       OR u.RolId<>d.RolId OR u.ProvinciaId<>d.ProvinciaId OR u.LocalidadId<>d.LocalidadId OR u.Activo<>1
)
    THROW 54117, 'Un DNI demo ya existe con otros datos. Revise Usuario sin sobrescribir cuentas existentes.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    SET IDENTITY_INSERT dbo.Provincia ON;
    INSERT dbo.Provincia(Id,Nombre) SELECT p.Id,p.Nombre FROM @Provincias p
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Provincia x WITH (UPDLOCK,HOLDLOCK) WHERE x.Id=p.Id);
    SET IDENTITY_INSERT dbo.Provincia OFF;

    SET IDENTITY_INSERT dbo.Localidad ON;
    INSERT dbo.Localidad(Id,Nombre,ProvinciaId) SELECT l.Id,l.Nombre,l.ProvinciaId FROM @Localidades l
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Localidad x WITH (UPDLOCK,HOLDLOCK) WHERE x.Id=l.Id);
    SET IDENTITY_INSERT dbo.Localidad OFF;

    SET IDENTITY_INSERT dbo.MetodoPago ON;
    INSERT dbo.MetodoPago(Id,Nombre) SELECT m.Id,m.Nombre FROM @Metodos m
    WHERE NOT EXISTS (SELECT 1 FROM dbo.MetodoPago x WITH (UPDLOCK,HOLDLOCK) WHERE x.Id=m.Id);
    SET IDENTITY_INSERT dbo.MetodoPago OFF;

    SET IDENTITY_INSERT dbo.Categoria ON;
    INSERT dbo.Categoria(Id,Nombre) SELECT c.Id,c.Nombre FROM @Categorias c
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Categoria x WITH (UPDLOCK,HOLDLOCK) WHERE x.Id=c.Id);
    SET IDENTITY_INSERT dbo.Categoria OFF;

    SET IDENTITY_INSERT dbo.Proveedor ON;
    INSERT dbo.Proveedor(Id,RazonSocial,CUIT,Activo)
    SELECT p.Id,p.RazonSocial,p.CUIT,p.Activo FROM @Proveedores p
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Proveedor x WITH (UPDLOCK,HOLDLOCK) WHERE x.Id=p.Id);
    SET IDENTITY_INSERT dbo.Proveedor OFF;

    SET IDENTITY_INSERT dbo.Producto ON;
    INSERT dbo.Producto(Id,Nombre,CategoriaId,PrecioVenta,StockMinimo,SeVendePorPeso,
                        Descripcion,Activo,StockActual,CodigoBarras,ControlaVencimiento)
    SELECT p.Id,p.Nombre,p.CategoriaId,p.PrecioVenta,p.StockMinimo,p.SeVendePorPeso,
           NULL,1,0,NULL,p.ControlaVencimiento
    FROM @Productos p
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Producto x WITH (UPDLOCK,HOLDLOCK) WHERE x.Id=p.Id);
    SET IDENTITY_INSERT dbo.Producto OFF;

    INSERT dbo.ProveedorProducto(ProveedorId,ProductoId,Activo)
    SELECT pp.ProveedorId,pp.ProductoId,pp.Activo FROM @ProveedorProducto pp
    WHERE NOT EXISTS
      (SELECT 1 FROM dbo.ProveedorProducto x WITH (UPDLOCK,HOLDLOCK)
       WHERE x.ProveedorId=pp.ProveedorId AND x.ProductoId=pp.ProductoId);

    INSERT dbo.Usuario(Nombre,Apellido,DNI,ClaveHash,RolId,ProvinciaId,LocalidadId,Activo)
    SELECT d.Nombre,d.Apellido,d.DNI,d.ClaveHash,d.RolId,d.ProvinciaId,d.LocalidadId,1
    FROM @UsuariosDemo d
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Usuario u WITH (UPDLOCK,HOLDLOCK) WHERE u.DNI=d.DNI);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    BEGIN TRY SET IDENTITY_INSERT dbo.Provincia OFF; END TRY BEGIN CATCH END CATCH;
    BEGIN TRY SET IDENTITY_INSERT dbo.Localidad OFF; END TRY BEGIN CATCH END CATCH;
    BEGIN TRY SET IDENTITY_INSERT dbo.MetodoPago OFF; END TRY BEGIN CATCH END CATCH;
    BEGIN TRY SET IDENTITY_INSERT dbo.Categoria OFF; END TRY BEGIN CATCH END CATCH;
    BEGIN TRY SET IDENTITY_INSERT dbo.Proveedor OFF; END TRY BEGIN CATCH END CATCH;
    BEGIN TRY SET IDENTITY_INSERT dbo.Producto OFF; END TRY BEGIN CATCH END CATCH;
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

/* Resumen de verificación sin exponer hashes ni datos personales. */
SELECT N'Provincia' AS Tipo,COUNT_BIG(*) AS Cantidad FROM dbo.Provincia WHERE Id BETWEEN 1 AND 24
UNION ALL SELECT N'Localidad',COUNT_BIG(*) FROM dbo.Localidad WHERE Id BETWEEN 1 AND 88
UNION ALL SELECT N'Método de pago',COUNT_BIG(*) FROM dbo.MetodoPago WHERE Id BETWEEN 1 AND 4
UNION ALL SELECT N'Categoría',COUNT_BIG(*) FROM dbo.Categoria WHERE Id IN (1,2,1002,2002,2003)
UNION ALL SELECT N'Proveedor activo',COUNT_BIG(*) FROM dbo.Proveedor WHERE Id BETWEEN 1 AND 6 AND Activo=1
UNION ALL SELECT N'Producto activo con stock cero',COUNT_BIG(*) FROM dbo.Producto WHERE Id IN (2006,3006,3007,3008,3009,3011,3012,3013) AND Activo=1 AND StockActual=0
UNION ALL SELECT N'Relación proveedor-producto activa',COUNT_BIG(*) FROM dbo.ProveedorProducto WHERE (ProveedorId=1 AND ProductoId=2006 OR ProveedorId=6 AND ProductoId IN (3009,3012)) AND Activo=1
UNION ALL SELECT N'Usuario demo activo',COUNT_BIG(*) FROM dbo.Usuario WHERE DNI IN ('11111111','22222222','33333333') AND Activo=1;

SELECT Id,Nombre FROM dbo.Rol WHERE Id IN (0,1,2) ORDER BY Id;
SELECT Id,Nombre FROM dbo.MetodoPago
WHERE Id IN (1,2,3,4,1002,1003,1004,1005,1006,1007,1008,1009,2006,2007,2008,2009)
ORDER BY Id;
SELECT Id,Nombre,CategoriaId,PrecioVenta,StockMinimo,SeVendePorPeso,ControlaVencimiento,Activo,StockActual
FROM dbo.Producto WHERE Id IN (2006,3006,3007,3008,3009,3011,3012,3013) ORDER BY Id;
SELECT Id,RazonSocial,Activo FROM dbo.Proveedor WHERE Id BETWEEN 1 AND 6 ORDER BY Id;
SELECT ProveedorId,ProductoId,Activo FROM dbo.ProveedorProducto
WHERE (ProveedorId=1 AND ProductoId=2006) OR (ProveedorId=6 AND ProductoId IN (3009,3012))
ORDER BY ProveedorId,ProductoId;
SELECT fk.name AS ClaveForanea,t.name AS Tabla, fk.is_disabled AS Deshabilitada,
       fk.is_not_trusted AS NoConfiable
FROM sys.foreign_keys fk
JOIN sys.tables t ON t.object_id=fk.parent_object_id
WHERE t.name IN (N'Localidad',N'Proveedor',N'Producto',N'Usuario')
ORDER BY t.name,fk.name;
