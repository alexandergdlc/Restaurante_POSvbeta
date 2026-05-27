using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using RestaurantePOS.Controllers;
using RestaurantePOS.Domain.Entities;
using RestaurantePOS.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePOS.Tests
{
    public class MesasControllerTests
    {
        private RestauranteDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<RestauranteDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var dbContext = new RestauranteDbContext(options);

            // Insertar mesas de prueba para el salón
            dbContext.Mesas.Add(new Mesa { MesaId = 1, NumeroMesa = 1, Capacidad = 4, Estado = "Libre" });
            dbContext.Mesas.Add(new Mesa { MesaId = 2, NumeroMesa = 2, Capacidad = 2, Estado = "Ocupada" });
            dbContext.Mesas.Add(new Mesa { MesaId = 3, NumeroMesa = 3, Capacidad = 6, Estado = "Cuenta" });
            dbContext.SaveChanges();

            return dbContext;
        }

        // Método auxiliar para inyectar una sesión activa y evitar que el controlador nos expulse
        private MesasController GetMesasController(RestauranteDbContext dbContext)
        {
            var controller = new MesasController(dbContext);

            var sessionMock = new Mock<ISession>();

            // Simulamos que el empleado con ID 1 ya inició sesión
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
        public async Task Index_DebeRetornarVistaConListaDeMesasOrdenadas()
        {
            // Arrange
            var dbContext = GetDbContext();

            // Usamos el nuevo método que inyecta la sesión en lugar de un "new MesasController" vacío
            var controller = GetMesasController(dbContext);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Mesa>>(viewResult.Model);

            // Verificamos que carguen las 3 mesas que insertamos
            Assert.Equal(3, model.Count());

            // Verificamos que vengan ordenadas por el Número de Mesa
            Assert.Equal(1, model.First().NumeroMesa);
            Assert.Equal(3, model.Last().NumeroMesa);
        }
    }
}