using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.RemoveItemRelationship;

public sealed class RemoveItemRelationshipCommandHandler(IProductOrderRepository repository)
    : IRequestHandler<RemoveItemRelationshipCommand, Result>
{
    public async Task<Result> Handle(RemoveItemRelationshipCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(Error.NotFound("ProductOrder", request.OrderId));

        try
        {
            order.RemoveItemRelationship(request.RelationshipId);
            await repository.UpdateAsync(order, cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }
    }
}
