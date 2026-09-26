using AlitasGo.Domain.Entities;
using AlitasGo.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlitasGo.Repository
{
    public class InsumoRepository : IInsumoRepository
    {
        private readonly AlitasGoDbContext _context;

        public InsumoRepository(AlitasGoDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene un insumo por su ID.
        /// </summary>
        public async Task<Insumo?> ObtenerPorIdAsync(int insumoId)
        {
            return await _context.Insumos
                .FindAsync(insumoId);
        }

        /// <summary>
        /// Retorna todos los insumos registrados en el sistema.
        /// </summary>
        public async Task<IEnumerable<Insumo>> ObtenerTodosAsync()
        {
            return await _context.Insumos
                .OrderBy(i => i.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Retorna únicamente los insumos cuyo stock actual es igual o menor al stock mínimo.
        /// Útil para mostrar alertas en el panel de inventario.
        /// </summary>
        public async Task<IEnumerable<Insumo>> ObtenerInsumosCriticosAsync()
        {
            return await _context.Insumos
                .Where(i => i.StockActual <= i.StockMinimo)
                .OrderBy(i => i.StockActual)
                .ToListAsync();
        }

        /// <summary>
        /// Verifica si hay stock suficiente sin modificar nada en la base de datos.
        /// </summary>
        public async Task<bool> HayStockSuficienteAsync(int insumoId, decimal cantidadRequerida)
        {
            var insumo = await _context.Insumos
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.InsumoId == insumoId);

            return insumo is not null && insumo.StockActual >= cantidadRequerida;
        }

        /// <summary>
        /// Registra un movimiento en el Kardex (entrada, salida, ajuste o merma).
        /// El StockPrevio y StockPosterior deben calcularse antes de llamar a este método.
        /// </summary>
        public async Task RegistrarMovimientoKardexAsync(MovimientoKardex movimiento)
        {
            await _context.MovimientosKardex.AddAsync(movimiento);
        }

        /// <summary>
        /// Descuenta la cantidad indicada del StockActual del insumo.
        /// Lanza excepción si no hay stock suficiente para proteger la integridad.
        /// </summary>
        public async Task DescontarStockAsync(int insumoId, decimal cantidad)
        {
            var insumo = await _context.Insumos.FindAsync(insumoId);
            if (insumo is null)
                throw new KeyNotFoundException($"No se encontró el insumo con Id {insumoId}.");

            if (insumo.StockActual < cantidad)
                throw new InvalidOperationException(
                    $"Stock insuficiente para '{insumo.Nombre}'. " +
                    $"Disponible: {insumo.StockActual} {insumo.UnidadMedida}, " +
                    $"Requerido: {cantidad} {insumo.UnidadMedida}.");

            insumo.StockActual -= cantidad;
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
