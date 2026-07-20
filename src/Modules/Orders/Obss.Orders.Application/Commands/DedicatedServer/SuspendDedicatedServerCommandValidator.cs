using FluentValidation;

namespace Obss.Orders.Application.Commands.DedicatedServer;

public sealed class SuspendDedicatedServerCommandValidator : AbstractValidator<SuspendDedicatedServerCommand>
{
    public SuspendDedicatedServerCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
