using System.Runtime.Serialization;
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.NumberInventory.Domain.Exceptions;

[Serializable]
public sealed class TelephoneNumberNotFoundException : DomainException
{
    public TelephoneNumberNotFoundException() { }

    public TelephoneNumberNotFoundException(string message)
        : base(message) { }

    public TelephoneNumberNotFoundException(Guid numberId)
        : base($"Telephone number with id '{numberId}' was not found.") { }

    public TelephoneNumberNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }

    private TelephoneNumberNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class InvalidNumberStateException : DomainException
{
    public InvalidNumberStateException() { }

    public InvalidNumberStateException(string message)
        : base(message) { }

    public InvalidNumberStateException(string message, Exception innerException)
        : base(message, innerException) { }

    private InvalidNumberStateException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
