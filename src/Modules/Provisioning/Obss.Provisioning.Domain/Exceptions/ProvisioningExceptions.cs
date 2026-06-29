using System.Runtime.Serialization;
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.Provisioning.Domain.Exceptions;

[Serializable]
public sealed class InvalidProvisioningStateException : DomainException
{
    public InvalidProvisioningStateException() { }
    public InvalidProvisioningStateException(string message)
        : base(message) { }
    public InvalidProvisioningStateException(string message, Exception innerException)
        : base(message, innerException) { }
    private InvalidProvisioningStateException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class ProvisioningJobNotFoundException : DomainException
{
    public ProvisioningJobNotFoundException() { }
    public ProvisioningJobNotFoundException(string message)
        : base(message) { }
    public ProvisioningJobNotFoundException(Guid jobId)
        : base($"Provisioning job with id '{jobId}' was not found.") { }
    public ProvisioningJobNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
    private ProvisioningJobNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class ProvisioningTemplateNotFoundException : DomainException
{
    public ProvisioningTemplateNotFoundException() { }
    public ProvisioningTemplateNotFoundException(string message)
        : base(message) { }
    public ProvisioningTemplateNotFoundException(Guid templateId)
        : base($"Provisioning template with id '{templateId}' was not found.") { }
    public ProvisioningTemplateNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
    private ProvisioningTemplateNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
