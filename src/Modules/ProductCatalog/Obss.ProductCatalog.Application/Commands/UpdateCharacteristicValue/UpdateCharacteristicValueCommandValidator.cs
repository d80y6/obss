using FluentValidation;

namespace Obss.ProductCatalog.Application.Commands.UpdateCharacteristicValue;

public sealed class UpdateCharacteristicValueCommandValidator : AbstractValidator<UpdateCharacteristicValueCommand>
{
    public UpdateCharacteristicValueCommandValidator()
    {
        RuleFor(x => x.ProductSpecificationId).NotEmpty();
        RuleFor(x => x.CharacteristicId).NotEmpty();
        RuleFor(x => x.ValueId).NotEmpty();
        RuleFor(x => x.Value).NotEmpty().MaximumLength(500);
        RuleFor(x => x.UnitOfMeasure).MaximumLength(100).When(x => x.UnitOfMeasure is not null);
        RuleFor(x => x.RangeInterval).MaximumLength(50).When(x => x.RangeInterval is not null);
    }
}
