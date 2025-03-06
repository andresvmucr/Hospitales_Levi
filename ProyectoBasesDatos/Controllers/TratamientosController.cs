using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProyectoBasesDatos.Models;

namespace ProyectoBasesDatos.Controllers
{
    public class TratamientosController : Controller
    {
        private readonly dbContext _context;

        public TratamientosController(dbContext context)
        {
            _context = context;
        }

        // GET: Tratamientoes
        public async Task<IActionResult> Index()
        {
            var dbContext = _context.Tratamientos.Include(t => t.IdCitaNavigation);
            return View(await dbContext.ToListAsync());
        }


        [HttpGet("Citas/Attend/{id}/GetMeds")]
        public IActionResult GetMeds(string id)
        {
            Console.WriteLine($"Obteniendo medicamentos para la cita con ID: {id}");

            var hospitalId = "H001"; // Puedes obtenerlo de la sesión si es necesario

            var medicamentos = _context.Medicamentos
                .Where(m => m.IdHospitalMedicamento.StartsWith(hospitalId))
                .Select(m => new { id = m.Id, nombre = m.Nombre })
                .ToList();

            if (medicamentos.IsNullOrEmpty())
            {
                Console.WriteLine("Sin meds");
            } else
            {
                Console.WriteLine("Con meds");
            }

            // Imprimir los medicamentos en consola
            foreach (var med in medicamentos)
            {
                Console.WriteLine($"ID: {med.id}, Nombre: {med.nombre}");
            }

            return Json(medicamentos);
        }



        // GET: Tratamientoes/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tratamiento = await _context.Tratamientos
                .Include(t => t.IdCitaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tratamiento == null)
            {
                return NotFound();
            }

            return View(tratamiento);
        }

        public IActionResult Create()
        {
            Console.WriteLine("Método create");
            //// Obtén el hospitalId de la sesión
            //var hospitalId = HttpContext.Session.GetString("IdHospital");
            var hospitalId = "H001";

            if (string.IsNullOrEmpty(hospitalId))
            {
                // Si no hay hospitalId en la sesión, redirige o muestra un error
                return RedirectToAction("Error", "Home");
            }

            // Filtra los medicamentos donde el Id comience con el hospitalId
            var medicamentos = _context.Medicamentos
                .Where(m => m.Id.StartsWith(hospitalId)) // Filtra por Id que comience con hospitalId
                .ToList();

            // Pasa los medicamentos a la vista usando ViewBag
            ViewBag.Medicamentos = new SelectList(medicamentos, "Id", "Nombre");
            Console.WriteLine(string.Join(", ", medicamentos));

            // Si necesitas otras listas desplegables, como citas, también las puedes cargar aquí
            ViewData["IdCita"] = new SelectList(_context.Citas, "Id", "Id");

            return View();
        }

        // POST: Tratamientoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Precio,IdCita")] Tratamiento tratamiento)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tratamiento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCita"] = new SelectList(_context.Citas, "Id", "Id", tratamiento.IdCita);
            return View(tratamiento);
        }

        // GET: Tratamientoes/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tratamiento = await _context.Tratamientos.FindAsync(id);
            if (tratamiento == null)
            {
                return NotFound();
            }
            ViewData["IdCita"] = new SelectList(_context.Citas, "Id", "Id", tratamiento.IdCita);
            return View(tratamiento);
        }

        // POST: Tratamientoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Precio,IdCita")] Tratamiento tratamiento)
        {
            if (id != tratamiento.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tratamiento);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TratamientoExists(tratamiento.Id))
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
            ViewData["IdCita"] = new SelectList(_context.Citas, "Id", "Id", tratamiento.IdCita);
            return View(tratamiento);
        }

        // GET: Tratamientoes/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tratamiento = await _context.Tratamientos
                .Include(t => t.IdCitaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tratamiento == null)
            {
                return NotFound();
            }

            return View(tratamiento);
        }

        // POST: Tratamientoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var tratamiento = await _context.Tratamientos.FindAsync(id);
            if (tratamiento != null)
            {
                _context.Tratamientos.Remove(tratamiento);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TratamientoExists(string id)
        {
            return _context.Tratamientos.Any(e => e.Id == id);
        }
    }
}
