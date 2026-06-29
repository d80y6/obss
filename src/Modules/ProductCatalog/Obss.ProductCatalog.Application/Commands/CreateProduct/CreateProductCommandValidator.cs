using FluentValidation;

namespace Obss.ProductCatalog.Application.Commands.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.ProductType)
            .IsInEnum().WithMessage("Invalid product type.");

        RuleFor(x => x.TaxCategory)
            .MaximumLength(100).WithMessage("Tax category must not exceed 100 characters.")
            .When(x => x.TaxCategory is not null);
    }
}
