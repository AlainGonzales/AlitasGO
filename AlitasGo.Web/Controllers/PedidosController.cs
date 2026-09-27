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
        public async Task<IActionResult> Registrar(
            [FromBody] CrearPedidoDto dto,
            [FromServices] AlitasGoDbContext db)
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
                // 1. Guardar el pedido en Base de Datos vía servicio de negocio
                var resultado = await _pedidoService.IniciarYRegistrarPedidoAsync(dto);

                // 2. Cambiar estado de la mesa a 'Ocupada' en la base de datos
                var mesa = await db.Mesas.FindAsync(dto.MesaId);
                if (mesa != null)
                {
                    mesa.Estado = "Ocupada";
                    await db.SaveChangesAsync();
                }

                // 3. Notificar en tiempo real por SignalR a Cocina y Salón
                try
                {
                    await _hubContext.Clients.All.SendAsync("NuevoPedidoRegistrado", resultado);
                    await _hubContext.Clients.All.SendAsync("MesaOcupada", dto.MesaId);
                }
                catch (Exception exSignalR)
                {
                    System.Diagnostics.Debug.WriteLine($"[SignalR Error]: {exSignalR.Message}");
                }

                return Ok(new
                {
                    exito = true,
                    mensaje = "Pedido registrado y enviado a cocina.",
                    pedido = resultado
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { exito = false, mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { exito = false, mensaje = "Error interno al procesar el pedido.", detalle = ex.Message });
            }
        }

        // POST: /Pedidos/LiberarMesa (Disparado por el mozo al desocupar la mesa)
        [HttpPost]
        public async Task<IActionResult> LiberarMesa(
            [FromBody] int mesaId,
            [FromServices] AlitasGoDbContext db)
        {
            var mesa = await db.Mesas.FindAsync(mesaId);
            if (mesa == null)
                return NotFound(new { exito = false, mensaje = "Mesa no encontrada." });

            mesa.Estado = "Disponible";
            await db.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("MesaLiberada", mesaId);
            return Ok(new { exito = true });
        }

        // GET: /Pedidos/DetallePorMesa?mesaId=1 (Para el modal visor de consumos)
        [HttpGet]
        public async Task<IActionResult> DetallePorMesa(
            int mesaId,
            [FromServices] AlitasGoDbContext db)
        {
            var pedido = await db.Pedidos
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.MesaId == mesaId && p.Estado != "Pagado" && p.Estado != "Anulado")
                .OrderByDescending(p => p.FechaHoraRegistro)
                .FirstOrDefaultAsync();

            if (pedido == null)
                return NotFound(new { mensaje = "No hay pedido activo para esta mesa." });

            return Ok(new
            {
                pedidoId = pedido.PedidoId,
                codigoPedido = pedido.CodigoPedido,
                numeroMesa = mesaId,
                estado = pedido.Estado,
                subTotal = pedido.SubTotal,
                igv = pedido.Igv,
                totalPagar = pedido.TotalPagar,
                items = pedido.Detalles.Select(d => new
                {
                    nombreProducto = d.Producto?.Nombre ?? "Producto",
                    cantidad = d.Cantidad,
                    saborSalsa = d.SaborSalsa,
                    precioUnitario = d.PrecioUnitario,
                    importe = d.Importe,
                    notasCocina = d.NotasCocina
                })
            });
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
        [Route("")]
        [Route("Pedidos/Salon")]        
        public async Task<IActionResult> FrmPedidosSalon([FromServices] AlitasGoDbContext db)
        {
            var mesas = await db.Mesas.AsNoTracking().OrderBy(m => m.NumeroMesa).ToListAsync();
            ViewBag.Productos = await db.Productos
                .AsNoTracking()
                .Where(p => p.Activo)
                .OrderBy(p => p.CategoriaId)
                .ThenBy(p => p.PrecioUnitario)
                .ToListAsync();
            return View("FrmPedidosSalon", mesas);
        }
    }

    public class CambiarEstadoDto
    {
        public int PedidoId { get; set; }
        public string NuevoEstado { get; set; } = string.Empty;
    }
}