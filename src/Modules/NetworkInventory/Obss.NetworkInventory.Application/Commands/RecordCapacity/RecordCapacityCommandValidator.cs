using FluentValidation;

namespace Obss.NetworkInventory.Application.Commands.RecordCapacity;

internal sealed class RecordCapacityCommandValidator : AbstractValidator<RecordCapacityCommand>
{
    public RecordCapacityCommandValidator()
    {
        RuleFor(x => x.ElementId)
            .NotEmpty().WithMessage("Element ID is required.");

        RuleFor(x => x.CapacityType)
            .NotEmpty().WithMessage("Capacity type is required.")
            .MaximumLength(100).WithMessage("Capacity type must not exceed 100 characters.");

        RuleFor(x => x.TotalCapacity)
            .GreaterThan(0).WithMessage("Total capacity must be greater than zero.");

        RuleFor(x => x.UsedCapacity)
            .GreaterThanOrEqualTo(0).WithMessage("Used capacity must be non-negative.");

        RuleFor(x => x.UsedCapacity)
            .LessThanOrEqualTo(x => x.TotalCapacity)
            .WithMessage("Used capacity must not exceed total capacity.");
    }
}
