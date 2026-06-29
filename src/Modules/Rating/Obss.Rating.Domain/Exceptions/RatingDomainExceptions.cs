using System.Runtime.Serialization;
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.Rating.Domain.Exceptions;

[Serializable]
public sealed class RatingRuleNotFoundException : DomainException
{
    public RatingRuleNotFoundException() { }
    public RatingRuleNotFoundException(string message)
        : base(message) { }
    public RatingRuleNotFoundException(Guid ruleId)
        : base($"Rating rule with identifier '{ruleId}' was not found.") { }
    public RatingRuleNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
    private RatingRuleNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class InvalidUsageRecordException : DomainException
{
    public InvalidUsageRecordException() { }
    public InvalidUsageRecordException(string message)
        : base(message) { }
    public InvalidUsageRecordException(string message, Exception innerException)
        : base(message, innerException) { }
    private InvalidUsageRecordException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
