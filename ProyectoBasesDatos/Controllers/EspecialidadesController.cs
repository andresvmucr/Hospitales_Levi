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
    public class EspecialidadesController : Controller
    {
        private readonly dbContext _context;

        public EspecialidadesController(dbContext context)
        {
            _context = context;
        }

        // GET: Especialidades
        public async Task<IActionResult> Index()
        {
            var hospId = HttpContext.Session.GetString("IdHospital");
            if (string.IsNullOrEmpty(hospId))
            {
                return NotFound("No se encontró el ID del hospital en la sesión.");
            }

            var especialidades = await _context.Especialidades
                .Where(e => e.Id.StartsWith(hospId))
                .ToListAsync();

            return View(especialidades);
        }

        // GET: Especialidades/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var especialidade = await _context.Especialidades
                .FirstOrDefaultAsync(m => m.Id == id);
            if (especialidade == null)
            {
                return NotFound();
            }

            return View(especialidade);
        }


        [HttpGet]
        public async Task<IActionResult> GenerateSpecialtyId()
        {
            string newId = await GenerateNextIDSpecialty();
            return Json(new { id = newId });
        }

        public async Task<string> GenerateNextIDSpecialty()
        {
            var specialty = await _context.Especialidades
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();
            var nextID = 0;
            var hospId = HttpContext.Session.GetString("IdHospital");
            if (specialty != null)
            {
                string lastID = specialty.Id;
                Console.WriteLine("LAST ID:" + lastID);
                if (lastID.Contains("ESP"))
                {
                        
                    string number = lastID.Substring(8);
                    if (int.TryParse(number, out int lastNumber))
                    {
                        nextID = lastNumber + 1;
                    }
                }
            }

            string newId = $"{hospId}-ESP{nextID:D3}";
            Console.WriteLine("NEW ID:" + newId);
            return newId;
        }

        // GET: Especialidades/Create
        public async Task<IActionResult> Create()
        {
            var newId = await GenerateNextIDSpecialty();
            ViewBag.GeneratedId = newId;
            return View();
        }

        // POST: Especialidades/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Descripcion")] Especialidade especialidade)
        {
            if (ModelState.IsValid)
            {
                _context.Add(especialidade);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(especialidade);
        }

        // GET: Especialidades/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var especialidade = await _context.Especialidades.FindAsync(id);
            if (especialidade == null)
            {
                return NotFound();
            }
            return View(especialidade);
        }

        // POST: Especialidades/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Nombre,Descripcion")] Especialidade especialidade)
        {
            if (id != especialidade.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(especialidade);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EspecialidadeExists(especialidade.Id))
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
            return View(especialidade);
        }

        // GET: Especialidades/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var especialidade = await _context.Especialidades
                .FirstOrDefaultAsync(m => m.Id == id);
            if (especialidade == null)
            {
                return NotFound();
            }

            return View(especialidade);
        }

        // POST: Especialidades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var especialidade = await _context.Especialidades.FindAsync(id);
            if (especialidade != null)
            {
                _context.Especialidades.Remove(especialidade);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EspecialidadeExists(string id)
        {
            return _context.Especialidades.Any(e => e.Id == id);
        }
    }
}
