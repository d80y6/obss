using Mapster;
using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.AddBillingAccountRelatedParty;

public sealed class AddBillingAccountRelatedPartyCommandHandler(
    IRepository<BillingAccount> repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddBillingAccountRelatedPartyCommand, Result<BillingAccountDto>>
{
    public async Task<Result<BillingAccountDto>> Handle(AddBillingAccountRelatedPartyCommand request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.BillingAccountId, cancellationToken);
        if (account is null)
            return Result.Failure<BillingAccountDto>(Error.NotFound(nameof(BillingAccount), request.BillingAccountId));

        account.AddRelatedParty(request.PartyId, request.PartyName, request.Role);
        await repository.UpdateAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(account.Adapt<BillingAccountDto>());
    }
}
