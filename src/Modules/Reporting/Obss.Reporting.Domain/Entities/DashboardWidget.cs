using Obss.Reporting.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Reporting.Domain.Entities;

public class DashboardWidget : AggregateRoot<Guid>
{
    private DashboardWidget() { }

    private DashboardWidget(
        Guid id,
        string tenantId,
        WidgetType widgetType,
        string title,
        string configuration,
        int position,
        WidgetSize size,
        string dataSource,
        string query,
        int? refreshInterval)
        : base(id)
    {
        TenantId = tenantId;
        WidgetType = widgetType;
        Title = title;
        Configuration = configuration;
        Position = position;
        Size = size;
        DataSource = dataSource;
        Query = query;
        RefreshInterval = refreshInterval;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public WidgetType WidgetType { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Configuration { get; private set; } = string.Empty;
    public int Position { get; private set; }
    public WidgetSize Size { get; private set; }
    public string DataSource { get; private set; } = string.Empty;
    public string Query { get; private set; } = string.Empty;
    public int? RefreshInterval { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static DashboardWidget Create(
        string tenantId,
        WidgetType widgetType,
        string title,
        string configuration,
        int position,
        WidgetSize size,
        string dataSource,
        string query,
        int? refreshInterval = null)
    {
        return new DashboardWidget(
            Guid.NewGuid(),
            tenantId,
            widgetType,
            title,
            configuration,
            position,
            size,
            dataSource,
            query,
            refreshInterval);
    }

    public void UpdatePosition(int position)
    {
        Position = position;
    }

    public void UpdateConfiguration(string configuration)
    {
        Configuration = configuration;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
