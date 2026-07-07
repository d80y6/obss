using Obss.ServiceInventory.Domain.Events;
using Obss.ServiceInventory.Domain.Exceptions;
using Obss.ServiceInventory.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ServiceInventory.Domain.Entities;

public class Service : AggregateRoot<Guid>
{
    private readonly List<ServiceResource> _resources = [];

    private Service() { }

    private Service(
        Guid id,
        Guid tenantId,
        Guid customerId,
        Guid subscriptionId,
        ServiceType serviceType,
        string serviceIdentifier,
        string? location,
        string? configuration,
        Guid? serviceSpecificationId = null)
        : base(id)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        SubscriptionId = subscriptionId;
        ServiceType = serviceType;
        ServiceIdentifier = serviceIdentifier;
        Status = ServiceStatus.Pending;
        Location = location;
        Configuration = configuration;
        AtType = "Service";
        AtBaseType = "ServiceInventory";
        AtSchemaLocation = "https://tmf-open-api-table.s3.amazonaws.com/schemas/TMF638-ServiceInventory-v4.0.0.json";
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        ServiceSpecificationId = serviceSpecificationId;
    }

    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public ServiceType ServiceType { get; private set; }
    public string ServiceIdentifier { get; private set; } = string.Empty;
    public ServiceStatus Status { get; private set; }
    public DateTime? ActivationDate { get; private set; }
    public DateTime? SuspendedAt { get; private set; }
    public DateTime? DecommissionedAt { get; private set; }
    public string? Configuration { get; private set; }
    public string? Location { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid? ServiceSpecificationId { get; private set; }

    public string? Href { get; private set; }
    public string? AtType { get; private set; }
    public string? AtBaseType { get; private set; }
    public string? AtSchemaLocation { get; private set; }
    public string? Name { get; private set; }
    public string? Description { get; private set; }
    public string? ExternalId { get; private set; }
    public string? Place { get; private set; }
    public string? Category { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? CompletionDate { get; private set; }
    public string? RelatedPartyId { get; private set; }
    public string? RelatedPartyName { get; private set; }
    public string? RelatedPartyRole { get; private set; }

    public IReadOnlyCollection<ServiceResource> Resources => _resources.AsReadOnly();

    public static Service Create(
        Guid tenantId,
        Guid customerId,
        Guid subscriptionId,
        ServiceType serviceType,
        string serviceIdentifier,
        string? location = null,
        string? configuration = null,
        Guid? serviceSpecificationId = null)
    {
        return new Service(
            Guid.NewGuid(),
            tenantId,
            customerId,
            subscriptionId,
            serviceType,
            serviceIdentifier,
            location,
            configuration,
            serviceSpecificationId);
    }

    public void Activate()
    {
        if (Status != ServiceStatus.Pending && Status != ServiceStatus.Provisioning)
            throw new InvalidServiceStateException(
                $"Cannot activate service in '{Status}' status. Only 'Pending' or 'Provisioning' services can be activated.");

        Status = ServiceStatus.Active;
        ActivationDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ServiceActivatedDomainEvent(Id, TenantId, CustomerId));
    }

    public void Suspend(string reason)
    {
        if (Status != ServiceStatus.Active)
            throw new InvalidServiceStateException(
                $"Cannot suspend service in '{Status}' status. Only 'Active' services can be suspended.");

        Status = ServiceStatus.Suspended;
        SuspendedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ServiceSuspendedDomainEvent(Id, reason));
    }

    public void Resume()
    {
        if (Status != ServiceStatus.Suspended)
            throw new InvalidServiceStateException(
                $"Cannot resume service in '{Status}' status. Only 'Suspended' services can be resumed.");

        Status = ServiceStatus.Active;
        SuspendedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Decommission()
    {
        if (Status == ServiceStatus.Decommissioned)
            throw new InvalidServiceStateException("Service is already decommissioned.");

        Status = ServiceStatus.Decommissioned;
        DecommissionedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        foreach (var resource in _resources.Where(r => r.Status != ResourceStatus.Released))
        {
            resource.Release();
        }

        AddDomainEvent(new ServiceDecommissionedDomainEvent(Id, CustomerId));
    }

    public void UpdateConfiguration(string configuration)
    {
        Configuration = configuration;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLocation(string location)
    {
        Location = location;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddResource(ServiceResource resource)
    {
        _resources.Add(resource);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetProvisioning()
    {
        if (Status != ServiceStatus.Pending)
            throw new InvalidServiceStateException(
                $"Cannot set service to provisioning in '{Status}' status.");

        Status = ServiceStatus.Provisioning;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTmfDetails(string? name = null, string? description = null, string? externalId = null, string? place = null, string? category = null, DateTime? startDate = null, DateTime? completionDate = null, string? href = null, string? relatedPartyId = null, string? relatedPartyName = null, string? relatedPartyRole = null)
    {
        Name = name ?? Name;
        Description = description ?? Description;
        ExternalId = externalId ?? ExternalId;
        Place = place ?? Place;
        Category = category ?? Category;
        StartDate = startDate ?? StartDate;
        CompletionDate = completionDate ?? CompletionDate;
        Href = href ?? Href;
        RelatedPartyId = relatedPartyId ?? RelatedPartyId;
        RelatedPartyName = relatedPartyName ?? RelatedPartyName;
        RelatedPartyRole = relatedPartyRole ?? RelatedPartyRole;
        UpdatedAt = DateTime.UtcNow;
    }
}
