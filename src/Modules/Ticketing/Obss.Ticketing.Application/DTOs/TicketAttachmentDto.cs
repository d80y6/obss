namespace Obss.Ticketing.Application.DTOs;

public sealed record TicketAttachmentDto(
    Guid Id,
    Guid TicketId,
    string FileName,
    string ContentType,
    long FileSize,
    string StoragePath,
    string UploadedById,
    DateTime CreatedAt);
