namespace PacuIbera.Dominio
{
    public sealed record ResolucionRemitoBorrador(int RecepcionRemitoLoteId, int RemitoLoteId, int DepositoStockId,
        string Producto, bool SeVendePorPeso, decimal Enviado, decimal Recibido, decimal Resuelto,
        decimal Pendiente, decimal Cantidad, DateTime? Vencimiento)
    {
        public decimal PendientePosterior => Pendiente - Cantidad;
    }

    public sealed record LoteRecepcionRemitoBorrador(int RemitoLoteId, int RemitoDetalleId, int DepositoStockId,
        string Producto, bool SeVendePorPeso, decimal Enviado, decimal Recibido, DateTime? Vencimiento)
    {
        public decimal Faltante => Enviado - Recibido;
    }

    public sealed record ProductoDisponibleRemito(int ProductoId, string Producto, bool SeVendePorPeso, decimal StockDisponible)
    {
        public string Unidad => SeVendePorPeso ? "Por peso" : "Por unidad";
    }

    public sealed record ProductoEmisionRemito(int ProductoId, string Producto, bool SeVendePorPeso,
        decimal StockDisponible, decimal Cantidad);

    public sealed class RemitoInterno
    {
        public int Id { get; init; }
        public string Estado { get; init; } = "";
        public string Creador { get; init; } = "";
        public DateTime FechaCreacion { get; init; }
        public string Emisor { get; init; } = "";
        public DateTime? FechaEmision { get; init; }
        public string Receptor { get; init; } = "";
        public DateTime? FechaRecepcion { get; init; }
        public string Cancelador { get; init; } = "";
        public DateTime? FechaCancelacion { get; init; }
        public string? MotivoCancelacion { get; init; }
    }

    public sealed class RemitoProductoDetalle
    {
        public int Id { get; init; }
        public int ProductoId { get; init; }
        public string Producto { get; init; } = "";
        public bool SeVendePorPeso { get; init; }
        public decimal Enviado { get; init; }
        public decimal? Recibido { get; set; }
        public decimal? DiferenciaOriginal { get; set; }
        public decimal Resuelto { get; set; }
        public decimal? Pendiente { get; set; }
    }

    public sealed class RemitoLoteDetalle
    {
        public int Id { get; init; }
        public int RemitoDetalleId { get; init; }
        public int DepositoStockId { get; init; }
        public int? RecepcionRemitoLoteId { get; init; }
        public int? LoteLocalId { get; init; }
        public string Producto { get; init; } = "";
        public bool SeVendePorPeso { get; init; }
        public DateTime? FechaIngresoOrigen { get; init; }
        public DateTime? Vencimiento { get; init; }
        public decimal Enviado { get; init; }
        public decimal? Recibido { get; init; }
        public decimal? DiferenciaOriginal { get; init; }
        public decimal Resuelto { get; init; }
        public decimal? Pendiente { get; init; }
        public string? Observacion { get; init; }
    }

    public sealed class RemitoResolucionDetalle
    {
        public int ResolucionId { get; init; }
        public int RecepcionRemitoLoteId { get; init; }
        public int RemitoLoteId { get; init; }
        public string Producto { get; init; } = "";
        public bool SeVendePorPeso { get; init; }
        public string Tipo { get; init; } = "";
        public string Estado { get; init; } = "";
        public string Responsable { get; init; } = "";
        public DateTime Fecha { get; init; }
        public string Motivo { get; init; } = "";
        public decimal Cantidad { get; init; }
        public int? LoteLocalDestinoId { get; init; }
        public int? DepositoStockDestinoId { get; init; }
        public DateTime? Vencimiento { get; init; }
    }

    public sealed class DetalleRemitoInterno
    {
        public required RemitoInterno Cabecera { get; init; }
        public List<RemitoProductoDetalle> Productos { get; } = new();
        public List<RemitoLoteDetalle> Lotes { get; } = new();
        public List<RemitoResolucionDetalle> Resoluciones { get; } = new();
    }
}
