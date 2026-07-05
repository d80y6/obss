using FluentValidation;

namespace Obss.NetworkInventory.Application.Commands.RegisterONT;

internal sealed class RegisterONTCommandValidator : AbstractValidator<RegisterONTCommand>
{
    public RegisterONTCommandValidator()
    {
        RuleFor(x => x.OLTId)
            .NotEmpty().WithMessage("OLT ID is required.");

        RuleFor(x => x.PortNumber)
            .GreaterThan(0).WithMessage("Port number must be greater than zero.")
            .LessThanOrEqualTo(256).WithMessage("Port number must not exceed 256.");
    }
}
