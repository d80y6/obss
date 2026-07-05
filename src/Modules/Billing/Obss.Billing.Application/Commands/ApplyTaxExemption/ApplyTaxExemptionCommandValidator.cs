using FluentValidation;

namespace Obss.Billing.Application.Commands.ApplyTaxExemption;

internal sealed class ApplyTaxExemptionCommandValidator : AbstractValidator<ApplyTaxExemptionCommand>
{
    public ApplyTaxExemptionCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.TaxRuleId)
            .NotEmpty().WithMessage("Tax rule ID is required.");

        RuleFor(x => x.ExemptionCertificate)
            .NotEmpty().WithMessage("Exemption certificate is required.")
            .MaximumLength(200).WithMessage("Exemption certificate must not exceed 200 characters.");

        RuleFor(x => x.ExemptionRate)
            .GreaterThanOrEqualTo(0).WithMessage("Exemption rate must be greater than or equal to 0.")
            .LessThanOrEqualTo(100).WithMessage("Exemption rate must not exceed 100.");

        RuleFor(x => x.ValidFrom)
            .LessThan(x => x.ValidTo).WithMessage("Valid from date must be before valid to date.");

        RuleFor(x => x.ValidTo)
            .GreaterThan(x => x.ValidFrom).WithMessage("Valid to date must be after valid from date.");

        RuleFor(x => x.ApprovedBy)
            .NotEmpty().WithMessage("Approved by is required.")
            .MaximumLength(200).WithMessage("Approved by must not exceed 200 characters.");
    }
}
