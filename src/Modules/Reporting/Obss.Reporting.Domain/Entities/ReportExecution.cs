using Obss.Reporting.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Reporting.Domain.Entities;

public class ReportExecution : Entity<Guid>
{
    private ReportExecution() { }

    public ReportExecution(
        Guid id,
        Guid reportDefinitionId,
        string executedBy)
        : base(id)
    {
        ReportDefinitionId = reportDefinitionId;
        Status = ExecutionStatus.Queued;
        ExecutedBy = executedBy;
    }

    public Guid ReportDefinitionId { get; private set; }
    public ExecutionStatus Status { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? FilePath { get; private set; }
    public long? FileSize { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string ExecutedBy { get; private set; } = string.Empty;

    public void Start()
    {
        Status = ExecutionStatus.Running;
        StartedAt = DateTime.UtcNow;
    }

    public void Complete(string filePath, long fileSize)
    {
        Status = ExecutionStatus.Completed;
        FilePath = filePath;
        FileSize = fileSize;
        CompletedAt = DateTime.UtcNow;
    }

    public void Fail(string errorMessage)
    {
        Status = ExecutionStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
    }
}
