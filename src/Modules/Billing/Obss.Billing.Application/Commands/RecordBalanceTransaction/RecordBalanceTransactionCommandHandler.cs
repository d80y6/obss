using MediatR;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.RecordBalanceTransaction;

public sealed class RecordBalanceTransactionCommandHandler(
    IAccountBalanceRepository balanceRepository,
    IRepository<BillingAccount> accountRepository,
    IUnitOfWork unitOfWork,
    ICurrentTenant currentTenant)
    : IRequestHandler<RecordBalanceTransactionCommand, Result>
{
    public async Task<Result> Handle(RecordBalanceTransactionCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TransactionType>(request.TransactionType, true, out var transactionType))
            return Result.Failure(Error.Validation($"Invalid transaction type: '{request.TransactionType}'."));

        var account = await accountRepository.GetByIdAsync(request.BillingAccountId, cancellationToken);
        if (account is null)
            return Result.Failure(Error.NotFound(nameof(BillingAccount), request.BillingAccountId));

        var balance = await balanceRepository.GetByBillingAccountIdAsync(request.BillingAccountId, cancellationToken);
        if (balance is null)
        {
            balance = new AccountBalance(currentTenant.TenantId!, request.BillingAccountId, 0, 0, account.CreditLimit, account.Currency);
            await balanceRepository.AddAsync(balance, cancellationToken);
        }

        balance.RecordTransaction(request.Amount, transactionType, request.Description, request.ReferenceId, request.ReferenceType);
        await balanceRepository.UpdateAsync(balance, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
