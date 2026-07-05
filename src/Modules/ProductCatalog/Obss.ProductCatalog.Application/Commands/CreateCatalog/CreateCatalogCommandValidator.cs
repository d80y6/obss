using FluentValidation;

namespace Obss.ProductCatalog.Application.Commands.CreateCatalog;

public sealed class CreateCatalogCommandValidator : AbstractValidator<CreateCatalogCommand>
{
    public CreateCatalogCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Catalog name is required.")
            .MaximumLength(200).WithMessage("Catalog name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.CatalogType)
            .MaximumLength(100).WithMessage("Catalog type must not exceed 100 characters.")
            .When(x => x.CatalogType is not null);

        RuleFor(x => x.Version)
            .GreaterThanOrEqualTo(1).WithMessage("Version must be at least 1.");

        RuleFor(x => x.ValidTo)
            .GreaterThan(x => x.ValidFrom)
            .WithMessage("ValidTo must be after ValidFrom.")
            .When(x => x.ValidFrom.HasValue && x.ValidTo.HasValue);
    }
}
