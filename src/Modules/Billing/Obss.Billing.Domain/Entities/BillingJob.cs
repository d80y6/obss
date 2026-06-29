using Obss.SharedKernel.Domain.Common;

namespace Obss.Billing.Domain.Entities;

public class BillingJob : AggregateRoot<Guid>
{
    private BillingJob() { }

    public BillingJob(Guid id, string jobType) : base(id)
    {
        JobType = jobType;
        Status = "Pending";
        CreatedAt = DateTime.UtcNow;
    }

    public string JobType { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public string? ErrorMessage { get; private set; }
    public int TotalProcessed { get; private set; }
    public int TotalErrors { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public void Start()
    {
        Status = "Running";
        StartedAt = DateTime.UtcNow;
    }

    public void Complete(int processed, int errors)
    {
        Status = "Completed";
        TotalProcessed = processed;
        TotalErrors = errors;
        CompletedAt = DateTime.UtcNow;
    }

    public void Fail(string error)
    {
        Status = "Failed";
        ErrorMessage = error;
        CompletedAt = DateTime.UtcNow;
    }
}
