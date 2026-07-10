using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.AddItemRelationship;

public sealed class AddItemRelationshipCommandHandler(IProductOrderRepository repository)
    : IRequestHandler<AddItemRelationshipCommand, Result>
{
    public async Task<Result> Handle(AddItemRelationshipCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(Error.NotFound("ProductOrder", request.OrderId));

        if (!Enum.TryParse<RelationshipType>(request.Type, true, out var type))
            return Result.Failure(Error.Validation("Invalid relationship type"));

        try
        {
            order.AddItemRelationship(request.ItemId, request.TargetItemId, type);
            await repository.UpdateAsync(order, cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }
    }
}
