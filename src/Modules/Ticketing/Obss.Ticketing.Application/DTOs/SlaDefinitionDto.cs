namespace Obss.Ticketing.Application.DTOs;

public sealed record SlaDefinitionDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    string Priority,
    int ResponseTimeHours,
    int ResolutionTimeHours,
    int EscalationTimeHours,
    bool IsActive,
    DateTime CreatedAt);
