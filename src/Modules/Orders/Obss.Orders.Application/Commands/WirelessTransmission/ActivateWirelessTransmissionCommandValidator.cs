using FluentValidation;

namespace Obss.Orders.Application.Commands.WirelessTransmission;

public sealed class ActivateWirelessTransmissionCommandValidator : AbstractValidator<ActivateWirelessTransmissionCommand>
{
    public ActivateWirelessTransmissionCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.BandwidthMbps)
            .InclusiveBetween(1, 10000).WithMessage("Bandwidth must be between 1 and 10,000 Mbps.");

        RuleFor(x => x.EndpointA)
            .NotEmpty().WithMessage("Endpoint A is required.")
            .MaximumLength(200).WithMessage("Endpoint A must not exceed 200 characters.");

        RuleFor(x => x.EndpointB)
            .NotEmpty().WithMessage("Endpoint B is required.")
            .MaximumLength(200).WithMessage("Endpoint B must not exceed 200 characters.");

        RuleFor(x => x.AntennaType)
            .NotEmpty().WithMessage("Antenna type is required.")
            .MaximumLength(100).WithMessage("Antenna type must not exceed 100 characters.");

        RuleFor(x => x.FrequencyBand)
            .NotEmpty().WithMessage("Frequency band is required.")
            .MaximumLength(50).WithMessage("Frequency band must not exceed 50 characters.");

        RuleFor(x => x.RangeKm)
            .InclusiveBetween(0.1, 200).WithMessage("Range must be between 0.1 and 200 km.");

        RuleFor(x => x.SlaLevel)
            .NotEmpty().WithMessage("SLA level is required.")
            .Must(x => x is "standard" or "premium" or "enterprise").WithMessage("SLA level must be 'standard', 'premium', or 'enterprise'.");
    }
}
