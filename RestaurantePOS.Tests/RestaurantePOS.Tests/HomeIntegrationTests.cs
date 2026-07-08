using System.Net;
using Xunit;

namespace RestaurantePOS.Tests;

public class HomeIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HomeIntegrationTests(CustomWebApplicationFactory factory)
    {
        // Usa la factoría de Testcontainers que creamos previamente
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_PantallaLogin_RetornaExito()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }
}