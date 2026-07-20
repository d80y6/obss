using FluentValidation;

namespace Obss.Orders.Application.Commands.Lte;

public sealed class Resume4GCommandValidator : AbstractValidator<Resume4GCommand>
{
    public Resume4GCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");
    }
}
