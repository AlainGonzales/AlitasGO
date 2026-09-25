namespace AlitasGo.Domain.Entities
{
    public class Categoria
    {
        public int CategoriaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;

        // Relación 1:N -> Una categoría agrupa múltiples productos
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}