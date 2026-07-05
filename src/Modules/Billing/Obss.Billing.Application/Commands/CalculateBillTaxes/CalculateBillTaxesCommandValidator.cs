using FluentValidation;

namespace Obss.Billing.Application.Commands.CalculateBillTaxes;

internal sealed class CalculateBillTaxesCommandValidator : AbstractValidator<CalculateBillTaxesCommand>
{
    public CalculateBillTaxesCommandValidator()
    {
        RuleFor(x => x.BillId)
            .NotEmpty().WithMessage("Bill ID is required.");
    }
}
