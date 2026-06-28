
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestauranteCbba.Models;
using RestauranteCbba.Data;

public class MesasController : Controller
{
    private readonly ApplicationDbContext _context;

    public MesasController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: MESAS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Mesas.ToListAsync());
    }

    // GET: MESAS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var mesa = await _context.Mesas
            .FirstOrDefaultAsync(m => m.Id == id);
        if (mesa == null)
        {
            return NotFound();
        }

        return View(mesa);
    }

    // GET: MESAS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: MESAS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,NumeroMesa,Capacidad,Ubicacion,Estado")] Mesa mesa)
    {
        if (ModelState.IsValid)
        {
            _context.Add(mesa);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(mesa);
    }

    // GET: MESAS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var mesa = await _context.Mesas.FindAsync(id);
        if (mesa == null)
        {
            return NotFound();
        }
        return View(mesa);
    }

    // POST: MESAS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,NumeroMesa,Capacidad,Ubicacion,Estado")] Mesa mesa)
    {
        if (id != mesa.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(mesa);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MesaExists(mesa.Id))
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
        return View(mesa);
    }

    // GET: MESAS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var mesa = await _context.Mesas
            .FirstOrDefaultAsync(m => m.Id == id);
        if (mesa == null)
        {
            return NotFound();
        }

        return View(mesa);
    }

    // POST: MESAS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var mesa = await _context.Mesas.FindAsync(id);
        if (mesa != null)
        {
            _context.Mesas.Remove(mesa);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool MesaExists(int? id)
    {
        return _context.Mesas.Any(e => e.Id == id);
    }
}
