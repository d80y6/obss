using System.Runtime.Serialization;
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.Billing.Domain.Exceptions;

[Serializable]
public sealed class BillNotFoundException : DomainException
{
    public BillNotFoundException() { }

    public BillNotFoundException(string message)
        : base(message) { }

    public BillNotFoundException(Guid billId)
        : base($"Bill with id '{billId}' was not found.") { }

    public BillNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }

    private BillNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class BillingCycleNotFoundException : DomainException
{
    public BillingCycleNotFoundException() { }

    public BillingCycleNotFoundException(string message)
        : base(message) { }

    public BillingCycleNotFoundException(Guid cycleId)
        : base($"Billing cycle with id '{cycleId}' was not found.") { }

    public BillingCycleNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }

    private BillingCycleNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class InvalidBillStateException : DomainException
{
    public InvalidBillStateException() { }

    public InvalidBillStateException(string message)
        : base(message) { }

    public InvalidBillStateException(string message, Exception innerException)
        : base(message, innerException) { }

    private InvalidBillStateException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
