using Mapster;
using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.UpdateBillingAccount;

public sealed class UpdateBillingAccountCommandHandler(
    IRepository<BillingAccount> repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateBillingAccountCommand, Result<BillingAccountDto>>
{
    public async Task<Result<BillingAccountDto>> Handle(UpdateBillingAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (account is null)
            return Result.Failure<BillingAccountDto>(Error.NotFound(nameof(BillingAccount), request.Id));

        account.UpdateDetails(request.Name, request.CreditLimit, request.Currency, request.Description);
        await repository.UpdateAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(account.Adapt<BillingAccountDto>());
    }
}
