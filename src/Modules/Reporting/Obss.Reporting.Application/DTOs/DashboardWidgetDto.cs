namespace Obss.Reporting.Application.DTOs;

public sealed record DashboardWidgetDto(
    Guid Id,
    string TenantId,
    string WidgetType,
    string Title,
    string Configuration,
    int Position,
    string Size,
    string DataSource,
    string Query,
    int? RefreshInterval,
    bool IsActive,
    DateTime CreatedAt);
