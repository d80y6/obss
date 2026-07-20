using FluentValidation;

namespace Obss.Orders.Application.Commands.HatifTawasol;

public sealed class ChangeHatifTawasolFeaturesCommandValidator : AbstractValidator<ChangeHatifTawasolFeaturesCommand>
{
    public ChangeHatifTawasolFeaturesCommandValidator()
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

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
    }
}
