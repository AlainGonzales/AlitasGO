using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using AlitasGo.Web.Hubs;
using AlitasGo.Domain.DTOs;

namespace AlitasGo.Web.Controllers
{
    public class CocinaController : Controller
    {
        private readonly IHubContext<CocinaHub> _hubContext;

        public CocinaController(IHubContext<CocinaHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // Vista principal: Monitor de Cocina
        public IActionResult Tablero()
        {
            return View();
        }

        // Endpoint para SIMULAR la llegada de una comanda desde el salón/caja
        [HttpPost]
        public async Task<IActionResult> SimularNuevoPedido([FromBody] PedidoResumenDto? nuevoPedido)
        {
            if (nuevoPedido == null)
            {
                // Si no viene nada, generamos una comanda aleatoria de alitas
                var random = new Random();
                nuevoPedido = new PedidoResumenDto
                {
                    PedidoId = random.Next(100, 999),
                    CodigoPedido = $"PED-2026-{random.Next(1000, 9999)}",
                    NumeroMesa = random.Next(1, 12),
                    Estado = "EnCocina",
                    FechaHoraRegistro = DateTime.Now,
                    TotalPagar = 48.00m,
                    Items = new List<DetalleResumenDto>
                    {
                        new DetalleResumenDto
                        {
                            NombreProducto = "Alitas x 12 Piezas",
                            Cantidad = 1,
                            SaborSalsa = "Acevichada",
                            NotasCocina = "Bien crujientes",
                            PrecioUnitario = 38.00m,
                            Importe = 38.00m
                        },
                        new DetalleResumenDto
                        {
                            NombreProducto = "Papas Nativas Familiares",
                            Cantidad = 1,
                            SaborSalsa = null,
                            NotasCocina = "Sin sal",
                            PrecioUnitario = 10.00m,
                            Importe = 10.00m
                        }
                    }
                };
            }

            // ¡MAGIA DE SIGNALR!: Se envía a todos los navegadores abiertos en el monitor de cocina
            await _hubContext.Clients.All.SendAsync("RecibirNuevoPedido", nuevoPedido);

            return Ok(new { mensaje = "Pedido transmitido a Cocina vía SignalR con éxito", pedido = nuevoPedido });
        }
    }
}