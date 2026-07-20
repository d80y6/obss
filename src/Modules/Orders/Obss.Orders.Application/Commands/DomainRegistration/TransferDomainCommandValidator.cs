using FluentValidation;

namespace Obss.Orders.Application.Commands.DomainRegistration;

public sealed class TransferDomainCommandValidator : AbstractValidator<TransferDomainCommand>
{
    public TransferDomainCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.AuthCode).NotEmpty().MaximumLength(100);
    }
}
