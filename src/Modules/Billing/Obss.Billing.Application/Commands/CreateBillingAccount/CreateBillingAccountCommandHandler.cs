using Mapster;
using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Billing.Application.Commands.CreateBillingAccount;

public sealed class CreateBillingAccountCommandHandler(
    IRepository<BillingAccount> repository,
    IUnitOfWork unitOfWork,
    ICurrentTenant currentTenant)
    : IRequestHandler<CreateBillingAccountCommand, Result<BillingAccountDto>>
{
    public async Task<Result<BillingAccountDto>> Handle(CreateBillingAccountCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AccountType>(request.AccountType, true, out var accountType))
            return Result.Failure<BillingAccountDto>(Error.Validation($"Invalid account type: '{request.AccountType}'."));

        var account = new BillingAccount(currentTenant.TenantId!, request.CustomerId, accountType, request.Name, request.CreditLimit, request.Currency);
        await repository.AddAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(account.Adapt<BillingAccountDto>());
    }
}
