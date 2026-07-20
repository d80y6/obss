using System.Runtime.Serialization;

namespace Obss.SharedKernel.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException() { }
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
    protected DomainException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

public sealed class InvalidOperationException : DomainException
{
    public InvalidOperationException() { }
    public InvalidOperationException(string message) : base(message) { }
    public InvalidOperationException(string message, Exception innerException) : base(message, innerException) { }
    private InvalidOperationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

public sealed class NotFoundException : DomainException
{
    public NotFoundException() { }
    public NotFoundException(string entityName, object id)
        : base($"Entity '{entityName}' with identifier '{id}' was not found.") { }
    public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
    private NotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

public sealed class ValidationException : DomainException
{
    public ValidationException() { }
    public ValidationException(string message) : base(message) { }
    public ValidationException(string message, Exception innerException) : base(message, innerException) { }
    private ValidationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

public sealed class UnauthorizedException : DomainException
{
    public UnauthorizedException() { }
    public UnauthorizedException(string message) : base(message) { }
    public UnauthorizedException(string message, Exception innerException) : base(message, innerException) { }
    private UnauthorizedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

public sealed class ConflictException : DomainException
{
    public ConflictException() { }
    public ConflictException(string message) : base(message) { }
    public ConflictException(string message, Exception innerException) : base(message, innerException) { }
    private ConflictException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
