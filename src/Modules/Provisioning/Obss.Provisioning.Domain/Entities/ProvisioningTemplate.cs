using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Entities;

public class ProvisioningTemplate : AggregateRoot<Guid>
{
    private ProvisioningTemplate() { }

    private ProvisioningTemplate(
        Guid id,
        Guid tenantId,
        string name,
        string? description,
        string serviceType,
        string action,
        Guid workflowDefinitionId)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        ServiceType = serviceType;
        Action = action;
        WorkflowDefinitionId = workflowDefinitionId;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string ServiceType { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public Guid WorkflowDefinitionId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static ProvisioningTemplate Create(
        Guid tenantId,
        string name,
        string? description,
        string serviceType,
        string action,
        Guid workflowDefinitionId)
    {
        return new ProvisioningTemplate(
            Guid.NewGuid(),
            tenantId,
            name,
            description,
            serviceType,
            action,
            workflowDefinitionId);
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
    }

    public void AssignWorkflow(Guid workflowDefId)
    {
        WorkflowDefinitionId = workflowDefId;
    }
}
