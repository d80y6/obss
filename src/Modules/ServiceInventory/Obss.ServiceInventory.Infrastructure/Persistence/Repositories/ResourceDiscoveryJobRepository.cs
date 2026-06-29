using Microsoft.EntityFrameworkCore;
using Obss.ServiceInventory.Application.Abstractions;
using Obss.ServiceInventory.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ServiceInventory.Infrastructure.Persistence.Repositories;

public sealed class ResourceDiscoveryJobRepository : EfRepository<ResourceDiscoveryJob>, IResourceDiscoveryJobRepository
{
    public ResourceDiscoveryJobRepository(ServiceDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<ResourceDiscoveryJob>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(j => j.TenantId == tenantId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ResourceDiscoveryJob>> GetUnmatchedAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(j => j.TenantId == tenantId && j.ResourcesFound > j.ResourcesMatched)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
