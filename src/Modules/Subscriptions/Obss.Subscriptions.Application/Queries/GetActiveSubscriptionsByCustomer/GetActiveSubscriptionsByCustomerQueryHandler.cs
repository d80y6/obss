using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Queries.GetActiveSubscriptionsByCustomer;

public sealed class GetActiveSubscriptionsByCustomerQueryHandler
    : IRequestHandler<GetActiveSubscriptionsByCustomerQuery, Result<IReadOnlyList<SubscriptionSummaryDto>>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public GetActiveSubscriptionsByCustomerQueryHandler(ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<Result<IReadOnlyList<SubscriptionSummaryDto>>> Handle(
        GetActiveSubscriptionsByCustomerQuery request,
        CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionRepository.GetActiveByCustomerAsync(request.CustomerId, cancellationToken);
        var result = subscriptions.Adapt<List<SubscriptionSummaryDto>>();
        return Result.Success<IReadOnlyList<SubscriptionSummaryDto>>(result);
    }
}
