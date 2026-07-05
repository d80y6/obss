using Mapster;
using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.DTOs;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.Orders.Application.Commands.PartialUpdateOrder;

public sealed class PartialUpdateOrderCommandHandler : IRequestHandler<PartialUpdateOrderCommand, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PartialUpdateOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OrderDto>> Handle(PartialUpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
            return Result.Failure<OrderDto>(Error.NotFound("Order", request.Id));

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
                request.Priority,
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

            return Result.Success(order.Adapt<OrderDto>());
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<OrderDto>(Error.Validation(ex.Message));
        }
    }
}
