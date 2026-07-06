using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Contracts;
using Obss.Subscriptions.Application.DTOs;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Application.Queries.GetSubscriptions;

public sealed record GetSubscriptionsQuery(
    Guid? CustomerId,
    SubscriptionStatus? Status,
    DateTime? FromDate,
    DateTime? ToDate,
    string? SearchTerm,
    int Offset = 0,
    int Limit = 20) : IRequest<Result<PaginatedResult<SubscriptionSummaryDto>>>;
