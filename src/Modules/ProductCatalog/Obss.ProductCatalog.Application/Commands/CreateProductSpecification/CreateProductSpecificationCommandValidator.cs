using FluentValidation;

namespace Obss.ProductCatalog.Application.Commands.CreateProductSpecification;

public sealed class CreateProductSpecificationCommandValidator : AbstractValidator<CreateProductSpecificationCommand>
{
    public CreateProductSpecificationCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description is not null);
        RuleFor(x => x.Brand).MaximumLength(200).When(x => x.Brand is not null);
        RuleFor(x => x.Version).MaximumLength(100).When(x => x.Version is not null);
        RuleFor(x => x.ProductNumber).MaximumLength(100).When(x => x.ProductNumber is not null);
    }
}
