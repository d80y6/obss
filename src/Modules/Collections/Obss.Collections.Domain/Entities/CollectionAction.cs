using Obss.Collections.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Collections.Domain.Entities;

public class CollectionAction : Entity<Guid>
{
    private CollectionAction() { }

    private CollectionAction(
        Guid id,
        Guid collectionCaseId,
        CollectionActionType actionType,
        int dunningLevel,
        string description,
        string performedBy,
        DateTime? nextActionDate)
        : base(id)
    {
        CollectionCaseId = collectionCaseId;
        ActionType = actionType;
        DunningLevel = dunningLevel;
        Description = description;
        PerformedBy = performedBy;
        PerformedAt = DateTime.UtcNow;
        NextActionDate = nextActionDate;
        IsCompleted = true;
    }

    public Guid CollectionCaseId { get; private set; }
    public CollectionActionType ActionType { get; private set; }
    public int DunningLevel { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime PerformedAt { get; private set; }
    public string PerformedBy { get; private set; } = string.Empty;
    public DateTime? NextActionDate { get; private set; }
    public bool IsCompleted { get; private set; }

    public static CollectionAction Create(
        Guid collectionCaseId,
        CollectionActionType actionType,
        int dunningLevel,
        string description,
        string performedBy,
        DateTime? nextActionDate = null)
    {
        return new CollectionAction(
            Guid.NewGuid(),
            collectionCaseId,
            actionType,
            dunningLevel,
            description,
            performedBy,
            nextActionDate);
    }

    public void Complete()
    {
        IsCompleted = true;
    }

    public void Reschedule(DateTime newDate)
    {
        NextActionDate = newDate;
        IsCompleted = false;
    }
}
