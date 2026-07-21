using FluentValidation;

namespace Obss.AAA.Application.Commands.RegisterNas;

public sealed class RegisterNasCommandValidator : AbstractValidator<RegisterNasCommand>
{
    public RegisterNasCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("NAS name is required.")
            .MaximumLength(200).WithMessage("NAS name must not exceed 200 characters.");

        RuleFor(x => x.NasIpAddress)
            .NotEmpty().WithMessage("NAS IP address is required.")
            .Matches(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$").WithMessage("NAS IP address must be a valid IPv4 address.");

        RuleFor(x => x.NasSecret)
            .NotEmpty().WithMessage("NAS secret is required.")
            .MinimumLength(8).WithMessage("NAS secret must be at least 8 characters.");

        RuleFor(x => x.NasType)
            .IsInEnum().WithMessage("Invalid NAS type.");

        RuleFor(x => x.Location)
            .MaximumLength(500).WithMessage("Location must not exceed 500 characters.");
    }
}
