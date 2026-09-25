namespace AlitasGo.Domain.DTOs
{
    public class PedidoResumenDto
    {
        public int PedidoId { get; set; }
        public string CodigoPedido { get; set; } = string.Empty;
        public int NumeroMesa { get; set; }
        public string Estado { get; set; } = string.Empty; // "Pendiente", "EnCocina", "Listo"
        public DateTime FechaHoraRegistro { get; set; }
        public decimal TotalPagar { get; set; }
        public List<DetalleResumenDto> Items { get; set; } = new List<DetalleResumenDto>();
    }

    public class DetalleResumenDto
    {
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public string? SaborSalsa { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; set; }
        public string? NotasCocina { get; set; }
    }
}