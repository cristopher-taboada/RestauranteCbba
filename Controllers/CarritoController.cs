using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestauranteCbba.Data;
using RestauranteCbba.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestauranteCbba.Controllers
{
    [Authorize]
    public class CarritoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarritoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var carrito = GetCarrito();
            return View(carrito);
        }

        public IActionResult Agregar(int id)
        {
            var producto = _context.Productos.FirstOrDefault(p => p.Id == id && p.Activo == true);
            if (producto == null)
            {
                TempData["Error"] = "❌ Producto no encontrado.";
                return RedirectToAction("Index", "Home");
            }

            if (producto.Stock <= 0)
            {
                TempData["Error"] = $"❌ {producto.Nombre} está agotado.";
                return RedirectToAction("Index", "Home");
            }

            var carrito = GetCarrito();
            var item = carrito.FirstOrDefault(c => c.Id == id);
            if (item != null)
            {
                if (item.Cantidad + 1 > producto.Stock)
                {
                    TempData["Error"] = $"❌ Solo hay {producto.Stock} unidades de {producto.Nombre}.";
                    return RedirectToAction("Index", "Home");
                }
                item.Cantidad++;
            }
            else
            {
                carrito.Add(new CarritoItem
                {
                    Id = producto.Id,
                    Nombre = producto.Nombre,
                    Precio = producto.Precio,
                    Cantidad = 1
                });
            }

            GuardarCarrito(carrito);
            TempData["Success"] = $"✅ {producto.Nombre} agregado al carrito!";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult EliminarDelCarrito(int id)
        {
            var carrito = GetCarrito();
            var item = carrito.FirstOrDefault(c => c.Id == id);
            if (item != null)
            {
                if (item.Cantidad > 1)
                {
                    item.Cantidad--;
                }
                else
                {
                    carrito.Remove(item);
                }
                GuardarCarrito(carrito);
            }
            return RedirectToAction("Index");
        }

        public IActionResult Vaciar()
        {
            GuardarCarrito(new List<CarritoItem>());
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Confirmar()
        {
            var carrito = GetCarrito();
            if (!carrito.Any())
            {
                TempData["Error"] = "❌ Tu carrito está vacío.";
                return RedirectToAction("Index");
            }

            // Verificar stock de todos los productos antes de confirmar
            foreach (var item in carrito)
            {
                var producto = await _context.Productos.FindAsync(item.Id);
                if (producto == null || producto.Stock < item.Cantidad)
                {
                    TempData["Error"] = $"❌ No hay suficiente stock de {item.Nombre}. Disponible: {producto?.Stock ?? 0}";
                    return RedirectToAction("Index");
                }
            }

            // Reducir stock
            foreach (var item in carrito)
            {
                var producto = await _context.Productos.FindAsync(item.Id);
                if (producto != null)
                {
                    producto.Stock -= item.Cantidad;
                }
            }

            await _context.SaveChangesAsync();

            // Vaciar carrito después de confirmar
            GuardarCarrito(new List<CarritoItem>());

            TempData["Success"] = "🎉 ¡Pedido confirmado con éxito! Gracias por tu compra.";
            return RedirectToAction("Index", "Home");
        }

        private List<CarritoItem> GetCarrito()
        {
            var carrito = HttpContext.Session.GetObject<List<CarritoItem>>("Carrito");
            return carrito ?? new List<CarritoItem>();
        }

        private void GuardarCarrito(List<CarritoItem> carrito)
        {
            HttpContext.Session.SetObject("Carrito", carrito);
        }
    }

    public static class SessionExtensions
    {
        public static void SetObject(this ISession session, string key, object value)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(value);
            session.SetString(key, json);
        }

        public static T? GetObject<T>(this ISession session, string key)
        {
            var json = session.GetString(key);
            if (string.IsNullOrEmpty(json))
                return default;
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
    }
}