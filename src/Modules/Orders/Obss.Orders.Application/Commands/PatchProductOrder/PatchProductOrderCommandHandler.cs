using Mapster;
using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.DTOs;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.Orders.Application.Commands.PatchProductOrder;

public sealed class PatchProductOrderCommandHandler : IRequestHandler<PatchProductOrderCommand, Result<ProductOrderDto>>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PatchProductOrderCommandHandler(IProductOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductOrderDto>> Handle(PatchProductOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
            return Result.Failure<ProductOrderDto>(Error.NotFound("ProductOrder", request.Id));

        Priority? priority = null;
        if (request.Priority is not null && Enum.TryParse<Priority>(request.Priority, true, out var parsedPriority))
            priority = parsedPriority;

        Address? billingAddress = null;
        if (request.BillingAddressStreet is not null)
        {
            billingAddress = Address.Create(
                request.BillingAddressStreet,
                request.BillingAddressCity ?? string.Empty,
                request.BillingAddressState,
                request.BillingAddressPostalCode,
                request.BillingAddressCountry ?? string.Empty);
        }

        Address? shippingAddress = null;
        if (request.ShippingAddressStreet is not null)
        {
            shippingAddress = Address.Create(
                request.ShippingAddressStreet,
                request.ShippingAddressCity ?? string.Empty,
                request.ShippingAddressState,
                request.ShippingAddressPostalCode,
                request.ShippingAddressCountry ?? string.Empty);
        }

        try
        {
            order.UpdateDetails(
                request.Description,
                request.Channel,
                priority,
                request.Notes,
                request.RequestedStartDate,
                request.RequestedCompletionDate,
                request.ExpectedCompletionDate,
                request.NotificationContact,
                request.ExternalId,
                request.QuoteId,
                billingAddress,
                shippingAddress);

            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(order.Adapt<ProductOrderDto>());
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<ProductOrderDto>(Error.Validation(ex.Message));
        }
    }
}
