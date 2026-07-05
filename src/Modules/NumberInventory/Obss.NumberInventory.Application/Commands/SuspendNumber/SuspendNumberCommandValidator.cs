using FluentValidation;

namespace Obss.NumberInventory.Application.Commands.SuspendNumber;

internal sealed class SuspendNumberCommandValidator : AbstractValidator<SuspendNumberCommand>
{
    public SuspendNumberCommandValidator()
    {
        RuleFor(x => x.NumberId)
            .NotEmpty().WithMessage("Number ID is required.");
    }
}
