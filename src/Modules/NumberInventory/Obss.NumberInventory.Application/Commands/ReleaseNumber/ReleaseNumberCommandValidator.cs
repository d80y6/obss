using FluentValidation;

namespace Obss.NumberInventory.Application.Commands.ReleaseNumber;

internal sealed class ReleaseNumberCommandValidator : AbstractValidator<ReleaseNumberCommand>
{
    public ReleaseNumberCommandValidator()
    {
        RuleFor(x => x.NumberId)
            .NotEmpty().WithMessage("Number ID is required.");
    }
}
