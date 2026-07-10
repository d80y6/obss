using FluentValidation;

namespace Obss.Orders.Application.Commands.AddProductOrderItem;

public sealed class AddProductOrderItemCommandValidator : AbstractValidator<AddProductOrderItemCommand>
{
    public AddProductOrderItemCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.OfferId)
            .NotEmpty().WithMessage("Offer ID is required.");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

        RuleFor(x => x.OfferName)
            .NotEmpty().WithMessage("Offer name is required.")
            .MaximumLength(200).WithMessage("Offer name must not exceed 200 characters.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price must not be negative.");

        RuleFor(x => x.RecurringPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Recurring price must not be negative.");

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount amount must not be negative.");

        RuleFor(x => x.TaxAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Tax amount must not be negative.");

        RuleFor(x => x.BillingPeriod)
            .NotEmpty().WithMessage("Billing period is required.")
            .Must(b => b is "Monthly" or "Quarterly" or "SemiAnnual" or "Annual")
            .WithMessage("Billing period must be one of: Monthly, Quarterly, SemiAnnual, Annual.");

        When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
        {
            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate!.Value)
                .WithMessage("End date must be after start date.");
        });
    }
}
