using FluentValidation;

namespace Obss.ProductCatalog.Application.Commands.AddCharacteristic;

public sealed class AddCharacteristicCommandValidator : AbstractValidator<AddCharacteristicCommand>
{
    public AddCharacteristicCommandValidator()
    {
        RuleFor(x => x.ProductSpecificationId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description is not null);
        RuleFor(x => x.ValueType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Regex).MaximumLength(500).When(x => x.Regex is not null);
    }
}
