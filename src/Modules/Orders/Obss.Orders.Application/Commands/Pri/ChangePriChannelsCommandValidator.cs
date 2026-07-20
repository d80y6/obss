using FluentValidation;

namespace Obss.Orders.Application.Commands.Pri;

public sealed class ChangePriChannelsCommandValidator : AbstractValidator<ChangePriChannelsCommand>
{
    public ChangePriChannelsCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.NewChannelCount)
            .InclusiveBetween(1, 30).WithMessage("Channel count must be between 1 and 30.");
    }
}
