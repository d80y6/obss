using Microsoft.EntityFrameworkCore;
using Obss.SharedKernel.Infrastructure.Persistence;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.ServiceQualification.Infrastructure.Persistence;

public class ServiceQualificationDbContext : EfDbContext
{
    public ServiceQualificationDbContext(
        DbContextOptions<ServiceQualificationDbContext> options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options, currentTenant, domainEventDispatcher)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServiceQualificationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
