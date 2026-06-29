namespace Obss.Ticketing.Application.DTOs;

public sealed record TicketCommentDto(
    Guid Id,
    Guid TicketId,
    string Content,
    bool IsInternal,
    string AuthorId,
    string AuthorName,
    DateTime CreatedAt);
