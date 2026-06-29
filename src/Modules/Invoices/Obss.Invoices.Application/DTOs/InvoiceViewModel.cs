namespace Obss.Invoices.Application.DTOs;

public sealed record InvoiceViewModel(
    Guid Id,
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
    bool HasQrCode,
    List<InvoiceViewLineItem> Lines,
    List<InvoiceViewPayment> Payments);

public sealed record InvoiceViewLineItem(
    Guid Id,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TotalAmount,
    decimal TaxAmount,
    string LineType,
    string Currency);

public sealed record InvoiceViewPayment(
    Guid Id,
    decimal Amount,
    string PaymentReference,
    DateTime PaidAt);
