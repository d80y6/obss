using FluentValidation;

namespace Obss.NumberInventory.Application.Commands.DisconnectNumber;

internal sealed class DisconnectNumberCommandValidator : AbstractValidator<DisconnectNumberCommand>
{
    public DisconnectNumberCommandValidator()
    {
        RuleFor(x => x.NumberId)
            .NotEmpty().WithMessage("Number ID is required.");
    }
}
