using Obss.Billing.Domain.Entities;

namespace Obss.Billing.Domain.Services;

public interface ITaxCalculator
{
    Task<Bill> CalculateTaxesAsync(Bill bill, CancellationToken cancellationToken = default);
}
