using FluentValidation;

namespace Obss.Invoices.Application.Commands.MarkInvoiceAsSent;

internal sealed class MarkInvoiceAsSentCommandValidator : AbstractValidator<MarkInvoiceAsSentCommand>
{
    public MarkInvoiceAsSentCommandValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("Invoice ID is required.");
    }
}
