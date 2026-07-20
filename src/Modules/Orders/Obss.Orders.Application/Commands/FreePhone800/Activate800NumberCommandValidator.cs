using FluentValidation;

namespace Obss.Orders.Application.Commands.FreePhone800;

public sealed class Activate800NumberCommandValidator : AbstractValidator<Activate800NumberCommand>
{
    public Activate800NumberCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.FreePhoneNumber)
            .NotEmpty().WithMessage("Free phone number is required.")
            .MaximumLength(20).WithMessage("Free phone number must not exceed 20 characters.");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(200).WithMessage("Customer name must not exceed 200 characters.");

        RuleFor(x => x.CustomerNameAr)
            .NotEmpty().WithMessage("Customer name (Arabic) is required.")
            .MaximumLength(200).WithMessage("Customer name (Arabic) must not exceed 200 characters.");

        RuleFor(x => x.TerminatingNumbers)
            .NotEmpty().WithMessage("At least one terminating number is required.");

        RuleFor(x => x.TerminatingNumbers)
            .Must(t => t.All(n => !string.IsNullOrWhiteSpace(n)))
                .WithMessage("Terminating numbers must not be empty.")
            .When(x => x.TerminatingNumbers.Count > 0);

        RuleFor(x => x.MaxMonthlyMinutes)
            .GreaterThan(0).WithMessage("Maximum monthly minutes must be greater than zero.");

        RuleFor(x => x.ChargingParty)
            .NotEmpty().WithMessage("Charging party is required.")
            .Must(c => c == "subscriber" || c == "caller")
                .WithMessage("Charging party must be 'subscriber' or 'caller'.");
    }
}
