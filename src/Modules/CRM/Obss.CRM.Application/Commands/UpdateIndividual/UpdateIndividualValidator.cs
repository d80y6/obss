using FluentValidation;

namespace Obss.CRM.Application.Commands.UpdateIndividual;

internal sealed class UpdateIndividualValidator : AbstractValidator<UpdateIndividualCommand>
{
    public UpdateIndividualValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MiddleName).MaximumLength(100);
        RuleFor(x => x.Salutation).MaximumLength(20);
        RuleFor(x => x.Title).MaximumLength(50);
        RuleFor(x => x.Nationality).MaximumLength(3);
        RuleFor(x => x.Gender).MaximumLength(20);
    }
}
