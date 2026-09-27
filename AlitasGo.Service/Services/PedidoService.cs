using AlitasGo.Domain.DTOs;
using AlitasGo.Domain.Entities;
using AlitasGo.Domain.Interfaces;

namespace AlitasGo.Service.Services
{
    /// <summary>
    /// Cerebro de las reglas de negocio de pedidos: valida stock, calcula importes,
    /// registra la comanda y ordena el descuento de inventario.
    /// </summary>
    public class PedidoService : IPedidoService
    {
        private const decimal TasaIgv = 0.18m;

        private readonly IPedidoRepository _pedidoRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IInventarioService _inventarioService;

        public PedidoService(
            IPedidoRepository pedidoRepository,
            IProductoRepository productoRepository,
            IInventarioService inventarioService)
        {
            _pedidoRepository = pedidoRepository ?? throw new ArgumentNullException(nameof(pedidoRepository));
            _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
            _inventarioService = inventarioService ?? throw new ArgumentNullException(nameof(inventarioService));
        }

        public async Task<PedidoResumenDto> IniciarYRegistrarPedidoAsync(CrearPedidoDto dto)
        {
            ValidarDatosPedido(dto);

            // Consolidamos por ProductoId para validar el stock total solicitado incluso
            // si el mismo producto aparece en más de una línea del pedido.
            var cantidadesPorProducto = dto.Detalles
                .GroupBy(d => d.ProductoId)
                .Select(g => new
                {
                    ProductoId = g.Key,
                    Cantidad = g.Sum(x => x.Cantidad)
                })
                .ToList();

            foreach (var item in cantidadesPorProducto)
            {
                var disponible = await _inventarioService.ValidarDisponibilidadPorProductoAsync(
                    item.ProductoId,
                    item.Cantidad);

                if (!disponible)
                {
                    throw new InvalidOperationException(
                        $"No existe stock suficiente para confirmar el producto con ID {item.ProductoId}.");
                }
            }

            // Recupera los productos una sola vez por ID y evita consultas repetidas.
            var productos = new Dictionary<int, Producto>();
            foreach (var productoId in cantidadesPorProducto.Select(x => x.ProductoId))
            {
                var producto = await _productoRepository.ObtenerPorIdAsync(productoId)
                    ?? throw new InvalidOperationException($"No existe el producto con ID {productoId}.");

                if (!producto.Activo)
                    throw new InvalidOperationException($"El producto '{producto.Nombre}' no se encuentra activo.");

                productos[productoId] = producto;
            }

            var pedido = new Pedido
            {
                CodigoPedido = GenerarCodigoPedido(),
                MesaId = dto.MesaId,
                UsuarioId = dto.UsuarioId.Trim(),
                FechaHoraRegistro = DateTime.UtcNow,
                Estado = "EnCocina",
                Observaciones = dto.Observaciones?.Trim(),
                Detalles = dto.Detalles.Select(detalle =>
                {
                    var producto = productos[detalle.ProductoId];
                    var importe = Math.Round(producto.PrecioUnitario * detalle.Cantidad, 2);

                    return new DetallePedido
                    {
                        ProductoId = detalle.ProductoId,
                        Cantidad = detalle.Cantidad,
                        SaborSalsa = detalle.SaborSalsa?.Trim(),
                        NotasCocina = detalle.NotasCocina?.Trim(),
                        PrecioUnitario = producto.PrecioUnitario,
                        Importe = importe,
                        Producto = producto
                    };
                }).ToList()
            };

            // Requisito del Nivel 4: subtotal + IGV del 18 % + total.
            pedido.SubTotal = Math.Round(pedido.Detalles.Sum(d => d.Importe), 2);
            pedido.Igv = Math.Round(pedido.SubTotal * TasaIgv, 2);
            pedido.TotalPagar = pedido.SubTotal + pedido.Igv;

            await _pedidoRepository.CrearPedidoAsync(pedido);
            await _pedidoRepository.GuardarCambiosAsync();

            // Solo se descuenta el inventario después de que el pedido haya sido registrado.
            // InventarioService vuelve a validar el pedido completo antes de modificar stock.
            await _inventarioService.DescontarInsumosPorPedidoAsync(pedido.PedidoId);

            return ConvertirAResumen(pedido);
        }

        public async Task<bool> CambiarEstadoPedidoAsync(int pedidoId, string nuevoEstado)
        {
            if (pedidoId <= 0)
                throw new ArgumentOutOfRangeException(nameof(pedidoId));

            var estadoNormalizado = NormalizarEstado(nuevoEstado);
            var pedido = await _pedidoRepository.ObtenerPorIdAsync(pedidoId);

            if (pedido == null)
                return false;

            if (!EsTransicionValida(pedido.Estado, estadoNormalizado))
                throw new InvalidOperationException(
                    $"No se puede cambiar el pedido de '{pedido.Estado}' a '{estadoNormalizado}'.");

            await _pedidoRepository.ActualizarEstadoAsync(pedidoId, estadoNormalizado);
            await _pedidoRepository.GuardarCambiosAsync();
            return true;
        }

        public async Task<IEnumerable<PedidoResumenDto>> ObtenerComandasCocinaAsync()
        {
            var pedidos = await _pedidoRepository.ObtenerPedidosActivosAsync();

            return pedidos
                .Where(p => p.Estado is "Pendiente" or "EnCocina" or "Listo")
                .OrderBy(p => p.FechaHoraRegistro)
                .Select(ConvertirAResumen)
                .ToList();
        }

        public async Task<PedidoResumenDto?> ObtenerDetallePedidoAsync(int pedidoId)
        {
            if (pedidoId <= 0)
                throw new ArgumentOutOfRangeException(nameof(pedidoId));

            var pedido = await _pedidoRepository.ObtenerPorIdAsync(pedidoId);
            return pedido == null ? null : ConvertirAResumen(pedido);
        }

        private static void ValidarDatosPedido(CrearPedidoDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.MesaId <= 0)
                throw new ArgumentException("Debe seleccionar una mesa válida.", nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.UsuarioId))
                throw new ArgumentException("Debe indicar el usuario que registra el pedido.", nameof(dto));

            if (dto.Detalles == null || dto.Detalles.Count == 0)
                throw new ArgumentException("El pedido debe contener al menos un producto.", nameof(dto));

            if (dto.Detalles.Any(d => d.ProductoId <= 0))
                throw new ArgumentException("Todos los productos deben tener un ID válido.", nameof(dto));

            if (dto.Detalles.Any(d => d.Cantidad <= 0))
                throw new ArgumentException("Todas las cantidades deben ser mayores que cero.", nameof(dto));
        }

        private static string GenerarCodigoPedido()
        {
            // Evita depender del ID de BD antes de guardar y mantiene un código legible.
            return $"PED-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}";
        }

        private static string NormalizarEstado(string nuevoEstado)
        {
            if (string.IsNullOrWhiteSpace(nuevoEstado))
                throw new ArgumentException("El estado no puede estar vacío.", nameof(nuevoEstado));

            var estados = new[] { "Pendiente", "EnCocina", "Listo", "Servido", "Pagado", "Anulado" };

            var estado = estados.FirstOrDefault(e =>
                string.Equals(e, nuevoEstado.Trim(), StringComparison.OrdinalIgnoreCase));

            return estado ?? throw new ArgumentException(
                $"Estado no permitido: '{nuevoEstado}'.",
                nameof(nuevoEstado));
        }

        private static bool EsTransicionValida(string estadoActual, string nuevoEstado)
        {
            if (nuevoEstado == "Anulado")
                return estadoActual is "Pendiente" or "EnCocina";

            return (estadoActual, nuevoEstado) switch
            {
                ("Pendiente", "EnCocina") => true,
                ("EnCocina", "Listo") => true,
                ("Listo", "Servido") => true,
                ("Servido", "Pagado") => true,
                _ => false
            };
        }

        private static PedidoResumenDto ConvertirAResumen(Pedido pedido)
        {
            return new PedidoResumenDto
            {
                PedidoId = pedido.PedidoId,
                CodigoPedido = pedido.CodigoPedido,
                NumeroMesa = pedido.Mesa?.NumeroMesa ?? pedido.MesaId,
                Estado = pedido.Estado,
                FechaHoraRegistro = pedido.FechaHoraRegistro,
                TotalPagar = pedido.TotalPagar,
                Items = pedido.Detalles?
                    .Select(d => new DetalleResumenDto
                    {
                        NombreProducto = d.Producto?.Nombre ?? $"Producto {d.ProductoId}",
                        Cantidad = d.Cantidad,
                        SaborSalsa = d.SaborSalsa,
                        PrecioUnitario = d.PrecioUnitario,
                        Importe = d.Importe,
                        NotasCocina = d.NotasCocina
                    })
                    .ToList() ?? new List<DetalleResumenDto>()
            };
        }
    }
}
