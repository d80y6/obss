using FluentValidation;

namespace Obss.Orders.Application.Commands.RemoveMilestone;

public sealed class RemoveMilestoneCommandValidator : AbstractValidator<RemoveMilestoneCommand>
{
    public RemoveMilestoneCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.MilestoneId).NotEmpty();
    }
}
