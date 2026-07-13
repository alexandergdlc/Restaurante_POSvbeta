# Arquitectura del Sistema POS

## 1. Stack Tecnológico
* **Framework Backend:** ASP.NET Core MVC (Versión .NET 10.0)
* **Lenguaje:** C# 12
* **ORM:** Entity Framework Core 10.0 (Proveedor: `Npgsql`)
* **Base de Datos:** PostgreSQL alojada en Supabase (BaaS)
* **Frontend:** Razor Pages, HTML5, CSS3, JavaScript (Fetch API)
* **Pruebas:** xUnit, Moq, Testcontainers (Docker)
* **Despliegue:** Render.com (Contenedor Docker Linux)

## 2. Arquitectura de 3 Capas Limpias
El sistema implementa el patrón de Inversión de Dependencias para asegurar la testabilidad:

### Capa 1: Dominio (`RestaurantePOS.Domain`)
Contiene la lógica core del negocio. Es agnóstica a cualquier framework de base de datos o UI.
* **Entidades POCO:** `Mesa`, `Plato`, `Orden`, `DetalleOrden`, `Empleado`.

### Capa 2: Infraestructura (`RestaurantePOS.Infrastructure`)
Responsable de la persistencia de los datos en la nube.
* Define el `RestauranteDbContext`.
* Implementa las migraciones Code-First hacia Supabase.
* Configura la resiliencia de red (`NpgsqlRetryingExecutionStrategy`).

### Capa 3: Web / Presentación (`RestaurantePOS`)
Maneja las peticiones HTTP y la interfaz gráfica.
* Controladores MVC y vistas Razor.
* Gestión de Sesiones Seguras (`HttpOnly`).