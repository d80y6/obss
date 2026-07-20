using FluentValidation;

namespace Obss.Orders.Application.Commands.DedicatedServer;

public sealed class ResumeDedicatedServerCommandValidator : AbstractValidator<ResumeDedicatedServerCommand>
{
    public ResumeDedicatedServerCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
    }
}
