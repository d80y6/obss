namespace Obss.Invoices.Application.DTOs;

public sealed record InvoiceDisputeDto(
    Guid Id,
    Guid InvoiceId,
    Guid CustomerId,
    string Reason,
    string Description,
    string Status,
    decimal DisputedAmount,
    string? Resolution,
    Guid? ResolvedById,
    DateTime? ResolvedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<DisputeAttachmentDto> Attachments);
