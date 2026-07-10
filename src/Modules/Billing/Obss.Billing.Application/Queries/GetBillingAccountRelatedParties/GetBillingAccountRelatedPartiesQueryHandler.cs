using Mapster;
using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Queries.GetBillingAccountRelatedParties;

public sealed class GetBillingAccountRelatedPartiesQueryHandler(
    IRepository<BillingAccount> repository)
    : IRequestHandler<GetBillingAccountRelatedPartiesQuery, Result<List<RelatedPartyDto>>>
{
    public async Task<Result<List<RelatedPartyDto>>> Handle(GetBillingAccountRelatedPartiesQuery request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.BillingAccountId, cancellationToken);
        if (account is null)
            return Result.Failure<List<RelatedPartyDto>>(Error.NotFound(nameof(BillingAccount), request.BillingAccountId));

        var dtos = account.RelatedParties.Adapt<List<RelatedPartyDto>>();
        return Result.Success(dtos);
    }
}
