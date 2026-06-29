namespace Obss.Collections.Application.DTOs;

public sealed record CollectionActionDto(
    Guid Id,
    Guid CollectionCaseId,
    string ActionType,
    int DunningLevel,
    string Description,
    DateTime PerformedAt,
    string? PerformedBy,
    DateTime? NextActionDate,
    bool IsCompleted);
