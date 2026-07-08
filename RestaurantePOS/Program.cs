using Microsoft.EntityFrameworkCore;
using RestaurantePOS.Infrastructure.Data;
using System.Diagnostics.CodeAnalysis;

// Solución global para la compatibilidad de DateTime (Kind=UTC) con las columnas timestamp de Supabase
System.AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configuración de Sesión para mantener al empleado logueado
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configuración con cadena directa y Estrategia de Reintentos ante fallas transitorias de red
builder.Services.AddDbContext<RestauranteDbContext>(options =>
    options.UseNpgsql(
        "Host=db.bmeirocalsaiaiifbzbv.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=RestaurantePos2026;sslmode=require;",
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null
        )
    ));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession(); // Agregar Session middleware antes de Authorization
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();