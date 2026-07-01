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
                return RedirectToAction("Index", "Productos");
            }

            if (producto.Cantidad <= 0)
            {
                TempData["Error"] = $"❌ {producto.Nombre} está agotado.";
                return RedirectToAction("Index", "Productos");
            }

            var carrito = GetCarrito();
            var item = carrito.FirstOrDefault(c => c.Id == id);
            if (item != null)
            {
                if (item.Cantidad + 1 > producto.Cantidad)
                {
                    TempData["Error"] = $"❌ Solo hay {producto.Cantidad} unidades de {producto.Nombre}.";
                    return RedirectToAction("Index", "Productos");
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
                    Cantidad = 1,
                    Stock = producto.Cantidad
                });
            }

            GuardarCarrito(carrito);
            TempData["Success"] = $"✅ {producto.Nombre} agregado al carrito!";
            return RedirectToAction("Index", "Productos");
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

        [HttpPost]
        public async Task<IActionResult> Confirmar(string metodoPago)
        {
            var carrito = GetCarrito();
            if (!carrito.Any())
            {
                TempData["Error"] = "❌ Tu carrito está vacío.";
                return RedirectToAction("Index");
            }

            foreach (var item in carrito)
            {
                var producto = await _context.Productos.FindAsync(item.Id);
                if (producto == null || producto.Cantidad < item.Cantidad)
                {
                    TempData["Error"] = $"❌ No hay suficiente stock de {item.Nombre}.";
                    return RedirectToAction("Index");
                }
            }

            foreach (var item in carrito)
            {
                var producto = await _context.Productos.FindAsync(item.Id);
                if (producto != null)
                {
                    producto.Cantidad -= item.Cantidad;
                }
            }

            await _context.SaveChangesAsync();
            GuardarCarrito(new List<CarritoItem>());

            string mensajePago = metodoPago switch
            {
                "Efectivo" => "💵 Pagarás en efectivo al momento de recibir tu pedido.",
                "Tarjeta" => "💳 El pago se procesará con tarjeta.",
                "Transferencia" => "🏦 Realiza la transferencia al número de cuenta indicado.",
                _ => "Gracias por tu compra."
            };

            TempData["Success"] = $"🎉 ¡Pedido confirmado! {mensajePago}";
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
}