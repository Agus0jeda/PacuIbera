/*
  Paso 0 de instalación. Requiere SSMS en modo SQLCMD.
  Cambiar DatabaseName mediante :setvar; no ejecutar sobre master sin revisar.
  Este archivo solo crea la base vacía si no existe; nunca la reemplaza.
*/
:on error exit
:setvar DatabaseName "PacuIbera_Entrega"

USE master;
GO
SET NOCOUNT ON;

DECLARE @NombreBase sysname = N'$(DatabaseName)';

IF DB_NAME() <> N'master'
    THROW 54000, 'Ejecute este paso conectado a master.', 1;

IF NULLIF(LTRIM(RTRIM(@NombreBase)), N'') IS NULL
   OR LEN(@NombreBase) > 128
   OR @NombreBase IN (N'master',N'model',N'msdb',N'tempdb',N'PacuIberaDB',N'PacuIberaDB_Pruebas')
    THROW 54001, 'Nombre de base invalido o protegido. Elija una base nueva distinta de las bases PacuIbera.', 1;

IF DB_ID(@NombreBase) IS NOT NULL
    THROW 54002, 'La base destino ya existe. No se sobrescribira ni modificara.', 1;

DECLARE @Sql nvarchar(max) = N'CREATE DATABASE ' + QUOTENAME(@NombreBase) + N';';
EXEC sys.sp_executesql @Sql;

PRINT CONCAT(N'Base vacia creada: ', QUOTENAME(@NombreBase), N'. Conectese a ella antes de ejecutar futuras etapas.');
