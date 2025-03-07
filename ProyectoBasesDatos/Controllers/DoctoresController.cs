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
    public class DoctoresController : Controller
    {
        private readonly dbContext _context;

        public DoctoresController(dbContext context)
        {
            _context = context;
        }

        // GET: Doctores
        public async Task<IActionResult> Index(string searchString)
        {
            //var idHospital = "H001"; // Obtén el ID del hospital desde la sesión o donde corresponda
            var idHospital = HttpContext.Session.GetString("IdHospital");
            Console.WriteLine($"idHospital: {idHospital}");

            // Consulta base
            var doctoresQuery = _context.Doctores
                .Include(d => d.CorreoNavigation)
                .Include(d => d.Horarios)
                .Include(d => d.IdEspecialidadNavigation)
                .Where(d => d.CorreoNavigation.IdHospital == idHospital);

            // Filtrar por nombre o especialidad si se proporciona un término de búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                doctoresQuery = doctoresQuery.Where(d =>
                    d.CorreoNavigation.Nombre.Contains(searchString) ||
                    d.CorreoNavigation.PrimerApellido.Contains(searchString) ||
                    d.CorreoNavigation.SegundoApellido.Contains(searchString) ||
                    d.IdEspecialidadNavigation.Nombre.Contains(searchString));
            }

            // Proyectar los resultados
            var doctores = await doctoresQuery
                .Select(d => new
                {
                    Cedula = d.Cedula,
                    NombreCompleto = d.CorreoNavigation.Nombre + " " + d.CorreoNavigation.PrimerApellido + " " + d.CorreoNavigation.SegundoApellido,
                    Especialidad = d.IdEspecialidadNavigation.Nombre,
                    Horarios = d.Horarios
                        .OrderBy(h => h.Dia == "L" ? 1
                                   : h.Dia == "M" ? 2
                                   : h.Dia == "K" ? 3
                                   : h.Dia == "J" ? 4
                                   : h.Dia == "V" ? 5
                                   : h.Dia == "S" ? 6
                                   : 7) // D (domingo)
                        .Select(h => new
                        {
                            Dia = h.Dia,
                            HoraInicio = h.Horainicio.ToString("HH:mm"),
                            HoraFin = h.Horafin.ToString("HH:mm")
                        }).ToList(),
                    Correo = d.CorreoNavigation.Correo,
                    Telefono = d.CorreoNavigation.Telefono
                })
                .ToListAsync();


            Console.WriteLine($"Doctores encontrados: {doctores.Count}");
            ViewBag.Doctores = doctores.Cast<dynamic>().ToList();
            ViewBag.CurrentFilter = searchString; // Pasar el término de búsqueda a la vista
            return View();
        }

        // GET: Doctores/Details/5
        // GET: Doctores/Details/5
        // GET: Doctores/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctores
                .Include(d => d.CorreoNavigation)
                .Include(d => d.Horarios)
                .Include(d => d.IdEspecialidadNavigation)
                .FirstOrDefaultAsync(d => d.Cedula == id);

            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor); // Pasamos el objeto completo, no un anónimo
        }


        // GET: Doctores/Create
        public IActionResult Create()
        {
            var idHospital = HttpContext.Session.GetString("IdHospital");
            var especialidades = _context.Especialidades   
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(), // Valor que se enviará (ID)
                    Text = e.Nombre // Texto que se mostrará (Nombre)
                })
                .ToList();

            // Verificar si hay especialidades
            if (!especialidades.Any())
            {
                ViewBag.NoEspecialidades = true; // Indicar que no hay especialidades
            }
            else
            {
                ViewBag.Especialidades = especialidades; // Pasar la lista a la vista
            }

            ViewBag.IdHospital = idHospital;
            return View();
        }

        // Reemplazar DateTime por TimeSpan en los lugares correspondientes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Cedula,IdEspecialidad,Correo")] Doctore doctore,
                                          string Nombre,
                                          string PrimerApellido,
                                          string SegundoApellido,
                                          string Telefono,
                                          string IdHospital,
                                          string[] Dias,
                                          TimeSpan HoraInicio,
                                          TimeSpan HoraFin)
        {
            var usuario = new Usuario
            {
                Correo = doctore.Correo,
                Nombre = Nombre,
                PrimerApellido = PrimerApellido,
                SegundoApellido = SegundoApellido,
                Contrasenna = "12345678",
                Rol = "Doctor",
                Telefono = Telefono,
                IdHospital = IdHospital
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            doctore.CorreoNavigation = usuario;
            _context.Doctores.Add(doctore);
            await _context.SaveChangesAsync();

            foreach (var dia in Dias)
            {
                var horario = new Horario
                {
                    Dia = dia,
                    Horainicio = DateTime.Today.Add(HoraInicio),
                    Horafin = DateTime.Today.Add(HoraFin),
                    CedulaDoctor = doctore.Cedula
                };
                _context.Horarios.Add(horario);
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // GET: Doctores/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Cargar el doctor con todos los datos relacionados
            var doctore = await _context.Doctores
                .Include(d => d.CorreoNavigation)  // Cargar los datos del usuario
                .Include(d => d.Horarios)          // Cargar los horarios
                .Include(d => d.IdEspecialidadNavigation) // Cargar la especialidad actual
                .FirstOrDefaultAsync(d => d.Cedula == id);

            if (doctore == null)
            {
                return NotFound();
            }

            // Cargar las especialidades disponibles
            var especialidades = await _context.Especialidades.ToListAsync();

            // Crear una lista de SelectListItem para las especialidades
            var especialidadesSelectList = especialidades
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Nombre,
                    Selected = e.Id == doctore.IdEspecialidad // Marcar la especialidad actual como seleccionada
                })
                .ToList();

            // Verificar si hay especialidades registradas
            ViewBag.NoEspecialidades = especialidades.Count == 0;

            // Pasar las especialidades a la vista
            ViewBag.Especialidades = especialidadesSelectList;

            // Pasar los datos del doctor a la vista
            ViewData["Correo"] = doctore.Correo;
            ViewData["Nombre"] = doctore.CorreoNavigation?.Nombre;
            ViewData["PrimerApellido"] = doctore.CorreoNavigation?.PrimerApellido;
            ViewData["SegundoApellido"] = doctore.CorreoNavigation?.SegundoApellido;
            ViewData["Telefono"] = doctore.CorreoNavigation?.Telefono;
            ViewData["IdHospital"] = doctore.IdEspecialidadNavigation;

            // Pasar los horarios y días de trabajo
            if (doctore.Horarios != null)
            {
                ViewData["HoraInicio"] = doctore.Horarios.FirstOrDefault()?.Horainicio.ToString("HH:mm");
                ViewData["HoraFin"] = doctore.Horarios.FirstOrDefault()?.Horafin.ToString("HH:mm");
                ViewData["Dias"] = doctore.Horarios.Select(h => h.Dia).ToList();
            }

            return View(doctore);
        }
        // POST: Doctores/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Cedula,IdEspecialidad,Correo")] Doctore doctore,
                                     string[] Dias, TimeSpan HoraInicio, TimeSpan HoraFin, string Nombre, string PrimerApellido, string SegundoApellido, string Telefono)
        {

            Console.WriteLine("Nombre: " + Nombre);
            Console.WriteLine("PrimerApellido: " + PrimerApellido);
            Console.WriteLine("SegundoApellido: " + SegundoApellido);
            Console.WriteLine("Telefono: " + Telefono);
            Console.WriteLine("Dias: " + string.Join(", ", Dias));
            Console.WriteLine("HoraInicio: " + HoraInicio);
            Console.WriteLine("HoraFin: " + HoraFin);
            Console.WriteLine("Especialidad: " + doctore.IdEspecialidad);

            var usuario = await _context.Usuarios.FindAsync(doctore.Correo);
            if (usuario != null)
            {
                usuario.Nombre = Nombre;
                usuario.PrimerApellido = PrimerApellido;
                usuario.SegundoApellido = SegundoApellido;
                usuario.Telefono = Telefono;

                _context.Update(usuario);
            }

            _context.Update(doctore);

            var horariosExistentes = await _context.Horarios.Where(h => h.CedulaDoctor == id).ToListAsync();
            _context.Horarios.RemoveRange(horariosExistentes);


            foreach (var dia in Dias)
            {
                var horario = new Horario
                {
                    Dia = dia,
                    Horainicio = DateTime.Today.Add(HoraInicio),
                    Horafin = DateTime.Today.Add(HoraFin),
                    CedulaDoctor = doctore.Cedula
                };
                _context.Horarios.Add(horario);
            }

            await _context.SaveChangesAsync();
            // ToDo: si tiene citas cambair el horario de las citas
            return RedirectToAction(nameof(Index));
        }

        private bool DoctoreExists(string id)
        {
            return _context.Doctores.Any(e => e.Cedula == id);
        }
    }
}
