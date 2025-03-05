using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoBasesDatos.Models;

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
            var dbContext = _context.Citas.Include(c => c.CedulaDoctorNavigation).Include(c => c.CedulaPacienteNavigation);
            return View(await dbContext.ToListAsync());
        }

        // GET: Citas/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Obtener la cita con la información relacionada del doctor y el paciente
            var cita = await _context.Citas
                .Include(c => c.CedulaDoctorNavigation) // Incluir la información del doctor
                    .ThenInclude(d => d.CorreoNavigation) // Incluir la información del usuario (correo)
                .Include(c => c.CedulaDoctorNavigation) // Incluir la información del doctor
                    .ThenInclude(d => d.IdEspecialidadNavigation) // Incluir la información de la especialidad
                .Include(c => c.CedulaPacienteNavigation) // Incluir la información del paciente
                    .ThenInclude(p => p.CorreoNavigation) // Incluir la información del usuario (correo)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cita == null)
            {
                return NotFound();
            }

            return View(cita);
        }

        public async Task<IActionResult> GetDoctoresByEspecialidad(string especialidad)
        {
            Console.WriteLine("Especialidad: " + especialidad);
            // Obtener el IdHospital de la sesión
            var hospitalId = HttpContext.Session.GetString("IdHospital");

            // Validar que el IdHospital no sea nulo o vacío
            if (string.IsNullOrEmpty(hospitalId))
            {
                return Json(new { error = "Hospital no seleccionado o sesión inválida." });
            }

            // Filtrar doctores por especialidad y hospital
            var doctores = await _context.Doctores
                .Where(d => d.IdEspecialidad == especialidad && d.CorreoNavigation.IdHospital == hospitalId)
                .Select(d => new { d.CorreoNavigation.Nombre, d.Cedula }) // Seleccionar los campos que necesitas
                .ToListAsync();

            return Json(doctores);
        }

        public async Task<IActionResult> GetDiasTrabajoDoctor(string idDoctor)
        {
            Console.WriteLine("idDoctor: " + idDoctor);
            // Obtener los días de trabajo del doctor desde la base de datos
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


            foreach (var dia in diasTrabajo)
            {
                Console.WriteLine($"Inicial: {dia.Dia} - Día completo: {dia.DiaCompleto}");
            }
            // Devolver los días de trabajo en formato JSON
            return Json(diasTrabajo);
        }

        public async Task<IActionResult> GetHorasDisponibles(string idDoctor, string dia)
        {
            Console.WriteLine($"Doctor: {idDoctor}, Día: {dia}");

            DateTime fecha = DateTime.Parse(dia);
            string[] diasSemana = { "D", "L", "K", "M", "J", "V", "S" };
            string diaSemana = diasSemana[(int)fecha.DayOfWeek];

            Console.WriteLine("DIA: " + diaSemana);

            var horarios = await _context.Horarios
                .Where(h => h.CedulaDoctor == idDoctor && h.Dia == diaSemana)
                .Select(h => new { h.Horainicio, h.Horafin })
                .ToListAsync();

            List<string> horasDisponibles = new List<string>();

            foreach (var horario in horarios)
            {
                TimeSpan inicio = horario.Horainicio.TimeOfDay;
                TimeSpan fin = horario.Horafin.TimeOfDay;

                Console.WriteLine("Inicio: " + inicio);
                Console.WriteLine("Fin: " + fin);

                // Si el horario no cruza la medianoche
                if (inicio < fin)
                {
                    for (TimeSpan hora = inicio; hora < fin; hora = hora.Add(TimeSpan.FromMinutes(60)))
                    {
                        Console.WriteLine("Hora: " + hora);
                        horasDisponibles.Add(hora.ToString(@"hh\:mm"));
                    }
                }
                else // Si el horario cruza la medianoche
                {
                    // Desde inicio hasta 23:59
                    for (TimeSpan hora = inicio; hora < TimeSpan.FromHours(24); hora = hora.Add(TimeSpan.FromMinutes(60)))
                    {
                        Console.WriteLine("Hora: " + hora);
                        horasDisponibles.Add(hora.ToString(@"hh\:mm"));
                    }

                    // Desde 00:00 hasta fin
                    for (TimeSpan hora = TimeSpan.Zero; hora < fin; hora = hora.Add(TimeSpan.FromMinutes(60)))
                    {
                        Console.WriteLine("Hora: " + hora);
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
            Console.WriteLine("NEW ID:" + newId);
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
            Console.WriteLine("DIA: " + cita.Dia);
            Console.WriteLine("Hora: " + cita.Hora);
            Console.WriteLine("CedulaPaciente: " + cita.CedulaPaciente);
            Console.WriteLine("CedulaDoctor: " + cita.CedulaDoctor);


            cita.Id = await GenerateNextIDApp();
            _context.Add(cita);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Citas/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            Console.WriteLine("Id: " + id);
            if (id == null)
            {
                return NotFound();
            }

            // Obtain the appointment with related doctor and patient information
            var cita = await _context.Citas
                .Include(c => c.CedulaDoctorNavigation)
                    .ThenInclude(d => d.IdEspecialidadNavigation)
                .Include(c => c.CedulaPacienteNavigation)
                    .ThenInclude(p => p.CorreoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            // Get the specialization ID of the doctor associated with the selected appointment
            var especialidadId = cita.CedulaDoctorNavigation.IdEspecialidad;

            // Fetch doctors based on the specialization ID
            var doctores = _context.Doctores
                .Where(d => d.IdEspecialidad == especialidadId)
                .Select(d => new { d.Cedula, d.CorreoNavigation.Nombre })
                .ToList();

            // Create SelectLists
            ViewBag.CedulaDoctor = new SelectList(doctores, "Cedula", "Nombre", cita.CedulaDoctor);
            ViewBag.CedulaPaciente = new SelectList(_context.Pacientes, "Cedula", "Nombre", cita.CedulaPaciente);
            ViewBag.Especialidad = new SelectList(_context.Especialidades, "Id", "Nombre", cita.CedulaDoctorNavigation?.IdEspecialidad);

            // Convert DateOnly to string
            string diaString = cita.Dia.ToString("yyyy-MM-dd");

            // Await the GetHorasDisponibles method and extract the hours
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


        private bool CitaExists(string id)
        {
            return _context.Citas.Any(e => e.Id == id);
        }
    }
}