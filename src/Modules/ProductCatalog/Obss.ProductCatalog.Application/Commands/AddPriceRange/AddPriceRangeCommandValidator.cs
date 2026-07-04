using FluentValidation;

namespace Obss.ProductCatalog.Application.Commands.AddPriceRange;

internal sealed class AddPriceRangeCommandValidator : AbstractValidator<AddPriceRangeCommand>
{
    public AddPriceRangeCommandValidator()
    {
        RuleFor(x => x.OfferPricingId)
            .NotEmpty().WithMessage("Offer pricing ID is required.");

        RuleFor(x => x.MinQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Min quantity must be greater than or equal to 0.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0.");
    }
}
