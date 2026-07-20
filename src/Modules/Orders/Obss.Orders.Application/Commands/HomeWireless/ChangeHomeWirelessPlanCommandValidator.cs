using FluentValidation;

namespace Obss.Orders.Application.Commands.HomeWireless;

public sealed class ChangeHomeWirelessPlanCommandValidator : AbstractValidator<ChangeHomeWirelessPlanCommand>
{
    public ChangeHomeWirelessPlanCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.ApnName)
            .NotEmpty().WithMessage("APN name is required.")
            .MaximumLength(100).WithMessage("APN name must not exceed 100 characters.");

        RuleFor(x => x.QosProfile)
            .NotEmpty().WithMessage("QoS profile is required.")
            .MaximumLength(50).WithMessage("QoS profile must not exceed 50 characters.");
    }
}
