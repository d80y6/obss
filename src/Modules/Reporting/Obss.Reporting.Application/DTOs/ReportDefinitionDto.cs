namespace Obss.Reporting.Application.DTOs;

public sealed record ReportDefinitionDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    string ReportType,
    string DataSource,
    string Query,
    string OutputFormat,
    string? Schedule,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
