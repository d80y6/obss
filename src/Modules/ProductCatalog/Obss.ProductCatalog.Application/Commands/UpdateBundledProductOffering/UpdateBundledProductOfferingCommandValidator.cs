using FluentValidation;

namespace Obss.ProductCatalog.Application.Commands.UpdateBundledProductOffering;

internal sealed class UpdateBundledProductOfferingCommandValidator : AbstractValidator<UpdateBundledProductOfferingCommand>
{
    public UpdateBundledProductOfferingCommandValidator()
    {
        RuleFor(x => x.OfferId)
            .NotEmpty().WithMessage("Offer ID is required.");

        RuleFor(x => x.BundledOfferingId)
            .NotEmpty().WithMessage("Bundled offering ID is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");
    }
}
