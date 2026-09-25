namespace AlitasGo.Domain.Entities
{
    public class DetallePedido
    {
        public int DetallePedidoId { get; set; }
        public int PedidoId { get; set; }
        public int ProductoId { get; set; }
        public string? SaborSalsa { get; set; } // BBQ, Acevichada, Buffalo, etc.
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; set; }
        public string? NotasCocina { get; set; } // "Bien doradas", "salsa aparte"

        // Navegación
        public Pedido Pedido { get; set; } = null!;
        public Producto Producto { get; set; } = null!;
    }
}