using Obss.OCS.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.OCS.Application.Abstractions;

public interface IBalanceRepository : IRepository<Balance>
{
    Task<Balance?> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
}
