using FluentValidation;

namespace Obss.Orders.Application.Commands.Adsl;

public sealed class ChangeAdslProfileCommandValidator : AbstractValidator<ChangeAdslProfileCommand>
{
    public ChangeAdslProfileCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.LineProfile)
            .NotEmpty().WithMessage("Line profile is required.")
            .MaximumLength(100).WithMessage("Line profile must not exceed 100 characters.");

        RuleFor(x => x.DownstreamSpeedKbps)
            .InclusiveBetween(64, 100_000).WithMessage("Downstream speed must be between 64 and 100,000 Kbps.");

        RuleFor(x => x.UpstreamSpeedKbps)
            .InclusiveBetween(64, 100_000).WithMessage("Upstream speed must be between 64 and 100,000 Kbps.");
    }
}
