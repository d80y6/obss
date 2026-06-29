using Microsoft.EntityFrameworkCore;
using Obss.Provisioning.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Provisioning.Infrastructure.Persistence;

public class ProvisioningDbContext : EfDbContext
{
    public ProvisioningDbContext(
        DbContextOptions<ProvisioningDbContext> options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options, currentTenant, domainEventDispatcher)
    {
    }

    public DbSet<ProvisioningJob> ProvisioningJobs => Set<ProvisioningJob>();
    public DbSet<ProvisioningTask> ProvisioningTasks => Set<ProvisioningTask>();
    public DbSet<ProvisioningTemplate> ProvisioningTemplates => Set<ProvisioningTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProvisioningDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
