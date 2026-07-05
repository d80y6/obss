using FluentValidation;

namespace Obss.Billing.Application.Commands.FinalizeBill;

internal sealed class FinalizeBillCommandValidator : AbstractValidator<FinalizeBillCommand>
{
    public FinalizeBillCommandValidator()
    {
        RuleFor(x => x.BillId)
            .NotEmpty().WithMessage("Bill ID is required.");
    }
}
