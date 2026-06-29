namespace Obss.Reporting.Application.DTOs;

public sealed record ReportExecutionDto(
    Guid Id,
    Guid ReportDefinitionId,
    string Status,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string? FilePath,
    long? FileSize,
    string? ErrorMessage,
    string ExecutedBy);
