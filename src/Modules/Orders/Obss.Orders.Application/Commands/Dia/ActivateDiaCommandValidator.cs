using FluentValidation;

namespace Obss.Orders.Application.Commands.Dia;

public sealed class ActivateDiaCommandValidator : AbstractValidator<ActivateDiaCommand>
{
    public ActivateDiaCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.BandwidthMbps)
            .InclusiveBetween(1, 100000).WithMessage("Bandwidth must be between 1 and 100,000 Mbps.");

        RuleFor(x => x.InterfaceType)
            .NotEmpty().WithMessage("Interface type is required.")
            .Must(x => x is "Ethernet" or "Fiber").WithMessage("Interface type must be 'Ethernet' or 'Fiber'.");

        RuleFor(x => x.HandoffLocation)
            .NotEmpty().WithMessage("Handoff location is required.")
            .MaximumLength(200).WithMessage("Handoff location must not exceed 200 characters.");

        RuleFor(x => x.StaticIpCount)
            .InclusiveBetween(0, 256).WithMessage("Static IP count must be between 0 and 256.");

        RuleFor(x => x.SlaLevel)
            .NotEmpty().WithMessage("SLA level is required.")
            .Must(x => x is "standard" or "premium" or "enterprise").WithMessage("SLA level must be 'standard', 'premium', or 'enterprise'.");
    }
}
