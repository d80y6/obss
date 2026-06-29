using FluentValidation;

namespace Obss.Subscriptions.Application.Commands.CreateSubscription;

public sealed class CreateSubscriptionCommandValidator : AbstractValidator<CreateSubscriptionCommand>
{
    public CreateSubscriptionCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(200).WithMessage("Customer name must not exceed 200 characters.");

        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.OfferId)
            .NotEmpty().WithMessage("Offer ID is required.");

        RuleFor(x => x.OfferName)
            .NotEmpty().WithMessage("Offer name is required.")
            .MaximumLength(200).WithMessage("Offer name must not exceed 200 characters.");

        RuleFor(x => x.BillingPeriod)
            .IsInEnum().WithMessage("Invalid billing period.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(3).WithMessage("Currency must be a 3-letter code.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be non-negative.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");
    }
}
