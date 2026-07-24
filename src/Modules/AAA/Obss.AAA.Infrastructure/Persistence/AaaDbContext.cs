using Microsoft.EntityFrameworkCore;
using Obss.AAA.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.AAA.Infrastructure.Persistence;

public class AaaDbContext : EfDbContext
{
    public AaaDbContext(
        DbContextOptions<AaaDbContext> options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options, currentTenant, domainEventDispatcher)
    {
    }

    public DbSet<NetworkAccessServer> NasDevices => Set<NetworkAccessServer>();
    public DbSet<RadiusSession> RadiusSessions => Set<RadiusSession>();
    public DbSet<AaaAuditLog> AaaAuditLogs => Set<AaaAuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AaaDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
