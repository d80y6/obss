using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Entities;

public class RealizingResource : Entity<Guid>
{
    private RealizingResource() { }

    public RealizingResource(Guid id, Guid resourceId, string resourceType, string status)
        : base(id)
    {
        ResourceId = resourceId;
        ResourceType = resourceType;
        Status = status;
    }

    public Guid ResourceId { get; private set; }
    public string ResourceType { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
}
