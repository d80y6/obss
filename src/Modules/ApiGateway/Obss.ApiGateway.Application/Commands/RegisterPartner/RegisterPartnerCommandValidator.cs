using FluentValidation;

namespace Obss.ApiGateway.Application.Commands.RegisterPartner;

public sealed class RegisterPartnerCommandValidator : AbstractValidator<RegisterPartnerCommand>
{
    public RegisterPartnerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.ContactName)
            .NotEmpty().WithMessage("Contact name is required.")
            .MaximumLength(200).WithMessage("Contact name must not exceed 200 characters.");

        RuleFor(x => x.ContactEmail)
            .NotEmpty().WithMessage("Contact email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(256).WithMessage("Contact email must not exceed 256 characters.");

        RuleFor(x => x.MaxRequestsPerDay)
            .GreaterThan(0).WithMessage("Max requests per day must be greater than 0.");
    }
}
