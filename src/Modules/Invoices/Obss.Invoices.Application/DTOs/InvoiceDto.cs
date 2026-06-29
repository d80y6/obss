namespace Obss.Invoices.Application.DTOs;

public sealed record InvoiceDto(
    Guid Id,
    string TenantId,
    string InvoiceNumber,
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    string CustomerAddress,
    DateTime InvoiceDate,
    DateTime DueDate,
    string Status,
    decimal SubTotal,
    decimal DiscountTotal,
    decimal TaxTotal,
    decimal GrandTotal,
    decimal AmountPaid,
    decimal AmountDue,
    string Currency,
    string? Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? SentAt,
    DateTime? PaidAt,
    List<InvoiceLineDto> Lines,
    List<InvoicePaymentDto> Payments,
    List<InvoiceNoteDto> NotesList);

public sealed record InvoicePaymentDto(
    Guid Id,
    Guid InvoiceId,
    decimal Amount,
    string PaymentReference,
    DateTime PaidAt);

public sealed record InvoiceNoteDto(
    Guid Id,
    Guid InvoiceId,
    string Content,
    DateTime CreatedAt);
