using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Subscriptions.Application.Commands.SetSubscriptionEntitlements;

public sealed record SetSubscriptionEntitlementsCommand(
    Guid SubscriptionId,
    List<EntitlementDefinition> Entitlements) : IRequest<Result>;

public sealed record EntitlementDefinition(
    string EntitlementType,
    string Name,
    decimal Limit,
    decimal Used,
    string Unit,
    bool IsUnlimited,
    bool IsOverridable,
    DateTime ValidFrom,
    DateTime? ValidTo);
