using FluentValidation;

namespace Obss.NumberInventory.Application.Commands.PortInNumber;

internal sealed class PortInNumberCommandValidator : AbstractValidator<PortInNumberCommand>
{
    public PortInNumberCommandValidator()
    {
        RuleFor(x => x.NumberId)
            .NotEmpty().WithMessage("Number ID is required.");
    }
}
