using FluentValidation;

namespace Obss.NumberInventory.Application.Commands.AddNumber;

public sealed class AddNumberCommandValidator : AbstractValidator<AddNumberCommand>
{
    public AddNumberCommandValidator()
    {
        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("Number is required.")
            .Matches(@"^\+?[1-9]\d{6,14}$").WithMessage("Number must be a valid E.164 format.");

        RuleFor(x => x.NumberType)
            .IsInEnum().WithMessage("Invalid number type.");

        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0).WithMessage("Cost must be non-negative.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(3).WithMessage("Currency must be a 3-letter code.");
    }
}
