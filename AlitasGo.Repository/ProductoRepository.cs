using AlitasGo.Domain.Entities;
using AlitasGo.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlitasGo.Repository
{
    public class ProductoRepository : IProductoRepository
    {
        private readonly AlitasGoDbContext _context;

        public ProductoRepository(AlitasGoDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Producto?> ObtenerPorIdAsync(int productoId)
        {
            return await _context.Productos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductoId == productoId);
        }
    }
}