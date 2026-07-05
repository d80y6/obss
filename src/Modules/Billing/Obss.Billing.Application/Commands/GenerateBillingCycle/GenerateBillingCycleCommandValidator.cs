using FluentValidation;

namespace Obss.Billing.Application.Commands.GenerateBillingCycle;

internal sealed class GenerateBillingCycleCommandValidator : AbstractValidator<GenerateBillingCycleCommand>
{
    public GenerateBillingCycleCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.BillingPeriod)
            .NotEmpty().WithMessage("Billing period is required.")
            .Must(v => v is "Monthly" or "Quarterly" or "SemiAnnual" or "Annual")
            .WithMessage("Billing period must be Monthly, Quarterly, SemiAnnual, or Annual.");

        RuleFor(x => x.LastBillingDate)
            .NotEmpty().WithMessage("Last billing date is required.")
            .LessThan(x => x.NextBillingDate).WithMessage("Last billing date must be before next billing date.");

        RuleFor(x => x.NextBillingDate)
            .NotEmpty().WithMessage("Next billing date is required.")
            .GreaterThan(x => x.LastBillingDate).WithMessage("Next billing date must be after last billing date.");
    }
}
