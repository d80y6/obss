using Mapster;
using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Queries.GetProductOrderMilestones;

public sealed class GetProductOrderMilestonesQueryHandler(IProductOrderRepository repository)
    : IRequestHandler<GetProductOrderMilestonesQuery, Result<List<ProductOrderMilestoneDto>>>
{
    public async Task<Result<List<ProductOrderMilestoneDto>>> Handle(GetProductOrderMilestonesQuery request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<List<ProductOrderMilestoneDto>>(Error.NotFound("ProductOrder", request.OrderId));

        return Result.Success(order.Milestones.Adapt<List<ProductOrderMilestoneDto>>());
    }
}
