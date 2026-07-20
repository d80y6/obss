using FluentValidation;

namespace Obss.Orders.Application.Commands.Pri;

public sealed class ActivatePriTrunkCommandValidator : AbstractValidator<ActivatePriTrunkCommand>
{
    public ActivatePriTrunkCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.ChannelCount)
            .InclusiveBetween(1, 30).WithMessage("Channel count must be between 1 and 30.");

        RuleFor(x => x.SignalingProtocol)
            .NotEmpty().WithMessage("Signaling protocol is required.")
            .Must(x => x is "ISDN" or "QSIG" or "SS7").WithMessage("Signaling protocol must be 'ISDN', 'QSIG', or 'SS7'.");

        RuleFor(x => x.TrunkGroup)
            .NotEmpty().WithMessage("Trunk group is required.")
            .MaximumLength(100).WithMessage("Trunk group must not exceed 100 characters.");

        RuleFor(x => x.Endpoint)
            .NotEmpty().WithMessage("Endpoint is required.")
            .MaximumLength(200).WithMessage("Endpoint must not exceed 200 characters.");

        RuleFor(x => x.RoutingNumber)
            .NotEmpty().WithMessage("Routing number is required.")
            .MaximumLength(50).WithMessage("Routing number must not exceed 50 characters.");
    }
}
