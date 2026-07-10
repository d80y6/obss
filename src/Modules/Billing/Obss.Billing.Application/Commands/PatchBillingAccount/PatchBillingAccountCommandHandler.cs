using Mapster;
using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.PatchBillingAccount;

public sealed class PatchBillingAccountCommandHandler(
    IRepository<BillingAccount> repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<PatchBillingAccountCommand, Result<BillingAccountDto>>
{
    public async Task<Result<BillingAccountDto>> Handle(PatchBillingAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (account is null)
            return Result.Failure<BillingAccountDto>(Error.NotFound(nameof(BillingAccount), request.Id));

        if (request.Name is not null || request.CreditLimit.HasValue || request.Currency is not null || request.Description is not null)
        {
            account.UpdateDetails(
                request.Name ?? account.Name,
                request.CreditLimit ?? account.CreditLimit,
                request.Currency ?? account.Currency,
                request.Description ?? account.Description);
        }

        if (request.Status is not null)
            account.SetStatus(request.Status);

        if (request.PaymentMethodId is not null)
            account.SetPaymentMethodId(request.PaymentMethodId);

        if (request.AccountHolder is not null)
        {
            var holder = new AccountHolder(
                request.AccountHolder.Name,
                request.AccountHolder.Email,
                request.AccountHolder.Phone,
                request.AccountHolder.ContactId);
            account.SetAccountHolder(holder);
        }

        await repository.UpdateAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(account.Adapt<BillingAccountDto>());
    }
}
