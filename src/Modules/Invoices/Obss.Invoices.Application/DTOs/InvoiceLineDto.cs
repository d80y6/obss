namespace Obss.Invoices.Application.DTOs;

public sealed record InvoiceLineDto(
    Guid Id,
    Guid InvoiceId,
    Guid BillId,
    Guid? BillLineId,
    string LineType,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TotalAmount,
    decimal TaxAmount,
    decimal TaxRate,
    string Currency);
