using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlitasGo.Domain.DTOs;
using AlitasGo.Domain.Entities;
using AlitasGo.Domain.Interfaces;

namespace AlitasGo.Domain.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IProductoRepository _productoRepository;

        public PedidoService(IPedidoRepository pedidoRepository, IProductoRepository productoRepository)
        {
            _pedidoRepository = pedidoRepository;
            _productoRepository = productoRepository;
        }

        public async Task<PedidoResumenDto> InitiarYRegistrarPedidoAsync(CrearPedidoDto dto)
        {
            var productos = (await _productoRepository.ObtenerPorIdsAsync(dto.Detalles.Select(d => d.ProductoId)))
                .ToDictionary(p => p.ProductoId);

            var pedido = new Pedido
            {
                MesaId = dto.MesaId,
                UsuarioId = dto.UsuarioId,
                Observaciones = dto.Observaciones,
                Estado = "Pendiente",
                FechaHoraRegistro = DateTime.UtcNow,
                Detalles = dto.Detalles.Select(d =>
                {
                    productos.TryGetValue(d.ProductoId, out var producto);
                    var precioUnitario = producto?.PrecioUnitario ?? 0m;
                    return new DetallePedido
                    {
                        ProductoId = d.ProductoId,
                        Cantidad = d.Cantidad,
                        SaborSalsa = d.SaborSalsa,
                        NotasCocina = d.NotasCocina,
                        PrecioUnitario = precioUnitario,
                        Importe = precioUnitario * d.Cantidad
                    };
                }).ToList()
            };

            pedido.CalcularTotales();

            await _pedidoRepository.CrearPedidoAsync(pedido);
            await _pedidoRepository.GuardarCambiosAsync();

            pedido.CodigoPedido = $"P{pedido.PedidoId:D6}";
            await _pedidoRepository.GuardarCambiosAsync();

            return MapearAResumen(pedido, productos);
        }

        public async Task<bool> CambiarEstadoPedidoAsync(int pedidoId, string nuevoEstado)
        {
            var pedido = await _pedidoRepository.ObtenerPorIdAsync(pedidoId);
            if (pedido == null)
                return false;

            await _pedidoRepository.ActualizarEstadoAsync(pedidoId, nuevoEstado);
            await _pedidoRepository.GuardarCambiosAsync();
            return true;
        }

        public async Task<IEnumerable<PedidoResumenDto>> ObtenerComandasCoocinaAsync()
        {
            var pedidos = await _pedidoRepository.ObtenerPedidosActivosAsync();
            return pedidos.Select(p => MapearAResumen(p, null)).ToList();
        }

        public async Task<PedidoResumenDto?> ObtenerDetallePedidoAsync(int pedidoId)
        {
            var pedido = await _pedidoRepository.ObtenerPorIdAsync(pedidoId);
            return pedido == null ? null : MapearAResumen(pedido, null);
        }

        private static PedidoResumenDto MapearAResumen(Pedido pedido, Dictionary<int, Producto>? productosCache)
        {
            return new PedidoResumenDto
            {
                PedidoId = pedido.PedidoId,
                CodigoPedido = pedido.CodigoPedido,
                NumeroMesa = pedido.MesaId,
                Estado = pedido.Estado,
                FechaHoraRegistro = pedido.FechaHoraRegistro,
                TotalPagar = pedido.TotalPagar,
                Items = pedido.Detalles.Select(d => new DetalleResumenDto
                {
                    NombreProducto = ResolverNombreProducto(d, productosCache),
                    Cantidad = d.Cantidad,
                    SaborSalsa = d.SaborSalsa,
                    PrecioUnitario = d.PrecioUnitario,
                    Importe = d.Importe,
                    NotasCocina = d.NotasCocina
                }).ToList()
            };
        }

        private static string ResolverNombreProducto(DetallePedido detalle, Dictionary<int, Producto>? productosCache)
        {
            if (detalle.Producto != null)
                return detalle.Producto.Nombre;

            if (productosCache != null && productosCache.TryGetValue(detalle.ProductoId, out var producto))
                return producto.Nombre;

            return $"Producto{detalle.ProductoId}";
        }
    }
}
