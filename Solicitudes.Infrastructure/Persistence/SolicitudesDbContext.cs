
using Microsoft.EntityFrameworkCore;
using Solicitudes.Domain.Entities;

namespace Solicitudes.Infrastructure.Persistence;

public sealed class SolicitudesDbContext : DbContext
{
    public SolicitudesDbContext(
        DbContextOptions<SolicitudesDbContext> options)
        : base(options)
    {
    }

    public DbSet<Request> Requests => Set<Request>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Request>(entity =>
        {
            entity.HasIndex(request => request.ClientRequestId)
                .IsUnique();
        });
    }
}
