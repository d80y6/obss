using Microsoft.EntityFrameworkCore;
using Obss.ServiceInventory.Application.Abstractions;
using Obss.ServiceInventory.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ServiceInventory.Infrastructure.Persistence.Repositories;

public sealed class ServiceTopologyRepository : EfRepository<ServiceTopology>, IServiceTopologyRepository
{
    public ServiceTopologyRepository(ServiceDbContext context)
        : base(context)
    {
    }

    public async Task<ServiceTopology?> GetByIdWithLinksAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(t => t.Links)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<ServiceTopology?> GetByServiceIdWithLinksAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(t => t.Links)
            .FirstOrDefaultAsync(t => t.ServiceId == serviceId, cancellationToken);
    }

    public async Task<IReadOnlyList<TopologyLink>> GetUpstreamLinksAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<TopologyLink>()
            .Where(l => l.TargetServiceId == serviceId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TopologyLink>> GetDownstreamLinksAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<TopologyLink>()
            .Where(l => l.SourceServiceId == serviceId)
            .ToListAsync(cancellationToken);
    }
}
