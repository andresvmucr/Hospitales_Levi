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
        var doctorId = HttpContext.Session.GetString("DoctorId");
        var citas = await _context.Citas
            .Include(c => c.CedulaDoctorNavigation)
                .ThenInclude(d => d.CorreoNavigation)
            .Include(c => c.CedulaPacienteNavigation)
                .ThenInclude(p => p.CorreoNavigation)
            .Where(c => c.CedulaDoctor == doctorId)
            .ToListAsync();

        return View(citas);
    }

    public async Task<IActionResult> PatientHome()
    {
        var patientId = HttpContext.Session.GetString("PatientId");
        var citas = await _context.Citas
            .Include(c => c.CedulaDoctorNavigation)
                .ThenInclude(d => d.CorreoNavigation)
            .Include(c => c.CedulaPacienteNavigation)
                .ThenInclude(p => p.CorreoNavigation)
            .Where(c => c.CedulaPaciente == patientId)
            .OrderBy(c => c.Estado == "Cancelado" ? 1 : 0)
            .ThenBy(c => c.Hora)
            .ThenBy(c => c.Dia)
            
            .ToListAsync();

        return View(citas);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}