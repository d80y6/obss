using Microsoft.EntityFrameworkCore;
using Obss.IAM.Infrastructure.Persistence;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Domain.Common;

namespace Obss.IAM.Infrastructure.Services;

internal sealed class NullTenant : ICurrentTenant
{
    public string? TenantId => null;
    public string? Name => null;
    public string? ConnectionString => null;
    public bool IsReseller => false;
    public bool IsActive => true;
}

public sealed class IamTenantStore : ITenantStore
{
    private readonly DbContextOptions<IamDbContext> _options;

    public IamTenantStore(DbContextOptions<IamDbContext> options)
    {
        _options = options;
    }

    public async Task<TenantInfo?> GetTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(tenantId, out var guid))
            return null;

        await using var dbContext = new IamDbContext(
            _options,
            new NullTenant(),
            NullDomainEventDispatcher.Instance);

        var tenant = await dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == guid, cancellationToken);

        if (tenant is null)
            return null;

        return new TenantInfo(
            tenant.Id.ToString("N"),
            tenant.Name,
            tenant.IsActive,
            false,
            tenant.ConnectionString);
    }
}

internal sealed class NullDomainEventDispatcher : IDomainEventDispatcher
{
    public static readonly NullDomainEventDispatcher Instance = new();
    public Task DispatchAsync(IReadOnlyCollection<DomainEvent> domainEvents, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task DispatchAndClearAsync(Entity<Guid> entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
