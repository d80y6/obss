using FluentValidation;

namespace Obss.Billing.Application.Commands.AddAdjustment;

internal sealed class AddAdjustmentCommandValidator : AbstractValidator<AddAdjustmentCommand>
{
    public AddAdjustmentCommandValidator()
    {
        RuleFor(x => x.BillId)
            .NotEmpty().WithMessage("Bill ID is required.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.Amount)
            .NotEmpty().WithMessage("Amount is required.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(3).WithMessage("Currency must not exceed 3 characters.");
    }
}
