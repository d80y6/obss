using FluentValidation;

namespace Obss.Orders.Application.Commands.WirelessTransmission;

public sealed class ChangeWirelessTransmissionCommandValidator : AbstractValidator<ChangeWirelessTransmissionCommand>
{
    public ChangeWirelessTransmissionCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");
    }
}
