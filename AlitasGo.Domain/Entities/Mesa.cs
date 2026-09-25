namespace AlitasGo.Domain.Entities
{
    public class Mesa
    {
        public int MesaId { get; set; }
        public int NumeroMesa { get; set; }
        public int Capacidad { get; set; }
        public string? Ubicacion { get; set; } // "Salón Principal", "Terraza", "Bar"
        public string Estado { get; set; } = "Disponible"; // Disponible, Ocupada, En Limpieza
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        // Relación 1:N -> Una mesa tiene muchos pedidos históricos
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}