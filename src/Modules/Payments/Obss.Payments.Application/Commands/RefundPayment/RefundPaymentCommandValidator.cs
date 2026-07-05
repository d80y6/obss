using FluentValidation;

namespace Obss.Payments.Application.Commands.RefundPayment;

internal sealed class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
{
    public RefundPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty().WithMessage("Payment ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Refund amount must be greater than zero.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
    }
}
