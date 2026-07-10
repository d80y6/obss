using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.CreateMilestone;

public sealed class CreateMilestoneCommandHandler(IProductOrderRepository repository)
    : IRequestHandler<CreateMilestoneCommand, Result>
{
    public async Task<Result> Handle(CreateMilestoneCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(Error.NotFound("ProductOrder", request.OrderId));

        var milestone = new ProductOrderMilestone(request.OrderId, request.Name, request.Description, request.MilestoneDate);
        order.AddMilestone(milestone);
        await repository.UpdateAsync(order, cancellationToken);
        return Result.Success();
    }
}
