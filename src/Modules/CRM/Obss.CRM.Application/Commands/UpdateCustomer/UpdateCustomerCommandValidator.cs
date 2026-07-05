using FluentValidation;

namespace Obss.CRM.Application.Commands.UpdateCustomer;

internal sealed class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(200).WithMessage("Display name must not exceed 200 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.CompanyName)
            .MaximumLength(200).WithMessage("Company name must not exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.CompanyName));

        RuleFor(x => x.TaxNumber)
            .MaximumLength(50).WithMessage("Tax number must not exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.TaxNumber));

        RuleFor(x => x.RegistrationNumber)
            .MaximumLength(50).WithMessage("Registration number must not exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.RegistrationNumber));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.CountryCode)
            .MaximumLength(10).WithMessage("Country code must not exceed 10 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.CountryCode));

        RuleFor(x => x.Website)
            .MaximumLength(500).WithMessage("Website must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Website));
    }
}
