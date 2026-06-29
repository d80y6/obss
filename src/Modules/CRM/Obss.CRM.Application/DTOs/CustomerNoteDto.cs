namespace Obss.CRM.Application.DTOs;

public sealed record CustomerNoteDto(
    Guid Id,
    Guid CustomerId,
    string Content,
    string Category,
    string CreatedById,
    DateTime CreatedAt);
