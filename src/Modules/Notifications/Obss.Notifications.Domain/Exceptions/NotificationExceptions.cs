using System.Runtime.Serialization;
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.Notifications.Domain.Exceptions;

[Serializable]
public sealed class TemplateNotFoundException : DomainException
{
    public TemplateNotFoundException() { }
    public TemplateNotFoundException(string message)
        : base(message) { }
    public TemplateNotFoundException(Guid templateId)
        : base($"Notification template '{templateId}' was not found.") { }
    public TemplateNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
    private TemplateNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class TemplateRenderException : DomainException
{
    public TemplateRenderException() { }
    public TemplateRenderException(string message)
        : base(message) { }
    public TemplateRenderException(string message, Exception innerException)
        : base(message, innerException) { }
    private TemplateRenderException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class NotificationDeliveryException : DomainException
{
    public NotificationDeliveryException() { }
    public NotificationDeliveryException(string message)
        : base(message) { }
    public NotificationDeliveryException(string message, Exception innerException)
        : base(message, innerException) { }
    private NotificationDeliveryException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
