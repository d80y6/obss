using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Application.Queries.CheckEntitlementAvailability;

public sealed class CheckEntitlementAvailabilityQueryHandler : IRequestHandler<CheckEntitlementAvailabilityQuery, Result<bool>>
{
    private readonly ISubscriptionEntitlementRepository _entitlementRepository;

    public CheckEntitlementAvailabilityQueryHandler(ISubscriptionEntitlementRepository entitlementRepository)
    {
        _entitlementRepository = entitlementRepository;
    }

    public async Task<Result<bool>> Handle(CheckEntitlementAvailabilityQuery request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<EntitlementType>(request.EntitlementType, out var entitlementType))
            return Result.Failure<bool>(Error.Validation($"Invalid entitlement type: '{request.EntitlementType}'."));

        var entitlement = await _entitlementRepository.GetBySubscriptionAndTypeAsync(
            request.SubscriptionId, entitlementType, cancellationToken);

        if (entitlement is null)
            return Result.Failure<bool>(Error.NotFound("SubscriptionEntitlement", $"{request.SubscriptionId}/{request.EntitlementType}"));

        return Result.Success(entitlement.IsAvailable(request.RequestedAmount));
    }
}
