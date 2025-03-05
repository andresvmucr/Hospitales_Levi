using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProyectoBasesDatos.Models;

namespace ProyectoBasesDatos.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult SuperAdminHome()
    {
        return View();
    }
    public IActionResult AdminHome()
    {
        return View();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}