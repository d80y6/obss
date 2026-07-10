using System.Runtime.Serialization;
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.Orders.Domain.Exceptions;

[Serializable]
public sealed class InvalidProductOrderItemStateException : DomainException
{
    public InvalidProductOrderItemStateException() { }
    public InvalidProductOrderItemStateException(string message)
        : base(message) { }
    public InvalidProductOrderItemStateException(string message, Exception innerException)
        : base(message, innerException) { }
    private InvalidProductOrderItemStateException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
