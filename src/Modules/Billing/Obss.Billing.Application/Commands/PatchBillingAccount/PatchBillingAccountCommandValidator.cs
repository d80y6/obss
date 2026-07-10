using FluentValidation;

namespace Obss.Billing.Application.Commands.PatchBillingAccount;

public sealed class PatchBillingAccountCommandValidator : AbstractValidator<PatchBillingAccountCommand>
{
    public PatchBillingAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Billing account ID is required.");

        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.")
            .When(x => x.Name is not null);

        RuleFor(x => x.Currency)
            .MaximumLength(3).WithMessage("Currency must not exceed 3 characters.")
            .When(x => x.Currency is not null);

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Credit limit must be greater than or equal to 0.")
            .When(x => x.CreditLimit.HasValue);
    }
}
