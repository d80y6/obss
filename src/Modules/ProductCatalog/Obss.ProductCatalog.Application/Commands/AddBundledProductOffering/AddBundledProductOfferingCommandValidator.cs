using FluentValidation;

namespace Obss.ProductCatalog.Application.Commands.AddBundledProductOffering;

internal sealed class AddBundledProductOfferingCommandValidator : AbstractValidator<AddBundledProductOfferingCommand>
{
    public AddBundledProductOfferingCommandValidator()
    {
        RuleFor(x => x.OfferId)
            .NotEmpty().WithMessage("Offer ID is required.");

        RuleFor(x => x.BundledOfferId)
            .NotEmpty().WithMessage("Bundled offer ID is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");
    }
}
