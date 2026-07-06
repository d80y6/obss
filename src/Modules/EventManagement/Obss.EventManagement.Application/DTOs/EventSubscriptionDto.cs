namespace Obss.EventManagement.Application.DTOs;

public sealed record EventSubscriptionDto(
    Guid Id,
    string Name,
    string CallbackUrl,
    string? Query,
    string Status,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<EventFilterDto> Filters);

public sealed record EventFilterDto(
    string EventType,
    string? FilterCriteria);
