/*
  Carga idempotente de los roles con los Id confirmados por C# y pacutxt2.rpt.
  Se conserva Id 0=Administrador, 1=Vendedor, 2=Gerente. El registro duplicado
  Gerente Id 3 del origen no se importa: no forma parte del catálogo objetivo.
  Ejecutar en SQLCMD Mode luego de instalar el esquema.
*/
:on error exit
:setvar DatabaseName "PacuIbera_Entrega"
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME()<>N'$(DatabaseName)'
    THROW 54040, 'Conectese a DatabaseName antes de cargar los roles.', 1;
IF DB_NAME() IN (N'PacuIberaDB',N'PacuIberaDB_Pruebas')
    THROW 54043, 'La carga de roles no se permite en las bases actuales de PacuIbera.', 1;
IF OBJECT_ID(N'dbo.Rol',N'U') IS NULL
    THROW 54041, 'Falta dbo.Rol; instale primero 01_InstalarEsquemaCompleto_Configurable.sql.', 1;

DECLARE @Esperados TABLE(Id int PRIMARY KEY,Nombre varchar(50) NOT NULL UNIQUE);
INSERT @Esperados VALUES (0,'Administrador'),(1,'Vendedor'),(2,'Gerente');

/* Si un Id o nombre ya existe con otra asignación, no corregirlo a ciegas. */
IF EXISTS
(
    SELECT 1 FROM @Esperados e
    LEFT JOIN dbo.Rol rId ON rId.Id=e.Id
    WHERE rId.Id IS NOT NULL AND rId.Nombre<>e.Nombre
)
    THROW 54044, 'Un Id de rol esperado ya pertenece a otro nombre. Revise el destino.', 1;
IF EXISTS
(
    SELECT 1 FROM @Esperados e
    JOIN dbo.Rol rNombre ON rNombre.Nombre=e.Nombre AND rNombre.Id<>e.Id
)
    THROW 54045, 'Un nombre de rol esperado ya existe con otro Id. Revise el destino.', 1;
IF EXISTS
(
    SELECT Nombre FROM dbo.Rol
    WHERE Nombre IN ('Administrador','Vendedor','Gerente')
    GROUP BY Nombre HAVING COUNT_BIG(*)>1
)
    THROW 54046, 'Hay roles duplicados en el destino; no se modifico ninguno.', 1;

BEGIN TRY
    BEGIN TRANSACTION;
    SET IDENTITY_INSERT dbo.Rol ON;
    INSERT dbo.Rol(Id,Nombre)
    SELECT e.Id,e.Nombre
    FROM @Esperados e
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Rol r WITH (UPDLOCK,HOLDLOCK) WHERE r.Id=e.Id);
    SET IDENTITY_INSERT dbo.Rol OFF;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    BEGIN TRY SET IDENTITY_INSERT dbo.Rol OFF; END TRY BEGIN CATCH END CATCH;
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT Id,Nombre FROM dbo.Rol
WHERE Id IN (0,1,2) OR Nombre IN ('Administrador','Vendedor','Gerente')
ORDER BY Id;
