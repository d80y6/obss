using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Application.DTOs;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Application.Queries.GetEntitlementUsage;

public sealed class GetEntitlementUsageQueryHandler : IRequestHandler<GetEntitlementUsageQuery, Result<EntitlementUsageDto>>
{
    private readonly ISubscriptionEntitlementRepository _entitlementRepository;

    public GetEntitlementUsageQueryHandler(ISubscriptionEntitlementRepository entitlementRepository)
    {
        _entitlementRepository = entitlementRepository;
    }

    public async Task<Result<EntitlementUsageDto>> Handle(GetEntitlementUsageQuery request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<EntitlementType>(request.EntitlementType, out var entitlementType))
            return Result.Failure<EntitlementUsageDto>(Error.Validation($"Invalid entitlement type: '{request.EntitlementType}'."));

        var entitlement = await _entitlementRepository.GetBySubscriptionAndTypeAsync(
            request.SubscriptionId, entitlementType, cancellationToken);

        if (entitlement is null)
            return Result.Failure<EntitlementUsageDto>(Error.NotFound("SubscriptionEntitlement", $"{request.SubscriptionId}/{request.EntitlementType}"));

        var available = entitlement.IsUnlimited
            ? -1
            : entitlement.Limit - entitlement.Used;

        return Result.Success(new EntitlementUsageDto(
            entitlement.EntitlementType.ToString(),
            entitlement.Used,
            entitlement.Limit,
            available,
            entitlement.Unit,
            entitlement.IsUnlimited));
    }
}
