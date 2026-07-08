using System.Net;
using Xunit;

namespace RestaurantePOS.Tests;

public class FlujoPrincipalIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public FlujoPrincipalIntegrationTests(CustomWebApplicationFactory factory)
    {
        // Crea un cliente HTTP que ataca a nuestro servidor de pruebas en memoria
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PantallaLogin_DebeRetornarStatusCode200_CuandoSistemaInicia()
    {
        // Act (WHEN): Hacemos una petición GET a la raíz (pantalla de inicio/login)
        var response = await _client.GetAsync("/");

        // Assert (THEN): Validamos que cargue con éxito usando Testcontainers
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }
}