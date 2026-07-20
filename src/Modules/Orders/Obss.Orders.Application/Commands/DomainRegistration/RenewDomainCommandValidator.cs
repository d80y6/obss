using FluentValidation;

namespace Obss.Orders.Application.Commands.DomainRegistration;

public sealed class RenewDomainCommandValidator : AbstractValidator<RenewDomainCommand>
{
    public RenewDomainCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.RenewalPeriodYears).InclusiveBetween(1, 10);
    }
}
