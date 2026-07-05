using FluentValidation;

namespace Obss.CRM.Application.Commands.RemoveCharacteristic;

internal sealed class RemoveCharacteristicValidator : AbstractValidator<RemoveCharacteristicCommand>
{
    public RemoveCharacteristicValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Key).NotEmpty().MaximumLength(100);
    }
}
