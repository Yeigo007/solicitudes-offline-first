using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Solicitudes.Infrastructure.Persistence;

namespace Solicitudes.Tests.Integration;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _connectionString =
        Environment.GetEnvironmentVariable("SOLICITUDES_TEST_CONNECTION")
        ?? "Host=localhost;Port=5432;Database=SolicitudesTestsDb;Username=postgres;Password=admin";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _connectionString
            });
        });
    }

    public async Task InitializeAsync()
    {
        // Fuerza la construcción del host y aplica migraciones solo a la BD de pruebas.
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SolicitudesDbContext>();
        await db.Database.MigrateAsync();
        await db.Requests.ExecuteDeleteAsync();
    }

    public new async Task DisposeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SolicitudesDbContext>();
        await db.Requests.ExecuteDeleteAsync();
        await base.DisposeAsync();
    }

    public async Task ResetAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SolicitudesDbContext>();
        await db.Requests.ExecuteDeleteAsync();
    }
}
