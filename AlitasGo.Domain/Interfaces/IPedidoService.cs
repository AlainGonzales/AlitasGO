using AlitasGo.Domain.DTOs;

namespace AlitasGo.Domain.Interfaces
{
    public interface IPedidoService
    {
        Task<PedidoResumenDto> IniciarYRegistrarPedidoAsync(CrearPedidoDto dto);
        Task<bool> CambiarEstadoPedidoAsync(int pedidoId, string nuevoEstado);
        Task<IEnumerable<PedidoResumenDto>> ObtenerComandasCocinaAsync();
        Task<PedidoResumenDto?> ObtenerDetallePedidoAsync(int pedidoId);
    }
}