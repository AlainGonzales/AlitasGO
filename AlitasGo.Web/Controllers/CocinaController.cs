using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using AlitasGo.Web.Hubs;
using AlitasGo.Domain.DTOs;
using AlitasGo.Domain.Interfaces;

namespace AlitasGo.Web.Controllers
{
    public class CocinaController : Controller
    {
        private readonly IHubContext<CocinaHub> _hubContext;
        private readonly IPedidoService _pedidoService;

        public CocinaController(
            IHubContext<CocinaHub> hubContext,
            IPedidoService pedidoService)
        {
            _hubContext = hubContext;
            _pedidoService = pedidoService;
        }

        // GET: /Cocina/Tablero
        // Carga los pedidos que ya están en cola ("EnCocina", "Pendiente", "Listo")
        [HttpGet]
        public async Task<IActionResult> Tablero()
        {
            var comandas = await _pedidoService.ObtenerComandasCocinaAsync();
            return View(comandas);
        }

        // Endpoint para SIMULAR la llegada de una comanda desde el salón/caja
        [HttpPost]
        public async Task<IActionResult> SimularNuevoPedido([FromBody] PedidoResumenDto? nuevoPedido)
        {
            if (nuevoPedido == null)
            {
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

            // Nombre del evento alineado con PedidosController y la vista
            await _hubContext.Clients.All.SendAsync("NuevoPedidoRegistrado", nuevoPedido);

            return Ok(new { mensaje = "Pedido transmitido a Cocina vía SignalR con éxito", pedido = nuevoPedido });
        }
    }
}