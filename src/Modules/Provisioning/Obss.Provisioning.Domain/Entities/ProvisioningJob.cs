using System.Text.Json;
using Obss.Provisioning.Domain.Events;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Entities;

public class ProvisioningJob : AggregateRoot<Guid>
{
    private readonly List<ProvisioningTask> _tasks = [];

    private ProvisioningJob() { }

    private ProvisioningJob(
        Guid id,
        Guid tenantId,
        Guid orderId,
        Guid orderItemId,
        Guid customerId,
        string serviceType,
        ProvisioningAction action)
        : base(id)
    {
        TenantId = tenantId;
        OrderId = orderId;
        OrderItemId = orderItemId;
        CustomerId = customerId;
        ServiceType = serviceType;
        Action = action;
        Status = JobStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid TenantId { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid OrderItemId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid? ServiceId { get; private set; }
    public string ServiceType { get; private set; } = string.Empty;
    public ProvisioningAction Action { get; private set; }
    public JobStatus Status { get; private set; }
    public Guid? WorkflowInstanceId { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<ProvisioningTask> Tasks => _tasks.AsReadOnly();

    public static ProvisioningJob Create(
        Guid tenantId,
        Guid orderId,
        Guid orderItemId,
        Guid customerId,
        string serviceType,
        ProvisioningAction action)
    {
        return new ProvisioningJob(
            Guid.NewGuid(),
            tenantId,
            orderId,
            orderItemId,
            customerId,
            serviceType,
            action);
    }

    public void Queue()
    {
        if (Status != JobStatus.Pending)
            return;

        Status = JobStatus.Queued;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Start()
    {
        if (Status != JobStatus.Queued)
            return;

        Status = JobStatus.InProgress;
        StartedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProvisioningJobStartedDomainEvent(Id, OrderId, ServiceId, Action.ToString()));
    }

    public void Complete()
    {
        if (Status != JobStatus.InProgress)
            return;

        Status = JobStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProvisioningJobCompletedDomainEvent(Id, ServiceId));
    }

    public void Fail(string error)
    {
        if (Status is JobStatus.Completed or JobStatus.RolledBack)
            return;

        Status = JobStatus.Failed;
        ErrorMessage = error;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProvisioningJobFailedDomainEvent(Id, error));
    }

    public void Rollback()
    {
        if (Status != JobStatus.Failed)
            return;

        Status = JobStatus.RolledBack;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Retry()
    {
        if (Status != JobStatus.Failed && Status != JobStatus.RolledBack)
            throw new InvalidOperationException($"Cannot retry job in '{Status}' state.");

        Status = JobStatus.Queued;
        ErrorMessage = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignWorkflow(Guid workflowInstanceId)
    {
        WorkflowInstanceId = workflowInstanceId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignService(Guid serviceId)
    {
        ServiceId = serviceId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddTask(ProvisioningTask task)
    {
        _tasks.Add(task);
        UpdatedAt = DateTime.UtcNow;
    }
}
