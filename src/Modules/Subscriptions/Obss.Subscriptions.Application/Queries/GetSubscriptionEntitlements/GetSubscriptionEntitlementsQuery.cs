using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Queries.GetSubscriptionEntitlements;

public sealed record GetSubscriptionEntitlementsQuery(Guid SubscriptionId) : IRequest<Result<List<SubscriptionEntitlementDto>>>;
