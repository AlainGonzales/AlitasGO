namespace AlitasGo.Domain.Entities
{
    public class Pedido
    {
        public int PedidoId { get; set; }
        public string CodigoPedido { get; set; } = string.Empty; // Ej. "PED-20260925-001"
        public int MesaId { get; set; }
        public string UsuarioId { get; set; } = string.Empty; // ID del cajero o mozo
        public DateTime FechaHoraRegistro { get; set; } = DateTime.UtcNow;
        public string Estado { get; set; } = "Pendiente"; // Pendiente, EnCocina, Listo, Servido, Pagado, Anulado
        public decimal SubTotal { get; set; } = 0.00m;
        public decimal Igv { get; set; } = 0.00m;
        public decimal TotalPagar { get; set; } = 0.00m;
        public string? Observaciones { get; set; }

        // Navegación
        public Mesa Mesa { get; set; } = null!;
        public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();

        // Relación 1:1 con el comprobante de pago
        public ComprobantePago? ComprobantePago { get; set; }

        // Método de dominio para recalcular totales
        public void CalcularTotales()
        {
            TotalPagar = Detalles.Sum(d => d.Importe);
            SubTotal = Math.Round(TotalPagar / 1.18m, 2);
            Igv = TotalPagar - SubTotal;
        }
    }
}