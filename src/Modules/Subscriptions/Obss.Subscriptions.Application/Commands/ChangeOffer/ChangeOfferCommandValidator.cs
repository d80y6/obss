using FluentValidation;

namespace Obss.Subscriptions.Application.Commands.ChangeOffer;

internal sealed class ChangeOfferCommandValidator : AbstractValidator<ChangeOfferCommand>
{
    public ChangeOfferCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.NewOfferId)
            .NotEmpty().WithMessage("New offer ID is required.");

        RuleFor(x => x.NewPrice)
            .GreaterThanOrEqualTo(0).WithMessage("New price must be zero or greater.");
    }
}
