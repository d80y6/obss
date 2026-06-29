using FluentValidation;

namespace Obss.Payments.Application.Commands.RecordPayment;

public sealed class RecordPaymentCommandValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(3).WithMessage("Currency must be a 3-letter code.");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required.")
            .Must(m => m is "Cash" or "BankTransfer" or "MobileMoney" or "CreditCard")
            .WithMessage("Invalid payment method. Allowed: Cash, BankTransfer, MobileMoney, CreditCard.");

        RuleFor(x => x.PaymentReference)
            .MaximumLength(200).WithMessage("Payment reference must not exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.PaymentReference));
    }
}
