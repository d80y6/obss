using FluentValidation;

namespace Obss.Orders.Application.Commands.Adsl;

public sealed class ResumeAdslCommandValidator : AbstractValidator<ResumeAdslCommand>
{
    public ResumeAdslCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");
    }
}
