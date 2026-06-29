using FluentValidation;

namespace Obss.ProductCatalog.Application.Commands.CreateOffer;

public sealed class CreateOfferCommandValidator : AbstractValidator<CreateOfferCommand>
{
    public CreateOfferCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Offer name is required.")
            .MaximumLength(200).WithMessage("Offer name must not exceed 200 characters.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.OfferType)
            .IsInEnum().WithMessage("Invalid offer type.");

        RuleFor(x => x.ContractDurationMonths)
            .GreaterThan(0).WithMessage("Contract duration must be greater than 0.")
            .When(x => x.IsContract && x.ContractDurationMonths.HasValue);

        RuleFor(x => x.BillingPeriod)
            .IsInEnum().WithMessage("Invalid billing period.")
            .When(x => x.BillingPeriod.HasValue);

        RuleFor(x => x.ValidTo)
            .GreaterThan(x => x.ValidFrom).WithMessage("ValidTo must be after ValidFrom.")
            .When(x => x.ValidFrom.HasValue && x.ValidTo.HasValue);
    }
}
