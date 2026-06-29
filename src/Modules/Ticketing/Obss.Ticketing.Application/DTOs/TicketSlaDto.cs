namespace Obss.Ticketing.Application.DTOs;

public sealed record TicketSlaDto(
    Guid? SlaDefinitionId,
    string? SlaDefinitionName,
    DateTime? SlaDeadline,
    DateTime? SlaResponseDeadline,
    DateTime? SlaBreachedAt,
    bool IsSlaBreached,
    string? Status);
