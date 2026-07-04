using FluentValidation;

namespace Obss.ProductCatalog.Application.Commands.RemoveBundledProductOffering;

internal sealed class RemoveBundledProductOfferingCommandValidator : AbstractValidator<RemoveBundledProductOfferingCommand>
{
    public RemoveBundledProductOfferingCommandValidator()
    {
        RuleFor(x => x.OfferId)
            .NotEmpty().WithMessage("Offer ID is required.");

        RuleFor(x => x.BundledOfferingId)
            .NotEmpty().WithMessage("Bundled offering ID is required.");
    }
}
