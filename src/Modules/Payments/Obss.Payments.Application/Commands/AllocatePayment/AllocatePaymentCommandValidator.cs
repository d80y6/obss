using FluentValidation;

namespace Obss.Payments.Application.Commands.AllocatePayment;

internal sealed class AllocatePaymentCommandValidator : AbstractValidator<AllocatePaymentCommand>
{
    public AllocatePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty().WithMessage("Payment ID is required.");

        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("Invoice ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Allocation amount must be greater than zero.");
    }
}
