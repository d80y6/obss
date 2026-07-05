using FluentValidation;

namespace Obss.Payments.Application.Commands.ReconcilePayment;

internal sealed class ReconcilePaymentCommandValidator : AbstractValidator<ReconcilePaymentCommand>
{
    public ReconcilePaymentCommandValidator()
    {
        RuleFor(x => x.ReconciliationId)
            .NotEmpty().WithMessage("Reconciliation ID is required.");

        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Item ID is required.");

        RuleFor(x => x.MatchedInvoiceId)
            .NotEmpty().WithMessage("Matched invoice ID is required.");

        RuleFor(x => x.MatchedPaymentId)
            .NotEmpty().WithMessage("Matched payment ID is required.");
    }
}
