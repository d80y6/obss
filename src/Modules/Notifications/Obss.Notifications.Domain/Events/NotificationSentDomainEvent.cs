using Obss.SharedKernel.Domain.Common;

namespace Obss.Notifications.Domain.Events;

public sealed class NotificationSentDomainEvent : DomainEvent
{
    public NotificationSentDomainEvent(
        Guid notificationId,
        string recipient,
        string channel,
        string? referenceType,
        Guid? referenceId)
    {
        NotificationId = notificationId;
        Recipient = recipient;
        Channel = channel;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
    }

    public Guid NotificationId { get; }
    public string Recipient { get; }
    public string Channel { get; }
    public string? ReferenceType { get; }
    public Guid? ReferenceId { get; }
}
