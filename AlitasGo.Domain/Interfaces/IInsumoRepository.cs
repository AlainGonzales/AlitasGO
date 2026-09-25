using AlitasGo.Domain.Entities;

namespace AlitasGo.Domain.Interfaces
{
    public interface IInsumoRepository
    {
        Task<Insumo?> ObtenerPorIdAsync(int insumoId);
        Task<IEnumerable<Insumo>> ObtenerTodosAsync();
        Task<IEnumerable<Insumo>> ObtenerInsumosCriticosAsync();
        Task<bool> HayStockSuficienteAsync(int insumoId, decimal cantidadRequerida);
        Task RegistrarMovimientoKardexAsync(MovimientoKardex movimiento);
        Task DescontarStockAsync(int insumoId, decimal cantidad);
        Task GuardarCambiosAsync();
    }
}