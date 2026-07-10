using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.CompleteOrderFulfillment;

public sealed class CompleteOrderFulfillmentCommandHandler : IRequestHandler<CompleteOrderFulfillmentCommand, Result>
{
    private readonly IOrderFulfillmentRepository _fulfillmentRepository;
    private readonly IProductOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteOrderFulfillmentCommandHandler(
        IOrderFulfillmentRepository fulfillmentRepository,
        IProductOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _fulfillmentRepository = fulfillmentRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CompleteOrderFulfillmentCommand request, CancellationToken cancellationToken)
    {
        var fulfillment = await _fulfillmentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (fulfillment is null)
            return Result.Failure(Error.NotFound("OrderFulfillment", request.OrderId));

        try
        {
            if (request.IsSuccess)
            {
                fulfillment.Complete();

                var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
                if (order is not null)
                {
                    order.MarkCompleted();
                    await _orderRepository.UpdateAsync(order, cancellationToken);
                }
            }
            else
            {
                fulfillment.Fail(request.ErrorMessage ?? "Unknown error");
            }

            await _fulfillmentRepository.UpdateAsync(fulfillment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }
    }
}
