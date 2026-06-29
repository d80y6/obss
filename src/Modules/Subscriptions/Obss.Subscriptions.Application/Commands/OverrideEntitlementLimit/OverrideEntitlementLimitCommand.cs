using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Subscriptions.Application.Commands.OverrideEntitlementLimit;

public sealed record OverrideEntitlementLimitCommand(
    Guid SubscriptionId,
    string EntitlementType,
    decimal NewLimit) : IRequest<Result>;
