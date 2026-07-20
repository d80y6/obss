using FluentValidation;

namespace Obss.Orders.Application.Commands.HatifTawasol;

public sealed class ActivateHatifTawasolCommandValidator : AbstractValidator<ActivateHatifTawasolCommand>
{
    public ActivateHatifTawasolCommandValidator()
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

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(200).WithMessage("Customer name must not exceed 200 characters.");

        RuleFor(x => x.CustomerNameAr)
            .NotEmpty().WithMessage("Customer name (Arabic) is required.")
            .MaximumLength(200).WithMessage("Customer name (Arabic) must not exceed 200 characters.");

        RuleFor(x => x.ServicePackage)
            .NotEmpty().WithMessage("Service package is required.")
            .MaximumLength(100).WithMessage("Service package must not exceed 100 characters.");
    }
}
