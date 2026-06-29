using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.Services;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.ValidateOrder;

public sealed class ValidateOrderCommandHandler : IRequestHandler<ValidateOrderCommand, Result<OrderValidationResult>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly OrderValidationService _validationService;

    public ValidateOrderCommandHandler(
        IOrderRepository orderRepository,
        OrderValidationService validationService)
    {
        _orderRepository = orderRepository;
        _validationService = validationService;
    }

    public async Task<Result<OrderValidationResult>> Handle(ValidateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<OrderValidationResult>(Error.NotFound("Order", request.OrderId));

        var result = await _validationService.ValidateAsync(order, cancellationToken);
        return Result.Success(result);
    }
}
