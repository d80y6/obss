using FluentValidation;

namespace Obss.ProductCatalog.Application.Commands.UpdateProductOfferingTerm;

internal sealed class UpdateProductOfferingTermCommandValidator : AbstractValidator<UpdateProductOfferingTermCommand>
{
    public UpdateProductOfferingTermCommandValidator()
    {
        RuleFor(x => x.OfferId)
            .NotEmpty().WithMessage("Offer ID is required.");

        RuleFor(x => x.TermId)
            .NotEmpty().WithMessage("Term ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Term name is required.")
            .MaximumLength(200).WithMessage("Term name must not exceed 200 characters.");

        RuleFor(x => x.Duration)
            .GreaterThan(0).WithMessage("Duration must be greater than 0.");

        RuleFor(x => x.DurationUnit)
            .IsInEnum().WithMessage("Invalid duration unit.");

        RuleFor(x => x.TermType)
            .IsInEnum().WithMessage("Invalid term type.");
    }
}
