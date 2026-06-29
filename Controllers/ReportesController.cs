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
            var totalClientes = await _context.Clientes.CountAsync();

            // Calcular ventas totales (suma de precios de productos en reservas)
            var totalVentas = await _context.Reservas
                .Where(r => r.Estado == "Atendida" || r.Estado == "Confirmada")
                .CountAsync() * 100;

            ViewBag.TotalMesas = totalMesas;
            ViewBag.TotalProductos = totalProductos;
            ViewBag.TotalReservas = totalReservas;
            ViewBag.TotalClientes = totalClientes;
            ViewBag.TotalVentas = totalVentas;

            return View();
        }
    }
}