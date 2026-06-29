using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestauranteCbba.Models;
using System.Collections.Generic;
using System.Linq;

namespace RestauranteCbba.Controllers
{
    [Authorize]
    public class CarritoController : Controller
    {
        private readonly List<Producto> _productos = new List<Producto>
        {
            new Producto { Id = 1, Nombre = "Pique Macho", Precio = 45.00m, Categoria = "Plato Fuerte" },
            new Producto { Id = 2, Nombre = "Silpancho", Precio = 40.00m, Categoria = "Plato Fuerte" },
            new Producto { Id = 3, Nombre = "Saice", Precio = 38.00m, Categoria = "Plato Fuerte" },
            new Producto { Id = 4, Nombre = "Sopa de Maní", Precio = 25.00m, Categoria = "Entrada" },
            new Producto { Id = 5, Nombre = "Chicharrón", Precio = 50.00m, Categoria = "Plato Fuerte" },
            new Producto { Id = 6, Nombre = "Jugo Natural", Precio = 12.00m, Categoria = "Bebida" },
            new Producto { Id = 7, Nombre = "Api con Pastel", Precio = 15.00m, Categoria = "Postre" },
            new Producto { Id = 8, Nombre = "Salteña", Precio = 18.00m, Categoria = "Entrada" }
        };

        public IActionResult Index()
        {
            var carrito = GetCarrito();
            return View(carrito);
        }

        public IActionResult Agregar(int id)
        {
            var producto = _productos.FirstOrDefault(p => p.Id == id);
            if (producto != null)
            {
                var carrito = GetCarrito();
                var item = carrito.FirstOrDefault(c => c.Id == id);
                if (item != null)
                {
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
            }
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Eliminar(int id)
        {
            var carrito = GetCarrito();
            var item = carrito.FirstOrDefault(c => c.Id == id);
            if (item != null)
            {
                carrito.Remove(item);
                GuardarCarrito(carrito);
            }
            return RedirectToAction("Index");
        }

        public IActionResult Vaciar()
        {
            GuardarCarrito(new List<CarritoItem>());
            return RedirectToAction("Index");
        }

        private List<CarritoItem> GetCarrito()
        {
            return HttpContext.Session.GetObject<List<CarritoItem>>("Carrito")! ?? new List<CarritoItem>();
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
            return json == null ? default : System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
    }
}