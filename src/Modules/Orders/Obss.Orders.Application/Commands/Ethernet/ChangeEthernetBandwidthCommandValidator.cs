using FluentValidation;

namespace Obss.Orders.Application.Commands.Ethernet;

public sealed class ChangeEthernetBandwidthCommandValidator : AbstractValidator<ChangeEthernetBandwidthCommand>
{
    public ChangeEthernetBandwidthCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.NewBandwidthMbps)
            .InclusiveBetween(1, 100000).WithMessage("Bandwidth must be between 1 and 100,000 Mbps.");
    }
}
