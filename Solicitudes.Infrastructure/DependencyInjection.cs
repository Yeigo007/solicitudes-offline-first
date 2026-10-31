
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Solicitudes.Application.Abstractions;
using Solicitudes.Infrastructure.Persistence;
using Solicitudes.Infrastructure.Repositories;

namespace Solicitudes.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
       var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("The database connection string 'DefaultConnection' was not found.");

        services.AddDbContext<SolicitudesDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IRequestRepository, RequestRepository>();

        return services;
    }
}
