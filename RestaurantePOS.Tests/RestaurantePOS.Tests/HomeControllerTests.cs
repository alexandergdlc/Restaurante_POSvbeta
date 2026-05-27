using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using RestaurantePOS.Controllers;
using RestaurantePOS.Domain.Entities;
using RestaurantePOS.Infrastructure;
using RestaurantePOS.Infrastructure.Data;
using Xunit;

namespace RestaurantePOS.Tests
{
    public class HomeControllerTests
    {
        private RestauranteDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<RestauranteDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var dbContext = new RestauranteDbContext(options);

            // Seed an employee
            dbContext.Empleados.Add(new Empleado
            {
                EmpleadoId = 1,
                Nombres = "Juan Mesero",
                Rol = "Mesero",
                Apellidos = "Test",
                PinAcceso = "1234",
                Activo = true
            });
            dbContext.SaveChanges();

            return dbContext;
        }

        private HomeController GetHomeController(RestauranteDbContext dbContext)
        {
            var controller = new HomeController(dbContext);

            var sessionMock = new Mock<ISession>();

            var httpContext = new DefaultHttpContext
            {
                Session = sessionMock.Object
            };

            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            controller.TempData = tempData;

            return controller;
        }

        [Fact]
        public async Task Login_ConPinIncorrecto_DebeRetornarIndexYMostrarError()
        {
            // Arrange
            var dbContext = GetDbContext();
            var controller = GetHomeController(dbContext);

            // Act
            var result = await controller.Login("9999");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", viewResult.ViewName);
            Assert.True(controller.TempData.ContainsKey("Error"));
            Assert.Equal("PIN incorrecto.", controller.TempData["Error"]);
        }

        [Fact]
        public async Task Login_ConPinCorrecto_DebeRedirigirAMesasYAsignarSesion()
        {
            // Arrange
            var dbContext = GetDbContext();
            var controller = GetHomeController(dbContext);

            // Act
            var result = await controller.Login("1234");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Mesas", redirectResult.ControllerName);
        }
        [Fact]
        public async Task Login_ConPinVacioONulo_DebeRetornarIndexYMostrarError()
        {
            // Arrange
            var dbContext = GetDbContext();
            var controller = GetHomeController(dbContext);

            // Act
            var result = await controller.Login("");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", viewResult.ViewName);
            Assert.True(controller.TempData.ContainsKey("Error"));
            Assert.Equal("Por favor, ingresa un PIN.", controller.TempData["Error"]);
        }

        [Fact]
        public void Index_SiUsuarioYaEstaLogueado_DebeRedirigirAMesas()
        {
            // Arrange
            var dbContext = GetDbContext();
            var controller = GetHomeController(dbContext);

            // Forzamos que la sesión ya tenga un ID de empleado (Simulando que ya hizo login antes)
            byte[] empleadoIdBytes = BitConverter.GetBytes(1);
            Mock.Get(controller.HttpContext.Session)
                .Setup(s => s.TryGetValue("EmpleadoID", out empleadoIdBytes))
                .Returns(true);

            // Act
            var result = controller.Index();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Mesas", redirectResult.ControllerName);
        }

        [Fact]
        public void Logout_DebeLimpiarSesionYRedirigirAIndex()
        {
            // Arrange
            var dbContext = GetDbContext();
            var controller = GetHomeController(dbContext);

            // Act
            var result = controller.Logout();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Verificamos que se haya llamado al método Clear() de la sesión
            Mock.Get(controller.HttpContext.Session).Verify(s => s.Clear(), Times.Once);
        }
    }
}