namespace Obss.Audit.Application.DTOs;

public sealed record AuditSummaryDto(
    IDictionary<string, int> CountByAction,
    IDictionary<string, int> CountByEntityType,
    int TotalEntries,
    int TotalByEntityType);
