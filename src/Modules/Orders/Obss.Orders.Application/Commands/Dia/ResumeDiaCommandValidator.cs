using FluentValidation;

namespace Obss.Orders.Application.Commands.Dia;

public sealed class ResumeDiaCommandValidator : AbstractValidator<ResumeDiaCommand>
{
    public ResumeDiaCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");
    }
}
