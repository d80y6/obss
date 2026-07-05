using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Application.DTOs;
using Obss.Subscriptions.Domain.Exceptions;

namespace Obss.Subscriptions.Application.Commands.ChangeQuantity;

public sealed class ChangeQuantityCommandHandler : IRequestHandler<ChangeQuantityCommand, Result<SubscriptionDto>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeQuantityCommandHandler(
        ISubscriptionRepository subscriptionRepository,
        IUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SubscriptionDto>> Handle(ChangeQuantityCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(request.SubscriptionId, cancellationToken);

        if (subscription is null)
            return Result.Failure<SubscriptionDto>(Error.NotFound("Subscription", request.SubscriptionId));

        try
        {
            subscription.ChangeQuantity(request.NewQuantity);
        }
        catch (InvalidSubscriptionStateException ex)
        {
            return Result.Failure<SubscriptionDto>(Error.Validation(ex.Message));
        }

        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(subscription.Adapt<SubscriptionDto>());
    }
}
