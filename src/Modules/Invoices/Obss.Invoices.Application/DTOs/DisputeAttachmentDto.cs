namespace Obss.Invoices.Application.DTOs;

public sealed record DisputeAttachmentDto(
    Guid Id,
    Guid InvoiceDisputeId,
    string FileName,
    string ContentType,
    long FileSize,
    DateTime UploadedAt);
