using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
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
            var hospitalId = HttpContext.Session.GetString("IdHospital");

            if (string.IsNullOrEmpty(hospitalId))
            {
                return RedirectToAction("Error", "Home");
            }

            var tratamientos = await _context.Tratamientos
                .Include(t => t.IdCitaNavigation) 
                    .ThenInclude(c => c.CedulaDoctorNavigation)
                        .ThenInclude(d => d.CorreoNavigation) 
                .Include(t => t.IdCitaNavigation) 
                    .ThenInclude(c => c.CedulaPacienteNavigation) 
                        .ThenInclude(p => p.CorreoNavigation) 
                .Where(t => t.IdCitaNavigation.Id.StartsWith(hospitalId))
                .ToListAsync();

            return View(tratamientos);
        }


        [HttpGet("Citas/Attend/{id}/GetMeds")]
        public IActionResult GetMeds(string id)
        {
            var hospitalId = HttpContext.Session.GetString("IdHospital");

            var medicamentos = _context.Medicamentos
                .Where(m => m.IdHospitalMedicamento.StartsWith(hospitalId))
                .Select(m => new { id = m.Id, nombre = m.Nombre })
                .ToList();

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
                .Include(t => t.TratamientosMeds)
                    .ThenInclude(tm => tm.IdMedicamentoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tratamiento == null)
            {
                return NotFound();
            }

            return View(tratamiento);
        }


        public async Task<string> GenerateNextTreatmentID()
        {
            var tratamientos = await _context.Tratamientos
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();
            var nextID = 0;

            if (tratamientos != null)
            {
                string lastID = tratamientos.Id;
                string number = lastID.Substring(3);
                if (int.TryParse(number, out int lastNumber))
                {
                    nextID = lastNumber + 1;
                }

            }

            string newId = $"TRM{nextID:D3}";
            return newId;
        }

        public async Task<string> GenerateNextID()
        {
            var tratamientoMed = await _context.TratamientosMeds
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();
            var nextID = 0;

            if (tratamientoMed != null)
            {
                string lastID = tratamientoMed.Id;
                string number = lastID.Substring(3);
                if (int.TryParse(number, out int lastNumber))
                {
                    nextID = lastNumber + 1;
                }

            }
            string newId = $"TMD{nextID:D3}";
            return newId;
        }

        // POST: Tratamientoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string IdCita, List<string> TratamientosMeds, List<int> Cantidad, List<string> Frecuencia)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(); 

            try
            {
                var cita = await _context.Citas.FindAsync(IdCita);
                if (cita == null)
                {
                    return NotFound();
                }

                var tratamientos = TratamientosMeds.Zip(Cantidad, (med, cant) => new { Medicamento = med, Cantidad = cant })
                                                   .Zip(Frecuencia, (trat, frec) => new { trat.Medicamento, trat.Cantidad, Frecuencia = frec });

                int precioTotal = 0;

                foreach (var medicinas in tratamientos)
                {
                    var medicamento = await _context.Medicamentos.FindAsync(medicinas.Medicamento);
                    if (medicamento == null)
                    {
                        return BadRequest($"El medicamento con ID {medicinas.Medicamento} no existe.");
                    }

                    var hospitalMed = await _context.HospitalMeds.FindAsync(medicamento.IdHospitalMedicamento);
                    precioTotal += hospitalMed.Precio * medicinas.Cantidad;
                }

                var tratamiento = new Tratamiento
                {
                    Id = await GenerateNextTreatmentID(),
                    Precio = precioTotal,
                    IdCita = IdCita,
                };

                _context.Tratamientos.Add(tratamiento);
                await _context.SaveChangesAsync();

                foreach (var meds_tratamiento in tratamientos)
                {
                    var tratamientoMed = new TratamientosMed
                    {
                        Id = await GenerateNextID(),
                        Dosis = meds_tratamiento.Cantidad.ToString(),
                        Frecuencia = meds_tratamiento.Frecuencia,
                        Fecha = cita.Dia,
                        IdTratamiento = tratamiento.Id,
                        IdMedicamento = meds_tratamiento.Medicamento
                    };
                    _context.TratamientosMeds.Add(tratamientoMed);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync(); 



                cita.Estado = "Completada";
                _context.Update(cita);
                await _context.SaveChangesAsync();


                return RedirectToAction("DoctorHome", "Home");
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Message.Contains("Stock insuficiente"))
                {
    
                    string errorMessage = sqlEx.Message;

           
                    int firstDotIndex = errorMessage.IndexOf('.');
                    if (firstDotIndex > 0) 
                    {
                        errorMessage = errorMessage.Substring(0, firstDotIndex);
                    }

             
                    TempData["ErrorMessage"] = errorMessage;

             
                    await transaction.RollbackAsync(); 
                    return RedirectToAction("Attend", "Citas", new { id = IdCita });
                }

                await transaction.RollbackAsync(); 
                throw; 
            }
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

        private bool TratamientoExists(string id)
        {
            return _context.Tratamientos.Any(e => e.Id == id);
        }
    }
}
