using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using RestaurantePOS.Controllers;
using RestaurantePOS.Domain.Entities;
using RestaurantePOS.Infrastructure;
using RestaurantePOS.Infrastructure.Data;
using RestaurantePOS.Models;
using Xunit;

namespace RestaurantePOS.Tests
{
    public class OrdenesControllerTests
    {
        private RestauranteDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<RestauranteDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var dbContext = new RestauranteDbContext(options);

            // Seed reference data
            dbContext.Mesas.Add(new Mesa { MesaId = 1, NumeroMesa = 1, Capacidad = 4, Estado = "Libre" });
            dbContext.Empleados.Add(new Empleado { EmpleadoId = 1, Nombres = "Test", Apellidos="Test2", PinAcceso="1234", Rol = "Mesero", Activo = true });

            var category = new CategoriaPlato { CategoriaId = 1, Nombre = "Principal" };
            dbContext.CategoriasPlato.Add(category);

            dbContext.Platos.Add(new Plato 
            { 
                PlatoId = 1, 
                Nombre = "Steak", 
                Precio = 25.00m, 
                CategoriaId = 1,
                Activo = true
            });
            dbContext.Platos.Add(new Plato 
            { 
                PlatoId = 2, 
                Nombre = "Agua", 
                Precio = 5.00m, 
                CategoriaId = 1,
                Activo = true
            });

            dbContext.SaveChanges();

            return dbContext;
        }

        private OrdenesController GetOrdenesController(RestauranteDbContext dbContext)
        {
            var controller = new OrdenesController(dbContext);

            var sessionMock = new Mock<ISession>();

            // Mock session to return an integer ID
            byte[] empleadoIdBytes = BitConverter.GetBytes(1);
            sessionMock.Setup(s => s.TryGetValue("EmpleadoID", out empleadoIdBytes)).Returns(true);

            var httpContext = new DefaultHttpContext
            {
                Session = sessionMock.Object
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        [Fact]
        public async Task CrearDesdePOS_ConOrdenVacia_DebeRetornarBadRequest()
        {
            // Arrange
            var dbContext = GetDbContext();
            var controller = GetOrdenesController(dbContext);
            var ordenDto = new OrdenDto { MesaId = 1, Detalles = new List<DetalleOrdenDto>() };

            // Act
            var result = await controller.CrearDesdePOS(ordenDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("La orden está vacía", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task CrearDesdePOS_OrdenValida_DebeCrearOrdenYCambiarMesaAOcupada()
        {
            // Arrange
            var dbContext = GetDbContext();
            var controller = GetOrdenesController(dbContext);
            var ordenDto = new OrdenDto 
            { 
                MesaId = 1, 
                Detalles = new List<DetalleOrdenDto>
                {
                    new DetalleOrdenDto { PlatoId = 1, Cantidad = 2, PrecioUnitario = 25.00m }
                }
            };

            // Act
            var result = await controller.CrearDesdePOS(ordenDto);

            // Assert
            Assert.True(result is JsonResult || result is ObjectResult, "Result should be Json-like result");

            // Verify DB changes
            var orden = await dbContext.Ordenes.Include(o => o.Detalles).FirstOrDefaultAsync();
            Assert.NotNull(orden);
            Assert.Equal("Pendiente", orden.Estado);
            Assert.Single(orden.Detalles);
            Assert.Equal(50.00m, orden.Total); // 2 * 25.00

            var mesa = await dbContext.Mesas.FindAsync(1);
            Assert.Equal("Ocupada", mesa.Estado);
        }

        [Fact]
        public async Task CrearDesdePOS_CalculoTotal_SumaCorrectamenteCantidadesYPrecios()
        {
            // Arrange
            var dbContext = GetDbContext();
            var controller = GetOrdenesController(dbContext);
            var ordenDto = new OrdenDto 
            { 
                MesaId = 1, 
                Detalles = new List<DetalleOrdenDto>
                {
                    new DetalleOrdenDto { PlatoId = 1, Cantidad = 2, PrecioUnitario = 25.00m }, // 2 * 25 = 50
                    new DetalleOrdenDto { PlatoId = 2, Cantidad = 3, PrecioUnitario = 5.00m }  // 3 * 5 = 15
                }
            };

            // Act
            await controller.CrearDesdePOS(ordenDto);

            // Assert
            var orden = await dbContext.Ordenes.FirstOrDefaultAsync();
            Assert.Equal(65.00m, orden.Total); // 50 + 15
        }

        [Fact]
        public async Task PagarYLiberarMesa_ProcesaCorrectamenteYCambiaMesaALibre()
        {
            // Arrange
            var dbContext = GetDbContext();
            var controller = GetOrdenesController(dbContext);

            // Create pending order
            var orden = new Orden 
            { 
                EmpleadoId = 1, 
                MesaId = 1, 
                Estado = "Entregada", 
                Total = 50.00m,
                FechaHoraInicio = DateTime.UtcNow
            };
            dbContext.Ordenes.Add(orden);

            // Set mesa to Cuenta
            var mesa = await dbContext.Mesas.FindAsync(1);
            mesa.Estado = "Cuenta";
            await dbContext.SaveChangesAsync();

            // Act
            var result = await controller.PagarYLiberarMesa(orden.OrdenId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Mesas", redirectResult.ControllerName);

            // Verify order is paid
            var ordenDb = await dbContext.Ordenes.FindAsync(orden.OrdenId);
            Assert.Equal("Pagada", ordenDb.Estado);

            // Verify mesa is free
            var mesaDb = await dbContext.Mesas.FindAsync(1);
            Assert.Equal("Libre", mesaDb.Estado);

            // Verify transaction created
            var transaccion = await dbContext.TransaccionesPago.FirstOrDefaultAsync(t => t.OrdenId == orden.OrdenId);
            Assert.NotNull(transaccion);
            Assert.Equal(50.00m, transaccion.Monto);
        }

        [Fact]
        public async Task PagarYLiberarMesa_OrdenInexistente_RetornaNotFound()
        {
            // Arrange
            var dbContext = GetDbContext();
            var controller = GetOrdenesController(dbContext);

            // Act
            var result = await controller.PagarYLiberarMesa(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
        [Fact]
        public async Task DetalleOpcionesMesa_MesaLibre_DebeRedirigirATomaPedido()
        {
            // Arrange
            var dbContext = GetDbContext();
            var controller = GetOrdenesController(dbContext);

            // Act: Intentamos ver las opciones de la mesa 1 (que está 'Libre' en el Seed del contexto)
            var result = await controller.DetalleOpcionesMesa(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("TomaPedido", redirectResult.ActionName);
            Assert.Equal(1, redirectResult.RouteValues["mesaId"]);
        }

        [Fact]
        public async Task Checkout_MesaOcupada_DebeCambiarEstadoACuentaYRetornarVista()
        {
            // Arrange
            var dbContext = GetDbContext();
            var controller = GetOrdenesController(dbContext);

            // Cambiamos el estado de la mesa 1 a 'Ocupada' y le creamos una orden activa
            var mesa = await dbContext.Mesas.FindAsync(1);
            mesa.Estado = "Ocupada";

            var orden = new Orden { EmpleadoId = 1, MesaId = 1, Estado = "Pendiente", FechaHoraInicio = DateTime.UtcNow };
            dbContext.Ordenes.Add(orden);
            await dbContext.SaveChangesAsync();

            // Act: Simulamos que el mozo presiona "Proceder al Pago"
            var result = await controller.Checkout(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var mesaActualizada = await dbContext.Mesas.FindAsync(1);

            // Verificamos la transición de estado correcta
            Assert.Equal("Cuenta", mesaActualizada.Estado);
        }
        [Fact]
        public async Task TomaPedido_SesionInvalida_RedirigeAHome()
        {
            // Arrange
            var dbContext = GetDbContext();
            var controller = new OrdenesController(dbContext);
            // Simular sesión vacía
            var mockSession = new Mock<ISession>();
            byte[] value;
            mockSession.Setup(s => s.TryGetValue("EmpleadoID", out value!)).Returns(false);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = mockSession.Object }
            };

            // Act
            var result = await controller.TomaPedido(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public async Task TomaPedido_MesaInexistente_RetornaNotFound()
        {
            // Arrange
            var dbContext = GetDbContext();
            var controller = GetOrdenesController(dbContext); // Usa sesión válida

            // Act: Buscamos la mesa 999 que no existe
            var result = await controller.TomaPedido(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Mesa no encontrada.", notFoundResult.Value);
        }

        [Fact]
        public async Task TomaPedido_MesaValida_RetornaVistaConPlatos()
        {
            // Arrange
            var dbContext = GetDbContext();
            var controller = GetOrdenesController(dbContext);

            // Act: Buscamos la mesa 1 que sí existe
            var result = await controller.TomaPedido(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Plato>>(viewResult.Model);
            Assert.True(model.Count > 0); // Verifica que cargue el menú
            Assert.Equal(1, controller.ViewBag.MesaId);
        }

        [Fact]
        public async Task Checkout_SinOrdenActiva_RedirigeAMesas()
        {
            // Arrange
            var dbContext = GetDbContext();
            var controller = GetOrdenesController(dbContext);
            // La mesa 1 no tiene órdenes activas en el seed actual de tus pruebas

            // Act
            var result = await controller.Checkout(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Mesas", redirectResult.ControllerName);
        }
    }
}