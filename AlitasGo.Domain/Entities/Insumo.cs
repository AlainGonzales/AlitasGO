namespace AlitasGo.Domain.Entities
{
    public class Insumo
    {
        public int InsumoId { get; set; }
        public string CodigoInsumo { get; set; } = string.Empty; // Ej. "INS-ALITA", "INS-SALSA-BBQ"
        public string Nombre { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = "Unidades"; // Unidades, Gramos, Mililitros
        public decimal StockActual { get; set; } = 0.00m;
        public decimal StockMinimo { get; set; } = 10.00m;
        public decimal CostoUnitario { get; set; } = 0.0000m;

        public bool EsStockCritico => StockActual <= StockMinimo;

        // Navegación
        public ICollection<RecetaProducto> Recetas { get; set; } = new List<RecetaProducto>();
        public ICollection<MovimientoKardex> MovimientosKardex { get; set; } = new List<MovimientoKardex>();
    }
}