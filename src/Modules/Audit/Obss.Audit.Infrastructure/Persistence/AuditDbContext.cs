using Microsoft.EntityFrameworkCore;
using Obss.Audit.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Audit.Infrastructure.Persistence;

public class AuditDbContext : EfDbContext
{
    public AuditDbContext(
        DbContextOptions<AuditDbContext> options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options, currentTenant, domainEventDispatcher)
    {
    }

    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<AuditPolicy> AuditPolicies => Set<AuditPolicy>();
    public DbSet<AuditAlert> AuditAlerts => Set<AuditAlert>();
    public DbSet<AuditAlertRule> AuditAlertRules => Set<AuditAlertRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
