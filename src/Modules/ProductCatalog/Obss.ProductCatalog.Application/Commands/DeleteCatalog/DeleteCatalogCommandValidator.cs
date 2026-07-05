using FluentValidation;

namespace Obss.ProductCatalog.Application.Commands.DeleteCatalog;

public sealed class DeleteCatalogCommandValidator : AbstractValidator<DeleteCatalogCommand>
{
    public DeleteCatalogCommandValidator()
    {
        RuleFor(x => x.CatalogId)
            .NotEmpty().WithMessage("Catalog ID is required.");
    }
}
