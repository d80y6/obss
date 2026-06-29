using FluentValidation;

namespace Obss.Billing.Application.Commands.GenerateBill;

public sealed class GenerateBillCommandValidator : AbstractValidator<GenerateBillCommand>
{
    public GenerateBillCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(200).WithMessage("Customer name must not exceed 200 characters.");

        RuleFor(x => x.BillingPeriod)
            .NotEmpty().WithMessage("Billing period is required.")
            .Must(v => v is "Monthly" or "Quarterly" or "SemiAnnual" or "Annual")
            .WithMessage("Billing period must be Monthly, Quarterly, SemiAnnual, or Annual.");

        RuleFor(x => x.PeriodStart)
            .LessThan(x => x.PeriodEnd).WithMessage("Period start must be before period end.");

        RuleFor(x => x.DueDate)
            .GreaterThan(x => x.PeriodEnd).WithMessage("Due date must be after the billing period end.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(3).WithMessage("Currency must not exceed 3 characters.");
    }
}
