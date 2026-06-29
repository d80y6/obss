namespace Obss.Subscriptions.Application.DTOs;

public sealed record SubscriptionEntitlementDto(
    Guid Id,
    Guid SubscriptionId,
    string EntitlementType,
    string Name,
    decimal Limit,
    decimal Used,
    string Unit,
    bool IsUnlimited,
    bool IsOverridable,
    DateTime ValidFrom,
    DateTime? ValidTo);

public sealed record EntitlementUsageDto(
    string EntitlementType,
    decimal Used,
    decimal Limit,
    decimal Available,
    string Unit,
    bool IsUnlimited);
