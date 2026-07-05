using FluentValidation;

namespace Obss.Payments.Application.Commands.ProcessGatewayPayment;

internal sealed class ProcessGatewayPaymentCommandValidator : AbstractValidator<ProcessGatewayPaymentCommand>
{
    public ProcessGatewayPaymentCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(3).WithMessage("Currency must be a 3-letter code.");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required.")
            .MaximumLength(50).WithMessage("Payment method must not exceed 50 characters.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => x.Description is not null);
    }
}
