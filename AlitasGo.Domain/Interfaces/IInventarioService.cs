namespace AlitasGo.Domain.Interfaces
{
    public interface IInventarioService
    {
        Task<bool> ValidarDisponibilidadPorProductoAsync(int productoId, int cantidadPedida);
        Task DescontarInsumosPorPedidoAsync(int pedidoId);
        Task<bool> VerificarAlertaQuiebreStockAsync(int insumoId);
    }
}