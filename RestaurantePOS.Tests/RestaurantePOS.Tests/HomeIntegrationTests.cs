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

  
}