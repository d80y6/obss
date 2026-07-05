using FluentValidation;

namespace Obss.CRM.Application.Commands.AddContact;

internal sealed class AddContactCommandValidator : AbstractValidator<AddContactCommand>
{
    public AddContactCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.CountryCode)
            .MaximumLength(10).WithMessage("Country code must not exceed 10 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.CountryCode));

        RuleFor(x => x.MobileNumber)
            .MaximumLength(20).WithMessage("Mobile number must not exceed 20 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.MobileNumber));

        RuleFor(x => x.MobileCountryCode)
            .MaximumLength(10).WithMessage("Mobile country code must not exceed 10 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.MobileCountryCode));

        RuleFor(x => x.Position)
            .MaximumLength(200).WithMessage("Position must not exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Position));
    }
}
