using FluentValidation;

namespace Obss.Orders.Application.Commands.HatifFawtara;

public sealed class ChangeHatifFawtaraPlanCommandValidator : AbstractValidator<ChangeHatifFawtaraPlanCommand>
{
    public ChangeHatifFawtaraPlanCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.TelephoneNumber)
            .NotEmpty().WithMessage("Telephone number is required.")
            .MaximumLength(20).WithMessage("Telephone number must not exceed 20 characters.");

        RuleFor(x => x.BillingCycle)
            .NotEmpty().WithMessage("Billing cycle is required.")
            .MaximumLength(50).WithMessage("Billing cycle must not exceed 50 characters.");

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Credit limit must be zero or greater.");

        RuleFor(x => x.ServicePackage)
            .NotEmpty().WithMessage("Service package is required.")
            .MaximumLength(100).WithMessage("Service package must not exceed 100 characters.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
    }
}
