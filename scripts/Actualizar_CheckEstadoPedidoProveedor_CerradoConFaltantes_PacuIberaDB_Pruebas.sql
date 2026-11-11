/*
	Amplía los estados permitidos de PedidoProveedor.
	DESTINO EXCLUSIVO: PacuIberaDB_Pruebas.

	No modifica registros, procedimientos, tablas ni stock.
	Solo reemplaza CK_PedidoProveedor_Estado por la misma restricción
	incluyendo N'Cerrado con faltantes'.
*/

USE PacuIberaDB_Pruebas;

IF DB_NAME() <> N'PacuIberaDB_Pruebas'
	THROW 51230, 'El script solo puede ejecutarse sobre PacuIberaDB_Pruebas.', 1;

IF OBJECT_ID(N'dbo.PedidoProveedor', N'U') IS NULL
	THROW 51231, 'Falta dbo.PedidoProveedor.', 1;

DECLARE @RestriccionId int = OBJECT_ID(N'dbo.PedidoProveedor', N'U');
DECLARE @Definicion nvarchar(max);
DECLARE @DefinicionNormalizada nvarchar(max);

SELECT @Definicion = cc.definition
FROM sys.check_constraints cc
WHERE cc.name = N'CK_PedidoProveedor_Estado'
  AND cc.parent_object_id = @RestriccionId;

IF @Definicion IS NULL
	THROW 51232, 'No se encontró CK_PedidoProveedor_Estado en dbo.PedidoProveedor.', 1;

SET @DefinicionNormalizada = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@Definicion, N' ', N''), NCHAR(13), N''), NCHAR(10), N''), NCHAR(9), N''), N'[', N'');
SET @DefinicionNormalizada = REPLACE(@DefinicionNormalizada, N']', N'');

/*
	Se verifica la definición actual antes de reemplazarla. Si el nombre
	corresponde a otra regla, el script se detiene sin modificarla.
*/
IF @DefinicionNormalizada NOT IN
(
	N'(Estado=N''Completado''OREstado=N''ParcialmenteRecibido''OREstado=N''Pendiente'')',
	N'(EstadoIN(N''Pendiente'',N''ParcialmenteRecibido'',N''Completado''))'
)
BEGIN
	/* Si ya contiene el nuevo estado, la segunda ejecución es segura. */
	IF @DefinicionNormalizada IN
	(
		N'(Estado=N''Cerradoconfaltantes''OREstado=N''Completado''OREstado=N''ParcialmenteRecibido''OREstado=N''Pendiente'')',
		N'(EstadoIN(N''Pendiente'',N''ParcialmenteRecibido'',N''Completado'',N''Cerradoconfaltantes''))'
	)
		RETURN;

	THROW 51233, 'La definición actual de CK_PedidoProveedor_Estado no coincide con la esperada. No se modificó.', 1;
END;

SET XACT_ABORT ON;

BEGIN TRY
	BEGIN TRANSACTION;

	ALTER TABLE dbo.PedidoProveedor
		DROP CONSTRAINT CK_PedidoProveedor_Estado;

	ALTER TABLE dbo.PedidoProveedor
		ADD CONSTRAINT CK_PedidoProveedor_Estado
		CHECK (Estado IN
		(
			N'Pendiente',
			N'Parcialmente Recibido',
			N'Completado',
			N'Cerrado con faltantes'
		));

	COMMIT TRANSACTION;
END TRY
BEGIN CATCH
	IF XACT_STATE() <> 0
		ROLLBACK TRANSACTION;
	THROW;
END CATCH;
