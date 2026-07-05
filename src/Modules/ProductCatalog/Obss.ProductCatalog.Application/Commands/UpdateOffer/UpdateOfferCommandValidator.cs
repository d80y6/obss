using FluentValidation;

namespace Obss.ProductCatalog.Application.Commands.UpdateOffer;

internal sealed class UpdateOfferCommandValidator : AbstractValidator<UpdateOfferCommand>
{
    public UpdateOfferCommandValidator()
    {
        RuleFor(x => x.OfferId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OfferType).IsInEnum();
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
