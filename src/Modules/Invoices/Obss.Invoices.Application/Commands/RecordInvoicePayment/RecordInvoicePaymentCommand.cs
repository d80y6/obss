using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Commands.RecordInvoicePayment;

public sealed record RecordInvoicePaymentCommand(
    Guid InvoiceId,
    decimal Amount,
    string PaymentReference) : IRequest<Result>;
