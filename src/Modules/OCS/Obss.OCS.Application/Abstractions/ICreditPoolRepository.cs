using Obss.OCS.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.OCS.Application.Abstractions;

public interface ICreditPoolRepository : IRepository<CreditPool>
{
    Task<IReadOnlyList<CreditPool>> GetActiveBySubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
}
