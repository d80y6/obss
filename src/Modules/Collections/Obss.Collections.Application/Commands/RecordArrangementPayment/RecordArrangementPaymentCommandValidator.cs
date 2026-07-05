using FluentValidation;

namespace Obss.Collections.Application.Commands.RecordArrangementPayment;

internal sealed class RecordArrangementPaymentCommandValidator : AbstractValidator<RecordArrangementPaymentCommand>
{
    public RecordArrangementPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentArrangementId)
            .NotEmpty().WithMessage("Payment arrangement ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");
    }
}
