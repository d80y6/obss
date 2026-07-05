using FluentValidation;

namespace Obss.Billing.Application.Commands.CreateBillingAccount;

public sealed class CreateBillingAccountValidator : AbstractValidator<CreateBillingAccountCommand>
{
    public CreateBillingAccountValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.AccountType)
            .NotEmpty().WithMessage("Account type is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Credit limit must be greater than or equal to 0.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(3).WithMessage("Currency must not exceed 3 characters.");
    }
}
