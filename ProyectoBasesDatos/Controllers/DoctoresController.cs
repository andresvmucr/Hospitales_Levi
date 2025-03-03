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
        private readonly HospitalesDbContext _context;

        public DoctoresController(HospitalesDbContext context)
        {
            _context = context;
        }

        // GET: Doctores
        public async Task<IActionResult> Index()
        {
            var idHospital = HttpContext.Session.GetString("IdHospital");

            if (string.IsNullOrEmpty(idHospital))
            {
                return RedirectToAction("Error");
            }
            var doctores = await _context.Doctores
                .Include(d => d.CorreoNavigation)
                .Include(d => d.Horarios)
                .Where(d => d.CorreoNavigation.IdHospital == idHospital) 
                .Select(d => new
                {
                    Cedula = d.Cedula,
                    NombreCompleto = d.CorreoNavigation.Nombre + " " + d.CorreoNavigation.Primerapellido + " " + d.CorreoNavigation.Segundoapellido,
                    Especialidad = d.Especialidad,
                    Horarios = d.Horarios.Select(h => new
                    {
                        Dia = h.Dia,
                        HoraInicio = h.Horainicio.ToString("HH:mm"),
                        HoraFin = h.Horafin.ToString("HH:mm")
                    }).ToList(),
                    Correo = d.CorreoNavigation.Correo,
                    Telefono = d.CorreoNavigation.Telefono
                })
                .ToListAsync();

            return View(doctores);
        }

        // GET: Doctores/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Obtener el doctor incluyendo el usuario asociado y los horarios
            var doctore = await _context.Doctores
                .Include(d => d.CorreoNavigation) // Incluir el usuario asociado
                .Include(d => d.Horarios) // Incluir los horarios del doctor
                .FirstOrDefaultAsync(m => m.Cedula == id);

            if (doctore == null)
            {
                return NotFound();
            }

            return View(doctore);
        }

        // GET: Doctores/Create
        // GET: Doctores/Create
        public IActionResult Create()
        {
            var idHospital = HttpContext.Session.GetString("IdHospital");

            if (string.IsNullOrEmpty(idHospital))
            {
                return RedirectToAction("Error");
            }

            ViewBag.IdHospital = idHospital;
            return View();
        }

        // Reemplazar DateTime por TimeSpan en los lugares correspondientes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Cedula,Especialidad,Correo")] Doctore doctore,
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
                Primerapellido = PrimerApellido,
                Segundoapellido = SegundoApellido,
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
            var doctore = await _context.Doctores
                .Include(d => d.CorreoNavigation)
                .Include(d => d.Horarios) 
                .FirstOrDefaultAsync(d => d.Cedula == id);

            if (doctore == null)
            {
                return NotFound();
            }

            
            var horario = doctore.Horarios.FirstOrDefault();
            if (horario != null)
            {
                ViewBag.HoraInicio = horario.Horainicio.ToString("HH:mm");
                ViewBag.HoraFin = horario.Horafin.ToString("HH:mm");
            }
            else
            {
                ViewBag.HoraInicio = "08:00";
                ViewBag.HoraFin = "17:00";
            }

          
            var diasOrdenados = new List<string> { "L", "M", "K", "J", "V", "S", "D" };
            ViewBag.Dias = doctore.Horarios
                .Select(h => h.Dia.Trim().ToUpper()) 
                .OrderBy(d => diasOrdenados.IndexOf(d))
                .ToList();
            

            return View(doctore);
        }

        // POST: Doctores/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Cedula,Especialidad,Correo")] Doctore doctore,
                                     string[] Dias, TimeSpan HoraInicio, TimeSpan HoraFin, string Nombre, string PrimerApellido, string SegundoApellido, string Telefono)
        {

            Console.WriteLine("Nombre: " + Nombre);
            Console.WriteLine("PrimerApellido: " + PrimerApellido);
            Console.WriteLine("SegundoApellido: " + SegundoApellido);
            Console.WriteLine("Telefono: " + Telefono);
            // ToDo: si tiene una cita en el horario a eliminar cancelarla
            ModelState.Remove("CorreoNavigation");
            if (ModelState.IsValid)
            {
                try
                {
                    var usuario = await _context.Usuarios.FindAsync(doctore.Correo);
                    if (usuario != null)
                    {
                        usuario.Nombre = Nombre;
                        usuario.Primerapellido = PrimerApellido;
                        usuario.Segundoapellido = SegundoApellido;
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
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctoreExists(doctore.Cedula))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index)); 
            } else
            {
                var error = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                Console.WriteLine("Errores: " + string.Join(", ", error));
            }

                ViewData["Correo"] = new SelectList(_context.Usuarios, "Correo", "Correo", doctore.Correo);
            return View(doctore);
        }

 
        private bool DoctoreExists(string id)
        {
            return _context.Doctores.Any(e => e.Cedula == id);
        }
    }
}
