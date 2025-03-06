﻿using System;
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
            Console.WriteLine("NEW ID:" + newId);
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
            Console.WriteLine("NEW ID:" + newId);
            return newId;
        }

        // POST: Tratamientoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string IdCita, List<string> TratamientosMeds, List<int> Cantidad, List<string> Frecuencia)
        {
            var cita = await _context.Citas.FindAsync(IdCita);

            var tratamientos = TratamientosMeds.Zip(Cantidad, (med, cant) => new { Medicamento = med, Cantidad = cant })
                                                .Zip(Frecuencia, (trat, frec) => new { trat.Medicamento, trat.Cantidad, Frecuencia = frec });

            int index = 1;
            var precioTotal = 0;
            foreach (var medicinas in tratamientos)
            {
                Console.WriteLine($"Medicamento {index++}:");
                Console.WriteLine($"  - ID Medicamento: {medicinas.Medicamento}");
                Console.WriteLine($"  - Cantidad: {medicinas.Cantidad}");
                Console.WriteLine($"  - Frecuencia: {medicinas.Frecuencia}");
                var medicamento  = await _context.Medicamentos.FindAsync(medicinas.Medicamento);
                var hospital_med = await _context.HospitalMeds.FindAsync(medicamento.IdHospitalMedicamento);
                precioTotal += medicamento.IdHospitalMedicamentoNavigation.Precio * medicinas.Cantidad;
            }

            
            Console.WriteLine($"Precio total: {precioTotal}");
            var tratamiento = new Tratamiento
            {
                Id = await GenerateNextTreatmentID(),
                Precio = precioTotal,
                IdCita = IdCita,
            };
            _context.Tratamientos.Add(tratamiento);
            await _context.SaveChangesAsync();

            index = 1;
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

            return RedirectToAction("DoctorHome", "Home");
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
