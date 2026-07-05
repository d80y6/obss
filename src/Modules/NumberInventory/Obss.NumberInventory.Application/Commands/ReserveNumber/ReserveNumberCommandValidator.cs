using FluentValidation;

namespace Obss.NumberInventory.Application.Commands.ReserveNumber;

internal sealed class ReserveNumberCommandValidator : AbstractValidator<ReserveNumberCommand>
{
    public ReserveNumberCommandValidator()
    {
        RuleFor(x => x.NumberId)
            .NotEmpty().WithMessage("Number ID is required.");
    }
}
