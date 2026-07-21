using System.Runtime.Serialization;
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.AAA.Domain.Exceptions;

[Serializable]
public sealed class NasNotFoundException : DomainException
{
    public NasNotFoundException() { }

    public NasNotFoundException(string message)
        : base(message) { }

    public NasNotFoundException(Guid nasId)
        : base($"Network access server with id '{nasId}' was not found.") { }

    public NasNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }

    private NasNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class InvalidNasStateException : DomainException
{
    public InvalidNasStateException() { }

    public InvalidNasStateException(string message)
        : base(message) { }

    public InvalidNasStateException(string message, Exception innerException)
        : base(message, innerException) { }

    private InvalidNasStateException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class SessionNotFoundException : DomainException
{
    public SessionNotFoundException() { }

    public SessionNotFoundException(string message)
        : base(message) { }

    public SessionNotFoundException(Guid sessionId)
        : base($"RADIUS session with id '{sessionId}' was not found.") { }

    public SessionNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }

    private SessionNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class InvalidSessionStateException : DomainException
{
    public InvalidSessionStateException() { }

    public InvalidSessionStateException(string message)
        : base(message) { }

    public InvalidSessionStateException(string message, Exception innerException)
        : base(message, innerException) { }

    private InvalidSessionStateException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
