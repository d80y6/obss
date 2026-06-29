using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.NetworkInventory.Infrastructure.Persistence.Repositories;

public sealed class TopologyMapRepository : EfRepository<TopologyMap>
{
    public TopologyMapRepository(NetworkDbContext context)
        : base(context)
    {
    }
}
