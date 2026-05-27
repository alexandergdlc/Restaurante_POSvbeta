using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantePOS.Models;
using RestaurantePOS.Infrastructure.Data;
using RestaurantePOS.Domain.Entities;

namespace RestaurantePOS.Controllers;

public class HomeController : Controller
{
    private readonly RestauranteDbContext _context;

    public HomeController(RestauranteDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        // Si ya está logueado, redirigir a mesas
        if (HttpContext.Session.GetInt32("EmpleadoID").HasValue)
        {
            return RedirectToAction("Index", "Mesas");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string pin)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(pin))
            {
                TempData["Error"] = "Por favor, ingresa un PIN.";
                return View("Index");
            }

            var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.PinAcceso == pin);

            if (empleado != null)
            {
                // Iniciar sesión
                HttpContext.Session.SetInt32("EmpleadoID", empleado.EmpleadoId);
                HttpContext.Session.SetString("Nombres", empleado.Nombres);
                HttpContext.Session.SetString("Rol", empleado.Rol ?? "");

                return RedirectToAction("Index", "Mesas");
            }

            TempData["Error"] = "PIN incorrecto.";
            return View("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Error de conexión con la base de datos: " + ex.Message;
            return View("Index");
        }
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
