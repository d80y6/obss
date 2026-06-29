using MediatR;
using Obss.Rating.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Queries.GetUsageBySubscription;

public sealed record GetUsageBySubscriptionQuery(
    Guid SubscriptionId,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 50) : IRequest<Result<IReadOnlyList<UsageRecordDto>>>;
