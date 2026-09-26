using AlitasGo.Domain.DTOs;

namespace AlitasGo.Domain.Interfaces
{
    public interface IValidadorPedido
    {
        bool Validar(CrearPedidoDto pedido);
        List<string> ObtenerErrores();
    }
}