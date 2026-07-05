using FluentValidation;

namespace Obss.CRM.Application.Commands.RemoveIdentityDocument;

internal sealed class RemoveIdentityDocumentValidator : AbstractValidator<RemoveIdentityDocumentCommand>
{
    public RemoveIdentityDocumentValidator()
    {
        RuleFor(x => x.IndividualId).NotEmpty();
        RuleFor(x => x.DocumentId).NotEmpty();
    }
}
