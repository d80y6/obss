using FluentValidation;

namespace Obss.Orders.Application.Commands.Ethernet;

public sealed class ActivateEthernetCommandValidator : AbstractValidator<ActivateEthernetCommand>
{
    public ActivateEthernetCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.BandwidthMbps)
            .InclusiveBetween(1, 100000).WithMessage("Bandwidth must be between 1 and 100,000 Mbps.");

        RuleFor(x => x.VlanId)
            .InclusiveBetween(1, 4094).WithMessage("VLAN ID must be between 1 and 4094.");

        RuleFor(x => x.Encapsulation)
            .NotEmpty().WithMessage("Encapsulation type is required.")
            .Must(x => x is "QinQ" or "Dot1Q").WithMessage("Encapsulation must be 'QinQ' or 'Dot1Q'.");

        RuleFor(x => x.EndpointA)
            .NotEmpty().WithMessage("Endpoint A is required.")
            .MaximumLength(200).WithMessage("Endpoint A must not exceed 200 characters.");

        RuleFor(x => x.EndpointB)
            .NotEmpty().WithMessage("Endpoint B is required.")
            .MaximumLength(200).WithMessage("Endpoint B must not exceed 200 characters.");
    }
}
