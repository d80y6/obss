using System.Runtime.Serialization;
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.NetworkInventory.Domain.Exceptions;

[Serializable]
public sealed class DuplicateHostnameException : DomainException
{
    public DuplicateHostnameException() { }
    public DuplicateHostnameException(string hostname)
        : base($"A network element with hostname '{hostname}' already exists.") { }
    public DuplicateHostnameException(string message, Exception innerException)
        : base(message, innerException) { }
    private DuplicateHostnameException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class InvalidIPAddressException : DomainException
{
    public InvalidIPAddressException() { }
    public InvalidIPAddressException(string ipAddress)
        : base($"The IP address '{ipAddress}' is invalid.") { }
    public InvalidIPAddressException(string message, Exception innerException)
        : base(message, innerException) { }
    private InvalidIPAddressException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class PONPortFullException : DomainException
{
    public PONPortFullException() { }
    public PONPortFullException(int portNumber)
        : base($"PON port {portNumber} has reached its maximum number of connected ONTs.") { }
    public PONPortFullException(string message, Exception innerException)
        : base(message, innerException) { }
    private PONPortFullException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
