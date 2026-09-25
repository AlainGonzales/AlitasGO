namespace AlitasGo.Domain.Entities
{
    public class MovimientoKardex
    {
        public int MovimientoId { get; set; }
        public int InsumoId { get; set; }
        public int? PedidoId { get; set; } // Opcional, si la deducción provino de una comanda
        public DateTime FechaHora { get; set; } = DateTime.UtcNow;
        public string TipoMovimiento { get; set; } = "SalidaPorVenta"; // SalidaPorVenta, EntradaPorCompra, Ajuste, Merma
        public decimal Cantidad { get; set; }
        public decimal StockPrevio { get; set; }
        public decimal StockPosterior { get; set; }
        public string? Motivo { get; set; }

        // Navegación
        public Insumo Insumo { get; set; } = null!;
        public Pedido? Pedido { get; set; }
    }
}