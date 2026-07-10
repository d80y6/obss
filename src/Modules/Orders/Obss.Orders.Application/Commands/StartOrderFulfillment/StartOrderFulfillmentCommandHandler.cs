using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.DTOs;
using Obss.Orders.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.StartOrderFulfillment;

public sealed class StartOrderFulfillmentCommandHandler : IRequestHandler<StartOrderFulfillmentCommand, Result<OrderFulfillmentDto>>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IOrderFulfillmentRepository _fulfillmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StartOrderFulfillmentCommandHandler> _logger;

    public StartOrderFulfillmentCommandHandler(
        IProductOrderRepository orderRepository,
        IOrderFulfillmentRepository fulfillmentRepository,
        IUnitOfWork unitOfWork,
        ILogger<StartOrderFulfillmentCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _fulfillmentRepository = fulfillmentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<OrderFulfillmentDto>> Handle(StartOrderFulfillmentCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<OrderFulfillmentDto>(Error.NotFound("ProductOrder", request.OrderId));

        var existingFulfillment = await _fulfillmentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (existingFulfillment is not null)
            return Result.Failure<OrderFulfillmentDto>(Error.Validation("Fulfillment already exists for this order."));

        try
        {
            order.StartFulfillment();

            var fulfillment = OrderFulfillment.Create(request.OrderId);
            fulfillment.StartFulfillment(Guid.NewGuid());

            await _fulfillmentRepository.AddAsync(fulfillment, cancellationToken);
            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Fulfillment started for order {OrderId} with fulfillment {FulfillmentId}",
                request.OrderId,
                fulfillment.Id);

            return Result.Success(fulfillment.Adapt<OrderFulfillmentDto>());
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<OrderFulfillmentDto>(Error.Validation(ex.Message));
        }
    }
}
