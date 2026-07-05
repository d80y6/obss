using FluentValidation;

namespace Obss.Payments.Application.Commands.ImportBankStatement;

internal sealed class ImportBankStatementCommandValidator : AbstractValidator<ImportBankStatementCommand>
{
    public ImportBankStatementCommandValidator()
    {
        RuleFor(x => x.ImportSource)
            .NotEmpty().WithMessage("Import source is required.")
            .MaximumLength(100).WithMessage("Import source must not exceed 100 characters.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(3).WithMessage("Currency must be a 3-letter code.");

        RuleFor(x => x.Transactions)
            .NotNull().WithMessage("Transactions list is required.")
            .NotEmpty().WithMessage("At least one transaction is required.");
    }
}
