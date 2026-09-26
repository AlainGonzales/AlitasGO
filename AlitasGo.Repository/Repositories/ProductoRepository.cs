using AlitasGo.Domain.Entities;
using AlitasGo.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlitasGo.Repository.Repositories
{
    public class ProductoRepository : IProductoRepository
    {
        private readonly AlitasGoDbContext _context;

        public ProductoRepository(AlitasGoDbContext context)
        {
            _context = context;
        }

        public Task<Producto?> ObtenerPorIdAsync(int productoId)
        {
            return _context.Productos.FirstOrDefaultAsync(p => p.ProductoId == productoId);
        }

        public async Task<IEnumerable<Producto>> ObtenerPorIdsAsync(IEnumerable<int> productoIds)
        {
            return await _context.Productos
                .Where(p => productoIds.Contains(p.ProductoId))
                .ToListAsync();
        }
    }
}
