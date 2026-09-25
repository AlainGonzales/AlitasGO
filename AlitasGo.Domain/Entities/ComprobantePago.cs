namespace AlitasGo.Domain.Entities
{
    public class ComprobantePago
    {
        public int ComprobanteId { get; set; }
        public int PedidoId { get; set; }
        public string TipoComprobante { get; set; } = "Boleta"; // Boleta, Factura, Ticket
        public string Serie { get; set; } = "B001";
        public int NumeroCorrelativo { get; set; }
        public string MetodoPago { get; set; } = "Efectivo"; // Efectivo, Yape, Plin, Tarjeta
        public decimal MontoRecibido { get; set; }
        public decimal Vuelto { get; set; } = 0.00m;
        public DateTime FechaEmision { get; set; } = DateTime.UtcNow;

        // Navegación 1:1
        public Pedido Pedido { get; set; } = null!;
    }
}