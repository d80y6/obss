using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Queries.GetSubscriptionEntitlements;

public sealed class GetSubscriptionEntitlementsQueryHandler : IRequestHandler<GetSubscriptionEntitlementsQuery, Result<List<SubscriptionEntitlementDto>>>
{
    private readonly ISubscriptionEntitlementRepository _entitlementRepository;

    public GetSubscriptionEntitlementsQueryHandler(ISubscriptionEntitlementRepository entitlementRepository)
    {
        _entitlementRepository = entitlementRepository;
    }

    public async Task<Result<List<SubscriptionEntitlementDto>>> Handle(GetSubscriptionEntitlementsQuery request, CancellationToken cancellationToken)
    {
        var entitlements = await _entitlementRepository.GetBySubscriptionIdAsync(request.SubscriptionId, cancellationToken);

        return Result.Success(entitlements.Adapt<List<SubscriptionEntitlementDto>>());
    }
}
