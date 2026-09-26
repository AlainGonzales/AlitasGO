using AlitasGo.Domain.DTOs;
using AlitasGo.Domain.Interfaces;

public class ReceptionistHandler : IReceptionistHandler
{
    private readonly IValidadorPedido _validador;
    private readonly IPedidoService _servicio;
    public List<string> Errores { get; private set; }

    public ReceptionistHandler(IValidadorPedido validador, IPedidoService servicio)
    {
        _validador = validador;
        _servicio = servicio;
        Errores = new List<string>();
    }

    public bool Procesar(CrearPedidoDto pedido)
    {
        if (!_validador.Validar(pedido))
        {
            Errores = _validador.ObtenerErrores();
            return false;
        }
        return true;
    }
}