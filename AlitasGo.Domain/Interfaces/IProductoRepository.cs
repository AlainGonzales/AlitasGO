using AlitasGo.Domain.Entities;

namespace AlitasGo.Domain.Interfaces
{
    public interface IProductoRepository
    {
        Task<Producto?> ObtenerPorIdAsync(int productoId);
        Task<IEnumerable<Producto>> ObtenerPorIdsAsync(IEnumerable<int> productoIds);
    }
}
