using FluentValidation;

namespace Obss.Orders.Application.Commands.DomainRegistration;

public sealed class RegisterDomainCommandValidator : AbstractValidator<RegisterDomainCommand>
{
    public RegisterDomainCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.DomainName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Tld).NotEmpty().MaximumLength(20);
        RuleFor(x => x.RegistrationPeriodYears).InclusiveBetween(1, 10);
        RuleFor(x => x.Nameservers).NotEmpty();
        RuleFor(x => x.Nameservers.Length).LessThanOrEqualTo(13);
        RuleFor(x => x.RegistrantName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RegistrantEmail).NotEmpty().EmailAddress().MaximumLength(255);
    }
}
