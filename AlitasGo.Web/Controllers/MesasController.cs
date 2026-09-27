using AlitasGo.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlitasGo.Web.Controllers
{
    public class MesasController : Controller
    {
        private readonly AlitasGoDbContext _context;

        public MesasController(AlitasGoDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
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
                    m.Estado // "Libre", "Ocupada", "Atendida"
                })
                .ToListAsync();

            return Json(mesas);
        }
    }
}