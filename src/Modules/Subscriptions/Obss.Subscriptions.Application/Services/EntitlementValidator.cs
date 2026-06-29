using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Domain.Services;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Application.Services;

public sealed class EntitlementValidator : IEntitlementValidator
{
    private readonly ISubscriptionEntitlementRepository _entitlementRepository;

    public EntitlementValidator(ISubscriptionEntitlementRepository entitlementRepository)
    {
        _entitlementRepository = entitlementRepository;
    }

    public bool ValidateEntitlement(Guid subscriptionId, string entitlementType, decimal requestedAmount)
    {
        if (!Enum.TryParse<EntitlementType>(entitlementType, out var type))
            return false;

        var entitlement = _entitlementRepository
            .GetBySubscriptionAndTypeAsync(subscriptionId, type)
            .GetAwaiter()
            .GetResult();

        return entitlement?.IsAvailable(requestedAmount) ?? false;
    }

    public decimal GetAvailableEntitlement(Guid subscriptionId, string entitlementType)
    {
        if (!Enum.TryParse<EntitlementType>(entitlementType, out var type))
            return 0;

        var entitlement = _entitlementRepository
            .GetBySubscriptionAndTypeAsync(subscriptionId, type)
            .GetAwaiter()
            .GetResult();

        if (entitlement is null)
            return 0;

        if (entitlement.IsUnlimited)
            return -1;

        return entitlement.Limit - entitlement.Used;
    }
}
