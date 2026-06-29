using MediatR;
using Obss.Invoices.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Commands.IssueCreditNote;

public sealed record IssueCreditNoteCommand(
    string TenantId,
    Guid InvoiceId,
    Guid CustomerId,
    string Reason,
    string Currency,
    List<CreditNoteLineInput> Lines) : IRequest<Result<CreditNoteDto>>;

public sealed record CreditNoteLineInput(
    Guid InvoiceLineId,
    string Description,
    decimal Amount,
    decimal Quantity);
