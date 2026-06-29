using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Billing.Application.Abstractions;

public interface ITaxRuleRepository : IRepository<TaxRule>
{
    Task<IReadOnlyList<TaxRule>> GetApplicableRulesAsync(string category, string country, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaxExemption>> GetCustomerExemptionsAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<TaxExemption?> GetExemptionAsync(Guid customerId, Guid taxRuleId, CancellationToken cancellationToken = default);
    Task AddExemptionAsync(TaxExemption exemption, CancellationToken cancellationToken = default);
}
