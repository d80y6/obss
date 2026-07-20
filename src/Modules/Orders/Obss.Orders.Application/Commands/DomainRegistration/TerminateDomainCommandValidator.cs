using FluentValidation;

namespace Obss.Orders.Application.Commands.DomainRegistration;

public sealed class TerminateDomainCommandValidator : AbstractValidator<TerminateDomainCommand>
{
    public TerminateDomainCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
