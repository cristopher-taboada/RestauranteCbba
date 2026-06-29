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

            ViewBag.ClienteId = clienteId;

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
                reserva.ClienteId = clienteId;
                reserva.Estado = "Pendiente";
                reserva.FechaCreacion = DateTime.UtcNow;
                reserva.FechaReserva = DateTime.SpecifyKind(reserva.FechaReserva, DateTimeKind.Utc);

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

        // GET: Editar reserva
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.Mesa)
                .Include(r => r.Cliente)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null)
            {
                return NotFound();
            }

            if (reserva.ClienteId != GetClienteId())
            {
                return Forbid();
            }

            ViewData["MesaId"] = new SelectList(
                _context.Mesas.Where(m => m.Estado == "Disponible" || m.Id == reserva.MesaId),
                "Id", "NumeroMesa", reserva.MesaId
            );
            return View(reserva);
        }

        // POST: Editar reserva
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MesaId,FechaReserva,HoraReserva,NumeroPersonas,Observacion")] Reserva reserva)
        {
            if (id != reserva.Id)
            {
                return NotFound();
            }

            var reservaOriginal = await _context.Reservas.FindAsync(id);
            if (reservaOriginal == null || reservaOriginal.ClienteId != GetClienteId())
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    reservaOriginal.MesaId = reserva.MesaId;
                    reservaOriginal.FechaReserva = DateTime.SpecifyKind(reserva.FechaReserva, DateTimeKind.Utc);
                    reservaOriginal.HoraReserva = reserva.HoraReserva;
                    reservaOriginal.NumeroPersonas = reserva.NumeroPersonas;
                    reservaOriginal.Observacion = reserva.Observacion;

                    _context.Update(reservaOriginal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservaExists(reserva.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["MesaId"] = new SelectList(
                _context.Mesas.Where(m => m.Estado == "Disponible" || m.Id == reserva.MesaId),
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

        // GET: Eliminar reserva (con validación de seguridad)
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

            // Verificar que la reserva pertenece al cliente logueado
            if (reserva.ClienteId != GetClienteId())
            {
                return Forbid();
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
                // Verificar que la reserva pertenece al cliente logueado
                if (reserva.ClienteId == GetClienteId())
                {
                    var mesa = await _context.Mesas.FindAsync(reserva.MesaId);
                    if (mesa != null)
                    {
                        mesa.Estado = "Disponible";
                    }

                    _context.Reservas.Remove(reserva);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ReservaExists(int id)
        {
            return _context.Reservas.Any(e => e.Id == id);
        }

        private int GetClienteId()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email)) return 0;

            var cliente = _context.Clientes.FirstOrDefault(c => c.Correo == email);
            return cliente?.Id ?? 0;
        }
    }
}