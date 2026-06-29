using Obss.Billing.Domain.Entities;

namespace Obss.Billing.Domain.Services;

public interface IBillingCalculator
{
    Task<Bill> CalculateBill(Guid customerId, DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken = default);
}
