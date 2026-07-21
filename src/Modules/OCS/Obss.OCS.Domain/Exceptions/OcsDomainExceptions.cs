using System.Runtime.Serialization;
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.OCS.Domain.Exceptions;

[Serializable]
public sealed class InsufficientBalanceException : DomainException
{
    public InsufficientBalanceException() { }

    public InsufficientBalanceException(string message) : base(message) { }

    public InsufficientBalanceException(string message, Exception innerException) : base(message, innerException) { }

    private InsufficientBalanceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public sealed class CreditPoolExhaustedException : DomainException
{
    public CreditPoolExhaustedException() { }

    public CreditPoolExhaustedException(string message) : base(message) { }

    public CreditPoolExhaustedException(Guid poolId)
        : base($"Credit pool {poolId} is exhausted") { }

    public CreditPoolExhaustedException(string message, Exception innerException) : base(message, innerException) { }

    private CreditPoolExhaustedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
