using Microsoft.EntityFrameworkCore;
using Obss.NetworkInventory.Application.Abstractions;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.NetworkInventory.Infrastructure.Persistence.Repositories;

public sealed class NetworkElementRepository : EfRepository<NetworkElement>, INetworkElementRepository
{
    public NetworkElementRepository(NetworkDbContext context)
        : base(context)
    {
    }

    public async Task<NetworkElement?> GetByHostnameAsync(string hostname, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(e => e.Hostname == hostname, cancellationToken);
    }

    public async Task<IReadOnlyList<NetworkElement>> GetFilteredAsync(
        string? type,
        string? status,
        string? location,
        int offset,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(e => e.ElementType.ToString() == type);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(e => e.Status.ToString() == status);
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            query = query.Where(e => e.Location != null && e.Location.Contains(location));
        }

        query = query
            .OrderBy(e => e.Name)
            .Skip(offset)
            .Take(limit);

        return await query.ToListAsync(cancellationToken);
    }
}
