namespace AlitasGo.Domain.Entities
{
    public class Producto
    {
        public int ProductoId { get; set; }
        public int CategoriaId { get; set; }
        public string Codigo { get; set; } = string.Empty; // Ej. "ALT-06", "ALT-12"
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal PrecioUnitario { get; set; }
        public bool RequiereSabor { get; set; } = false; // true si lleva salsa (BBQ, Acevichada, etc.)
        public bool Activo { get; set; } = true;

        // Navegación
        public Categoria Categoria { get; set; } = null!;
        public ICollection<DetallePedido> DetallesPedido { get; set; } = new List<DetallePedido>();

        // Relación N:M hacia Insumos mediante RecetaProducto
        public ICollection<RecetaProducto> Recetas { get; set; } = new List<RecetaProducto>();
    }
}