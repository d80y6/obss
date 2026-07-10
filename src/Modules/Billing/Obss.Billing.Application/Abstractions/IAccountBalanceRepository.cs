using Obss.Billing.Domain.Entities;

namespace Obss.Billing.Application.Abstractions;

public interface IAccountBalanceRepository
{
    Task<AccountBalance?> GetByBillingAccountIdAsync(Guid billingAccountId, CancellationToken cancellationToken = default);
    Task<AccountBalance?> GetByBillingAccountIdWithTransactionsAsync(Guid billingAccountId, CancellationToken cancellationToken = default);
    Task AddAsync(AccountBalance balance, CancellationToken cancellationToken = default);
    Task UpdateAsync(AccountBalance balance, CancellationToken cancellationToken = default);
}
