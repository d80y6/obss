using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Entities;

public class ServiceOrderMilestone : Entity<Guid>
{
    public ServiceOrderMilestone(
        Guid id, string name, string? description,
        DateTime date, MilestoneStatus status)
        : base(id)
    {
        Name = name;
        Description = description;
        Date = date;
        Status = status;
    }

    private ServiceOrderMilestone() { }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime Date { get; private set; }
    public MilestoneStatus Status { get; private set; }
}

public enum MilestoneStatus
{
    Pending,
    Reached,
    Missed,
    Cancelled
}
