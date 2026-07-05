using FluentValidation;

namespace Obss.CRM.Application.Commands.UpdateAgreement;

internal sealed class UpdateAgreementValidator : AbstractValidator<UpdateAgreementCommand>
{
    public UpdateAgreementValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
