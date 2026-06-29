using System.Runtime.Serialization;
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.Ticketing.Domain.Exceptions;

[Serializable]
public sealed class InvalidTicketStateException : DomainException
{
    public InvalidTicketStateException() { }
    public InvalidTicketStateException(string message) : base(message) { }
    public InvalidTicketStateException(string message, Exception innerException) : base(message, innerException) { }
    private InvalidTicketStateException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public sealed class TicketAlreadyResolvedException : DomainException
{
    public TicketAlreadyResolvedException() { }
    public TicketAlreadyResolvedException(string ticketNumber)
        : base($"Ticket '{ticketNumber}' is already resolved.") { }
    public TicketAlreadyResolvedException(string message, Exception innerException) : base(message, innerException) { }
    private TicketAlreadyResolvedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public sealed class TicketAlreadyClosedException : DomainException
{
    public TicketAlreadyClosedException() { }
    public TicketAlreadyClosedException(string ticketNumber)
        : base($"Ticket '{ticketNumber}' is already closed.") { }
    public TicketAlreadyClosedException(string message, Exception innerException) : base(message, innerException) { }
    private TicketAlreadyClosedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public sealed class TicketNotResolvedException : DomainException
{
    public TicketNotResolvedException() { }
    public TicketNotResolvedException(string ticketNumber)
        : base($"Ticket '{ticketNumber}' is not resolved and cannot be closed.") { }
    public TicketNotResolvedException(string message, Exception innerException) : base(message, innerException) { }
    private TicketNotResolvedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
