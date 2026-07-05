using FluentValidation;

namespace Obss.CRM.Application.Commands.AddCharacteristic;

internal sealed class AddCharacteristicValidator : AbstractValidator<AddCharacteristicCommand>
{
    public AddCharacteristicValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Key).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Value).MaximumLength(500);
        RuleFor(x => x.ValueType).MaximumLength(30);
    }
}
