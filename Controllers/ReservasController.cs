using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestauranteCbba.Data;
using RestauranteCbba.Models;

namespace RestauranteCbba.Controllers
{
    [Authorize]
    public class ReservasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Lista de reservas del usuario
        public async Task<IActionResult> Index()
        {
            var clienteId = GetClienteId();
            var reservas = await _context.Reservas
                .Include(r => r.Mesa)
                .Where(r => r.ClienteId == clienteId)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();
            return View(reservas);
        }

        // GET: Crear reserva
        public IActionResult Create()
        {
            var clienteId = GetClienteId();

            if (clienteId == 0)
            {
                ViewBag.Error = "⚠️ No tienes un perfil de cliente. Contacta al administrador.";
            }

            // Solo mostrar mesas disponibles
            ViewData["MesaId"] = new SelectList(
                _context.Mesas.Where(m => m.Estado == "Disponible"),
                "Id", "NumeroMesa"
            );
            return View();
        }

        // POST: Crear reserva
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MesaId,FechaReserva,HoraReserva,NumeroPersonas,Observacion")] Reserva reserva)
        {
            var clienteId = GetClienteId();

            if (clienteId == 0)
            {
                ModelState.AddModelError("", "⚠️ No tienes un perfil de cliente.");
                ViewData["MesaId"] = new SelectList(
                    _context.Mesas.Where(m => m.Estado == "Disponible"),
                    "Id", "NumeroMesa", reserva.MesaId
                );
                return View(reserva);
            }

            if (ModelState.IsValid)
            {
                // ===== CONVERTIR FECHA RESERVA A UTC =====
                reserva.FechaReserva = DateTime.SpecifyKind(reserva.FechaReserva, DateTimeKind.Utc);

                reserva.ClienteId = clienteId;
                reserva.Estado = "Pendiente";
                reserva.FechaCreacion = DateTime.UtcNow;

                var mesa = await _context.Mesas.FindAsync(reserva.MesaId);
                if (mesa != null)
                {
                    mesa.Estado = "Reservada";
                }

                _context.Add(reserva);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["MesaId"] = new SelectList(
                _context.Mesas.Where(m => m.Estado == "Disponible"),
                "Id", "NumeroMesa", reserva.MesaId
            );
            return View(reserva);
        }

        // GET: Cancelar reserva
        public async Task<IActionResult> Cancelar(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva != null && reserva.ClienteId == GetClienteId())
            {
                reserva.Estado = "Cancelada";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Eliminar reserva
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.Mesa)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }

        // POST: Eliminar reserva
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva != null)
            {
                _context.Reservas.Remove(reserva);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Obtener el ID del cliente logueado
        private int GetClienteId()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email)) return 0;

            var cliente = _context.Clientes.FirstOrDefault(c => c.Correo == email);
            return cliente?.Id ?? 0;
        }
    }
}