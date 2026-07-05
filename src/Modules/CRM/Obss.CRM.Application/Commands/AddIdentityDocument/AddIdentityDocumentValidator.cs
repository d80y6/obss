using FluentValidation;
using Obss.CRM.Domain.ValueObjects;

namespace Obss.CRM.Application.Commands.AddIdentityDocument;

internal sealed class AddIdentityDocumentValidator : AbstractValidator<AddIdentityDocumentCommand>
{
    public AddIdentityDocumentValidator()
    {
        RuleFor(x => x.IndividualId).NotEmpty();
        RuleFor(x => x.DocumentType).NotEmpty().Must(t => Enum.TryParse<DocumentType>(t, true, out _))
            .WithMessage("Invalid document type.");
        RuleFor(x => x.DocumentNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.IssuingAuthority).MaximumLength(200);
        RuleFor(x => x.IssuingCountry).MaximumLength(3);
    }
}
