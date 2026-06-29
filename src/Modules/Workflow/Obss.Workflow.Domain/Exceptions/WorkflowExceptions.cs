using System.Runtime.Serialization;
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.Workflow.Domain.Exceptions;

[Serializable]
public sealed class WorkflowNotFoundException : DomainException
{
    public WorkflowNotFoundException() { }
    public WorkflowNotFoundException(string message) : base(message) { }
    public WorkflowNotFoundException(Guid id)
        : base($"Workflow with id '{id}' was not found.") { }
    public WorkflowNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
    private WorkflowNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class WorkflowStepNotFoundException : DomainException
{
    public WorkflowStepNotFoundException() { }
    public WorkflowStepNotFoundException(string message) : base(message) { }
    public WorkflowStepNotFoundException(Guid stepId)
        : base($"Workflow step with id '{stepId}' was not found.") { }
    public WorkflowStepNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
    private WorkflowStepNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class WorkflowInstanceNotFoundException : DomainException
{
    public WorkflowInstanceNotFoundException() { }
    public WorkflowInstanceNotFoundException(string message) : base(message) { }
    public WorkflowInstanceNotFoundException(Guid instanceId)
        : base($"Workflow instance with id '{instanceId}' was not found.") { }
    public WorkflowInstanceNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
    private WorkflowInstanceNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class InvalidWorkflowStateException : DomainException
{
    public InvalidWorkflowStateException() { }
    public InvalidWorkflowStateException(string message) : base(message) { }
    public InvalidWorkflowStateException(string message, Exception innerException)
        : base(message, innerException) { }
    private InvalidWorkflowStateException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
