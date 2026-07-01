using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestauranteCbba.Data;
using RestauranteCbba.Models;
using System.Security.Claims;

namespace RestauranteCbba.Controllers
{
    [Authorize]
    public class TiendaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TiendaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var productos = await _context.Productos
                .Where(p => p.Activo == true)
                .ToListAsync();
            return View(productos);
        }

        public IActionResult Carrito()
        {
            var carrito = GetCarrito();
            return View(carrito);
        }

        [HttpPost]
        public IActionResult AgregarAlCarrito(int id)
        {
            var producto = _context.Productos.FirstOrDefault(p => p.Id == id && p.Activo == true);
            if (producto == null)
            {
                TempData["Error"] = "❌ Producto no encontrado.";
                return RedirectToAction("Index");
            }

            if (producto.Cantidad <= 0)
            {
                TempData["Error"] = $"❌ {producto.Nombre} está agotado.";
                return RedirectToAction("Index");
            }

            var carrito = GetCarrito();
            var item = carrito.FirstOrDefault(c => c.Id == id);
            if (item != null)
            {
                if (item.Cantidad + 1 > producto.Cantidad)
                {
                    TempData["Error"] = $"❌ Solo hay {producto.Cantidad} unidades de {producto.Nombre}.";
                    return RedirectToAction("Index");
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
            return RedirectToAction("Index");
        }

        [HttpPost]
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
            return RedirectToAction("Carrito");
        }

        public IActionResult Checkout()
        {
            var carrito = GetCarrito();
            if (!carrito.Any())
            {
                TempData["Error"] = "❌ Tu carrito está vacío.";
                return RedirectToAction("Index");
            }

            ViewBag.Total = carrito.Sum(i => i.Subtotal);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarPedido(string direccion, string metodoPago)
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
                    return RedirectToAction("Carrito");
                }
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "usuario-sin-id";
            var pedido = new Pedido
            {
                UsuarioId = userId,
                Direccion = direccion,
                MetodoPago = metodoPago,
                Total = carrito.Sum(i => i.Subtotal),
                FechaPedido = DateTime.UtcNow,
                Estado = "Confirmado"
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            foreach (var item in carrito)
            {
                var producto = await _context.Productos.FindAsync(item.Id);
                if (producto != null)
                {
                    producto.Cantidad -= item.Cantidad;

                    var detalle = new DetallePedido
                    {
                        PedidoId = pedido.Id,
                        ProductoId = item.Id,
                        NombreProducto = item.Nombre ?? "Producto",
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.Precio
                    };
                    _context.DetallePedidos.Add(detalle);
                }
            }

            await _context.SaveChangesAsync();

            GuardarCarrito(new List<CarritoItem>());

            TempData["Success"] = $"🎉 ¡Pedido confirmado! Total: Bs. {pedido.Total}. Gracias por tu compra.";
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Reportes()
        {
            var topProductos = await _context.DetallePedidos
                .GroupBy(d => d.ProductoId)
                .Select(g => new
                {
                    ProductoId = g.Key,
                    Nombre = g.First().NombreProducto,
                    Vendidos = g.Sum(d => d.Cantidad),
                    Recaudado = g.Sum(d => d.Cantidad * d.PrecioUnitario)
                })
                .OrderByDescending(g => g.Vendidos)
                .Take(10)
                .ToListAsync();

            ViewBag.TopProductos = topProductos;

            var ventasPorMes = await _context.Pedidos
                .Where(p => p.Estado == "Confirmado")
                .GroupBy(p => new { p.FechaPedido.Year, p.FechaPedido.Month })
                .Select(g => new
                {
                    Mes = g.Key.Month,
                    Year = g.Key.Year,
                    Vendidos = g.Count(),
                    Recaudado = g.Sum(p => p.Total)
                })
                .OrderByDescending(g => g.Year)
                .ThenByDescending(g => g.Mes)
                .ToListAsync();

            ViewBag.VentasPorMes = ventasPorMes;
            ViewBag.TotalProductos = await _context.Productos.CountAsync();
            ViewBag.TotalPedidos = await _context.Pedidos.CountAsync();

            return View();
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