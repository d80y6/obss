using FluentValidation;

namespace Obss.Billing.Application.Commands.CreateTaxRule;

public sealed class CreateTaxRuleCommandValidator : AbstractValidator<CreateTaxRuleCommand>
{
    public CreateTaxRuleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.TaxRate)
            .GreaterThanOrEqualTo(0).WithMessage("Tax rate must be greater than or equal to 0.");

        RuleFor(x => x.TaxType)
            .NotEmpty().WithMessage("Tax type is required.")
            .Must(v => v is "Percentage" or "Fixed")
            .WithMessage("Tax type must be 'Percentage' or 'Fixed'.");

        RuleFor(x => x.TaxCategory)
            .NotEmpty().WithMessage("Tax category is required.")
            .MaximumLength(100).WithMessage("Tax category must not exceed 100 characters.");

        RuleFor(x => x.Country)
            .MaximumLength(10).WithMessage("Country must not exceed 10 characters.");

        RuleFor(x => x.Region)
            .MaximumLength(100).WithMessage("Region must not exceed 100 characters.");

        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0).WithMessage("Priority must be greater than or equal to 0.");

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty().WithMessage("Effective from date is required.");
    }
}
