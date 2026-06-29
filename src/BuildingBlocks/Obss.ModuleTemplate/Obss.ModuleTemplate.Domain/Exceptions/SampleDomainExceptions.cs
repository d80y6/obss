using System.Runtime.Serialization;
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.ModuleTemplate.Domain.Exceptions;

[Serializable]
public sealed class DuplicateSampleException : DomainException
{
    public DuplicateSampleException() { }
    public DuplicateSampleException(string name)
        : base($"A sample with name '{name}' already exists.") { }
    public DuplicateSampleException(string message, Exception innerException)
        : base(message, innerException) { }
    private DuplicateSampleException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class InvalidSampleStateException : DomainException
{
    public InvalidSampleStateException() { }
    public InvalidSampleStateException(string message)
        : base(message) { }
    public InvalidSampleStateException(string message, Exception innerException)
        : base(message, innerException) { }
    private InvalidSampleStateException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
