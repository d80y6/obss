using System.Runtime.Serialization;
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.CRM.Domain.Exceptions;

[Serializable]
public sealed class CustomerNotFoundException : DomainException
{
    public CustomerNotFoundException() { }
    public CustomerNotFoundException(Guid customerId)
        : base($"Customer with id '{customerId}' was not found.") { }
    public CustomerNotFoundException(string message)
        : base(message) { }
    public CustomerNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
    private CustomerNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class DuplicateCustomerException : DomainException
{
    public DuplicateCustomerException() { }
    public DuplicateCustomerException(string displayName)
        : base($"A customer with display name '{displayName}' already exists.") { }
    public DuplicateCustomerException(string message, Exception innerException)
        : base(message, innerException) { }
    private DuplicateCustomerException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class InvalidCustomerStateException : DomainException
{
    public InvalidCustomerStateException() { }
    public InvalidCustomerStateException(string message)
        : base(message) { }
    public InvalidCustomerStateException(string message, Exception innerException)
        : base(message, innerException) { }
    private InvalidCustomerStateException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
