using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestauranteCbba.Data;
using RestauranteCbba.Models;
using System.Linq;
using System.Threading.Tasks;

namespace RestauranteCbba.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var productos = await _context.Productos
                .Where(p => p.Activo == true)
                .Take(6)
                .ToListAsync();
            return View(productos);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}