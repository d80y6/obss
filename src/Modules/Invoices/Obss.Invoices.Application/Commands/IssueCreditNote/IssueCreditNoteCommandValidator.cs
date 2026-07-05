using FluentValidation;

namespace Obss.Invoices.Application.Commands.IssueCreditNote;

internal sealed class IssueCreditNoteCommandValidator : AbstractValidator<IssueCreditNoteCommand>
{
    public IssueCreditNoteCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("Invoice ID is required.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter code.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("At least one credit note line is required.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.InvoiceLineId)
                .NotEmpty().WithMessage("Invoice line ID is required.");

            line.RuleFor(l => l.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

            line.RuleFor(l => l.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");

            line.RuleFor(l => l.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        });
    }
}
