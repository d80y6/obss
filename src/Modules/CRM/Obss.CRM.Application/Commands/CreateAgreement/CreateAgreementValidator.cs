using FluentValidation;

namespace Obss.CRM.Application.Commands.CreateAgreement;

internal sealed class CreateAgreementValidator : AbstractValidator<CreateAgreementCommand>
{
    public CreateAgreementValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AgreementType).NotEmpty();
    }
}
