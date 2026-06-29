using Obss.SharedKernel.Domain.Common;

namespace Obss.Notifications.Domain.Events;

public sealed class NotificationFailedDomainEvent : DomainEvent
{
    public NotificationFailedDomainEvent(
        Guid notificationId,
        string recipient,
        string channel,
        string errorMessage,
        string? referenceType,
        Guid? referenceId)
    {
        NotificationId = notificationId;
        Recipient = recipient;
        Channel = channel;
        ErrorMessage = errorMessage;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
    }

    public Guid NotificationId { get; }
    public string Recipient { get; }
    public string Channel { get; }
    public string ErrorMessage { get; }
    public string? ReferenceType { get; }
    public Guid? ReferenceId { get; }
}
