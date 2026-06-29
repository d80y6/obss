using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Subscriptions.Application.Commands.UpdateEntitlementUsage;

public sealed record UpdateEntitlementUsageCommand(
    Guid SubscriptionId,
    string EntitlementType,
    decimal Amount,
    bool IsReduction) : IRequest<Result>;
