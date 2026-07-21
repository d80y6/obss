using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;
using Obss.Subscriptions.Domain.Events;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Domain.Entities;

public class Product : AggregateRoot<Guid>, ITenantEntity
{
    private readonly List<ProductRelationship> _relationships = [];
    private readonly List<ProductCharacteristic> _characteristics = [];
    private readonly List<ProductPrice> _prices = [];
    private readonly List<ProductTerm> _terms = [];
    private readonly List<RealizingService> _realizingServices = [];
    private readonly List<RealizingResource> _realizingResources = [];

    private Product() { }

    private Product(
        Guid id,
        Guid tenantId,
        Guid customerId,
        string? name,
        string? description,
        Guid? productSpecificationId,
        Guid? productOfferingId)
        : base(id)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        Name = name;
        Description = description;
        ProductSpecificationId = productSpecificationId;
        ProductOfferingId = productOfferingId;
        Status = ProductStatus.Created;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid TenantId { get; private set; }
    string ITenantEntity.TenantId => TenantId.ToString("N");
    public Guid CustomerId { get; private set; }
    public string? Name { get; private set; }
    public string? Description { get; private set; }
    public Guid? ProductSpecificationId { get; private set; }
    public Guid? ProductOfferingId { get; private set; }
    public ProductStatus Status { get; private set; }
    public DateTime? ActivationDate { get; private set; }
    public DateTime? TerminationDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public BillingAccountRef? BillingAccount { get; private set; }
    public Place? Place { get; private set; }
    public AgreementRef? Agreement { get; private set; }

    public IReadOnlyCollection<ProductRelationship> Relationships => _relationships.AsReadOnly();
    public IReadOnlyCollection<ProductCharacteristic> Characteristics => _characteristics.AsReadOnly();
    public IReadOnlyCollection<ProductPrice> Prices => _prices.AsReadOnly();
    public IReadOnlyCollection<ProductTerm> Terms => _terms.AsReadOnly();
    public IReadOnlyCollection<RealizingService> RealizingServices => _realizingServices.AsReadOnly();
    public IReadOnlyCollection<RealizingResource> RealizingResources => _realizingResources.AsReadOnly();

    public static Product Create(
        Guid tenantId,
        Guid customerId,
        string? name,
        string? description,
        Guid? productSpecificationId,
        Guid? productOfferingId)
    {
        var product = new Product(
            Guid.NewGuid(), tenantId, customerId, name, description,
            productSpecificationId, productOfferingId);
        product.AddDomainEvent(new ProductCreatedDomainEvent(product.Id, customerId, productOfferingId));
        return product;
    }

    public void SetBillingAccount(BillingAccountRef? billingAccount) { BillingAccount = billingAccount; UpdatedAt = DateTime.UtcNow; }
    public void SetPlace(Place? place) { Place = place; UpdatedAt = DateTime.UtcNow; }
    public void SetAgreement(AgreementRef? agreement) { Agreement = agreement; UpdatedAt = DateTime.UtcNow; }

    public void Activate()
    {
        if (Status != ProductStatus.Created) return;
        Status = ProductStatus.Active;
        ActivationDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ProductActivatedDomainEvent(Id));
    }

    public void Suspend()
    {
        if (Status != ProductStatus.Active) return;
        Status = ProductStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ProductSuspendedDomainEvent(Id));
    }

    public void Cancel()
    {
        if (Status is ProductStatus.Cancelled or ProductStatus.Terminated) return;
        Status = ProductStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ProductCancelledDomainEvent(Id));
    }

    public void Terminate()
    {
        if (Status != ProductStatus.Cancelled) return;
        Status = ProductStatus.Terminated;
        TerminationDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ProductTerminatedDomainEvent(Id));
    }

    public void AddRelationship(ProductRelationship relationship) { _relationships.Add(relationship); UpdatedAt = DateTime.UtcNow; }
    public void AddCharacteristic(ProductCharacteristic characteristic) { _characteristics.Add(characteristic); UpdatedAt = DateTime.UtcNow; }
    public void AddPrice(ProductPrice price) { _prices.Add(price); UpdatedAt = DateTime.UtcNow; }
    public void AddTerm(ProductTerm term) { _terms.Add(term); UpdatedAt = DateTime.UtcNow; }
    public void AddRealizingService(RealizingService service) { _realizingServices.Add(service); UpdatedAt = DateTime.UtcNow; }
    public void AddRealizingResource(RealizingResource resource) { _realizingResources.Add(resource); UpdatedAt = DateTime.UtcNow; }

    public void UpdateDetails(string? name = null, string? description = null,
        Guid? productSpecificationId = null, Guid? productOfferingId = null)
    {
        if (name is not null) Name = name;
        if (description is not null) Description = description;
        if (productSpecificationId is not null) ProductSpecificationId = productSpecificationId;
        if (productOfferingId is not null) ProductOfferingId = productOfferingId;
        UpdatedAt = DateTime.UtcNow;
    }
}
