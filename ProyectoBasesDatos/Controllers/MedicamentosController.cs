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
                // Si no hay un IdHospital en la sesión, redirigir o mostrar un mensaje de error
                return RedirectToAction("Error", "Home");
            }

            // Filtrar los medicamentos por el IdHospital
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
                .Include(m => m.IdHospitalMedicamentoNavigation) // Relación con HospitalMed
                    .ThenInclude(hm => hm.IdHospitalNavigation) // Relación con Hospital
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medicamento == null)
            {
                return NotFound();
            }

            // Verifica si necesitas más información de otras tablas relacionadas
            // Por ejemplo, si tienes más entidades relacionadas con Medicamento

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
            // Generar el ID
            var newId = await GenerateNextIDMed();

            // Pasar el ID a la vista
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
            // Obtener el IdHospital de la sesión
            var idHospital = HttpContext.Session.GetString("IdHospital");

            // Generar el ID y asignarlo al modelo
            var hospital_med_id = await GenerateNextIDMed();

            // Crear el registro en HospitalMed
            var hospitalMed = new HospitalMed
            {
                Id = hospital_med_id,
                Precio = Precio,
                Cantidad = Cantidad,
                IdHospital = idHospital
            };

            // Guardar en la base de datos
            _context.Add(hospitalMed);
            await _context.SaveChangesAsync();

            medicamento.Id = hospital_med_id.Substring(5);
            Console.WriteLine(medicamento.Id);
            // Asignar el IdHospitalMedicamento al medicamento
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
            // Incluir la entidad relacionada HospitalMed
            var medicamento = await _context.Medicamentos
                .Include(m => m.IdHospitalMedicamentoNavigation) // Incluir HospitalMed
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medicamento == null)
            {
                return NotFound();
            }
            ViewBag.Cantidad = medicamento.IdHospitalMedicamentoNavigation.Cantidad;
            ViewBag.Precio = medicamento.IdHospitalMedicamentoNavigation.Precio;

            Console.WriteLine("Precio:" + medicamento.IdHospitalMedicamentoNavigation.Precio);
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
                // Obtener el registro de HospitalMed relacionado
                var hospitalMed = await _context.HospitalMeds
                    .FirstOrDefaultAsync(hm => hm.Id == medicamento.IdHospitalMedicamento);

                if (hospitalMed != null)
                {
                    // Actualizar los valores de Cantidad y Precio
                    hospitalMed.Cantidad = Cantidad;
                    hospitalMed.Precio = Precio;
                    _context.Update(hospitalMed);
                }

                // Actualizar el medicamento
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