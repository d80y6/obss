using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.NetworkInventory.Infrastructure.Persistence.Repositories;

public sealed class ConnectivityLinkRepository : EfRepository<ConnectivityLink>
{
    public ConnectivityLinkRepository(NetworkDbContext context)
        : base(context)
    {
    }
}
