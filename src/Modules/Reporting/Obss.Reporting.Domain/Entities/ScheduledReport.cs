using Obss.SharedKernel.Domain.Common;

namespace Obss.Reporting.Domain.Entities;

public class ScheduledReport : AggregateRoot<Guid>
{
    private readonly List<string> _recipients = [];

    private ScheduledReport() { }

    private ScheduledReport(
        Guid id,
        string tenantId,
        Guid reportDefinitionId,
        string cronExpression,
        List<string> recipients)
        : base(id)
    {
        TenantId = tenantId;
        ReportDefinitionId = reportDefinitionId;
        CronExpression = cronExpression;
        _recipients = recipients;
        IsActive = true;
    }

    public string TenantId { get; private set; } = string.Empty;
    public Guid ReportDefinitionId { get; private set; }
    public string CronExpression { get; private set; } = string.Empty;
    public IReadOnlyCollection<string> Recipients => _recipients.AsReadOnly();
    public DateTime? LastRunAt { get; private set; }
    public DateTime? NextRunAt { get; private set; }
    public bool IsActive { get; private set; }

    public static ScheduledReport Create(
        string tenantId,
        Guid reportDefinitionId,
        string cronExpression,
        List<string> recipients)
    {
        return new ScheduledReport(
            Guid.NewGuid(),
            tenantId,
            reportDefinitionId,
            cronExpression,
            recipients);
    }

    public void SetNextRun(DateTime nextRun)
    {
        NextRunAt = nextRun;
    }

    public void MarkRan()
    {
        LastRunAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
