using Obss.Reporting.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Reporting.Domain.Entities;

public class ReportDefinition : AggregateRoot<Guid>
{
    private ReportDefinition() { }

    private ReportDefinition(
        Guid id,
        string tenantId,
        string name,
        string? description,
        ReportType reportType,
        string dataSource,
        string query,
        OutputFormat outputFormat,
        string? schedule)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        ReportType = reportType;
        DataSource = dataSource;
        Query = query;
        OutputFormat = outputFormat;
        Schedule = schedule;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ReportType ReportType { get; private set; }
    public string DataSource { get; private set; } = string.Empty;
    public string Query { get; private set; } = string.Empty;
    public OutputFormat OutputFormat { get; private set; }
    public string? Schedule { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static ReportDefinition Create(
        string tenantId,
        string name,
        string? description,
        ReportType reportType,
        string dataSource,
        string query,
        OutputFormat outputFormat,
        string? schedule = null)
    {
        return new ReportDefinition(
            Guid.NewGuid(),
            tenantId,
            name,
            description,
            reportType,
            dataSource,
            query,
            outputFormat,
            schedule);
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSchedule(string cron)
    {
        Schedule = cron;
        UpdatedAt = DateTime.UtcNow;
    }
}
