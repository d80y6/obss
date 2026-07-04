using FluentValidation;

namespace Obss.ProductCatalog.Application.Commands.PatchProductSpecification;

public sealed class PatchProductSpecificationCommandValidator : AbstractValidator<PatchProductSpecificationCommand>
{
    public PatchProductSpecificationCommandValidator()
    {
        RuleFor(x => x.Name).MaximumLength(200).When(x => x.Name is not null);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description is not null);
        RuleFor(x => x.Brand).MaximumLength(200).When(x => x.Brand is not null);
        RuleFor(x => x.Version).MaximumLength(100).When(x => x.Version is not null);
        RuleFor(x => x.ProductNumber).MaximumLength(100).When(x => x.ProductNumber is not null);
        RuleFor(x => x.LifecycleStatus).IsInEnum().When(x => x.LifecycleStatus.HasValue);
    }
}
