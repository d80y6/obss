using FluentValidation;

namespace Obss.NetworkInventory.Application.Commands.CheckCapacityThreshold;

internal sealed class CheckCapacityThresholdCommandValidator : AbstractValidator<CheckCapacityThresholdCommand>
{
    public CheckCapacityThresholdCommandValidator()
    {
        RuleFor(x => x.ElementId)
            .NotEmpty().WithMessage("Element ID is required.");

        RuleFor(x => x.Threshold)
            .InclusiveBetween(0, 100).WithMessage("Threshold must be between 0 and 100.");
    }
}
