using FluentValidation;

namespace Obss.OCS.Application.Commands.AdjustBalance;

public sealed class AdjustBalanceCommandValidator : AbstractValidator<AdjustBalanceCommand>
{
    public AdjustBalanceCommandValidator()
    {
        RuleFor(x => x.BalanceId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Direction).NotEmpty().Must(d => d is "CREDIT" or "DEBIT");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
    }
}
