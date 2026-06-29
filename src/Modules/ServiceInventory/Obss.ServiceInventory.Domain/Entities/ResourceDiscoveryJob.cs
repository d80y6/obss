using Obss.ServiceInventory.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ServiceInventory.Domain.Entities;

public class ResourceDiscoveryJob : AggregateRoot<Guid>
{
    private ResourceDiscoveryJob() { }

    private ResourceDiscoveryJob(
        Guid id,
        Guid tenantId,
        DiscoveryType discoveryType,
        string? configuration,
        string createdBy)
        : base(id)
    {
        TenantId = tenantId;
        DiscoveryType = discoveryType;
        Configuration = configuration;
        Status = DiscoveryStatus.Pending;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid TenantId { get; private set; }
    public DiscoveryType DiscoveryType { get; private set; }
    public string? Configuration { get; private set; }
    public DiscoveryStatus Status { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int ResourcesFound { get; private set; }
    public int ResourcesMatched { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public static ResourceDiscoveryJob Create(
        Guid tenantId,
        DiscoveryType discoveryType,
        string? configuration,
        string createdBy)
    {
        return new ResourceDiscoveryJob(Guid.NewGuid(), tenantId, discoveryType, configuration, createdBy);
    }

    public void Start()
    {
        Status = DiscoveryStatus.Running;
        StartedAt = DateTime.UtcNow;
    }

    public void Complete(int resourcesFound, int resourcesMatched)
    {
        Status = DiscoveryStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        ResourcesFound = resourcesFound;
        ResourcesMatched = resourcesMatched;
    }

    public void Fail(string errorMessage)
    {
        Status = DiscoveryStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
    }
}
