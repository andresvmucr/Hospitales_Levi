using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoBasesDatos.Models;

namespace ProyectoBasesDatos.Controllers
{
    public class HospitalesController : Controller
    {
        private readonly dbContext _context;

        public HospitalesController(dbContext context)
        {
            _context = context;
        }

        // GET: Hospitales
        public async Task<IActionResult> Index(string searchString)
        {
            var hospitalesQuery = _context.Hospitales.AsQueryable();

            // Filtrar por nombre si se proporciona un término de búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                hospitalesQuery = hospitalesQuery.Where(h => h.Nombre.Contains(searchString));
            }

            var hospitales = await hospitalesQuery.ToListAsync();
            ViewBag.CurrentFilter = searchString; // Pasar el término de búsqueda a la vista
            return View(hospitales);
        }

        // GET: Hospitales/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hospitale = await _context.Hospitales
                .Include(h => h.IdSuperAdminNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (hospitale == null)
            {
                return NotFound();
            }

            return View(hospitale);
        }

        public async Task<string> GenerateNextID()
        {
            var hosp = await _context.Hospitales
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();
            var nextID = 0;

            if (hosp != null)
            {
                string lastID = hosp.Id;
                string number = lastID.Substring(1);
                if (int.TryParse(number, out int lastNumber))
                {
                    nextID = lastNumber + 1;
                }

            }

            string newId = $"H{nextID:D3}";
            Console.WriteLine("NEW ID:" + newId);
            return newId;
        }

        // GET: Hospitales/Create
        public async Task<IActionResult> Create()
        {
            string newId = await GenerateNextID();
            ViewData["GeneratedId"] = newId;

            ViewData["IdSuperAdmin"] = new SelectList(_context.SuperAdmins, "Id", "Id");
            return View();
        }

        // POST: Hospitales/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Direccion,Telefono")] Hospitale hospitale)
        {
           
            // Obtener el IdSuperAdmin de la sesión
            var idSuperAdmin = HttpContext.Session.GetString("Id");

            if (string.IsNullOrEmpty(idSuperAdmin))
            {
                // Manejar el caso en que no haya un SuperAdmin en la sesión
                ModelState.AddModelError("", "No se ha identificado al SuperAdmin.");
                return View(hospitale);
            }

            // Asignar el IdSuperAdmin al hospital
            hospitale.IdSuperAdmin = idSuperAdmin;

            // Guardar el hospital en la base de datos
            _context.Add(hospitale);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

        // GET: Hospitales/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hospitale = await _context.Hospitales.FindAsync(id);
            if (hospitale == null)
            {
                return NotFound();
            }
            ViewData["IdSuperAdmin"] = new SelectList(_context.SuperAdmins, "Id", "Id", hospitale.IdSuperAdmin);
            return View(hospitale);
        }

        // POST: Hospitales/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Nombre,Direccion,Telefono,IdSuperAdmin")] Hospitale hospitale)
        {

            try
            {
                var superAdminId = HttpContext.Session.GetString("Id");
                hospitale.IdSuperAdminNavigation = await _context.SuperAdmins.FindAsync(superAdminId);
                _context.Update(hospitale);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HospitaleExists(hospitale.Id))
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
        

        // GET: Hospitales/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hospitale = await _context.Hospitales
                .Include(h => h.IdSuperAdminNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (hospitale == null)
            {
                return NotFound();
            }

            return View(hospitale);
        }

        // POST: Hospitales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var hospitale = await _context.Hospitales.FindAsync(id);
            if (hospitale != null)
            {
                _context.Hospitales.Remove(hospitale);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HospitaleExists(string id)
        {
            return _context.Hospitales.Any(e => e.Id == id);
        }
    }
}
