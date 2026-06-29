using FluentValidation;

namespace Obss.Payments.Application.Commands.RegisterPaymentGateway;

public sealed class RegisterPaymentGatewayCommandValidator : AbstractValidator<RegisterPaymentGatewayCommand>
{
    public RegisterPaymentGatewayCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Gateway name is required.")
            .MaximumLength(100).WithMessage("Gateway name must not exceed 100 characters.");

        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required.")
            .Must(p => p is "Stripe" or "PayPal" or "LocalBank" or "MobileMoney" or "Cash")
            .WithMessage("Invalid provider. Allowed: Stripe, PayPal, LocalBank, MobileMoney, Cash.");

        RuleFor(x => x.Configuration)
            .NotEmpty().WithMessage("Configuration is required.");

        RuleFor(x => x.TransactionFee)
            .GreaterThanOrEqualTo(0).WithMessage("Transaction fee must be zero or greater.");

        RuleFor(x => x.FeeType)
            .NotEmpty().WithMessage("Fee type is required.")
            .Must(f => f is "Fixed" or "Percentage")
            .WithMessage("Fee type must be Fixed or Percentage.");
    }
}
