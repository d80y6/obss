using FluentValidation;

namespace Obss.Orders.Application.Commands.Lte;

public sealed class Change4GPlanCommandValidator : AbstractValidator<Change4GPlanCommand>
{
    public Change4GPlanCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.ApnName)
            .NotEmpty().WithMessage("APN name is required.")
            .MaximumLength(100).WithMessage("APN name must not exceed 100 characters.");

        RuleFor(x => x.QosProfile)
            .NotEmpty().WithMessage("QoS profile is required.")
            .MaximumLength(50).WithMessage("QoS profile must not exceed 50 characters.");

        RuleFor(x => x.DataAllowanceMb)
            .InclusiveBetween(1, 1_000_000).WithMessage("Data allowance must be between 1 and 1,000,000 MB.");

        RuleFor(x => x.ValidityDays)
            .InclusiveBetween(1, 365).WithMessage("Validity days must be between 1 and 365.");
    }
}
