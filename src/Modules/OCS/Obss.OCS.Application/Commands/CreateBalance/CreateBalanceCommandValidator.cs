using FluentValidation;

namespace Obss.OCS.Application.Commands.CreateBalance;

public sealed class CreateBalanceCommandValidator : AbstractValidator<CreateBalanceCommand>
{
    public CreateBalanceCommandValidator()
    {
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}
