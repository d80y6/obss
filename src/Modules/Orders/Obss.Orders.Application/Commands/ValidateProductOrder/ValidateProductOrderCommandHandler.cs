using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.Services;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.ValidateProductOrder;

public sealed class ValidateProductOrderCommandHandler : IRequestHandler<ValidateProductOrderCommand, Result<OrderValidationResult>>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly OrderValidationService _validationService;

    public ValidateProductOrderCommandHandler(
        IProductOrderRepository orderRepository,
        OrderValidationService validationService)
    {
        _orderRepository = orderRepository;
        _validationService = validationService;
    }

    public async Task<Result<OrderValidationResult>> Handle(ValidateProductOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<OrderValidationResult>(Error.NotFound("ProductOrder", request.OrderId));

        var result = await _validationService.ValidateAsync(order, cancellationToken);
        return Result.Success(result);
    }
}
