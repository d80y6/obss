using FluentValidation;
using Obss.Orders.Domain.ValueObjects;

namespace Obss.Orders.Application.Commands.UpdateMilestone;

public sealed class UpdateMilestoneCommandValidator : AbstractValidator<UpdateMilestoneCommand>
{
    public UpdateMilestoneCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.MilestoneId).NotEmpty();
        RuleFor(x => x.Status)
            .Must(s => string.IsNullOrWhiteSpace(s) || Enum.TryParse<MilestoneStatus>(s, true, out _))
            .WithMessage("Status must be one of: Pending, Achieved, Missed, Cancelled");
    }
}
