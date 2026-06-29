namespace Obss.Ticketing.Application.DTOs;

public sealed record TicketSummaryDto(
    Guid Id,
    string TicketNumber,
    Guid CustomerId,
    string CustomerName,
    string Subject,
    string Priority,
    string Category,
    string Status,
    string Source,
    string? AssignedTo,
    string? AssignedGroup,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? ClosedAt,
    DateTime? FirstResponseAt,
    DateTime? SlaDeadline,
    DateTime? SlaResponseDeadline,
    DateTime? SlaBreachedAt,
    Guid? SlaDefinitionId,
    string? SlaStatus,
    int CommentCount);
