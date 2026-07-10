using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.AcknowledgeProductOrderItem;

public sealed class AcknowledgeProductOrderItemCommandHandler(IProductOrderRepository repository)
    : IRequestHandler<AcknowledgeProductOrderItemCommand, Result>
{
    public async Task<Result> Handle(AcknowledgeProductOrderItemCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(Error.NotFound("ProductOrder", request.OrderId));

        var item = order.Items.FirstOrDefault(i => i.Id == request.ItemId);
        if (item is null)
            return Result.Failure(Error.NotFound("Item", request.ItemId));

        try
        {
            item.Acknowledge();
            await repository.UpdateAsync(order, cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }
    }
}
