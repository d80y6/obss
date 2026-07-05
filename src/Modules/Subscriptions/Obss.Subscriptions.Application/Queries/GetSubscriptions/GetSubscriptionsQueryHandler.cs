using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Application.Contracts;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Queries.GetSubscriptions;

public sealed class GetSubscriptionsQueryHandler : IRequestHandler<GetSubscriptionsQuery, Result<PaginatedResult<SubscriptionSummaryDto>>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public GetSubscriptionsQueryHandler(ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<Result<PaginatedResult<SubscriptionSummaryDto>>> Handle(GetSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionRepository.GetFilteredAsync(
            request.CustomerId,
            request.Status,
            request.FromDate,
            request.ToDate,
            request.SearchTerm,
            request.Page,
            request.PageSize,
            cancellationToken);

        var totalCount = await _subscriptionRepository.GetFilteredCountAsync(
            request.CustomerId,
            request.Status,
            request.FromDate,
            request.ToDate,
            request.SearchTerm,
            cancellationToken);

        var items = subscriptions.Adapt<List<SubscriptionSummaryDto>>();
        return Result.Success(new PaginatedResult<SubscriptionSummaryDto>(items, totalCount));
    }
}