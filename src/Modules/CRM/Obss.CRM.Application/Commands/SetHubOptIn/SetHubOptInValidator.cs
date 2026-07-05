using FluentValidation;

namespace Obss.CRM.Application.Commands.SetHubOptIn;

internal sealed class SetHubOptInValidator : AbstractValidator<SetHubOptInCommand>
{
    public SetHubOptInValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Identifier).NotEmpty().MaximumLength(200);
    }
}
