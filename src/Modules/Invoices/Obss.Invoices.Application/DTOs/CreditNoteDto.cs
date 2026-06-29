namespace Obss.Invoices.Application.DTOs;

public sealed record CreditNoteDto(
    Guid Id,
    string TenantId,
    string CreditNoteNumber,
    Guid InvoiceId,
    Guid CustomerId,
    string Reason,
    string Status,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    string Currency,
    DateTime IssuedAt,
    DateTime? AppliedAt,
    List<CreditNoteLineDto> Lines);

public sealed record CreditNoteLineDto(
    Guid Id,
    Guid CreditNoteId,
    Guid InvoiceLineId,
    string Description,
    decimal Amount,
    decimal Quantity);
