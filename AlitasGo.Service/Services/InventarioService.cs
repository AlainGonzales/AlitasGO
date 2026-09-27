using AlitasGo.Domain.Entities;
using AlitasGo.Domain.Interfaces;

namespace AlitasGo.Service.Services
{
    /// <summary>
    /// Reglas de negocio relacionadas con disponibilidad de insumos y Kardex.
    /// </summary>
    public class InventarioService : IInventarioService
    {
        private readonly IInsumoRepository _insumoRepository;
        private readonly IPedidoRepository _pedidoRepository;

        public InventarioService(
            IInsumoRepository insumoRepository,
            IPedidoRepository pedidoRepository)
        {
            _insumoRepository = insumoRepository ?? throw new ArgumentNullException(nameof(insumoRepository));
            _pedidoRepository = pedidoRepository ?? throw new ArgumentNullException(nameof(pedidoRepository));
        }

        /// <summary>
        /// Comprueba que todos los insumos de la receta tengan stock suficiente
        /// para preparar la cantidad solicitada de un producto.
        /// </summary>
        public async Task<bool> ValidarDisponibilidadPorProductoAsync(int productoId, int cantidadPedida)
        {
            if (productoId <= 0)
                throw new ArgumentOutOfRangeException(nameof(productoId), "El producto debe ser válido.");

            if (cantidadPedida <= 0)
                throw new ArgumentOutOfRangeException(nameof(cantidadPedida), "La cantidad debe ser mayor que cero.");

            var insumos = (await _insumoRepository.ObtenerTodosAsync()).ToList();

            // LINQ: toma únicamente las recetas del producto y acumula el requerimiento
            // por insumo. Esto también soporta más de una línea de receta del mismo insumo.
            var requerimientos = insumos
                .SelectMany(insumo => insumo.Recetas
                    .Where(receta => receta.ProductoId == productoId)
                    .Select(receta => new
                    {
                        Insumo = insumo,
                        CantidadRequerida = receta.CantidadRequerida * cantidadPedida
                    }))
                .GroupBy(x => x.Insumo.InsumoId)
                .Select(grupo => new
                {
                    Insumo = grupo.First().Insumo,
                    CantidadRequerida = grupo.Sum(x => x.CantidadRequerida)
                })
                .ToList();

            // Un producto sin receta registrada no puede confirmarse porque no se puede
            // garantizar su disponibilidad real en cocina.
            if (requerimientos.Count == 0)
                return false;

            return requerimientos.All(x => x.Insumo.StockActual >= x.CantidadRequerida);
        }

        /// <summary>
        /// Descuenta del inventario los insumos utilizados por un pedido y registra
        /// cada salida en el Kardex. Primero valida TODO el pedido para evitar
        /// descuentos parciales si un insumo resulta insuficiente.
        /// </summary>
        public async Task DescontarInsumosPorPedidoAsync(int pedidoId)
        {
            if (pedidoId <= 0)
                throw new ArgumentOutOfRangeException(nameof(pedidoId), "El pedido debe ser válido.");

            var pedido = await _pedidoRepository.ObtenerPorIdAsync(pedidoId)
                ?? throw new InvalidOperationException($"No se encontró el pedido {pedidoId}.");

            if (pedido.Detalles == null || pedido.Detalles.Count == 0)
                throw new InvalidOperationException("El pedido no contiene productos para descontar.");

            var insumos = (await _insumoRepository.ObtenerTodosAsync()).ToList();

            // Consolida el consumo total por insumo para todo el pedido.
            var consumos = insumos
                .Select(insumo => new
                {
                    Insumo = insumo,
                    Cantidad = insumo.Recetas.Sum(receta =>
                        pedido.Detalles
                            .Where(detalle => detalle.ProductoId == receta.ProductoId)
                            .Sum(detalle => receta.CantidadRequerida * detalle.Cantidad))
                })
                .Where(x => x.Cantidad > 0)
                .ToList();

            if (consumos.Count == 0)
                throw new InvalidOperationException("No existen recetas configuradas para los productos del pedido.");

            var faltantes = consumos
                .Where(x => x.Insumo.StockActual < x.Cantidad)
                .Select(x => $"{x.Insumo.Nombre}: requiere {x.Cantidad} {x.Insumo.UnidadMedida} y hay {x.Insumo.StockActual}")
                .ToList();

            if (faltantes.Count > 0)
            {
                throw new InvalidOperationException(
                    "Stock insuficiente. " + string.Join("; ", faltantes));
            }

            foreach (var consumo in consumos)
            {
                var stockPrevio = consumo.Insumo.StockActual;
                var stockPosterior = stockPrevio - consumo.Cantidad;

                await _insumoRepository.DescontarStockAsync(
                    consumo.Insumo.InsumoId,
                    consumo.Cantidad);

                await _insumoRepository.RegistrarMovimientoKardexAsync(new MovimientoKardex
                {
                    InsumoId = consumo.Insumo.InsumoId,
                    PedidoId = pedido.PedidoId,
                    FechaHora = DateTime.UtcNow,
                    TipoMovimiento = "SalidaPorVenta",
                    Cantidad = consumo.Cantidad,
                    StockPrevio = stockPrevio,
                    StockPosterior = stockPosterior,
                    Motivo = $"Consumo de insumo por pedido {pedido.CodigoPedido}"
                });
            }

            await _insumoRepository.GuardarCambiosAsync();
        }

        public async Task<bool> VerificarAlertaQuiebreStockAsync(int insumoId)
        {
            if (insumoId <= 0)
                throw new ArgumentOutOfRangeException(nameof(insumoId), "El insumo debe ser válido.");

            var insumo = await _insumoRepository.ObtenerPorIdAsync(insumoId)
                ?? throw new InvalidOperationException($"No se encontró el insumo {insumoId}.");

            return insumo.StockActual <= insumo.StockMinimo;
        }
    }
}
