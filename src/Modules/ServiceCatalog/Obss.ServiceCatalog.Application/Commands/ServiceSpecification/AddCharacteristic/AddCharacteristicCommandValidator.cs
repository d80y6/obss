using FluentValidation;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.AddCharacteristic;

public sealed class AddCharacteristicCommandValidator : AbstractValidator<AddCharacteristicCommand>
{
    public AddCharacteristicCommandValidator()
    {
        RuleFor(x => x.ServiceSpecificationId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.ValueType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Regex).MaximumLength(500);
    }
}
