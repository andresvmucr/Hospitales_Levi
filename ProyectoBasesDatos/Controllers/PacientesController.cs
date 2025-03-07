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
    public class PacientesController : Controller
    {
        private readonly dbContext _context;

        public PacientesController(dbContext context)
        {
            _context = context;
        }

        // GET: Pacientes
        public async Task<IActionResult> Index()
        {
            // var idHospital = "H001";
            var idHospital = HttpContext.Session.GetString("IdHospital");

            if (string.IsNullOrEmpty(idHospital))
            {
                return RedirectToAction("Error");
            }

            // Cargar los pacientes con sus datos relacionados
            var pacientes = await _context.Pacientes
                .Include(p => p.CorreoNavigation) // Incluir el usuario relacionado
                .Where(p => p.CorreoNavigation.IdHospital == idHospital)
                .ToListAsync();

            return View(pacientes);
        }

        // GET: Pacientes/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Cargar el paciente incluyendo el usuario relacionado
            var paciente = await _context.Pacientes
                .Include(p => p.CorreoNavigation) // Incluir el usuario relacionado
                .FirstOrDefaultAsync(m => m.Cedula == id);

            if (paciente == null)
            {
                return NotFound();
            }

            return View(paciente);
        }

        // GET: Pacientes/Create
        public IActionResult Create()
        {
            ViewData["Correo"] = new SelectList(_context.Usuarios, "Correo", "Correo");
            return View();
        }

        // POST: Pacientes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Cedula,Direccion,Genero,FechaNacimiento,Correo")] Paciente paciente,
                                         string Nombre,
                                         string PrimerApellido,
                                         string SegundoApellido,
                                         string Telefono)
        {
            Console.WriteLine("Cedula: " + paciente.Cedula);
            Console.WriteLine("Direccion: " + paciente.Direccion);
            Console.WriteLine("Correo: " + paciente.Correo);
            Console.WriteLine("Genero: " + paciente.Genero);
            Console.WriteLine("FechaNacimiento: " + paciente.FechaNacimiento);
            Console.WriteLine("Nombre: " + Nombre);
            Console.WriteLine("PrimerApellido: " + PrimerApellido);
            Console.WriteLine("SegundoApellido: " + SegundoApellido);
            Console.WriteLine("Telefono: " + Telefono);

            // Initialize CorreoNavigation if it is null
            if (paciente.CorreoNavigation == null)
            {
                paciente.CorreoNavigation = new Usuario();
            }

            paciente.CorreoNavigation.Correo = paciente.Correo;
            paciente.CorreoNavigation.Nombre = Nombre;
            paciente.CorreoNavigation.PrimerApellido = PrimerApellido;
            paciente.CorreoNavigation.SegundoApellido = SegundoApellido;
            paciente.CorreoNavigation.Telefono = Telefono;
            paciente.CorreoNavigation.Contrasenna = "1234"; // Default password
            paciente.CorreoNavigation.Rol = "Paciente";
            // paciente.CorreoNavigation.IdHospital = "H001";
            paciente.CorreoNavigation.IdHospital = HttpContext.Session.GetString("IdHospital");

            _context.Usuarios.Add(paciente.CorreoNavigation);
            await _context.SaveChangesAsync();

            _context.Add(paciente);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Pacientes/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paciente = await _context.Pacientes
                .Include(p => p.CorreoNavigation)
                .FirstOrDefaultAsync(m => m.Cedula == id);

            if (paciente == null)
            {
                return NotFound();
            }

            // Asegurar que CorreoNavigation no sea nulo
            if (paciente.CorreoNavigation == null)
            {
                paciente.CorreoNavigation = new Usuario();
            }

            return View(paciente);
        }

        // POST: Pacientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Cedula,Direccion,Genero,Fechanacimiento,Correo, CorreoNavigation")] Paciente paciente,
                                                     string Nombre, string PrimerApellido, string SegundoApellido, string Telefono)
        {
            Console.WriteLine("Nombre: " + Nombre);
            Console.WriteLine("PrimerApellido: " + PrimerApellido);
            Console.WriteLine("SegundoApellido: " + SegundoApellido);
            Console.WriteLine("Telefono: " + Telefono);


            if (id != paciente.Cedula)
            {
                return NotFound();
            }
            ModelState.Remove("CorreoNavigation");
            if (ModelState.IsValid)
            {
                try
                {
                    var usuario = await _context.Usuarios.FindAsync(paciente.Correo);
                    if (usuario != null)
                    {
                        usuario.Nombre = Nombre;
                        usuario.PrimerApellido = PrimerApellido;
                        usuario.SegundoApellido = SegundoApellido;
                        usuario.Telefono = Telefono;

                        _context.Update(usuario);
                    }

                    _context.Update(paciente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PacienteExists(paciente.Cedula))
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
            else
            {
                var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                Console.WriteLine("Errores: " + string.Join(", ", errores));
            }

            return View(paciente);
        }

        private bool PacienteExists(string id)
        {
            return _context.Pacientes.Any(e => e.Cedula == id);
        }
    }
}