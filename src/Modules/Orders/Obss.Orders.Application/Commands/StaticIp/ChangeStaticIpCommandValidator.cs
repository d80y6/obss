using FluentValidation;

namespace Obss.Orders.Application.Commands.StaticIp;

public sealed class ChangeStaticIpCommandValidator : AbstractValidator<ChangeStaticIpCommand>
{
    public ChangeStaticIpCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");
    }
}
