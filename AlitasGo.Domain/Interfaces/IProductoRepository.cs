using AlitasGo.Domain.Entities;

namespace AlitasGo.Domain.Interfaces
{
    /// <summary>
    /// Contrato mínimo de lectura de productos requerido por la lógica de pedidos.
    /// La implementación de persistencia puede resolverse con EF Core en la capa Repository.
    /// </summary>
    public interface IProductoRepository
    {
        Task<Producto?> ObtenerPorIdAsync(int productoId);
    }
}
