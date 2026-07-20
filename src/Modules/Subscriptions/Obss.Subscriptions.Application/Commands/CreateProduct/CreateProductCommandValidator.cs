using FluentValidation;

namespace Obss.Subscriptions.Application.Commands.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}
