using FluentValidation;

namespace Obss.Payments.Application.Commands.CompletePayment;

internal sealed class CompletePaymentCommandValidator : AbstractValidator<CompletePaymentCommand>
{
    public CompletePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty().WithMessage("Payment ID is required.");
    }
}
