using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Collections.Domain.Entities;

public class DunningPolicy : AggregateRoot<Guid>, ITenantEntity
{
    private Dictionary<int, decimal> _dunningFees = [];

    private DunningPolicy() { }

    private DunningPolicy(
        Guid id,
        string tenantId,
        string name,
        string description,
        int maxDunningLevel,
        Dictionary<int, decimal> dunningFees,
        int daysBetweenActions,
        int escalationAfterDays)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        IsActive = true;
        MaxDunningLevel = maxDunningLevel;
        _dunningFees = dunningFees ?? [];
        DaysBetweenActions = daysBetweenActions;
        EscalationAfterDays = escalationAfterDays;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public int MaxDunningLevel { get; private set; }
    public IReadOnlyDictionary<int, decimal> DunningFees => _dunningFees.AsReadOnly();
    public int DaysBetweenActions { get; private set; }
    public int EscalationAfterDays { get; private set; }

    public static DunningPolicy Create(
        string tenantId,
        string name,
        string description,
        int maxDunningLevel,
        Dictionary<int, decimal> dunningFees,
        int daysBetweenActions,
        int escalationAfterDays)
    {
        return new DunningPolicy(
            Guid.NewGuid(),
            tenantId,
            name,
            description,
            maxDunningLevel,
            dunningFees,
            daysBetweenActions,
            escalationAfterDays);
    }

    public void UpdateDetails(
        string name,
        string description,
        int maxDunningLevel,
        Dictionary<int, decimal> dunningFees,
        int daysBetweenActions,
        int escalationAfterDays)
    {
        Name = name;
        Description = description;
        MaxDunningLevel = maxDunningLevel;
        _dunningFees = dunningFees ?? [];
        DaysBetweenActions = daysBetweenActions;
        EscalationAfterDays = escalationAfterDays;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public decimal GetFeeForLevel(int level)
    {
        return _dunningFees.TryGetValue(level, out var fee) ? fee : 0;
    }

    public int GetNextDunningLevel(int currentLevel)
    {
        if (currentLevel >= MaxDunningLevel)
            return currentLevel;

        return currentLevel + 1;
    }
}
