using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Collections.Infrastructure.Persistence;

public class CollectionDbContextFactory : IDesignTimeDbContextFactory<CollectionDbContext>
{
    public CollectionDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CollectionDbContext>();
        var connectionString = args.Length > 0
            ? args[0]
            : "Host=localhost;Port=5432;Database=obss_collections;Username=obss_admin;Password=obss_s3cur3_p@ss";

        optionsBuilder.UseNpgsql(connectionString, npgsqlBuilder =>
        {
            npgsqlBuilder.MigrationsAssembly(typeof(CollectionDbContext).Assembly.FullName);
            npgsqlBuilder.CommandTimeout(120);
        });

        optionsBuilder.UseSnakeCaseNamingConvention();
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();

        return new CollectionDbContext(
            optionsBuilder.Options,
            new DesignTimeCurrentTenant(),
            new DesignTimeDomainEventDispatcher());
    }
}

internal class DesignTimeCurrentTenant : ICurrentTenant
{
    public string? TenantId => "design-time";
    public string? Name => "Design Time";
    public string? ConnectionString => null;
    public bool IsReseller => false;
    public bool IsActive => true;
}

internal class DesignTimeDomainEventDispatcher : IDomainEventDispatcher
{
    public Task DispatchAsync(IReadOnlyCollection<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task DispatchAndClearAsync(Entity<Guid> entity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
