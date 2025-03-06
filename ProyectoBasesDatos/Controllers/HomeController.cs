using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoBasesDatos.Models;

namespace ProyectoBasesDatos.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly dbContext _context;
    public HomeController(ILogger<HomeController> logger,dbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult SuperAdminHome()
    {
        return View();
    }
    public IActionResult AdminHome()
    {
        return View();
    }

    public async Task<IActionResult> DoctorHome()
    {
        var hospitalId = "H001";


        if (string.IsNullOrEmpty(hospitalId))
        {
            return RedirectToAction("Error", "Home"); // Redirigir a una página de error si no hay hospital seleccionado
        }

        // Filtrar citas por el hospital y cargar la información relacionada
        var citas = await _context.Citas
            .Include(c => c.CedulaDoctorNavigation)
                .ThenInclude(d => d.CorreoNavigation) // Incluir el usuario (nombre del doctor)
            .Include(c => c.CedulaPacienteNavigation)
                .ThenInclude(p => p.CorreoNavigation) // Incluir el usuario (nombre del paciente)
            .Where(c => c.CedulaDoctorNavigation.CorreoNavigation.IdHospital == hospitalId) // Filtrar por hospital
            .ToListAsync();

        return View(citas);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}