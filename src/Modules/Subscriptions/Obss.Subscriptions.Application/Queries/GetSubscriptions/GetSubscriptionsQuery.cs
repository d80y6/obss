using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.DTOs;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Application.Queries.GetSubscriptions;

public sealed record GetSubscriptionsQuery(
    Guid? CustomerId,
    SubscriptionStatus? Status,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<IReadOnlyList<SubscriptionSummaryDto>>>;
