using FluentValidation;

namespace Obss.CRM.Application.Commands.AddContactMedium;

internal sealed class AddContactMediumValidator : AbstractValidator<AddContactMediumCommand>
{
    public AddContactMediumValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.MediumType).IsInEnum();
    }
}
