using FluentValidation;

namespace Obss.OCS.Application.Commands.ReserveCredit;

public sealed class ReserveCreditCommandValidator : AbstractValidator<ReserveCreditCommand>
{
    public ReserveCreditCommandValidator()
    {
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}
