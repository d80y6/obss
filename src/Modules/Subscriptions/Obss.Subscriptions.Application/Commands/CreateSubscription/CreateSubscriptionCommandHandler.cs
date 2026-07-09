using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Application.DTOs;
using Obss.Subscriptions.Domain.Entities;

namespace Obss.Subscriptions.Application.Commands.CreateSubscription;

public sealed class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, Result<SubscriptionDto>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSubscriptionCommandHandler(
        ISubscriptionRepository subscriptionRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SubscriptionDto>> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var product = Product.Create(
            Guid.Parse(request.TenantId),
            request.CustomerId,
            request.OfferName,
            null,
            null,
            request.OfferId);

        var subscription = Subscription.Create(
            request.TenantId,
            request.CustomerId,
            request.CustomerName,
            request.OrderId,
            request.OrderItemId,
            product.Id,
            request.OfferId,
            request.OfferName,
            request.BillingPeriod,
            request.Currency,
            request.Price,
            request.Quantity,
            request.StartDate,
            request.EndDate);

        await _productRepository.AddAsync(product, cancellationToken);
        await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(subscription.Adapt<SubscriptionDto>());
    }
}
