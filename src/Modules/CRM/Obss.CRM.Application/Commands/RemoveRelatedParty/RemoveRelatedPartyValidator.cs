using FluentValidation;

namespace Obss.CRM.Application.Commands.RemoveRelatedParty;

internal sealed class RemoveRelatedPartyValidator : AbstractValidator<RemoveRelatedPartyCommand>
{
    public RemoveRelatedPartyValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.ReferredId).NotEmpty();
    }
}
