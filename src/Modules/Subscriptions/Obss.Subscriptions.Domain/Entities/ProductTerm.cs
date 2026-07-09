using Obss.SharedKernel.Domain.Common;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Domain.Entities;

public class ProductTerm : Entity<Guid>
{
    private ProductTerm() { }

    public ProductTerm(Guid id, string name, int duration, DurationUnit durationUnit,
        DateTime startDate, DateTime? endDate = null, string? description = null)
        : base(id)
    {
        Name = name;
        Duration = duration;
        DurationUnit = durationUnit;
        StartDate = startDate;
        EndDate = endDate;
        Description = description;
    }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int Duration { get; private set; }
    public DurationUnit DurationUnit { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
}
