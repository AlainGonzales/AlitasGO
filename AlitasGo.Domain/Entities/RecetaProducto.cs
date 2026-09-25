namespace AlitasGo.Domain.Entities
{
    public class RecetaProducto
    {
        public int RecetaId { get; set; }
        public int ProductoId { get; set; }
        public int InsumoId { get; set; }
        public decimal CantidadRequerida { get; set; } // Cantidad que se descuenta por cada unidad de producto

        // Navegación
        public Producto Producto { get; set; } = null!;
        public Insumo Insumo { get; set; } = null!;
    }
}