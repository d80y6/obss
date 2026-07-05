using FluentValidation;

namespace Obss.Invoices.Application.Commands.FinalizeInvoice;

internal sealed class FinalizeInvoiceCommandValidator : AbstractValidator<FinalizeInvoiceCommand>
{
    public FinalizeInvoiceCommandValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("Invoice ID is required.");
    }
}
