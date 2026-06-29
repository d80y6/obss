using System.Runtime.Serialization;
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.ServiceInventory.Domain.Exceptions;

[Serializable]
public sealed class InvalidServiceStateException : DomainException
{
    public InvalidServiceStateException() { }
    public InvalidServiceStateException(string message) : base(message) { }
    public InvalidServiceStateException(string message, Exception innerException) : base(message, innerException) { }
    private InvalidServiceStateException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public sealed class ServiceNotFoundException : DomainException
{
    public ServiceNotFoundException() { }
    public ServiceNotFoundException(string message) : base(message) { }
    public ServiceNotFoundException(Guid serviceId) : base($"Service with id '{serviceId}' was not found.") { }
    public ServiceNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    private ServiceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public sealed class DuplicateServiceIdentifierException : DomainException
{
    public DuplicateServiceIdentifierException() { }
    public DuplicateServiceIdentifierException(string identifier) : base($"A service with identifier '{identifier}' already exists.") { }
    public DuplicateServiceIdentifierException(string message, Exception innerException) : base(message, innerException) { }
    private DuplicateServiceIdentifierException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
