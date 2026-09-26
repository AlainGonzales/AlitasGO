using AlitasGo.Domain.Entities;
using AlitasGo.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlitasGo.Repository
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly AlitasGoDbContext _context;

        public PedidoRepository(AlitasGoDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene un pedido por su ID, incluyendo detalles, productos y mesa.
        /// </summary>
        public async Task<Pedido?> ObtenerPorIdAsync(int pedidoId)
        {
            return await _context.Pedidos
                .Include(p => p.Mesa)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .Include(p => p.ComprobantePago)
                .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);
        }

        /// <summary>
        /// Obtiene un pedido por su código único (ej. "PED-20260925-001").
        /// </summary>
        public async Task<Pedido?> ObtenerPorCodigoAsync(string codigoPedido)
        {
            return await _context.Pedidos
                .Include(p => p.Mesa)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.CodigoPedido == codigoPedido);
        }

        /// <summary>
        /// Retorna todos los pedidos con estado activo (Pendiente, EnCocina, Listo),
        /// ordenados del más reciente al más antiguo.
        /// </summary>
        public async Task<IEnumerable<Pedido>> ObtenerPedidosActivosAsync()
        {
            return await _context.Pedidos
                .Include(p => p.Mesa)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.Estado == "Pendiente" ||
                            p.Estado == "EnCocina" ||
                            p.Estado == "Listo")
                .OrderByDescending(p => p.FechaHoraRegistro)
                .ToListAsync();
        }

        /// <summary>
        /// Retorna el historial de pedidos de una mesa específica.
        /// </summary>
        public async Task<IEnumerable<Pedido>> ObtenerPorMesaAsync(int mesaId)
        {
            return await _context.Pedidos
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.MesaId == mesaId)
                .OrderByDescending(p => p.FechaHoraRegistro)
                .ToListAsync();
        }

        /// <summary>
        /// Persiste un nuevo pedido junto con todos sus detalles.
        /// </summary>
        public async Task CrearPedidoAsync(Pedido pedido)
        {
            await _context.Pedidos.AddAsync(pedido);
        }

        /// <summary>
        /// Actualiza únicamente el campo Estado de un pedido sin cargar la entidad completa.
        /// </summary>
        public async Task ActualizarEstadoAsync(int pedidoId, string nuevoEstado)
        {
            var pedido = await _context.Pedidos.FindAsync(pedidoId);
            if (pedido is null)
                throw new KeyNotFoundException($"No se encontró el pedido con Id {pedidoId}.");

            pedido.Estado = nuevoEstado;
        }

        /// <summary>
        /// Persiste todos los cambios pendientes en la unidad de trabajo.
        /// </summary>
        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
