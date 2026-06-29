using Microsoft.EntityFrameworkCore;
using Obss.ApiGateway.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ApiGateway.Infrastructure.Persistence;

public class GatewayDbContext : EfDbContext
{
    public GatewayDbContext(
        DbContextOptions<GatewayDbContext> options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options, currentTenant, domainEventDispatcher)
    {
    }

    public DbSet<ApiRoute> ApiRoutes => Set<ApiRoute>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<Partner> Partners => Set<Partner>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GatewayDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
