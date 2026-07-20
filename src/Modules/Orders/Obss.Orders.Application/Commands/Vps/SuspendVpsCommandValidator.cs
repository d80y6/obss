using FluentValidation;

namespace Obss.Orders.Application.Commands.Vps;

public sealed class SuspendVpsCommandValidator : AbstractValidator<SuspendVpsCommand>
{
    public SuspendVpsCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
