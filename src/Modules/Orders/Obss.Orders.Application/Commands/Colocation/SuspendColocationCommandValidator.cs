using FluentValidation;

namespace Obss.Orders.Application.Commands.Colocation;

public sealed class SuspendColocationCommandValidator : AbstractValidator<SuspendColocationCommand>
{
    public SuspendColocationCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
