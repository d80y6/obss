using FluentValidation;

namespace Obss.Orders.Application.Commands.DomainRegistration;

public sealed class UpdateNameserversCommandValidator : AbstractValidator<UpdateNameserversCommand>
{
    public UpdateNameserversCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.Nameservers).NotEmpty();
        RuleFor(x => x.Nameservers.Length).LessThanOrEqualTo(13);
    }
}
