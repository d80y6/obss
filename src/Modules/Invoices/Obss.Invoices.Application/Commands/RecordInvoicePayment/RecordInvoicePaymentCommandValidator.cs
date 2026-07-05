using FluentValidation;

namespace Obss.Invoices.Application.Commands.RecordInvoicePayment;

internal sealed class RecordInvoicePaymentCommandValidator : AbstractValidator<RecordInvoicePaymentCommand>
{
    public RecordInvoicePaymentCommandValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("Invoice ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than zero.");

        RuleFor(x => x.PaymentReference)
            .NotEmpty().WithMessage("Payment reference is required.")
            .MaximumLength(100).WithMessage("Payment reference must not exceed 100 characters.");
    }
}
