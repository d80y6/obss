using FluentValidation;

namespace Obss.Collections.Application.Commands.CreatePaymentArrangement;

public sealed class CreatePaymentArrangementCommandValidator : AbstractValidator<CreatePaymentArrangementCommand>
{
    public CreatePaymentArrangementCommandValidator()
    {
        RuleFor(x => x.CollectionCaseId)
            .NotEmpty().WithMessage("Collection case ID is required.");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage("Total amount must be greater than zero.");

        RuleFor(x => x.InstallmentCount)
            .InclusiveBetween(1, 60).WithMessage("Installment count must be between 1 and 60.");

        RuleFor(x => x.InstallmentAmount)
            .GreaterThan(0).WithMessage("Installment amount must be greater than zero.");

        RuleFor(x => x.Frequency)
            .NotEmpty().WithMessage("Frequency is required.")
            .Must(v => v is "Weekly" or "Monthly")
            .WithMessage("Frequency must be Weekly or Monthly.");

        RuleFor(x => x.FirstPaymentDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("First payment date must be in the future.");
    }
}
