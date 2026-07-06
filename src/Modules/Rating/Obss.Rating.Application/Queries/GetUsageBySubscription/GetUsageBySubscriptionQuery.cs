using MediatR;
using Obss.Rating.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Queries.GetUsageBySubscription;

public sealed record GetUsageBySubscriptionQuery(
    Guid SubscriptionId,
    DateTime? From,
    DateTime? To,
    int Offset = 0,
    int Limit = 50) : IRequest<Result<IReadOnlyList<UsageRecordDto>>>;
