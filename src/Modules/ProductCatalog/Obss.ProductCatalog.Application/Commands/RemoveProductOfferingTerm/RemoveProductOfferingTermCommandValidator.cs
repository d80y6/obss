using FluentValidation;

namespace Obss.ProductCatalog.Application.Commands.RemoveProductOfferingTerm;

internal sealed class RemoveProductOfferingTermCommandValidator : AbstractValidator<RemoveProductOfferingTermCommand>
{
    public RemoveProductOfferingTermCommandValidator()
    {
        RuleFor(x => x.OfferId)
            .NotEmpty().WithMessage("Offer ID is required.");

        RuleFor(x => x.TermId)
            .NotEmpty().WithMessage("Term ID is required.");
    }
}
