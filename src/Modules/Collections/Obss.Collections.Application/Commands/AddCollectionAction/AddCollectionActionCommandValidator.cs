using FluentValidation;

namespace Obss.Collections.Application.Commands.AddCollectionAction;

internal sealed class AddCollectionActionCommandValidator : AbstractValidator<AddCollectionActionCommand>
{
    public AddCollectionActionCommandValidator()
    {
        RuleFor(x => x.CollectionCaseId)
            .NotEmpty().WithMessage("Collection case ID is required.");

        RuleFor(x => x.ActionType)
            .NotEmpty().WithMessage("Action type is required.")
            .MaximumLength(100).WithMessage("Action type must not exceed 100 characters.");

        RuleFor(x => x.DunningLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Dunning level must be greater than or equal to 0.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.PerformedBy)
            .NotEmpty().WithMessage("Performed by is required.")
            .MaximumLength(200).WithMessage("Performed by must not exceed 200 characters.");
    }
}
