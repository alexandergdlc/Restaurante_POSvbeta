using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantePOS.Infrastructure.Data;
using RestaurantePOS.Domain.Entities;

namespace RestaurantePOS.Controllers;

public class MesasController : Controller
{
    private readonly RestauranteDbContext _context;

    public MesasController(RestauranteDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Validar sesión
        var empleadoId = HttpContext.Session.GetInt32("EmpleadoID");
        if (!empleadoId.HasValue)
        {
            return RedirectToAction("Index", "Home");
        }

        try
        {
            var mesas = await _context.Mesas
                                      .OrderBy(m => m.NumeroMesa)
                                      .ToListAsync();

            ViewBag.NombreMesero = HttpContext.Session.GetString("Nombres");

            return View(mesas);
        }
        catch (Exception ex)
        {
            // Manejo de errores de Supabase
            // Redirigir a error o mostrar mensaje
            return View("Error", ex); // Asume que existe vista de error
        }
    }
}