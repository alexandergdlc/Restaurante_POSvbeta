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

   
}