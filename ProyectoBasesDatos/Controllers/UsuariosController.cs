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
    public class UsuariosController : Controller
    {
        private readonly dbContext _context;

        public UsuariosController(dbContext context)
        {
            _context = context;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index(string searchString)
        {
            var usuariosQuery = _context.Usuarios
                .Include(u => u.IdHospitalNavigation)
                .Where(u => u.Rol == "Admin"); // Filtrar solo administradores

            // Filtrar por nombre o correo si se proporciona un término de búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                usuariosQuery = usuariosQuery.Where(u =>
                    u.Nombre.Contains(searchString) ||
                    u.PrimerApellido.Contains(searchString) ||
                    u.SegundoApellido.Contains(searchString) ||
                    u.Correo.Contains(searchString));
            }

            var usuarios = await usuariosQuery.ToListAsync();
            ViewBag.CurrentFilter = searchString; // Pasar el término de búsqueda a la vista
            return View(usuarios);
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.IdHospitalNavigation)
                .FirstOrDefaultAsync(m => m.Correo == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            ViewData["Hospitales"] = new SelectList(_context.Hospitales, "Id", "Nombre");
            return View();
        }

        // POST: Usuarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Correo,Nombre,PrimerApellido,SegundoApellido,Telefono,IdHospital")] Usuario usuario)
        {

            // Asignar valores predeterminados para Rol y Contrasenna
            usuario.Rol = "Admin"; // O el valor que desees por defecto
            usuario.Contrasenna = "Password"; // O genera una contraseña temporal

            _context.Add(usuario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.IdHospitalNavigation)
                .FirstOrDefaultAsync(m => m.Correo == id);
            if (usuario == null)
            {
                return NotFound();
            }

            // Cargar la lista de hospitales en el ViewBag
            ViewBag.Hospitales = new SelectList(_context.Hospitales, "Id", "Nombre");

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Correo,Nombre,PrimerApellido,SegundoApellido,Telefono,IdHospital")] Usuario usuario)
        {

                try
                {
                    // Obtener el usuario actual de la base de datos
                    var usuarioActual = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Correo == id);

                    if (usuarioActual == null)
                    {
                        return NotFound();
                    }

                    // Asignar los valores actuales de Contrasenna y Rol al objeto usuario
                    usuario.Contrasenna = usuarioActual.Contrasenna;
                    usuario.Rol = usuarioActual.Rol;

                    // Actualizar el usuario en la base de datos
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.Correo))
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

        private bool UsuarioExists(string id)
        {
            return _context.Usuarios.Any(e => e.Correo == id);
        }
    }
}
