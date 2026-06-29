namespace Obss.Audit.Application.DTOs;

public sealed record ComplianceSummaryDto(
    int TotalEntries,
    int TotalSensitiveOperations,
    double RetentionCompliancePercent,
    int RetentionBreaches,
    int UniqueActions,
    int UniqueEntityTypes,
    DateTime? OldestEntry,
    DateTime? NewestEntry);
