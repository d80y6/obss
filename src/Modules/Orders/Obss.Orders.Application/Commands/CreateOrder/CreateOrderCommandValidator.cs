using FluentValidation;

namespace Obss.Orders.Application.Commands.CreateOrder;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(200).WithMessage("Customer name must not exceed 200 characters.");

        RuleFor(x => x.OrderType)
            .NotEmpty().WithMessage("Order type is required.")
            .Must(t => t is "New" or "Renewal" or "Change" or "Termination" or "Transfer")
            .WithMessage("Order type must be one of: New, Renewal, Change, Termination, Transfer.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(3).WithMessage("Currency must be a 3-letter code.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one order item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("Product ID is required.");

            item.RuleFor(i => i.OfferId)
                .NotEmpty().WithMessage("Offer ID is required.");

            item.RuleFor(i => i.ProductName)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(200);

            item.RuleFor(i => i.OfferName)
                .NotEmpty().WithMessage("Offer name is required.")
                .MaximumLength(200);

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

            item.RuleFor(i => i.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must not be negative.");

            item.RuleFor(i => i.BillingPeriod)
                .NotEmpty().WithMessage("Billing period is required.")
                .Must(b => b is "Monthly" or "Quarterly" or "SemiAnnual" or "Annual")
                .WithMessage("Billing period must be one of: Monthly, Quarterly, SemiAnnual, Annual.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.BillingAddressStreet), () =>
        {
            RuleFor(x => x.BillingAddressCity)
                .NotEmpty().WithMessage("Billing city is required when street is provided.");

            RuleFor(x => x.BillingAddressCountry)
                .NotEmpty().WithMessage("Billing country is required when street is provided.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.ShippingAddressStreet), () =>
        {
            RuleFor(x => x.ShippingAddressCity)
                .NotEmpty().WithMessage("Shipping city is required when street is provided.");

            RuleFor(x => x.ShippingAddressCountry)
                .NotEmpty().WithMessage("Shipping country is required when street is provided.");
        });
    }
}
