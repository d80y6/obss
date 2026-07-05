using FluentValidation;

namespace Obss.ProductCatalog.Application.Commands.UpdateOfferPricing;

internal sealed class UpdateOfferPricingCommandValidator : AbstractValidator<UpdateOfferPricingCommand>
{
    public UpdateOfferPricingCommandValidator()
    {
        RuleFor(x => x.OfferId)
            .NotEmpty().WithMessage("Offer ID is required.");

        RuleFor(x => x.OfferPricingId)
            .NotEmpty().WithMessage("Offer pricing ID is required.");

        RuleFor(x => x.PricingType)
            .IsInEnum().WithMessage("Invalid pricing type.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(3).WithMessage("Currency must be a 3-letter code.");

        RuleFor(x => x.RecurringPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Recurring price must be zero or greater.");

        RuleFor(x => x.OneTimePrice)
            .GreaterThanOrEqualTo(0).WithMessage("One-time price must be zero or greater.");

        RuleFor(x => x.UsagePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Usage price must be zero or greater.");

        RuleFor(x => x.UnitOfMeasure)
            .MaximumLength(50).WithMessage("Unit of measure must not exceed 50 characters.")
            .When(x => x.UnitOfMeasure is not null);

        RuleFor(x => x.MinQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Min quantity must be zero or greater.")
            .When(x => x.MinQuantity.HasValue);

        RuleFor(x => x.MaxQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Max quantity must be zero or greater.")
            .When(x => x.MaxQuantity.HasValue);
    }
}
