using FluentValidation;

namespace Obss.CRM.Application.Commands.AddHub;

internal sealed class AddHubValidator : AbstractValidator<AddHubCommand>
{
    public AddHubValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Identifier).NotEmpty().MaximumLength(200);
    }
}
