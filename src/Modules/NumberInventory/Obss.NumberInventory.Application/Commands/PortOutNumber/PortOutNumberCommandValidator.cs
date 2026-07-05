using FluentValidation;

namespace Obss.NumberInventory.Application.Commands.PortOutNumber;

internal sealed class PortOutNumberCommandValidator : AbstractValidator<PortOutNumberCommand>
{
    public PortOutNumberCommandValidator()
    {
        RuleFor(x => x.NumberId)
            .NotEmpty().WithMessage("Number ID is required.");
    }
}
