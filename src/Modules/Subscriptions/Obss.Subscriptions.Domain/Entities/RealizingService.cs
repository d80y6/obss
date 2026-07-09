using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Entities;

public class RealizingService : Entity<Guid>
{
    private RealizingService() { }

    public RealizingService(Guid id, Guid serviceId, string serviceType, string status)
        : base(id)
    {
        ServiceId = serviceId;
        ServiceType = serviceType;
        Status = status;
    }

    public Guid ServiceId { get; private set; }
    public string ServiceType { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
}
