using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestauranteCbba.Data;

namespace RestauranteCbba.Controllers
{
    [Authorize]
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalMesas = await _context.Mesas.CountAsync();
            var totalProductos = await _context.Productos.CountAsync();
            var totalReservas = await _context.Reservas.CountAsync();

            ViewBag.TotalMesas = totalMesas;
            ViewBag.TotalProductos = totalProductos;
            ViewBag.TotalReservas = totalReservas;

            return View();
        }
    }
}