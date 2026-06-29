using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Queries.GetEntitlementUsage;

public sealed record GetEntitlementUsageQuery(
    Guid SubscriptionId,
    string EntitlementType) : IRequest<Result<EntitlementUsageDto>>;
