using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.UpdateMilestone;

public sealed class UpdateMilestoneCommandHandler(IProductOrderRepository repository)
    : IRequestHandler<UpdateMilestoneCommand, Result>
{
    public async Task<Result> Handle(UpdateMilestoneCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(Error.NotFound("ProductOrder", request.OrderId));

        var milestone = order.Milestones.FirstOrDefault(m => m.Id == request.MilestoneId);
        if (milestone is null)
            return Result.Failure(Error.NotFound("Milestone", request.MilestoneId));

        if (request.MilestoneDate.HasValue)
        {
            typeof(ProductOrderMilestone)
                .GetProperty(nameof(ProductOrderMilestone.MilestoneDate))!
                .SetValue(milestone, request.MilestoneDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<MilestoneStatus>(request.Status, true, out var status))
        {
            try
            {
                switch (status)
                {
                    case MilestoneStatus.Achieved:
                        milestone.Achieve();
                        break;
                    case MilestoneStatus.Missed:
                        milestone.MarkMissed();
                        break;
                    case MilestoneStatus.Cancelled:
                        milestone.Cancel();
                        break;
                }
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(Error.Validation(ex.Message));
            }
        }

        await repository.UpdateAsync(order, cancellationToken);
        return Result.Success();
    }
}
