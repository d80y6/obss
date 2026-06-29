using Obss.SharedKernel.Domain.Events;

namespace Obss.Provisioning.Application.IntegrationEvents;

public sealed class ProvisioningJobCompletedIntegrationEvent : IntegrationEvent
{
    public ProvisioningJobCompletedIntegrationEvent(
        Guid jobId,
        Guid orderId,
        Guid? serviceId,
        string action,
        string status)
    {
        JobId = jobId;
        OrderId = orderId;
        ServiceId = serviceId;
        Action = action;
        Status = status;
    }

    public Guid JobId { get; }
    public Guid OrderId { get; }
    public Guid? ServiceId { get; }
    public string Action { get; }
    public string Status { get; }
}
