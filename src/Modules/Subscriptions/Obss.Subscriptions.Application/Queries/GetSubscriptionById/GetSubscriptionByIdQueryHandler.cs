using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Queries.GetSubscriptionById;

public sealed class GetSubscriptionByIdQueryHandler : IRequestHandler<GetSubscriptionByIdQuery, Result<SubscriptionDto>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public GetSubscriptionByIdQueryHandler(ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<Result<SubscriptionDto>> Handle(GetSubscriptionByIdQuery request, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken);

        if (subscription is null)
            return Result.Failure<SubscriptionDto>(Error.NotFound("Subscription", request.Id));

        return Result.Success(subscription.Adapt<SubscriptionDto>());
    }
}
