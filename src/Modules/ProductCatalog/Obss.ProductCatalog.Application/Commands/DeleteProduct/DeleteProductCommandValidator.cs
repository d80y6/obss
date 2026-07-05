using FluentValidation;

namespace Obss.ProductCatalog.Application.Commands.DeleteProduct;

internal sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
    }
}
