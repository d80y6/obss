using FluentValidation;

namespace Obss.ProductCatalog.Application.Commands.AddSpecificationRelationship;

public sealed class AddSpecificationRelationshipCommandValidator : AbstractValidator<AddSpecificationRelationshipCommand>
{
    public AddSpecificationRelationshipCommandValidator()
    {
        RuleFor(x => x.ProductSpecificationId).NotEmpty();
        RuleFor(x => x.TargetSpecificationId).NotEmpty();
        RuleFor(x => x.RelationshipType).IsInEnum();
        RuleFor(x => x.Role).MaximumLength(200).When(x => x.Role is not null);
    }
}
