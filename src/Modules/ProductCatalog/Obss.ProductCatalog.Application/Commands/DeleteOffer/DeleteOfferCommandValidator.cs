using FluentValidation;

namespace Obss.ProductCatalog.Application.Commands.DeleteOffer;

internal sealed class DeleteOfferCommandValidator : AbstractValidator<DeleteOfferCommand>
{
    public DeleteOfferCommandValidator()
    {
        RuleFor(x => x.OfferId).NotEmpty();
    }
}
