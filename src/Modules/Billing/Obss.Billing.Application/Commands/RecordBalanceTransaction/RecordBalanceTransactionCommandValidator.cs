using FluentValidation;

namespace Obss.Billing.Application.Commands.RecordBalanceTransaction;

public sealed class RecordBalanceTransactionCommandValidator : AbstractValidator<RecordBalanceTransactionCommand>
{
    public RecordBalanceTransactionCommandValidator()
    {
        RuleFor(x => x.BillingAccountId)
            .NotEmpty().WithMessage("Billing account ID is required.");

        RuleFor(x => x.TransactionType)
            .NotEmpty().WithMessage("Transaction type is required.")
            .Must(t => t is "Charge" or "Payment" or "Credit" or "Debit" or "Adjustment" or "Refund")
            .WithMessage("Invalid transaction type.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
    }
}
