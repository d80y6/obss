using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Domain.Exceptions;

namespace Obss.Subscriptions.Application.Commands.ActivateSubscription;

public sealed class ActivateSubscriptionCommandHandler : IRequestHandler<ActivateSubscriptionCommand, Result>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateSubscriptionCommandHandler(
        ISubscriptionRepository subscriptionRepository,
        IUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ActivateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken);

        if (subscription is null)
            return Result.Failure(Error.NotFound("Subscription", request.SubscriptionId));

        try
        {
            subscription.Activate();
        }
        catch (InvalidSubscriptionStateException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
