namespace AlitasGo.Domain.DTOs
{
    public class CrearPedidoDto
    {
        public int MesaId { get; set; }
        public string UsuarioId { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public List<CrearDetallePedidoDto> Detalles { get; set; } = new List<CrearDetallePedidoDto>();
    }

    public class CrearDetallePedidoDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public string? SaborSalsa { get; set; } // "BBQ", "Acevichada", "Buffalo", etc.
        public string? NotasCocina { get; set; } // "Bien doradas", "salsa aparte"
    }
}