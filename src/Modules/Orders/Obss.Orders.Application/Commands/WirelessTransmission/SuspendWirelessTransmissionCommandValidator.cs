using FluentValidation;

namespace Obss.Orders.Application.Commands.WirelessTransmission;

public sealed class SuspendWirelessTransmissionCommandValidator : AbstractValidator<SuspendWirelessTransmissionCommand>
{
    public SuspendWirelessTransmissionCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
    }
}
