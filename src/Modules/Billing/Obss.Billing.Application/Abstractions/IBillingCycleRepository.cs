using Obss.SharedKernel.Application.Abstractions;
using Obss.Billing.Domain.Entities;

namespace Obss.Billing.Application.Abstractions;

public interface IBillingCycleRepository : IRepository<BillingCycle>
{
    Task<BillingCycle?> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BillingCycle>> GetCyclesDueAsync(DateTime upToDate, CancellationToken cancellationToken = default);
}
