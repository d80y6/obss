using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.NetworkInventory.Infrastructure.Persistence.Repositories;

public sealed class CapacityRecordRepository : EfRepository<CapacityRecord>
{
    public CapacityRecordRepository(NetworkDbContext context)
        : base(context)
    {
    }
}
