using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantePOS.Domain.Entities;
using RestaurantePOS.Infrastructure.Data;
using RestaurantePOS.Models;

namespace RestaurantePOS.Controllers;

public class OrdenesController : Controller
{
    private readonly RestauranteDbContext _context;

    public OrdenesController(RestauranteDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> DetalleOpcionesMesa(int mesaId)
    {
        var empleadoId = HttpContext.Session.GetInt32("EmpleadoID");
        if (!empleadoId.HasValue) return RedirectToAction("Index", "Home");

        try
        {
            var mesa = await _context.Mesas.FindAsync(mesaId);
            if (mesa == null) return NotFound("Mesa no encontrada.");

            if (mesa.Estado == "Libre")
            {
                return RedirectToAction("TomaPedido", new { mesaId = mesa.MesaId });
            }

            // Si está Ocupada o en Cuenta, buscar la orden activa
            var orden = await _context.Ordenes
                .Include(o => o.Detalles)
                .ThenInclude(d => d.Plato)
                .Where(o => o.MesaId == mesaId && o.Estado != "Pagada")
                .OrderByDescending(o => o.FechaHoraInicio)
                .FirstOrDefaultAsync();

            if (orden == null)
            {
                // Inconsistencia de estado, la forzamos a libre
                mesa.Estado = "Libre";
                await _context.SaveChangesAsync();
                return RedirectToAction("TomaPedido", new { mesaId = mesa.MesaId });
            }

            ViewBag.NumeroMesa = mesa.NumeroMesa;
            return View("OpcionesMesa", orden);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error de conexión: " + ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> TomaPedido(int mesaId)
    {
        // Validar sesión
        var empleadoId = HttpContext.Session.GetInt32("EmpleadoID");
        if (!empleadoId.HasValue) return RedirectToAction("Index", "Home");

        try
        {
            var mesa = await _context.Mesas.FindAsync(mesaId);
            if (mesa == null) return NotFound("Mesa no encontrada.");

            // Cargar platos activos (asumiendo que todos los listados están disponibles)
            // Se puede agregar filtro luego
            var platos = await _context.Platos
                .Include(p => p.Categoria)
                .ToListAsync();

            ViewBag.MesaId = mesaId;
            ViewBag.NumeroMesa = mesa.NumeroMesa;
            ViewBag.NombreMesero = HttpContext.Session.GetString("Nombres");

            return View(platos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error de conexión: " + ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CrearDesdePOS([FromBody] OrdenDto ordenDto)
    {
        // 1. Recuperar el Empleado Real desde la Sesión o buscar backup dinámico
        var empleadoId = HttpContext.Session.GetInt32("EmpleadoID");
        if (!empleadoId.HasValue)
        {
            var fallbackEmpleado = await _context.Empleados.FirstOrDefaultAsync(e => e.Activo);
            if (fallbackEmpleado == null)
            {
                return Unauthorized(new { success = false, message = "No hay sesión activa ni empleados de respaldo en la base de datos." });
            }
            empleadoId = fallbackEmpleado.EmpleadoId;
        }

        if (ordenDto == null || ordenDto.Detalles == null || !ordenDto.Detalles.Any())
        {
            return BadRequest(new { success = false, message = "La orden está vacía." });
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                var mesa = await _context.Mesas.FindAsync(ordenDto.MesaId);
                if (mesa == null) throw new InvalidOperationException("Mesa no válida.");

                // Crear la Orden usando el empleado recuperado
                var nuevaOrden = new Orden
                {
                    MesaId = ordenDto.MesaId,
                    EmpleadoId = empleadoId.Value,
                    FechaHoraInicio = DateTime.UtcNow,
                    Estado = "Pendiente", // u "Activa"
                    Total = ordenDto.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario),
                    Detalles = new List<DetalleOrden>()
                };

                foreach (var det in ordenDto.Detalles)
                {
                    if (det.Cantidad <= 0 || det.PrecioUnitario < 0) continue;

                    nuevaOrden.Detalles.Add(new DetalleOrden
                    {
                        PlatoId = det.PlatoId,
                        Cantidad = det.Cantidad,
                        PrecioUnitario = det.PrecioUnitario
                    });
                }

                _context.Ordenes.Add(nuevaOrden);

                // Actualizar estado de la mesa
                mesa.Estado = "Ocupada";
                _context.Mesas.Update(mesa);

                try
                {
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    await transaction.RollbackAsync();

                    // Extraer el mensaje interno específico de Supabase/PostgreSQL
                    var errorMessage = dbEx.InnerException != null
                        ? dbEx.InnerException.Message
                        : dbEx.Message;

                    throw new Exception($"Fallo al guardar en BD: {errorMessage}", dbEx);
                }
            });

            return Json(new { success = true, redirectUrl = Url.Action("Checkout", new { mesaId = ordenDto.MesaId }) });
        }
        catch (InvalidOperationException ioe)
        {
            return BadRequest(new { success = false, message = ioe.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Error interno: " + ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Checkout(int mesaId, int? ordenId = null)
    {
        var empleadoId = HttpContext.Session.GetInt32("EmpleadoID");
        if (!empleadoId.HasValue) return RedirectToAction("Index", "Home");

        try
        {
            Orden? orden;

            if (ordenId.HasValue)
            {
                orden = await _context.Ordenes
                    .Include(o => o.Mesa)
                    .Include(o => o.Detalles)
                    .ThenInclude(d => d.Plato)
                    .FirstOrDefaultAsync(o => o.OrdenId == ordenId.Value);

                ViewBag.PagoRegistrado = true;
            }
            else
            {
                orden = await _context.Ordenes
                    .Include(o => o.Mesa)
                    .Include(o => o.Detalles)
                    .ThenInclude(d => d.Plato)
                    .Where(o => o.MesaId == mesaId && o.Estado != "Pagada")
                    .OrderByDescending(o => o.FechaHoraInicio)
                    .FirstOrDefaultAsync();

                if (orden != null && orden.Mesa.Estado == "Ocupada")
                {
                    // Opcional: Cambiar estado a 'Cuenta' si venía de 'Ocupada'
                    orden.Mesa.Estado = "Cuenta";
                    await _context.SaveChangesAsync();
                }
            }

            if (orden == null)
            {
                // Si no hay orden, volver a mesas
                return RedirectToAction("Index", "Mesas");
            }

            return View(orden);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error: " + ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> PagarYLiberarMesa(int ordenId)
    {
        var empleadoId = HttpContext.Session.GetInt32("EmpleadoID");
        if (!empleadoId.HasValue) return RedirectToAction("Index", "Home");

        var strategy = _context.Database.CreateExecutionStrategy();
        int? ordenProcesadaId = null;
        int? mesaProcesadaId = null;

        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                var orden = await _context.Ordenes
                    .Include(o => o.Mesa)
                    .FirstOrDefaultAsync(o => o.OrdenId == ordenId);

                if (orden == null) throw new InvalidOperationException("Orden no encontrada.");

                ordenProcesadaId = orden.OrdenId;
                mesaProcesadaId = orden.MesaId;

                orden.Estado = "Pagada";
                orden.FechaHoraCierre = DateTime.UtcNow;

                if (orden.Mesa != null)
                {
                    orden.Mesa.Estado = "Libre";
                }

                // Aquí se podría guardar también la transacción de pago en la tabla TransaccionesPago si es necesario
                var pago = new TransaccionPago
                {
                    OrdenId = orden.OrdenId,
                    MetodoPago = "Efectivo", // Hardcoded para simplificar, idealmente viene de la UI
                    Monto = orden.Total,
                    FechaPago = DateTime.UtcNow
                };
                _context.TransaccionesPago.Add(pago);

                try
                {
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (DbUpdateException)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            return RedirectToAction("Checkout", new { mesaId = mesaProcesadaId ?? 0, ordenId = ordenProcesadaId });
        }
        catch (InvalidOperationException ioe)
        {
            return NotFound(ioe.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error procesando el pago: " + ex.Message);
        }

    }
}