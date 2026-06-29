namespace Obss.Reporting.Application.DTOs;

public sealed record ScheduledReportDto(
    Guid Id,
    string TenantId,
    Guid ReportDefinitionId,
    string CronExpression,
    List<string> Recipients,
    DateTime? LastRunAt,
    DateTime? NextRunAt,
    bool IsActive);
