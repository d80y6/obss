using Mapster;
using MediatR;
using Obss.OCS.Application.Abstractions;
using Obss.OCS.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.OCS.Application.Queries.GetCreditPools;

internal sealed class GetCreditPoolsQueryHandler : IRequestHandler<GetCreditPoolsQuery, Result<IReadOnlyList<CreditPoolDto>>>
{
    private readonly ICreditPoolRepository _repository;

    public GetCreditPoolsQueryHandler(ICreditPoolRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<CreditPoolDto>>> Handle(GetCreditPoolsQuery request, CancellationToken cancellationToken)
    {
        var pools = await _repository.GetActiveBySubscriptionAsync(request.SubscriptionId, cancellationToken);
        return Result.Success(pools.Adapt<IReadOnlyList<CreditPoolDto>>());
    }
}
