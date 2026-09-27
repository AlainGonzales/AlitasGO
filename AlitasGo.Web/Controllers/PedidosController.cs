using AlitasGo.Domain.DTOs;
using AlitasGo.Domain.Interfaces;
using AlitasGo.Repository;
using AlitasGo.Web.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AlitasGo.Web.Controllers
{
    public class PedidosController : Controller
    {
        private readonly IPedidoService _pedidoService;
        private readonly IHubContext<CocinaHub> _hubContext;

        public PedidosController(
            IPedidoService pedidoService,
            IHubContext<CocinaHub> hubContext)
        {
            _pedidoService = pedidoService ?? throw new ArgumentNullException(nameof(pedidoService));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        // GET: /Pedidos/Nuevo?mesaId=1
        [HttpGet]
        public IActionResult Nuevo(int? mesaId)
        {
            ViewBag.MesaId = mesaId ?? 0;
            return View();
        }

        // POST: /Pedidos/Registrar (vía AJAX / Fetch desde el modal de salón)
        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] CrearPedidoDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { exito = false, mensaje = "Datos inválidos", errores });
            }

            try
            {
                // Delegar al cerebro del negocio
                var resultado = await _pedidoService.IniciarYRegistrarPedidoAsync(dto);

                // Notificar en tiempo real a la pantalla de Cocina vía SignalR
                await _hubContext.Clients.All.SendAsync("NuevoPedidoRegistrado", resultado);

                return Ok(new
                {
                    exito = true,
                    mensaje = "Pedido registrado y enviado a cocina.",
                    pedido = resultado
                });
            }
            catch (InvalidOperationException ex)
            {
                // Captura errores de negocio (ej. falta de stock de alitas, salsas, etc.)
                return Conflict(new { exito = false, mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { exito = false, mensaje = "Error interno al procesar el pedido.", detalle = ex.Message });
            }
        }

        // POST: /Pedidos/CambiarEstado
        [HttpPost]
        public async Task<IActionResult> CambiarEstado([FromBody] CambiarEstadoDto dto)
        {
            if (dto.PedidoId <= 0 || string.IsNullOrWhiteSpace(dto.NuevoEstado))
                return BadRequest(new { exito = false, mensaje = "Parámetros inválidos." });

            try
            {
                var actualizado = await _pedidoService.CambiarEstadoPedidoAsync(dto.PedidoId, dto.NuevoEstado);
                if (!actualizado)
                    return NotFound(new { exito = false, mensaje = "Pedido no encontrado." });

                // Notificar cambio de estado en cocina y salón
                await _hubContext.Clients.All.SendAsync("EstadoPedidoCambiado", dto.PedidoId, dto.NuevoEstado);

                return Ok(new { exito = true, nuevoEstado = dto.NuevoEstado });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { exito = false, mensaje = ex.Message });
            }
        }

        // GET: /Pedidos/Cocina (Resumen de pedidos en cola para cocina)
        [HttpGet]
        public async Task<IActionResult> Cocina()
        {
            var comandas = await _pedidoService.ObtenerComandasCocinaAsync();
            return View(comandas);
        }

        // GET: /Pedidos/Salon o /Pedidos/FrmPedidosSalon
        [HttpGet]
        [Route("Pedidos/Salon")]
        [Route("Pedidos/FrmPedidosSalon")]
        public async Task<IActionResult> FrmPedidosSalon([FromServices] AlitasGoDbContext db)
        {
            var mesas = await db.Mesas.AsNoTracking().OrderBy(m => m.NumeroMesa).ToListAsync();
            return View("FrmPedidosSalon", mesas);
        }
    }

    public class CambiarEstadoDto
    {
        public int PedidoId { get; set; }
        public string NuevoEstado { get; set; } = string.Empty;
    }
}