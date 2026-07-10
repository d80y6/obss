using MediatR;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.RemoveBillingAccountRelatedParty;

public sealed class RemoveBillingAccountRelatedPartyCommandHandler(
    IRepository<BillingAccount> repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RemoveBillingAccountRelatedPartyCommand, Result>
{
    public async Task<Result> Handle(RemoveBillingAccountRelatedPartyCommand request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.BillingAccountId, cancellationToken);
        if (account is null)
            return Result.Failure(Error.NotFound(nameof(BillingAccount), request.BillingAccountId));

        var party = account.RelatedParties.FirstOrDefault(rp => rp.PartyId == request.PartyId);
        if (party is null)
            return Result.Failure(Error.NotFound("RelatedParty", request.PartyId));

        account.RemoveRelatedParty(party);
        await repository.UpdateAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
