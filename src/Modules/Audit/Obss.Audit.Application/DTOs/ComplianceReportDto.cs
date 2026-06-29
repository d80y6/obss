namespace Obss.Audit.Application.DTOs;

public sealed record ComplianceReportDto(
    int TotalEntries,
    IDictionary<string, int> EntriesByAction,
    IDictionary<string, int> EntriesByEntity,
    double RetentionCompliancePercent,
    int RetentionBreaches,
    int SensitiveOperations,
    DateTime? OldestEntry,
    DateTime? NewestEntry);
