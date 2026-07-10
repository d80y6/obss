using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.ResumeProductOrderItem;

public sealed class ResumeProductOrderItemCommandHandler(IProductOrderRepository repository)
    : IRequestHandler<ResumeProductOrderItemCommand, Result>
{
    public async Task<Result> Handle(ResumeProductOrderItemCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(Error.NotFound("ProductOrder", request.OrderId));

        var item = order.Items.FirstOrDefault(i => i.Id == request.ItemId);
        if (item is null)
            return Result.Failure(Error.NotFound("Item", request.ItemId));

        try
        {
            item.Resume();
            await repository.UpdateAsync(order, cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }
    }
}
