using FluentValidation;

namespace Obss.Orders.Application.Commands.StaticIp;

public sealed class AllocateStaticIpCommandValidator : AbstractValidator<AllocateStaticIpCommand>
{
    public AllocateStaticIpCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.IpVersion)
            .NotEmpty().WithMessage("IP version is required.")
            .Must(x => x is "IPv4" or "IPv6").WithMessage("IP version must be 'IPv4' or 'IPv6'.");

        RuleFor(x => x.IpCount)
            .InclusiveBetween(1, 256).WithMessage("IP count must be between 1 and 256.");

        RuleFor(x => x.CustomerSite)
            .NotEmpty().WithMessage("Customer site is required.")
            .MaximumLength(200).WithMessage("Customer site must not exceed 200 characters.");
    }
}
