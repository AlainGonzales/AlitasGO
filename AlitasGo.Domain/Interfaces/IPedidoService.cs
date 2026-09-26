using AlitasGo.Domain.DTOs;

namespace AlitasGo.Domain.Interfaces
{
    public interface IPedidoService
    {
        Task<PedidoResumenDto> InitiarYRegistrarPedidoAsync(CrearPedidoDto dto);
        Task<bool> CambiarEstadoPedidoAsync(int pedidoId, string nuevoEstado);
        Task<IEnumerable<PedidoResumenDto>> ObtenerComandasCoocinaAsync();
        Task<PedidoResumenDto?> ObtenerDetallePedidoAsync(int pedidoId);
    }
}