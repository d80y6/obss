using System.Runtime.Serialization;
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.Subscriptions.Domain.Exceptions;

[Serializable]
public sealed class SubscriptionNotFoundException : DomainException
{
    public SubscriptionNotFoundException() { }

    public SubscriptionNotFoundException(string message)
        : base(message) { }

    public SubscriptionNotFoundException(Guid subscriptionId)
        : base($"Subscription with id '{subscriptionId}' was not found.") { }

    public SubscriptionNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }

    private SubscriptionNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class InvalidSubscriptionStateException : DomainException
{
    public InvalidSubscriptionStateException() { }

    public InvalidSubscriptionStateException(string message)
        : base(message) { }

    public InvalidSubscriptionStateException(string message, Exception innerException)
        : base(message, innerException) { }

    private InvalidSubscriptionStateException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
