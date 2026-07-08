using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePOS.Infrastructure.Data;
using Testcontainers.PostgreSql;
using Xunit;

namespace RestaurantePOS.Tests;

// IAsyncLifetime permite levantar el contenedor antes de que inicien las pruebas
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithDatabase("restaurante_test")
        .WithUsername("postgres")
        .WithPassword("test2026")
        .Build();

    public async Task InitializeAsync()
    {
        // 1. Levanta el contenedor Docker de PostgreSQL
        await _postgresContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        // 4. Apaga y destruye el contenedor al terminar
        await _postgresContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 2. Buscamos la conexión original a Supabase y la eliminamos
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<RestauranteDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // 3. Inyectamos la conexión dinámica del contenedor Docker
            services.AddDbContext<RestauranteDbContext>(options =>
            {
                options.UseNpgsql(_postgresContainer.GetConnectionString());
            });

            // Aplicamos las migraciones automáticamente en el contenedor vacío
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RestauranteDbContext>();
            db.Database.Migrate();
        });
    }
}