using FluentValidation;

namespace Obss.Orders.Application.Commands.DedicatedServer;

public sealed class TerminateDedicatedServerCommandValidator : AbstractValidator<TerminateDedicatedServerCommand>
{
    public TerminateDedicatedServerCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
