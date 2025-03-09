using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProyectoBasesDatos.Models;
using System.Data;

namespace ProyectoBasesDatos.Controllers
{
    public class CitasController : Controller
    {
        private readonly dbContext _context;

        public CitasController(dbContext context)
        {
            _context = context;
        }

        
        // GET: Citas
        public async Task<IActionResult> Index()
        {
            var hospitalId = HttpContext.Session.GetString("IdHospital");
            if (string.IsNullOrEmpty(hospitalId))
            {
                return RedirectToAction("Error", "Home");
            }
            var citas = await _context.Citas
                .Include(c => c.CedulaDoctorNavigation)
                    .ThenInclude(d => d.CorreoNavigation) 
                .Include(c => c.CedulaPacienteNavigation)
                    .ThenInclude(p => p.CorreoNavigation) 
                .Where(c => c.CedulaDoctorNavigation.CorreoNavigation.IdHospital == hospitalId) 
                .OrderBy(c => c.Dia) 
                .ThenBy(c => c.Estado == "Pendiente" ? 0 : 1)
                .ToListAsync();

            return View(citas);
        }

        // GET: Citas/Details/5
        public async Task<IActionResult> Details(string id)
        {   
            var cita = await _context.Citas
                .Include(c => c.CedulaDoctorNavigation)
                    .ThenInclude(d => d.CorreoNavigation) 
                .Include(c => c.CedulaDoctorNavigation) 
                    .ThenInclude(d => d.IdEspecialidadNavigation) 
                .Include(c => c.CedulaPacienteNavigation) 
                    .ThenInclude(p => p.CorreoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

          
            return View(cita);
        }

        public async Task<IActionResult> GetDoctoresByEspecialidad(string especialidad)
        {
            var hospitalId = HttpContext.Session.GetString("IdHospital");
            var doctores = await _context.Doctores
                .Where(d => d.IdEspecialidad == especialidad && d.CorreoNavigation.IdHospital == hospitalId)
                 .Select(d => new
                 {
                     d.Cedula,
                     NombreCompleto = d.CorreoNavigation.Nombre + " " + d.CorreoNavigation.PrimerApellido + " " + d.CorreoNavigation.SegundoApellido
                 })
                .ToListAsync();

            return Json(doctores);
        }


        public async Task<IActionResult> GetDiasTrabajoDoctor(string idDoctor)
        {
            var diasTrabajo = await _context.Horarios
                .Where(d => d.CedulaDoctor == idDoctor)
                .Select(d => new
                {
                    d.Dia,
                    DiaCompleto = d.Dia.StartsWith("L") ? "Lunes" :
                                  d.Dia.StartsWith("M") ? "Martes" :
                                  d.Dia.StartsWith("K") ? "Miércoles" :
                                  d.Dia.StartsWith("J") ? "Jueves" :
                                  d.Dia.StartsWith("V") ? "Viernes" :
                                  d.Dia.StartsWith("S") ? "Sábado" :
                                  d.Dia.StartsWith("D") ? "Domingo" : "Desconocido"
                })
                .ToListAsync();
            return Json(diasTrabajo);
        }

        public async Task<IActionResult> GetHorasDisponibles(string idDoctor, string dia)
        {
            DateTime fecha = DateTime.Parse(dia);
            string[] diasSemana = { "D", "L", "K", "M", "J", "V", "S" };
            string diaSemana = diasSemana[(int)fecha.DayOfWeek];
            var horarios = await _context.Horarios
                .Where(h => h.CedulaDoctor == idDoctor && h.Dia == diaSemana)
                .Select(h => new { h.Horainicio, h.Horafin })
                .ToListAsync();

            List<string> horasDisponibles = new List<string>();

            foreach (var horario in horarios)
            {
                TimeSpan inicio = horario.Horainicio.TimeOfDay;
                TimeSpan fin = horario.Horafin.TimeOfDay;
                if (inicio < fin)
                {
                    for (TimeSpan hora = inicio; hora < fin; hora = hora.Add(TimeSpan.FromMinutes(60)))
                    {
                        horasDisponibles.Add(hora.ToString(@"hh\:mm"));
                    }
                }
                else
                {
                    for (TimeSpan hora = inicio; hora < TimeSpan.FromHours(24); hora = hora.Add(TimeSpan.FromMinutes(60)))
                    {
                        horasDisponibles.Add(hora.ToString(@"hh\:mm"));
                    }

                    for (TimeSpan hora = TimeSpan.Zero; hora < fin; hora = hora.Add(TimeSpan.FromMinutes(60)))
                    {
                        horasDisponibles.Add(hora.ToString(@"hh\:mm"));
                    }
                }
            }

            return Json(horasDisponibles);
        }


        public async Task<string> GenerateNextIDApp()
        {
            var app = await _context.Citas
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();
            var nextID = 0;
            var hospId = HttpContext.Session.GetString("IdHospital");
            if (app != null)
            {
                string lastID = app.Id;
                if (lastID.StartsWith($"{hospId}_APP-"))
                {
                    string number = lastID.Substring(9);
                    if (int.TryParse(number, out int lastNumber))
                    {
                        nextID = lastNumber + 1;
                    }
                }
            }

            string newId = $"{hospId}_APP-{nextID:D3}";
            return newId;
        }


        // GET: Citas/Create
        public async Task<IActionResult> Create()
        {
            ViewData["Especialidad"] = new SelectList(_context.Especialidades, "Id", "Nombre");
            return View();
        }

        // POST: Citas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Dia,Hora,Estado,CedulaPaciente,CedulaDoctor")] Cita cita)
        {
            try
            {
                cita.Id = await GenerateNextIDApp();
                var connectionString = _context.Database.GetDbConnection().ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("RegistrarCita", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@IdCita", cita.Id);
                        command.Parameters.AddWithValue("@Dia", cita.Dia);
                        command.Parameters.AddWithValue("@Hora", cita.Hora);
                        command.Parameters.AddWithValue("@CedulaPaciente", cita.CedulaPaciente);
                        command.Parameters.AddWithValue("@CedulaDoctor", cita.CedulaDoctor);
                        command.Parameters.AddWithValue("@Estado", cita.Estado);
                        await command.ExecuteNonQueryAsync();
                    }
                }

                if (HttpContext.Session.GetString("Rol") == "Admin")
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return RedirectToAction("PatientHome", "Home");
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error SQL: " + ex.Message);
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(cita); 
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error al registrar la cita.");
                return View(cita); 
            }
        }

        // GET: Citas/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var cita = await _context.Citas
                .Include(c => c.CedulaDoctorNavigation)
                    .ThenInclude(d => d.IdEspecialidadNavigation)
                .Include(c => c.CedulaPacienteNavigation)
                    .ThenInclude(p => p.CorreoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            var especialidadId = cita.CedulaDoctorNavigation.IdEspecialidad;
            var doctores = _context.Doctores
                .Where(d => d.IdEspecialidad == especialidadId)
                .Select(d => new { d.Cedula,
                    NombreCompleto = d.CorreoNavigation.Nombre + " " +
                         d.CorreoNavigation.PrimerApellido + " " +
                         d.CorreoNavigation.SegundoApellido
                })
                .ToList();

            ViewBag.CedulaDoctor = new SelectList(doctores, "Cedula", "NombreCompleto", cita.CedulaDoctor);
            ViewBag.CedulaPaciente = new SelectList(_context.Pacientes, "Cedula", "Nombre", cita.CedulaPaciente);
            ViewBag.Especialidad = new SelectList(_context.Especialidades, "Id", "Nombre", cita.CedulaDoctorNavigation?.IdEspecialidad);
            string diaString = cita.Dia.ToString("yyyy-MM-dd");
            var availableHoursResult = await GetHorasDisponibles(cita.CedulaDoctor, diaString);
            var availableHours = (availableHoursResult as JsonResult)?.Value as List<string> ?? new List<string>();
            ViewBag.AvailableHours = new SelectList(availableHours);
            return View(cita);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Dia,Hora,Estado,CedulaPaciente,CedulaDoctor")] Cita cita)
        {
            try
            {
                
                _context.Update(cita);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Citas.Any(e => e.Id == cita.Id))
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


        [HttpGet("Citas/Attend/{id}")]
        public IActionResult Attend(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            ViewBag.IdCita = id; 

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Cancelar(string id)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null)
            {
                return NotFound();
            }

            cita.Estado = "Cancelado";
            _context.Update(cita);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        public async Task<string> GenerateNextID()
        {
            var pagos = await _context.Pagos
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();
            var nextID = 0;
            if (pagos != null)
            {
                string lastID = pagos.Id;
                if (lastID.Contains("PAG"))
                {
                    string number = lastID.Substring(3);
                    if (int.TryParse(number, out int lastNumber))
                    {
                        nextID = lastNumber + 1;
                    }
                }
            }

            string newId = $"PAG{nextID:D3}";
            return newId;
        }


        [HttpPost]
        public async Task<IActionResult> Pagar(string id)
        {
            var cita = await _context.Citas.FindAsync(id);
            var tratamiento = await _context.Tratamientos
            .FirstOrDefaultAsync(t => t.IdCita == cita.Id);

            var pago = new Pago
            {
                Id = await GenerateNextID(),
                Fecha = DateOnly.FromDateTime(DateTime.Now), 
                Total = tratamiento.Precio,
                MetodoPago = "Tarjeta",
                Estado = "Pagado",
                CedulaPaciente = cita.CedulaPaciente,
                IdCita = cita.Id
            };

            _context.Add(pago);
            await _context.SaveChangesAsync();

            cita.Estado = "Pagada"; 
            _context.Update(cita);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool CitaExists(string id)
        {
            return _context.Citas.Any(e => e.Id == id);
        }
    }
}