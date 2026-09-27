using AlitasGo.Repository;
using AlitasGo.Web.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AlitasGo.Web.Controllers
{
    public class MesasController : Controller
    {
        private readonly AlitasGoDbContext _context;
        private readonly IHubContext<CocinaHub> _hubContext;

        public MesasController(AlitasGoDbContext context, IHubContext<CocinaHub> hubContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        // GET: /Mesas o /Mesas/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var mesas = await _context.Mesas
                .AsNoTracking()
                .OrderBy(m => m.NumeroMesa)
                .ToListAsync();

            return View(mesas);
        }

        // GET: /Mesas/ListadoJson (para llamadas fetch o ajax del frontend)
        [HttpGet]
        public async Task<IActionResult> ListadoJson()
        {
            var mesas = await _context.Mesas
                .AsNoTracking()
                .Select(m => new
                {
                    m.MesaId,
                    m.NumeroMesa,
                    m.Capacidad,
                    m.Estado // "Disponible", "Ocupada", "Listo"
                })
                .ToListAsync();

            return Json(mesas);
        }

        // POST: /Mesas/LiberarMesa
        [HttpPost]
        public async Task<IActionResult> LiberarMesa([FromBody] int mesaId)
        {
            var mesa = await _context.Mesas.FindAsync(mesaId);
            if (mesa == null)
                return NotFound(new { exito = false, mensaje = "Mesa no encontrada." });

            mesa.Estado = "Disponible";
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("MesaLiberada", mesaId);
            return Ok(new { exito = true });
        }

        // GET: /Mesas/DetallePorMesa?mesaId=1
        [HttpGet]
        public async Task<IActionResult> DetallePorMesa(int mesaId)
        {
            var pedido = await _context.Pedidos
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
    }
}