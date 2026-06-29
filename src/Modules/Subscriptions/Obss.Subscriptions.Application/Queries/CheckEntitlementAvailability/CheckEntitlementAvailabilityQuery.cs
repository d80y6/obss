using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Subscriptions.Application.Queries.CheckEntitlementAvailability;

public sealed record CheckEntitlementAvailabilityQuery(
    Guid SubscriptionId,
    string EntitlementType,
    decimal RequestedAmount) : IRequest<Result<bool>>;
