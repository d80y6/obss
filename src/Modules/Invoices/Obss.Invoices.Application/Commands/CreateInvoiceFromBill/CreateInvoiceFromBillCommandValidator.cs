using FluentValidation;

namespace Obss.Invoices.Application.Commands.CreateInvoiceFromBill;

public sealed class CreateInvoiceFromBillCommandValidator : AbstractValidator<CreateInvoiceFromBillCommand>
{
    public CreateInvoiceFromBillCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.BillId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CustomerEmail).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}