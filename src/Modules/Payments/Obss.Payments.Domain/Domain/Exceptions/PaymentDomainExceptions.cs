using System.Runtime.Serialization;
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.Payments.Domain.Exceptions;

[Serializable]
public sealed class PaymentNotFoundException : DomainException
{
    public PaymentNotFoundException() { }
    public PaymentNotFoundException(string message) : base(message) { }
    public PaymentNotFoundException(Guid paymentId)
        : base($"Payment with id '{paymentId}' was not found.") { }
    public PaymentNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
    private PaymentNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class PaymentAlreadyCompletedException : DomainException
{
    public PaymentAlreadyCompletedException() { }
    public PaymentAlreadyCompletedException(string message) : base(message) { }
    public PaymentAlreadyCompletedException(Guid paymentId)
        : base($"Payment '{paymentId}' is already completed.") { }
    public PaymentAlreadyCompletedException(string message, Exception innerException)
        : base(message, innerException) { }
    private PaymentAlreadyCompletedException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class InsufficientPaymentAmountException : DomainException
{
    public InsufficientPaymentAmountException() { }
    public InsufficientPaymentAmountException(string message) : base(message) { }
    public InsufficientPaymentAmountException(decimal requested, decimal available)
        : base($"Insufficient payment amount. Requested: {requested}, Available: {available}.") { }
    public InsufficientPaymentAmountException(string message, Exception innerException)
        : base(message, innerException) { }
    private InsufficientPaymentAmountException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class InvalidPaymentStateException : DomainException
{
    public InvalidPaymentStateException() { }
    public InvalidPaymentStateException(string message) : base(message) { }
    public InvalidPaymentStateException(string message, Exception innerException)
        : base(message, innerException) { }
    private InvalidPaymentStateException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
