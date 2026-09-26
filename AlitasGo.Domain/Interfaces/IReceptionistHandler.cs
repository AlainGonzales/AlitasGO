using AlitasGo.Domain.DTOs;

namespace AlitasGo.Domain.Interfaces
{
    public interface IReceptionistHandler
    {
        bool Procesar(CrearPedidoDto pedido);
        List<string> Errores { get; }
    }
}