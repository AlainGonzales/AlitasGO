using Microsoft.AspNetCore.SignalR;
using AlitasGo.Domain.DTOs;

namespace AlitasGo.Web.Hubs
{
    public class CocinaHub : Hub
    {
        // Método que invoca un cliente (ej. la cocina cambiando el estado del plato)
        public async Task CambiarEstadoPedido(int pedidoId, string nuevoEstado, int mesaId)
        {
            // Notifica a TODOS los clientes conectados (Cocina, Caja, Mozos) el cambio de estado
            await Clients.All.SendAsync("EstadoPedidoActualizado", pedidoId, nuevoEstado, mesaId);
        }

        // Notifica cuando el mesero desocupa la mesa
        public async Task LiberarMesa(int mesaId)
        {
            await Clients.All.SendAsync("MesaLiberada", mesaId);
        }

        // Evento cuando se conecta un cliente al monitor
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
    }
}