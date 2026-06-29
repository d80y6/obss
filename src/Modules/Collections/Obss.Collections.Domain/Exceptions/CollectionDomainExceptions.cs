using System.Runtime.Serialization;
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.Collections.Domain.Exceptions;

[Serializable]
public sealed class CollectionCaseNotFoundException : DomainException
{
    public CollectionCaseNotFoundException() { }
    public CollectionCaseNotFoundException(string message) : base(message) { }
    public CollectionCaseNotFoundException(Guid caseId) : base($"Collection case with id '{caseId}' was not found.") { }
    public CollectionCaseNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    private CollectionCaseNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public sealed class PaymentArrangementNotFoundException : DomainException
{
    public PaymentArrangementNotFoundException() { }
    public PaymentArrangementNotFoundException(string message) : base(message) { }
    public PaymentArrangementNotFoundException(Guid arrangementId) : base($"Payment arrangement with id '{arrangementId}' was not found.") { }
    public PaymentArrangementNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    private PaymentArrangementNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public sealed class DunningPolicyNotFoundException : DomainException
{
    public DunningPolicyNotFoundException() { }
    public DunningPolicyNotFoundException(string message) : base(message) { }
    public DunningPolicyNotFoundException(Guid policyId) : base($"Dunning policy with id '{policyId}' was not found.") { }
    public DunningPolicyNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    private DunningPolicyNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public sealed class InvalidCollectionStateException : DomainException
{
    public InvalidCollectionStateException() { }
    public InvalidCollectionStateException(string message) : base(message) { }
    public InvalidCollectionStateException(string message, Exception innerException) : base(message, innerException) { }
    private InvalidCollectionStateException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public sealed class DuplicatePaymentArrangementException : DomainException
{
    public DuplicatePaymentArrangementException() { }
    public DuplicatePaymentArrangementException(string message) : base(message) { }
    public DuplicatePaymentArrangementException(Guid caseId) : base($"An active payment arrangement already exists for collection case '{caseId}'.") { }
    public DuplicatePaymentArrangementException(string message, Exception innerException) : base(message, innerException) { }
    private DuplicatePaymentArrangementException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
