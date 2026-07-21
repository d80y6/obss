using Obss.OCS.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.OCS.Application.Abstractions;

public interface IOcsTransactionRepository : IRepository<OcsTransaction>
{
    Task<IReadOnlyList<OcsTransaction>> GetBySubscriptionAsync(Guid subscriptionId, int limit = 50, CancellationToken cancellationToken = default);
}
