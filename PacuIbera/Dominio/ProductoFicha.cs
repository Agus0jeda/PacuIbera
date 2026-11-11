namespace PacuIbera.Dominio;

public sealed record CategoriaProducto(int Id, string Nombre);

public sealed record ProductoFicha(int Id, string Nombre, int CategoriaId, string Categoria,
    decimal PrecioVenta, decimal StockActual, decimal StockMinimo, bool SeVendePorPeso,
    bool Activo, string Descripcion, string CodigoBarras, DateTime? ProximoVencimiento,
    int LotesPorVencer, int LotesVencidos)
{
    // Saldo de Producto y saldos por lote se conservan separados: ninguna propiedad corrige datos.
    public decimal StockUtilizable { get; init; }
    public decimal StockActualLotes { get; init; }
    public decimal StockLotesActivos { get; init; }
    public bool ControlaVencimiento { get; init; }
    public bool SinVencimientoLocal { get; init; }
    public string RequiereVencimiento => ControlaVencimiento ? "Sí" : "No";
    public bool SinStock => Activo && StockUtilizable <= 0;
    public bool StockBajo => Activo && StockUtilizable > 0 && StockMinimo > 0 && StockUtilizable < StockMinimo;
    public string TipoVenta => SeVendePorPeso ? "Por peso" : "Por unidad";
    public string Estado => Activo ? "Activo" : "Inactivo";
    public string Alerta => !Activo ? ""
        : SinStock ? "Sin stock"
        : StockBajo ? "Stock bajo"
        : LotesPorVencer > 0 ? "Próximo a vencer" : "Sin alerta";
}

public sealed record ProductoEdicion(string Nombre, int CategoriaId, decimal PrecioVenta,
    decimal StockMinimo, bool? SeVendePorPeso, string Descripcion, string CodigoBarras, bool? ControlaVencimiento = null);

public sealed record LoteVencidoProducto(string Producto, int NumeroLote, DateTime FechaVencimiento,
    decimal CantidadRegistrada, string Estado, bool SeVendePorPeso);
