using System.Data;
using Microsoft.Data.SqlClient;
using PacuIbera.Dominio;

namespace Datos;

public partial class ProductoDatos
{
    public const int DiasAlertaVencimientoLocal = 30;
    // El formulario antiguo guardaba esta fecha al desmarcar Tiene vencimiento.
    private static readonly DateTime FechaSinVencimientoLegada = new(2099, 12, 31);
    public static bool PuedeConsultarFichas => SesionActiva.IdUsuario > 0
        && SesionActiva.Rol is "Administrador" or "SuperAdministrador" or "Gerente" or "Vendedor";
    public static bool PuedeEditarFichas => SesionActiva.IdUsuario > 0
        && SesionActiva.Rol is "Administrador" or "SuperAdministrador";

    private async Task<SqlConnection> AbrirFichasPruebasAsync(bool escritura = false)
    {
        var configuracion = new SqlConnectionStringBuilder(CadenaGlobal);
        if (!PuedeConsultarFichas || (escritura && !PuedeEditarFichas))
            throw new UnauthorizedAccessException("Su usuario no tiene permiso para esta operación.");
        if (Environment.GetEnvironmentVariable("PACUIBERA_DB_PRUEBAS") != "1"
            || configuracion.InitialCatalog != "PacuIberaDB_Pruebas")
            throw new InvalidOperationException("Abra Productos con el perfil PacuIbera - Pruebas.");
        var conexion = ObtenerConexion();
        try
        {
            await conexion.OpenAsync();
            using var comprobar = new SqlCommand(@"
                SELECT DB_NAME() WHERE EXISTS
                (SELECT 1 FROM dbo.Usuario u JOIN dbo.Rol r ON r.Id=u.RolId
                 WHERE u.Id=@Usuario AND u.Activo=1 AND r.Nombre=@Rol);", conexion);
            comprobar.Parameters.Add("@Usuario", SqlDbType.Int).Value = SesionActiva.IdUsuario;
            comprobar.Parameters.Add("@Rol", SqlDbType.NVarChar, 30).Value = SesionActiva.Rol;
            if (!string.Equals(await comprobar.ExecuteScalarAsync() as string, "PacuIberaDB_Pruebas", StringComparison.Ordinal))
                throw new UnauthorizedAccessException("No se verificó la base de pruebas o el usuario activo.");
            return conexion;
        }
        catch { conexion.Dispose(); throw; }
    }

    public async Task<List<CategoriaProducto>> ConsultarCategoriasFichaAsync()
    {
        using var conexion = await AbrirFichasPruebasAsync();
        using var cmd = new SqlCommand("SELECT Id,Nombre FROM dbo.Categoria ORDER BY Nombre;", conexion);
        using var lector = await cmd.ExecuteReaderAsync();
        var categorias = new List<CategoriaProducto>();
        while (await lector.ReadAsync()) categorias.Add(new(lector.GetInt32(0), lector.GetString(1)));
        return categorias;
    }

    public async Task<List<ProductoFicha>> ConsultarFichasAsync()
    {
        using var conexion = await AbrirFichasPruebasAsync();
        // SELECT: separar el saldo registrado del stock utilizable por lotes; nunca corregirlos.
        using var cmd = new SqlCommand(@"
            SELECT p.Id,p.Nombre,p.CategoriaId,ISNULL(c.Nombre,'Sin categoría'),p.PrecioVenta,
                p.StockActual,p.StockMinimo,p.SeVendePorPeso,ISNULL(p.Activo,0),
                ISNULL(p.Descripcion,''),ISNULL(p.CodigoBarras,''),l.ProximoVencimiento,
                ISNULL(l.PorVencer,0),ISNULL(l.Vencidos,0),ISNULL(l.Utilizable,0),
                ISNULL(l.ActualTodos,0),ISNULL(l.ActualActivos,0),p.ControlaVencimiento,
                CONVERT(bit,CASE WHEN l.SinFecha>0 AND l.ConFecha=0 THEN 1 ELSE 0 END)
            FROM dbo.Producto p LEFT JOIN dbo.Categoria c ON c.Id=p.CategoriaId
            OUTER APPLY
            (SELECT MIN(CASE WHEN Activo=1 AND StockActual>0 AND FechaVencimiento>=DATEADD(day,1,CONVERT(date,GETDATE())) THEN FechaVencimiento END) AS ProximoVencimiento,
                SUM(CASE WHEN Activo=1 AND StockActual>0 AND FechaVencimiento>=DATEADD(day,1,CONVERT(date,GETDATE()))
                          AND FechaVencimiento<DATEADD(day,@DiasAlerta+1,CONVERT(date,GETDATE())) THEN 1 ELSE 0 END) AS PorVencer,
                SUM(CASE WHEN StockActual>0 AND FechaVencimiento<CONVERT(date,GETDATE()) THEN 1 ELSE 0 END) AS Vencidos,
                SUM(CASE WHEN Activo=1 AND StockActual>0 AND(FechaVencimiento IS NULL OR FechaVencimiento>=CONVERT(date,GETDATE()))
                         THEN CONVERT(decimal(28,3),StockActual) ELSE 0 END) AS Utilizable,
                SUM(CONVERT(decimal(28,3),StockActual)) AS ActualTodos,
                SUM(CASE WHEN Activo=1 THEN CONVERT(decimal(28,3),StockActual) ELSE 0 END) AS ActualActivos,
                SUM(CASE WHEN Activo=1 AND StockActual>0 AND FechaVencimiento IS NULL THEN 1 ELSE 0 END) AS SinFecha,
                SUM(CASE WHEN Activo=1 AND StockActual>0 AND FechaVencimiento IS NOT NULL THEN 1 ELSE 0 END) AS ConFecha
             FROM (SELECT ProductoId,Activo,StockActual,
                 CASE WHEN CONVERT(date,FechaVencimiento)=@SinVencimiento THEN NULL ELSE FechaVencimiento END AS FechaVencimiento
                 FROM dbo.Lote) fechas WHERE ProductoId=p.Id) l
            ORDER BY p.Nombre,p.Id;", conexion);
        cmd.Parameters.Add("@DiasAlerta",SqlDbType.Int).Value=DiasAlertaVencimientoLocal;
        cmd.Parameters.Add("@SinVencimiento",SqlDbType.Date).Value=FechaSinVencimientoLegada;
        using var lector = await cmd.ExecuteReaderAsync();
        var productos = new List<ProductoFicha>();
        while (await lector.ReadAsync()) productos.Add(new(lector.GetInt32(0), lector.GetString(1), lector.GetInt32(2),
            lector.GetString(3), lector.GetDecimal(4), lector.GetDecimal(5), lector.GetDecimal(6), lector.GetBoolean(7),
            lector.GetBoolean(8), lector.GetString(9), lector.GetString(10), lector.IsDBNull(11) ? null : lector.GetDateTime(11),
            lector.GetInt32(12), lector.GetInt32(13))
            {
                StockUtilizable=lector.GetDecimal(14), StockActualLotes=lector.GetDecimal(15), StockLotesActivos=lector.GetDecimal(16),
                ControlaVencimiento=lector.GetBoolean(17), SinVencimientoLocal=lector.GetBoolean(18)
            });
        return productos;
    }

    public async Task<int> CrearCategoriaFichaAsync(string nombre)
    {
        nombre=nombre.Trim();
        if(nombre.Length==0 || nombre.Length>100) throw new InvalidOperationException("Indique una categoría de hasta 100 caracteres.");
        using var conexion=await AbrirFichasPruebasAsync(escritura:true);
        using var cmd=new SqlCommand(@"IF EXISTS(SELECT 1 FROM dbo.Categoria WHERE Nombre=@Nombre)
                SELECT Id FROM dbo.Categoria WHERE Nombre=@Nombre;
            ELSE INSERT INTO dbo.Categoria(Nombre) OUTPUT INSERTED.Id VALUES(@Nombre);",conexion);
        cmd.Parameters.Add("@Nombre",SqlDbType.VarChar,100).Value=nombre;
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public async Task<DataTable> ConsultarLotesFichaAsync(int productoId)
    {
        // Mantener la restriccion previa del Vendedor a la consulta de lotes desde Productos.
        if(SesionActiva.Rol=="Vendedor") throw new UnauthorizedAccessException("Su rol no permite consultar lotes desde Productos.");
        using var conexion=await AbrirFichasPruebasAsync();
        using var cmd=new SqlCommand(@"SELECT Id,FechaIngreso,FechaVencimiento,StockInicial,StockActual,Activo,
            CASE WHEN Activo=0 THEN CASE WHEN FechaVencimiento<CONVERT(date,GETDATE()) THEN N'Inactivo (vencido)' ELSE N'Inactivo' END
                 WHEN StockActual<=0 THEN N'Agotado'
                 WHEN FechaVencimiento<CONVERT(date,GETDATE()) THEN N'Vencido'
                 WHEN FechaVencimiento>=DATEADD(day,1,CONVERT(date,GETDATE()))
                    AND FechaVencimiento<DATEADD(day,@DiasAlerta+1,CONVERT(date,GETDATE())) THEN N'Próximo a vencer'
                 ELSE N'Disponible' END AS Estado,
            CONVERT(bit,CASE WHEN FechaVencimiento<CONVERT(date,GETDATE()) THEN 1 ELSE 0 END) AS EsVencido,
            CONVERT(bit,CASE WHEN Activo=1 AND StockActual>0 AND FechaVencimiento>=DATEADD(day,1,CONVERT(date,GETDATE()))
                AND FechaVencimiento<DATEADD(day,@DiasAlerta+1,CONVERT(date,GETDATE())) THEN 1 ELSE 0 END) AS EsProximo,
            CONVERT(bit,CASE WHEN Activo=1 AND StockActual>0 AND(FechaVencimiento IS NULL OR FechaVencimiento>=CONVERT(date,GETDATE()))
                THEN 1 ELSE 0 END) AS EsUtilizable
            FROM (SELECT Id,ProductoId,FechaIngreso,StockInicial,StockActual,Activo,
                CASE WHEN CONVERT(date,FechaVencimiento)=@SinVencimiento THEN NULL ELSE FechaVencimiento END AS FechaVencimiento
                FROM dbo.Lote) fechas WHERE ProductoId=@Id ORDER BY CASE WHEN FechaVencimiento IS NULL THEN 1 ELSE 0 END,FechaVencimiento,Id;",conexion);
        cmd.Parameters.Add("@Id",SqlDbType.Int).Value=productoId;
        cmd.Parameters.Add("@DiasAlerta",SqlDbType.Int).Value=DiasAlertaVencimientoLocal;
        cmd.Parameters.Add("@SinVencimiento",SqlDbType.Date).Value=FechaSinVencimientoLegada;
        using var lector=await cmd.ExecuteReaderAsync();
        var tabla=new DataTable();tabla.Load(lector);return tabla;
    }

    public async Task<List<LoteVencidoProducto>> ConsultarHistorialVencidosAsync()
    {
        using var conexion=await AbrirFichasPruebasAsync();
        using var cmd=new SqlCommand(@"SELECT p.Nombre,l.Id,l.FechaVencimiento,l.StockActual,
                CASE WHEN l.Activo=0 THEN N'Vencido · Inactivo' ELSE N'Vencido' END,p.SeVendePorPeso
            FROM dbo.Lote l JOIN dbo.Producto p ON p.Id=l.ProductoId
            WHERE l.FechaVencimiento<CONVERT(date,GETDATE()) AND l.StockActual>0
                AND CONVERT(date,l.FechaVencimiento)<>@SinVencimiento
            ORDER BY l.FechaVencimiento,p.Nombre,l.Id;",conexion);
        cmd.Parameters.Add("@SinVencimiento",SqlDbType.Date).Value=FechaSinVencimientoLegada;
        using var lector=await cmd.ExecuteReaderAsync();
        var resultado=new List<LoteVencidoProducto>();
        while(await lector.ReadAsync()) resultado.Add(new(lector.GetString(0),lector.GetInt32(1),lector.GetDateTime(2),
            lector.GetDecimal(3),lector.GetString(4),lector.GetBoolean(5)));
        return resultado;
    }

    public static string? ErrorNombreFicha(string? nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return "El nombre es obligatorio.";
        if (nombre.Length > 100) return "El nombre admite hasta 100 caracteres.";
        if (nombre.Any(char.IsControl)) return "El nombre no puede contener saltos de línea ni caracteres de control.";
        if (!nombre.Any(char.IsLetter)) return "El nombre debe incluir letras; no puede contener solo números o símbolos.";
        return null;
    }

    public static string? ErrorCodigoBarrasFicha(string? codigo)
    {
        // La aplicacion busca un codigo de texto; no exige un largo EAN/UPC ni un checksum.
        // Conservar compatibilidad con codigos alfanumericos y espacios existentes.
        if (string.IsNullOrWhiteSpace(codigo)) return null;
        if (codigo.Length > 50) return "El código de barras admite hasta 50 caracteres.";
        if (codigo.Any(c => c < ' ' || c > '~'))
            return "El código de barras admite letras sin acentos, números, espacios y símbolos imprimibles; no saltos de línea ni caracteres de control.";
        return null;
    }

    public static void ValidarFicha(ProductoEdicion ficha)
    {
        if (ErrorNombreFicha(ficha.Nombre) is string errorNombre) throw new InvalidOperationException(errorNombre);
        if (ficha.CategoriaId <= 0) throw new InvalidOperationException("La categoría es obligatoria.");
        if (string.IsNullOrWhiteSpace(ficha.Descripcion)) throw new InvalidOperationException("La descripción es obligatoria.");
        if (ErrorCodigoBarrasFicha(ficha.CodigoBarras) is string errorCodigo) throw new InvalidOperationException(errorCodigo);
        if (!ficha.SeVendePorPeso.HasValue) throw new InvalidOperationException("Seleccione el tipo de venta: por unidad o por peso.");
        if (!ficha.ControlaVencimiento.HasValue) throw new InvalidOperationException("Indique si el producto requiere vencimiento: Sí o No.");
        if (ficha.Descripcion.Length > 255)
            throw new InvalidOperationException("La descripción admite hasta 255 caracteres.");
        if (ficha.PrecioVenta <= 0 || ficha.PrecioVenta > 99999999.99M || decimal.Round(ficha.PrecioVenta,2)!=ficha.PrecioVenta)
            throw new InvalidOperationException("Indique un precio positivo con hasta dos decimales.");
        if (ficha.StockMinimo < 0 || ficha.StockMinimo > 9999999.999M || decimal.Round(ficha.StockMinimo,3)!=ficha.StockMinimo
            || (!ficha.SeVendePorPeso.Value && decimal.Truncate(ficha.StockMinimo)!=ficha.StockMinimo))
            throw new InvalidOperationException("El mínimo debe ser positivo o cero: entero para unidades, hasta tres decimales para peso.");
    }

    private static void ParametrosFicha(SqlCommand cmd, ProductoEdicion ficha)
    {
        cmd.Parameters.Add("@Nombre", SqlDbType.VarChar,100).Value=ficha.Nombre.Trim();
        cmd.Parameters.Add("@Categoria",SqlDbType.Int).Value=ficha.CategoriaId;
        var precio=cmd.Parameters.Add("@Precio",SqlDbType.Decimal); precio.Precision=10; precio.Scale=2; precio.Value=ficha.PrecioVenta;
        var minimo=cmd.Parameters.Add("@Minimo",SqlDbType.Decimal); minimo.Precision=10; minimo.Scale=3; minimo.Value=ficha.StockMinimo;
        cmd.Parameters.Add("@Peso",SqlDbType.Bit).Value=ficha.SeVendePorPeso!.Value;
        cmd.Parameters.Add("@ControlaVencimiento",SqlDbType.Bit).Value=ficha.ControlaVencimiento!.Value;
        cmd.Parameters.Add("@Descripcion",SqlDbType.VarChar,255).Value=string.IsNullOrWhiteSpace(ficha.Descripcion) ? DBNull.Value : ficha.Descripcion.Trim();
        cmd.Parameters.Add("@Codigo",SqlDbType.VarChar,50).Value=string.IsNullOrWhiteSpace(ficha.CodigoBarras) ? DBNull.Value : ficha.CodigoBarras.Trim();
    }

    public async Task<int> GuardarFichaAsync(ProductoEdicion ficha, ProductoFicha? original)
    {
        ValidarFicha(ficha);
        if (original != null && (!original.Activo || original.SeVendePorPeso != ficha.SeVendePorPeso))
            throw new InvalidOperationException("No se puede editar un producto inactivo ni cambiar su tipo de venta histórico.");
        using var conexion=await AbrirFichasPruebasAsync(escritura:true);
        using var transaccion=conexion.BeginTransaction();
        using var cmd=new SqlCommand("",conexion,transaccion);
        ParametrosFicha(cmd,ficha);
        // No hay un SP seguro para ficha: el existente modifica stock y confunde descripcion/codigo.
        // La nueva alta crea SOLO el catalogo, con saldo cero; nunca un lote o ingreso de mercaderia.
        if(original==null)
        {
            cmd.CommandText=@"IF EXISTS(SELECT 1 FROM dbo.Producto WITH (UPDLOCK,HOLDLOCK)
                    WHERE Nombre=@Nombre AND CategoriaId=@Categoria)
                    THROW 52200,'Ya existe un producto con ese nombre y categoría. Revise la lista antes de crear otro.',1;
                INSERT INTO dbo.Producto
                (Nombre,CategoriaId,PrecioVenta,StockMinimo,SeVendePorPeso,Descripcion,CodigoBarras,ControlaVencimiento,Activo,StockActual)
                OUTPUT INSERTED.Id VALUES (@Nombre,@Categoria,@Precio,@Minimo,@Peso,@Descripcion,@Codigo,@ControlaVencimiento,1,0);";
        }
        else
        {
            // Comparacion optimista de la ficha original. No pisa ediciones de otra ventana.
            cmd.Parameters.Add("@Id",SqlDbType.Int).Value=original.Id;
            cmd.Parameters.Add("@NombreAnterior",SqlDbType.VarChar,100).Value=original.Nombre;
            cmd.Parameters.Add("@CategoriaAnterior",SqlDbType.Int).Value=original.CategoriaId;
            var precio=cmd.Parameters.Add("@PrecioAnterior",SqlDbType.Decimal); precio.Precision=10;precio.Scale=2;precio.Value=original.PrecioVenta;
            var minimo=cmd.Parameters.Add("@MinimoAnterior",SqlDbType.Decimal); minimo.Precision=10;minimo.Scale=3;minimo.Value=original.StockMinimo;
            cmd.Parameters.Add("@DescripcionAnterior",SqlDbType.VarChar,255).Value=original.Descripcion;
            cmd.Parameters.Add("@CodigoAnterior",SqlDbType.VarChar,50).Value=original.CodigoBarras;
            cmd.Parameters.Add("@CodigoCambiado",SqlDbType.Bit).Value=
                !string.Equals(ficha.CodigoBarras,original.CodigoBarras,StringComparison.Ordinal);
            cmd.Parameters.Add("@ControlAnterior",SqlDbType.Bit).Value=original.ControlaVencimiento;
            cmd.CommandText=@"UPDATE dbo.Producto SET Nombre=@Nombre,CategoriaId=@Categoria,PrecioVenta=@Precio,
                StockMinimo=@Minimo,Descripcion=@Descripcion,
                CodigoBarras=CASE WHEN @CodigoCambiado=1 THEN @Codigo ELSE CodigoBarras END,
                ControlaVencimiento=@ControlaVencimiento
                OUTPUT INSERTED.Id
                WHERE Id=@Id AND Activo=1 AND SeVendePorPeso=@Peso AND Nombre=@NombreAnterior
                  AND CategoriaId=@CategoriaAnterior AND PrecioVenta=@PrecioAnterior AND StockMinimo=@MinimoAnterior
                  AND ISNULL(Descripcion,'')=@DescripcionAnterior AND ISNULL(CodigoBarras,'')=@CodigoAnterior
                  AND ControlaVencimiento=@ControlAnterior;";
        }
        object? resultado=await cmd.ExecuteScalarAsync();
        if(resultado is not int id) throw new InvalidOperationException("El producto cambió o fue dado de baja. Actualice la lista antes de editarlo.");
        await transaccion.CommitAsync();
        return id;
    }

    public async Task DarBajaFichaAsync(int id)
    {
        using var conexion=await AbrirFichasPruebasAsync(escritura:true);
        // Baja logica, conservando todo el historial. No dejar mercaderia pendiente sin producto activo.
        using var cmd=new SqlCommand(@"UPDATE p SET Activo=0 FROM dbo.Producto p
            WHERE p.Id=@Id AND p.Activo=1 AND p.StockActual=0
              AND NOT EXISTS(SELECT 1 FROM dbo.Lote WHERE ProductoId=p.Id AND StockActual>0)
              AND NOT EXISTS(SELECT 1 FROM dbo.DepositoStock WHERE ProductoId=p.Id AND StockActual>0)
              AND NOT EXISTS(SELECT 1 FROM dbo.DetallePedidoProveedor d JOIN dbo.PedidoProveedor pedido ON pedido.Id=d.PedidoId
                             WHERE d.ProductoId=p.Id AND pedido.Estado=N'Pendiente')
              AND NOT EXISTS(SELECT 1 FROM dbo.RemitoDetalle d JOIN dbo.RemitoInterno r ON r.Id=d.RemitoId
                             WHERE d.ProductoId=p.Id AND r.Estado IN(N'En tránsito',N'Con diferencias'));",conexion);
        cmd.Parameters.Add("@Id",SqlDbType.Int).Value=id;
        if(await cmd.ExecuteNonQueryAsync()!=1)
            throw new InvalidOperationException("No se pudo dar de baja: el producto ya está inactivo o conserva mercadería, pedidos o remitos pendientes.");
    }
}
