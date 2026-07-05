using FluentValidation;

namespace Obss.Billing.Application.Commands.UpdateBillingAccount;

public sealed class UpdateBillingAccountValidator : AbstractValidator<UpdateBillingAccountCommand>
{
    public UpdateBillingAccountValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID is required.");

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
