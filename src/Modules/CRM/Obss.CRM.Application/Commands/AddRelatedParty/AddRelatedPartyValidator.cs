using FluentValidation;

namespace Obss.CRM.Application.Commands.AddRelatedParty;

internal sealed class AddRelatedPartyValidator : AbstractValidator<AddRelatedPartyCommand>
{
    public AddRelatedPartyValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Role).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ReferredId).NotEmpty();
        RuleFor(x => x.ReferredType).NotEmpty().MaximumLength(50);
    }
}
