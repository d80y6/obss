using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Entities;

public class SubscriptionService : Entity<Guid>
{
    private SubscriptionService() { }

    public SubscriptionService(
        Guid id,
        Guid subscriptionId,
        Guid serviceId,
        string serviceType,
        string status)
        : base(id)
    {
        SubscriptionId = subscriptionId;
        ServiceId = serviceId;
        ServiceType = serviceType;
        Status = status;
        ProvisionedAt = DateTime.UtcNow;
    }

    public Guid SubscriptionId { get; private set; }
    public Guid ServiceId { get; private set; }
    public string ServiceType { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public DateTime ProvisionedAt { get; private set; }

    public Subscription Subscription { get; private set; } = null!;

    public void UpdateStatus(string status)
    {
        Status = status;
    }
}
