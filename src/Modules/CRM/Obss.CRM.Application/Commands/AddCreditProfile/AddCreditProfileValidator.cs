using FluentValidation;

namespace Obss.CRM.Application.Commands.AddCreditProfile;

internal sealed class AddCreditProfileValidator : AbstractValidator<AddCreditProfileCommand>
{
    public AddCreditProfileValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Score).InclusiveBetween(0, 1000);
        RuleFor(x => x.ScoreType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.RiskRating).MaximumLength(20);
    }
}
