using FluentValidation;

namespace Obss.Orders.Application.Commands.SupplementaryTelephone;

public sealed class ActivateSupplementaryServiceCommandValidator : AbstractValidator<ActivateSupplementaryServiceCommand>
{
    public ActivateSupplementaryServiceCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.TelephoneNumber)
            .NotEmpty().WithMessage("Telephone number is required.")
            .MaximumLength(20).WithMessage("Telephone number must not exceed 20 characters.");

        RuleFor(x => x.ServiceFeature)
            .IsInEnum().WithMessage("Invalid service feature.");

        RuleFor(x => x.Configuration)
            .NotNull().WithMessage("Configuration is required.");
    }
}
