using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Queries.GetSubscriptions;

public sealed class GetSubscriptionsQueryHandler : IRequestHandler<GetSubscriptionsQuery, Result<IReadOnlyList<SubscriptionSummaryDto>>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public GetSubscriptionsQueryHandler(ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<Result<IReadOnlyList<SubscriptionSummaryDto>>> Handle(GetSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionRepository.GetFilteredAsync(
            request.CustomerId,
            request.Status,
            request.FromDate,
            request.ToDate,
            request.Page,
            request.PageSize,
            cancellationToken);

        var result = subscriptions.Adapt<List<SubscriptionSummaryDto>>();
        return Result.Success<IReadOnlyList<SubscriptionSummaryDto>>(result);
    }
}
