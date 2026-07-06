using Obss.ServiceCatalog.Domain.Enums;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ServiceCatalog.Domain.Entities;

public class ServiceCategory : AggregateRoot<Guid>, ITenantEntity
{
    private readonly List<ServiceCandidate> _candidates = [];

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public LifecycleStatus LifecycleStatus { get; private set; }
    public int Version { get; private set; } = 1;
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsRoot => !ParentCategoryId.HasValue;
    public IReadOnlyCollection<ServiceCandidate> Candidates => _candidates.AsReadOnly();

    private ServiceCategory() { }

    private ServiceCategory(Guid id, string tenantId, string name, string? description, Guid? parentCategoryId, DateTime? validFrom, DateTime? validTo) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        ParentCategoryId = parentCategoryId;
        LifecycleStatus = LifecycleStatus.Draft;
        ValidFrom = validFrom;
        ValidTo = validTo;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static ServiceCategory Create(string tenantId, string name, string? description = null, Guid? parentCategoryId = null, DateTime? validFrom = null, DateTime? validTo = null)
    {
        return new ServiceCategory(Guid.NewGuid(), tenantId, name, description, parentCategoryId, validFrom, validTo);
    }

    public void Activate()
    {
        LifecycleStatus = LifecycleStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Retire()
    {
        LifecycleStatus = LifecycleStatus.Retired;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string? description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetValidityPeriod(DateTime? validFrom, DateTime? validTo)
    {
        ValidFrom = validFrom;
        ValidTo = validTo;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddCandidate(ServiceCandidate candidate)
    {
        if (!_candidates.Any(c => c.Id == candidate.Id))
            _candidates.Add(candidate);
    }

    public void RemoveCandidate(Guid candidateId)
    {
        _candidates.RemoveAll(c => c.Id == candidateId);
    }
}
