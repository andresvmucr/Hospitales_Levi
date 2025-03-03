using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoBasesDatos.Models;
using System.Diagnostics;
using System.Security.Cryptography;

namespace ProyectoBasesDatos.Controllers
{
    public class AuthController : Controller
    {

        private readonly ILogger<AuthController> _logger;
        private readonly HospitalesDbContext _context;

        public AuthController(ILogger<AuthController> logger, HospitalesDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: AuthController
        public ActionResult Login()
        {


            return View();
        }

        public ActionResult Logout()
        {
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Correo, string Contrasenna)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == Correo && u.Contrasenna == Contrasenna);
            if (user == null)
            {
                ViewData["Error"] = "Los credenciales son incorrectos, intente nuevamente";
                return View("Login");
            }
            else
            {
                Console.WriteLine("Usuario encontrado");
                var userRole = await _context.Usuarios
                    .Where(u => u.Correo == user.Correo)
                    .Select(u => u.Rol)
                    .FirstOrDefaultAsync();

                var hospitalName = await _context.Hospitales
                    .Where(h => h.Id == user.IdHospital)
                    .Select(h => h.Nombre)
                    .FirstOrDefaultAsync();

                HttpContext.Session.SetString("Correo", user.Correo);
                Console.WriteLine("Correo: " + Correo);
                
                HttpContext.Session.SetString("Rol", userRole);
                Console.WriteLine("User role: " + userRole);

                HttpContext.Session.SetString("Nombre", user.Nombre);
                HttpContext.Session.SetString("PrimerApellido", user.Primerapellido);
                HttpContext.Session.SetString("SegundoApellido", user.Segundoapellido);
                Console.WriteLine("Nombre: " + user.Nombre + " " + user.Primerapellido + " " + user.Segundoapellido);

                HttpContext.Session.SetString("Telefono", user.Telefono);
                Console.WriteLine("Telefono: " + user.Telefono);

                HttpContext.Session.SetString("IdHospital", user.IdHospital);
                Console.WriteLine("IdHospital: " + user.IdHospital);

                HttpContext.Session.SetString("HospitalName", hospitalName);
                Console.WriteLine("Hospital name: " + hospitalName);


                switch (userRole)
                {
                    case "Admin": 
                        return RedirectToAction("AdminHome", "Home");
                    case "Doctor":
                        var doctorId = await _context.Doctores
                            .Where(d => d.Correo == user.Correo)
                            .Select(d => d.Cedula)
                            .FirstOrDefaultAsync();

                        HttpContext.Session.SetString("DoctorId", doctorId);
                        Console.WriteLine("DoctorId: " + doctorId);
                        return RedirectToAction("DoctorHome", "Home");

                    case "Paciente":
                        var patientId = await _context.Pacientes
                            .Where(p => p.Correo == user.Correo)
                            .Select(p => p.Cedula)
                            .FirstOrDefaultAsync();

                        HttpContext.Session.SetString("PatientId", patientId);
                        Console.WriteLine("PatientId: " + patientId);
                        return RedirectToAction("PatientHome", "Home");
                    default:
                        return RedirectToAction("Login");
                };
            }
        }

    }
}
