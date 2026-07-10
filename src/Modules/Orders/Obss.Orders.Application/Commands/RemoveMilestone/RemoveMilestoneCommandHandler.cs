using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.RemoveMilestone;

public sealed class RemoveMilestoneCommandHandler(IProductOrderRepository repository)
    : IRequestHandler<RemoveMilestoneCommand, Result>
{
    public async Task<Result> Handle(RemoveMilestoneCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(Error.NotFound("ProductOrder", request.OrderId));

        try
        {
            order.RemoveMilestone(request.MilestoneId);
            await repository.UpdateAsync(order, cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }
    }
}
