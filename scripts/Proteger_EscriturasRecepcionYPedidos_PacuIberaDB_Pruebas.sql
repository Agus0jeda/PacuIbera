/*
    DESTINO EXCLUSIVO: PacuIberaDB_Pruebas.
    Aplicar antes de habilitar sp_ConfirmarRecepcionPedido.
    Reemplaza solo dos procedimientos; no modifica tablas ni datos.
    No ejecutar nuevamente scripts antiguos que reemplacen estos procedimientos.
    Ambos cambios de definicion se publican en una unica transaccion.
*/
USE PacuIberaDB_Pruebas;

IF DB_NAME() <> N'PacuIberaDB_Pruebas'
    THROW 51400, 'El script solo puede ejecutarse sobre PacuIberaDB_Pruebas.', 1;

IF OBJECT_ID(N'dbo.PedidoProveedor', N'U') IS NULL
    THROW 51401, 'Falta dbo.PedidoProveedor.', 1;
IF OBJECT_ID(N'dbo.DetallePedidoProveedor', N'U') IS NULL
    THROW 51402, 'Falta dbo.DetallePedidoProveedor.', 1;
IF OBJECT_ID(N'dbo.RecepcionPedido', N'U') IS NULL
    THROW 51403, 'Falta dbo.RecepcionPedido.', 1;
IF OBJECT_ID(N'dbo.Producto', N'U') IS NULL
    THROW 51404, 'Falta dbo.Producto.', 1;

SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    EXEC(N'
CREATE OR ALTER PROCEDURE dbo.sp_RecepcionarItemPedido
    @DetalleId int,
    @CantidadIngresada int,
    @FechaVencimiento datetime2(0) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    THROW 51410, ''La recepcion por item esta deshabilitada. Utilice la confirmacion definitiva del pedido.'', 1;
END;');

    EXEC(N'
CREATE OR ALTER PROCEDURE dbo.sp_AgregarItemPedidoProveedor
    @PedidoId int,
    @ProductoId int,
    @Cantidad decimal(10,3),
    @PrecioCosto decimal(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- Participar de GuardarPedidoAtomico sin confirmar su transaccion externa.
    DECLARE @TransaccionesEntrada int = @@TRANCOUNT;
    DECLARE @Estado nvarchar(30);

    BEGIN TRY
        IF @TransaccionesEntrada = 0
            BEGIN TRANSACTION;
        ELSE
            SAVE TRANSACTION AgregarItemPedidoSeguro;

        IF @Cantidad IS NULL OR @Cantidad <= 0
            THROW 51110, ''La cantidad del pedido debe ser mayor que cero.'', 1;

        IF @PrecioCosto IS NULL OR @PrecioCosto < 0
            THROW 51111, ''El precio de costo no puede ser negativo.'', 1;

        -- Orden compartido con la confirmacion: cabecera -> historial -> detalle.
        -- Mantener estos bloqueos hasta el fin de la transaccion externa, si existe.
        SELECT @Estado = Estado
        FROM dbo.PedidoProveedor WITH (UPDLOCK, HOLDLOCK)
        WHERE Id = @PedidoId;

        IF @Estado IS NULL
            THROW 51112, ''El pedido indicado no existe.'', 1;

        IF @Estado <> N''Pendiente''
            THROW 51113, ''Solo se pueden agregar items a un pedido pendiente.'', 1;

        IF EXISTS
        (
            SELECT 1
            FROM dbo.RecepcionPedido WITH (UPDLOCK, HOLDLOCK)
            WHERE PedidoId = @PedidoId
        )
            THROW 51411, ''No se pueden agregar items a un pedido con recepcion definitiva.'', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.Producto WHERE Id = @ProductoId)
            THROW 51114, ''El producto indicado no existe.'', 1;

        INSERT INTO dbo.DetallePedidoProveedor
            (PedidoId, ProductoId, Cantidad, CantidadRecibida, PrecioCosto)
        VALUES
            (@PedidoId, @ProductoId, @Cantidad, 0, @PrecioCosto);

        IF @TransaccionesEntrada = 0
            COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @TransaccionesEntrada = 0
        BEGIN
            IF XACT_STATE() <> 0
                ROLLBACK TRANSACTION;
        END
        ELSE IF XACT_STATE() = 1
            ROLLBACK TRANSACTION AgregarItemPedidoSeguro;
        -- Si XACT_STATE() = -1, el llamador debe revertir la transaccion externa.
        -- GuardarPedidoAtomico ya realiza ese rollback en su catch.
        THROW;
    END CATCH;
END;');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
