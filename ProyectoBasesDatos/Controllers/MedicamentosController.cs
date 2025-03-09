using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoBasesDatos.Models;

namespace ProyectoBasesDatos.Controllers
{
    public class MedicamentosController : Controller
    {
        private readonly dbContext _context;

        public MedicamentosController(dbContext context)
        {
            _context = context;
        }

        // GET: Medicamentos
        public async Task<IActionResult> Index()
        {
            var idHospital = HttpContext.Session.GetString("IdHospital");
            if (string.IsNullOrEmpty(idHospital))
            {
                return RedirectToAction("Error", "Home");
            }
            var medicamentos = await _context.Medicamentos
                .Include(m => m.IdHospitalMedicamentoNavigation)
                    .ThenInclude(hm => hm.IdHospitalNavigation)
                .Where(m => m.IdHospitalMedicamentoNavigation.IdHospital == idHospital)
                .ToListAsync();

            return View(medicamentos);
        }

        // GET: Medicamentos/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicamento = await _context.Medicamentos
                .Include(m => m.IdHospitalMedicamentoNavigation) 
                    .ThenInclude(hm => hm.IdHospitalNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            return View(medicamento);
        }



        [HttpGet]
        public async Task<IActionResult> GenerateMedId()
        {
            string newId = await GenerateNextIDMed();
            return Json(new { id = newId });
        }

        // GET: Medicamentos/Create
        public async Task<IActionResult> Create()
        {
            var newId = await GenerateNextIDMed();
            ViewBag.GeneratedId = newId;

            return View();
        }

        // POST: Medicamentos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Descripcion,IdHospitalMedicamento")] Medicamento medicamento,
                                         int Precio,
                                         int Cantidad)
        {
            var idHospital = HttpContext.Session.GetString("IdHospital");

            var hospital_med_id = await GenerateNextIDMed();

            var hospitalMed = new HospitalMed
            {
                Id = hospital_med_id,
                Precio = Precio,
                Cantidad = Cantidad,
                IdHospital = idHospital
            };

            _context.Add(hospitalMed);
            await _context.SaveChangesAsync();

            medicamento.Id = hospital_med_id.Substring(5);
            medicamento.IdHospitalMedicamento = hospitalMed.Id;

            _context.Add(medicamento);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        public async Task<string> GenerateNextIDMed()
        {
            var med = await _context.Medicamentos
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();
            var nextID = 0;
            var hospId = HttpContext.Session.GetString("IdHospital");
            if (med != null)
            {
                string lastID = med.Id;
                if (lastID.StartsWith("MED-"))
                {
                    string number = lastID.Substring(4);
                    if (int.TryParse(number, out int lastNumber))
                    {
                        nextID = lastNumber + 1;
                    }
                }
            }

            string newId = $"{hospId}_MED-{nextID:D3}";
            Console.WriteLine("NEW ID:" + newId);
            return newId;
        }

        // GET: Medicamentos/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var medicamento = await _context.Medicamentos
                .Include(m => m.IdHospitalMedicamentoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medicamento == null)
            {
                return NotFound();
            }
            ViewBag.Cantidad = medicamento.IdHospitalMedicamentoNavigation.Cantidad;
            ViewBag.Precio = medicamento.IdHospitalMedicamentoNavigation.Precio;
            ViewData["IdHospitalMedicamento"] = new SelectList(_context.HospitalMeds, "Id", "Id", medicamento.IdHospitalMedicamento);
            return View(medicamento);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Nombre,Descripcion,IdHospitalMedicamento")] Medicamento medicamento,
     int Precio,
     int Cantidad)
        {
            try
            {
                Console.WriteLine("ID HOSPMED  " + medicamento.IdHospitalMedicamento);
                var hospitalMed = await _context.HospitalMeds
                    .FirstOrDefaultAsync(hm => hm.Id == medicamento.IdHospitalMedicamento);

                Console.WriteLine("Evaluando if");
                if (hospitalMed != null)
                {
                    hospitalMed.Cantidad = Cantidad;
                    hospitalMed.Precio = Precio;
                    _context.Update(hospitalMed);
                }
                _context.Update(medicamento);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MedicamentoExists(medicamento.Id))
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


        private bool MedicamentoExists(string id)
        {
            return _context.Medicamentos.Any(e => e.Id == id);
        }
    }
}