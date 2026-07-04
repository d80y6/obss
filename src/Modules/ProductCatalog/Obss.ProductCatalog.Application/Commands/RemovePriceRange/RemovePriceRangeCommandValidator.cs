using FluentValidation;

namespace Obss.ProductCatalog.Application.Commands.RemovePriceRange;

internal sealed class RemovePriceRangeCommandValidator : AbstractValidator<RemovePriceRangeCommand>
{
    public RemovePriceRangeCommandValidator()
    {
        RuleFor(x => x.OfferPricingId)
            .NotEmpty().WithMessage("Offer pricing ID is required.");

        RuleFor(x => x.PriceRangeId)
            .NotEmpty().WithMessage("Price range ID is required.");
    }
}
