using System.Runtime.Serialization;
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.Orders.Domain.Exceptions;

[Serializable]
public sealed class OrderNotFoundException : DomainException
{
    public OrderNotFoundException() { }
    public OrderNotFoundException(string message)
        : base(message) { }
    public OrderNotFoundException(Guid orderId)
        : base($"Order with id '{orderId}' was not found.") { }
    public OrderNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
    private OrderNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class InvalidOrderStateException : DomainException
{
    public InvalidOrderStateException() { }
    public InvalidOrderStateException(string message)
        : base(message) { }
    public InvalidOrderStateException(string message, Exception innerException)
        : base(message, innerException) { }
    private InvalidOrderStateException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class OrderItemNotFoundException : DomainException
{
    public OrderItemNotFoundException() { }
    public OrderItemNotFoundException(string message)
        : base(message) { }
    public OrderItemNotFoundException(Guid itemId)
        : base($"Order item with id '{itemId}' was not found.") { }
    public OrderItemNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
    private OrderItemNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
