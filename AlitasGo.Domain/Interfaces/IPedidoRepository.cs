using AlitasGo.Domain.Entities;

namespace AlitasGo.Domain.Interfaces
{
    public interface IPedidoRepository
    {
        Task<Pedido?> ObtenerPorIdAsync(int pedidoId);
        Task<Pedido?> ObtenerPorCodigoAsync(string codigoPedido);
        Task<IEnumerable<Pedido>> ObtenerPedidosActivosAsync();
        Task<IEnumerable<Pedido>> ObtenerPorMesaAsync(int mesaId);
        Task CrearPedidoAsync(Pedido pedido);
        Task ActualizarEstadoAsync(int pedidoId, string nuevoEstado);
        Task GuardarCambiosAsync();
    }
}