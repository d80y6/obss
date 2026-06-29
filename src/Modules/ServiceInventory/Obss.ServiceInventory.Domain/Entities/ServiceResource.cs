using Obss.ServiceInventory.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ServiceInventory.Domain.Entities;

public class ServiceResource : Entity<Guid>
{
    private ServiceResource() { }

    private ServiceResource(
        Guid id,
        Guid serviceId,
        ResourceType resourceType,
        string resourceIdentifier)
        : base(id)
    {
        ServiceId = serviceId;
        ResourceType = resourceType;
        ResourceIdentifier = resourceIdentifier;
        Status = ResourceStatus.Allocated;
        AllocatedAt = DateTime.UtcNow;
    }

    public Guid ServiceId { get; private set; }
    public ResourceType ResourceType { get; private set; }
    public string ResourceIdentifier { get; private set; } = string.Empty;
    public ResourceStatus Status { get; private set; }
    public DateTime AllocatedAt { get; private set; }
    public DateTime? ReleasedAt { get; private set; }

    public static ServiceResource Create(Guid serviceId, ResourceType resourceType, string resourceIdentifier)
    {
        return new ServiceResource(Guid.NewGuid(), serviceId, resourceType, resourceIdentifier);
    }

    public void MarkInUse()
    {
        if (Status != ResourceStatus.Allocated)
            return;

        Status = ResourceStatus.InUse;
    }

    public void Release()
    {
        if (Status == ResourceStatus.Released)
            return;

        Status = ResourceStatus.Released;
        ReleasedAt = DateTime.UtcNow;
    }
}
