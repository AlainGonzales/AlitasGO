using AlitasGo.Domain.Entities;
using AlitasGo.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlitasGo.Repository.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly AlitasGoDbContext _context;

        public PedidoRepository(AlitasGoDbContext context)
        {
            _context = context;
        }

        public Task<Pedido?> ObtenerPorIdAsync(int pedidoId)
        {
            return _context.Pedidos
                .Include(p => p.Mesa)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);
        }

        public Task<Pedido?> ObtenerPorCodigoAsync(string codigoPedido)
        {
            return _context.Pedidos
                .Include(p => p.Mesa)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.CodigoPedido == codigoPedido);
        }

        public async Task<IEnumerable<Pedido>> ObtenerPedidosActivosAsync()
        {
            return await _context.Pedidos
                .Include(p => p.Mesa)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.Estado != "Pagado" && p.Estado != "Anulado")
                .OrderBy(p => p.FechaHoraRegistro)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pedido>> ObtenerPorMesaAsync(int mesaId)
        {
            return await _context.Pedidos
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.MesaId == mesaId)
                .OrderByDescending(p => p.FechaHoraRegistro)
                .ToListAsync();
        }

        public async Task CrearPedidoAsync(Pedido pedido)
        {
            await _context.Pedidos.AddAsync(pedido);
        }

        public async Task ActualizarEstadoAsync(int pedidoId, string nuevoEstado)
        {
            var pedido = await _context.Pedidos.FirstOrDefaultAsync(p => p.PedidoId == pedidoId);
            if (pedido != null)
            {
                pedido.Estado = nuevoEstado;
            }
        }

        public Task GuardarCambiosAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
