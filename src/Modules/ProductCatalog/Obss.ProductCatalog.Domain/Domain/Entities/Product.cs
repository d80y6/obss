using Obss.ProductCatalog.Domain.Domain.Events;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ProductCatalog.Domain.Domain.Entities;

public class Product : AggregateRoot<Guid>, ITenantEntity
{
    private readonly List<ValueObjects.ProductSpecification> _specifications = [];
    private readonly List<ProductOffer> _productOffers = [];

    private Product() { }

    private Product(
        Guid id,
        string tenantId,
        string name,
        string? description,
        Guid? categoryId,
        ProductType productType,
        bool isShippable,
        bool taxable,
        string? taxCategory,
        string? productNumber)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        CategoryId = categoryId;
        ProductType = productType;
        IsActive = true;
        IsShippable = isShippable;
        Taxable = taxable;
        TaxCategory = taxCategory;
        ProductNumber = productNumber;
        LifecycleStatus = LifecycleStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProductCreatedDomainEvent(id, tenantId, name, categoryId?.ToString() ?? "none"));
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? CategoryId { get; private set; }
    public Category? Category { get; }
    public ProductType ProductType { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsShippable { get; private set; }
    public bool Taxable { get; private set; }
    public string? TaxCategory { get; private set; }
    public string? ProductNumber { get; private set; }
    public Guid? ProductSpecificationId { get; private set; }
    public ProductSpecification? ProductSpecification { get; }
    public LifecycleStatus LifecycleStatus { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<ValueObjects.ProductSpecification> Specifications => _specifications.AsReadOnly();
    public IReadOnlyCollection<ProductOffer> ProductOffers => _productOffers.AsReadOnly();

    public static Product Create(
        string tenantId,
        string name,
        string? description,
        Guid? categoryId,
        ProductType productType,
        bool isShippable,
        bool taxable,
        string? taxCategory,
        string? productNumber = null)
    {
        return new Product(
            Guid.NewGuid(),
            tenantId,
            name,
            description,
            categoryId,
            productType,
            isShippable,
            taxable,
            taxCategory,
            productNumber);
    }

    public void Activate()
    {
        if (LifecycleStatus == LifecycleStatus.Active)
            return;

        LifecycleStatus = LifecycleStatus.Active;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Retire()
    {
        if (LifecycleStatus == LifecycleStatus.Retired)
            return;

        LifecycleStatus = LifecycleStatus.Retired;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Discontinue()
    {
        if (LifecycleStatus == LifecycleStatus.Discontinued)
            return;

        LifecycleStatus = LifecycleStatus.Discontinued;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(
        string name,
        string? description,
        Guid? categoryId,
        bool isShippable,
        bool taxable,
        string? taxCategory)
    {
        Name = name;
        Description = description;
        CategoryId = categoryId;
        IsShippable = isShippable;
        Taxable = taxable;
        TaxCategory = taxCategory;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignProductSpecification(Guid productSpecificationId)
    {
        ProductSpecificationId = productSpecificationId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddSpecification(ValueObjects.ProductSpecification specification)
    {
        _specifications.Add(specification);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveSpecification(ValueObjects.ProductSpecification specification)
    {
        _specifications.Remove(specification);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddOffer(ProductOffer productOffer)
    {
        _productOffers.Add(productOffer);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveOffer(ProductOffer productOffer)
    {
        _productOffers.Remove(productOffer);
        UpdatedAt = DateTime.UtcNow;
    }
}
