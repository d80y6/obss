using Obss.ServiceCatalog.Domain.Enums;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ServiceCatalog.Domain.Entities;

public class ServiceCandidate : AggregateRoot<Guid>, ITenantEntity
{
    private readonly List<ServiceCategory> _categories = [];

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public LifecycleStatus LifecycleStatus { get; private set; }
    public int Version { get; private set; } = 1;
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
    public Guid? ServiceSpecificationId { get; private set; }
    public Guid? BaseCandidateId { get; private set; }
    public string? FeatureSpecification { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public IReadOnlyCollection<ServiceCategory> Categories => _categories.AsReadOnly();

    private ServiceCandidate() { }

    private ServiceCandidate(Guid id, string tenantId, string name, string? description, Guid? serviceSpecificationId, Guid? baseCandidateId, string? featureSpecification, DateTime? validFrom, DateTime? validTo) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        ServiceSpecificationId = serviceSpecificationId;
        BaseCandidateId = baseCandidateId;
        FeatureSpecification = featureSpecification;
        LifecycleStatus = LifecycleStatus.Draft;
        ValidFrom = validFrom;
        ValidTo = validTo;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static ServiceCandidate Create(string tenantId, string name, string? description = null, Guid? serviceSpecificationId = null, Guid? baseCandidateId = null, string? featureSpecification = null, DateTime? validFrom = null, DateTime? validTo = null)
    {
        return new ServiceCandidate(Guid.NewGuid(), tenantId, name, description, serviceSpecificationId, baseCandidateId, featureSpecification, validFrom, validTo);
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

    public void UpdateDetails(string name, string? description, string? featureSpecification)
    {
        Name = name;
        Description = description;
        FeatureSpecification = featureSpecification;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignSpecification(Guid serviceSpecificationId)
    {
        ServiceSpecificationId = serviceSpecificationId;
        UpdatedAt = DateTime.UtcNow;
    }
}
