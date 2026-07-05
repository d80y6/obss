using FluentValidation;
using Obss.CRM.Domain.ValueObjects;

namespace Obss.CRM.Application.Commands.UpdateOrganization;

internal sealed class UpdateOrganizationValidator : AbstractValidator<UpdateOrganizationCommand>
{
    public UpdateOrganizationValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.TradingName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CompanyType).NotEmpty().Must(t => Enum.TryParse<CompanyType>(t, true, out _))
            .WithMessage("Invalid company type.");
        RuleFor(x => x.Industry).MaximumLength(100);
        RuleFor(x => x.RegistrationNumber).MaximumLength(100);
        RuleFor(x => x.TaxNumber).MaximumLength(50);
        RuleFor(x => x.CountryOfRegistration).MaximumLength(3);
    }
}
