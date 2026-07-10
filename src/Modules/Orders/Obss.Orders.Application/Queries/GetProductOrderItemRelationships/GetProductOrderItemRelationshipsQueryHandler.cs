using Mapster;
using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Queries.GetProductOrderItemRelationships;

public sealed class GetProductOrderItemRelationshipsQueryHandler(IProductOrderRepository repository)
    : IRequestHandler<GetProductOrderItemRelationshipsQuery, Result<List<ProductOrderItemRelationshipDto>>>
{
    public async Task<Result<List<ProductOrderItemRelationshipDto>>> Handle(GetProductOrderItemRelationshipsQuery request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<List<ProductOrderItemRelationshipDto>>(Error.NotFound("ProductOrder", request.OrderId));

        var relationships = request.ItemId.HasValue
            ? order.GetItemRelationships(request.ItemId.Value).ToList()
            : order.ItemRelationships.ToList();

        return Result.Success(relationships.Adapt<List<ProductOrderItemRelationshipDto>>());
    }
}
