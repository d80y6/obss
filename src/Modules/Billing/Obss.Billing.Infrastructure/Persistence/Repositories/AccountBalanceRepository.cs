using Microsoft.EntityFrameworkCore;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Infrastructure.Persistence;

namespace Obss.Billing.Infrastructure.Persistence.Repositories;

public sealed class AccountBalanceRepository : IAccountBalanceRepository
{
    private readonly BillingDbContext _dbContext;

    public AccountBalanceRepository(BillingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AccountBalance?> GetByBillingAccountIdAsync(Guid billingAccountId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AccountBalances
            .FirstOrDefaultAsync(ab => ab.BillingAccountId == billingAccountId, cancellationToken);
    }

    public async Task<AccountBalance?> GetByBillingAccountIdWithTransactionsAsync(Guid billingAccountId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AccountBalances
            .Include(ab => ab.Transactions)
            .FirstOrDefaultAsync(ab => ab.BillingAccountId == billingAccountId, cancellationToken);
    }

    public async Task AddAsync(AccountBalance balance, CancellationToken cancellationToken = default)
    {
        await _dbContext.AccountBalances.AddAsync(balance, cancellationToken);
    }

    public Task UpdateAsync(AccountBalance balance, CancellationToken cancellationToken = default)
    {
        _dbContext.AccountBalances.Update(balance);
        return Task.CompletedTask;
    }
}
