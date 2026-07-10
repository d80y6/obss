using System.Runtime.Serialization;
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.Orders.Domain.Exceptions;

[Serializable]
public sealed class ProductOrderNotFoundException : DomainException
{
    public ProductOrderNotFoundException() { }
    public ProductOrderNotFoundException(string message)
        : base(message) { }
    public ProductOrderNotFoundException(Guid orderId)
        : base($"Product order with id '{orderId}' was not found.") { }
    public ProductOrderNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
    private ProductOrderNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class InvalidProductOrderStateException : DomainException
{
    public InvalidProductOrderStateException() { }
    public InvalidProductOrderStateException(string message)
        : base(message) { }
    public InvalidProductOrderStateException(string message, Exception innerException)
        : base(message, innerException) { }
    private InvalidProductOrderStateException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class ProductOrderItemNotFoundException : DomainException
{
    public ProductOrderItemNotFoundException() { }
    public ProductOrderItemNotFoundException(string message)
        : base(message) { }
    public ProductOrderItemNotFoundException(Guid itemId)
        : base($"Product order item with id '{itemId}' was not found.") { }
    public ProductOrderItemNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
    private ProductOrderItemNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
