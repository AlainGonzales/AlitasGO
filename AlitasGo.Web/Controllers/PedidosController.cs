using AlitasGo.Domain.DTOs;
using AlitasGo.Domain.Handlers;
using AlitasGo.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AlitasGo.Web.Controllers
{
    public class PedidosController : Controller
    {
        private readonly IReceptionistHandler _handler;
        private readonly IPedidoService _servicio;

        public PedidosController(IReceptionistHandler handler, IPedidoService servicio)
        {
            _handler = handler;
            _servicio = servicio;
        }

        public async Task<IActionResult> Index()
        {
            var pedidos = await _servicio.ObtenerComandasCoocinaAsync();
            return View(pedidos);
        }

        public IActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registrar(CrearPedidoDto pedido)
        {
            if (!_handler.Procesar(pedido))
            {
                ViewBag.Errores = _handler.Errores;
                return View();
            }

            await _servicio.InitiarYRegistrarPedidoAsync(pedido);
            return RedirectToAction("Index");
        }
    }
}