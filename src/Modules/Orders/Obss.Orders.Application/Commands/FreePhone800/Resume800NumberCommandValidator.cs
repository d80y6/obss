using FluentValidation;

namespace Obss.Orders.Application.Commands.FreePhone800;

public sealed class Resume800NumberCommandValidator : AbstractValidator<Resume800NumberCommand>
{
    public Resume800NumberCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");
    }
}
