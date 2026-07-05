namespace Obss.Collections.Application.DTOs;

public sealed record DunningPolicyDto(
    Guid Id,
    string TenantId,
    string Name,
    string Description,
    bool IsActive,
    int MaxDunningLevel,
    Dictionary<int, decimal> DunningFees,
    int DaysBetweenActions,
    int EscalationAfterDays);
