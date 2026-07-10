# WP-019: Product Inventory (TMF637) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a TMF637-compliant `Product` aggregate root to the Subscriptions module alongside the existing `Subscription`. `Subscription` references `Product` via its existing `ProductId` FK.

**Architecture:** New `Product` aggregate root with child entities (relationships, characteristics, prices, terms, realizing services/resources). `Subscription` creation updated to create `Product` first. TMF637 fields live on `Product`.

**Tech Stack:** .NET 9, EF Core/Npgsql, MediatR, Mapster, FluentValidation, Next.js/React/TanStack Query

---

### Task 1: Domain — Product aggregate root

**Files:**
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Domain/Entities/Product.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Domain/Entities/ProductRelationship.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Domain/Entities/ProductCharacteristic.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Domain/Entities/ProductPrice.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Domain/Entities/ProductTerm.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Domain/Entities/RealizingService.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Domain/Entities/RealizingResource.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Domain/ValueObjects/ProductStatus.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Domain/ValueObjects/ProductRelationshipType.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Domain/ValueObjects/PriceType.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Domain/ValueObjects/DurationUnit.cs`

- [ ] **Step 1: Create enums**

```csharp
// ProductStatus.cs
namespace Obss.Subscriptions.Domain.ValueObjects;

public enum ProductStatus
{
    Created,
    Active,
    Suspended,
    Cancelled,
    Terminated
}
```

```csharp
// ProductRelationshipType.cs
namespace Obss.Subscriptions.Domain.ValueObjects;

public enum ProductRelationshipType
{
    ReliesOn,
    IsReliedOn,
    Bundled,
    SubProduct
}
```

```csharp
// PriceType.cs
namespace Obss.Subscriptions.Domain.ValueObjects;

public enum PriceType
{
    OneTime,
    Recurring,
    Usage
}
```

```csharp
// DurationUnit.cs
namespace Obss.Subscriptions.Domain.ValueObjects;

public enum DurationUnit
{
    Days,
    Months,
    Years
}
```

- [ ] **Step 2: Create value object records**

```csharp
// Place.cs (in ValueObjects/)
namespace Obss.Subscriptions.Domain.ValueObjects;

public sealed record Place(
    string? Id,
    string? Role,
    string? Name,
    string? Street,
    string? City,
    string? State,
    string? Zip,
    string? Country);

public sealed record BillingAccountRef(
    string AccountId,
    string? Href);

public sealed record AgreementRef(
    string AgreementId,
    string? Name,
    string? Href);
```

- [ ] **Step 3: Create child entities**

```csharp
// ProductRelationship.cs
using Obss.SharedKernel.Domain.Common;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Domain.Entities;

public class ProductRelationship : Entity<Guid>
{
    private ProductRelationship() { }

    public ProductRelationship(Guid id, Guid relatedProductId, ProductRelationshipType type)
        : base(id)
    {
        RelatedProductId = relatedProductId;
        Type = type;
    }

    public Guid RelatedProductId { get; private set; }
    public ProductRelationshipType Type { get; private set; }
}
```

```csharp
// ProductCharacteristic.cs
using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Entities;

public class ProductCharacteristic : Entity<Guid>
{
    private ProductCharacteristic() { }

    public ProductCharacteristic(Guid id, string name, string value, string? valueType = null)
        : base(id)
    {
        Name = name;
        Value = value;
        ValueType = valueType;
    }

    public string Name { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string? ValueType { get; private set; }
}
```

```csharp
// ProductPrice.cs
using Obss.SharedKernel.Domain.Common;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Domain.Entities;

public class ProductPrice : Entity<Guid>
{
    private ProductPrice() { }

    public ProductPrice(Guid id, PriceType priceType, string name, decimal amount, string currency,
        int? recurringPeriod = null, string? recurringPeriodUnit = null)
        : base(id)
    {
        PriceType = priceType;
        Name = name;
        Amount = amount;
        Currency = currency;
        RecurringPeriod = recurringPeriod;
        RecurringPeriodUnit = recurringPeriodUnit;
    }

    public PriceType PriceType { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public int? RecurringPeriod { get; private set; }
    public string? RecurringPeriodUnit { get; private set; }
}
```

```csharp
// ProductTerm.cs
using Obss.SharedKernel.Domain.Common;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Domain.Entities;

public class ProductTerm : Entity<Guid>
{
    private ProductTerm() { }

    public ProductTerm(Guid id, string name, int duration, DurationUnit durationUnit,
        DateTime startDate, DateTime? endDate = null, string? description = null)
        : base(id)
    {
        Name = name;
        Duration = duration;
        DurationUnit = durationUnit;
        StartDate = startDate;
        EndDate = endDate;
        Description = description;
    }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int Duration { get; private set; }
    public DurationUnit DurationUnit { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
}
```

```csharp
// RealizingService.cs
using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Entities;

public class RealizingService : Entity<Guid>
{
    private RealizingService() { }

    public RealizingService(Guid id, Guid serviceId, string serviceType, string status)
        : base(id)
    {
        ServiceId = serviceId;
        ServiceType = serviceType;
        Status = status;
    }

    public Guid ServiceId { get; private set; }
    public string ServiceType { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
}
```

```csharp
// RealizingResource.cs
using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Entities;

public class RealizingResource : Entity<Guid>
{
    private RealizingResource() { }

    public RealizingResource(Guid id, Guid resourceId, string resourceType, string status)
        : base(id)
    {
        ResourceId = resourceId;
        ResourceType = resourceType;
        Status = status;
    }

    public Guid ResourceId { get; private set; }
    public string ResourceType { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
}
```

- [ ] **Step 4: Create Product aggregate root**

```csharp
// Product.cs
using Obss.SharedKernel.Domain.Common;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Domain.Entities;

public class Product : AggregateRoot<Guid>
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
    public string? Href { get; }

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
        return new Product(
            Guid.NewGuid(), tenantId, customerId, name, description,
            productSpecificationId, productOfferingId);
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
    }

    public void Suspend()
    {
        if (Status != ProductStatus.Active) return;
        Status = ProductStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status is ProductStatus.Cancelled or ProductStatus.Terminated) return;
        Status = ProductStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Terminate()
    {
        if (Status != ProductStatus.Cancelled) return;
        Status = ProductStatus.Terminated;
        TerminationDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
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
```

- [ ] **Step 5: Commit**

```bash
git add src/Modules/Subscriptions/Obss.Subscriptions.Domain/Entities/Product*.cs src/Modules/Subscriptions/Obss.Subscriptions.Domain/Entities/Realizing*.cs src/Modules/Subscriptions/Obss.Subscriptions.Domain/ValueObjects/Product*.cs src/Modules/Subscriptions/Obss.Subscriptions.Domain/ValueObjects/PriceType.cs src/Modules/Subscriptions/Obss.Subscriptions.Domain/ValueObjects/DurationUnit.cs
git commit -m "feat: add Product aggregate root and child entities"
```

---

### Task 2: Domain — Value objects and domain events

**Files:**
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Domain/Events/ProductCreatedDomainEvent.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Domain/Events/ProductActivatedDomainEvent.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Domain/Events/ProductSuspendedDomainEvent.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Domain/Events/ProductCancelledDomainEvent.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Domain/Events/ProductTerminatedDomainEvent.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Domain/Events/ProductModifiedDomainEvent.cs`

- [ ] **Step 1: Create domain events**

```csharp
// ProductCreatedDomainEvent.cs
using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Events;

public sealed class ProductCreatedDomainEvent : DomainEvent
{
    public ProductCreatedDomainEvent(Guid productId, Guid customerId, Guid? productOfferingId)
    {
        ProductId = productId;
        CustomerId = customerId;
        ProductOfferingId = productOfferingId;
    }

    public Guid ProductId { get; }
    public Guid CustomerId { get; }
    public Guid? ProductOfferingId { get; }
}
```

```csharp
// ProductActivatedDomainEvent.cs
using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Events;

public sealed class ProductActivatedDomainEvent : DomainEvent
{
    public ProductActivatedDomainEvent(Guid productId)
    {
        ProductId = productId;
    }

    public Guid ProductId { get; }
}
```

```csharp
// ProductSuspendedDomainEvent.cs
using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Events;

public sealed class ProductSuspendedDomainEvent : DomainEvent
{
    public ProductSuspendedDomainEvent(Guid productId)
    {
        ProductId = productId;
    }

    public Guid ProductId { get; }
}
```

```csharp
// ProductCancelledDomainEvent.cs
using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Events;

public sealed class ProductCancelledDomainEvent : DomainEvent
{
    public ProductCancelledDomainEvent(Guid productId)
    {
        ProductId = productId;
    }

    public Guid ProductId { get; }
}
```

```csharp
// ProductTerminatedDomainEvent.cs
using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Events;

public sealed class ProductTerminatedDomainEvent : DomainEvent
{
    public ProductTerminatedDomainEvent(Guid productId)
    {
        ProductId = productId;
    }

    public Guid ProductId { get; }
}
```

```csharp
// ProductModifiedDomainEvent.cs
using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Events;

public sealed class ProductModifiedDomainEvent : DomainEvent
{
    public ProductModifiedDomainEvent(Guid productId)
    {
        ProductId = productId;
    }

    public Guid ProductId { get; }
}
```

- [ ] **Step 2: Commit**

```bash
git add src/Modules/Subscriptions/Obss.Subscriptions.Domain/Events/Product*.cs
git commit -m "feat: add Product domain events"
```

---

### Task 3: Application — Repository, DTOs, and mapping

**Files:**
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Application/Abstractions/IProductRepository.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Application/DTOs/ProductDto.cs`

- [ ] **Step 1: Create repository interface**

```csharp
// IProductRepository.cs
using Obss.Subscriptions.Domain.Entities;

namespace Obss.Subscriptions.Application.Abstractions;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Product>> GetListAsync(CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
    Task DeleteAsync(Product product, CancellationToken ct = default);
}
```

- [ ] **Step 2: Create DTOs**

```csharp
// ProductDto.cs
namespace Obss.Subscriptions.Application.DTOs;

public sealed record ProductDto(
    Guid Id,
    Guid CustomerId,
    string? Name,
    string? Description,
    string Status,
    Guid? ProductSpecificationId,
    Guid? ProductOfferingId,
    DateTime? ActivationDate,
    DateTime? TerminationDate,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? Href,
    List<ProductRelationshipDto> Relationships,
    List<ProductCharacteristicDto> Characteristics,
    List<ProductPriceDto> Prices,
    List<ProductTermDto> Terms,
    List<RealizingServiceDto> RealizingServices,
    List<RealizingResourceDto> RealizingResources);

public sealed record ProductRelationshipDto(Guid Id, Guid RelatedProductId, string Type);
public sealed record ProductCharacteristicDto(Guid Id, string Name, string Value, string? ValueType);
public sealed record ProductPriceDto(Guid Id, string PriceType, string Name, decimal Amount, string Currency, int? RecurringPeriod, string? RecurringPeriodUnit);
public sealed record ProductTermDto(Guid Id, string Name, int Duration, string DurationUnit, DateTime StartDate, DateTime? EndDate, string? Description);
public sealed record RealizingServiceDto(Guid Id, Guid ServiceId, string ServiceType, string Status);
public sealed record RealizingResourceDto(Guid Id, Guid ResourceId, string ResourceType, string Status);
```

- [ ] **Step 3: Update mapping config**

Add to `SubscriptionMappingConfig.cs`:

```csharp
TypeAdapterConfig<Product, ProductDto>.NewConfig()
    .Map(dest => dest.Status, src => src.Status.ToString())
    .Map(dest => dest.Relationships, src => src.Relationships.Adapt<List<ProductRelationshipDto>>())
    .Map(dest => dest.Characteristics, src => src.Characteristics.Adapt<List<ProductCharacteristicDto>>())
    .Map(dest => dest.Prices, src => src.Prices.Adapt<List<ProductPriceDto>>())
    .Map(dest => dest.Terms, src => src.Terms.Adapt<List<ProductTermDto>>())
    .Map(dest => dest.RealizingServices, src => src.RealizingServices.Adapt<List<RealizingServiceDto>>())
    .Map(dest => dest.RealizingResources, src => src.RealizingResources.Adapt<List<RealizingResourceDto>>());

TypeAdapterConfig<ProductRelationship, ProductRelationshipDto>.NewConfig()
    .Map(dest => dest.Type, src => src.Type.ToString());
TypeAdapterConfig<ProductPrice, ProductPriceDto>.NewConfig()
    .Map(dest => dest.PriceType, src => src.PriceType.ToString());
TypeAdapterConfig<ProductTerm, ProductTermDto>.NewConfig()
    .Map(dest => dest.DurationUnit, src => src.DurationUnit.ToString());
```

- [ ] **Step 4: Commit**

```bash
git add src/Modules/Subscriptions/Obss.Subscriptions.Application/Abstractions/IProductRepository.cs src/Modules/Subscriptions/Obss.Subscriptions.Application/DTOs/ProductDto.cs
git commit -m "feat: add Product repository and DTOs"
```

---

### Task 4: Application — Commands for Product

**Files:**
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Application/Commands/CreateProduct/CreateProductCommand.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Application/Commands/CreateProduct/CreateProductCommandHandler.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Application/Commands/CreateProduct/CreateProductCommandValidator.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Application/Commands/UpdateProduct/UpdateProductCommand.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Application/Commands/UpdateProduct/UpdateProductCommandHandler.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Application/Commands/ActivateProduct/ActivateProductCommand.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Application/Commands/ActivateProduct/ActivateProductCommandHandler.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Application/Commands/SuspendProduct/SuspendProductCommand.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Application/Commands/SuspendProduct/SuspendProductCommandHandler.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Application/Commands/CancelProduct/CancelProductCommand.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Application/Commands/CancelProduct/CancelProductCommandHandler.cs`

- [ ] **Step 1: CreateProduct command**

```csharp
// CreateProductCommand.cs
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Commands.CreateProduct;

public sealed record CreateProductCommand(
    Guid TenantId,
    Guid CustomerId,
    string? Name,
    string? Description,
    Guid? ProductSpecificationId,
    Guid? ProductOfferingId) : IRequest<Result<ProductDto>>;
```

```csharp
// CreateProductCommandHandler.cs
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Application.DTOs;
using Obss.Subscriptions.Domain.Entities;

namespace Obss.Subscriptions.Application.Commands.CreateProduct;

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(IProductRepository repository, IUnitOfWork unitOfWork, ILogger<CreateProductCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = Product.Create(request.TenantId, request.CustomerId, request.Name, request.Description,
            request.ProductSpecificationId, request.ProductOfferingId);

        await _repository.AddAsync(product, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Created Product {Id} for customer {CustomerId}", product.Id, product.CustomerId);

        return Result.Success(product.Adapt<ProductDto>());
    }
}
```

```csharp
// CreateProductCommandValidator.cs
using FluentValidation;

namespace Obss.Subscriptions.Application.Commands.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}
```

- [ ] **Step 2: UpdateProduct command**

```csharp
// UpdateProductCommand.cs
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string? Name,
    string? Description,
    Guid? ProductSpecificationId,
    Guid? ProductOfferingId) : IRequest<Result<ProductDto>>;
```

```csharp
// UpdateProductCommandHandler.cs
using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(IProductRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await _repository.GetByIdAsync(request.Id, ct);
        if (product is null)
            return Result.Failure<ProductDto>(Error.NotFound("Product", request.Id));

        product.UpdateDetails(request.Name, request.Description, request.ProductSpecificationId, request.ProductOfferingId);

        await _repository.UpdateAsync(product, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(product.Adapt<ProductDto>());
    }
}
```

- [ ] **Step 3: Activate/Suspend/Cancel commands**

```csharp
// ActivateProductCommand.cs
using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Subscriptions.Application.Commands.ActivateProduct;

public sealed record ActivateProductCommand(Guid Id) : IRequest<Result>;
```

```csharp
// ActivateProductCommandHandler.cs
using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;

namespace Obss.Subscriptions.Application.Commands.ActivateProduct;

public sealed class ActivateProductCommandHandler : IRequestHandler<ActivateProductCommand, Result>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateProductCommandHandler(IProductRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ActivateProductCommand request, CancellationToken ct)
    {
        var product = await _repository.GetByIdAsync(request.Id, ct);
        if (product is null)
            return Result.Failure(Error.NotFound("Product", request.Id));

        product.Activate();

        await _repository.UpdateAsync(product, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
```

SuspendProduct and SuspendProductCommandHandler follow the same pattern (call `product.Suspend()`).

CancelProduct and CancelProductCommandHandler follow the same pattern (call `product.Cancel()`).

- [ ] **Step 4: Commit**

```bash
git add src/Modules/Subscriptions/Obss.Subscriptions.Application/Commands/CreateProduct/ src/Modules/Subscriptions/Obss.Subscriptions.Application/Commands/UpdateProduct/ src/Modules/Subscriptions/Obss.Subscriptions.Application/Commands/ActivateProduct/ src/Modules/Subscriptions/Obss.Subscriptions.Application/Commands/SuspendProduct/ src/Modules/Subscriptions/Obss.Subscriptions.Application/Commands/CancelProduct/
git commit -m "feat: add Product CRUD commands"
```

---

### Task 5: Application — Queries for Product

**Files:**
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Application/Queries/GetProductById/GetProductByIdQuery.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Application/Queries/GetProductById/GetProductByIdQueryHandler.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Application/Queries/GetProducts/GetProductsQuery.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Application/Queries/GetProducts/GetProductsQueryHandler.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Application/Queries/GetProductsByCustomer/GetProductsByCustomerQuery.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Application/Queries/GetProductsByCustomer/GetProductsByCustomerQueryHandler.cs`

- [ ] **Step 1: GetProductById query**

```csharp
// GetProductByIdQuery.cs
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductDto>>;
```

```csharp
// GetProductByIdQueryHandler.cs
using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IProductRepository _repository;

    public GetProductByIdQueryHandler(IProductRepository repository) => _repository = repository;

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var product = await _repository.GetByIdAsync(request.Id, ct);
        if (product is null)
            return Result.Failure<ProductDto>(Error.NotFound("Product", request.Id));

        return Result.Success(product.Adapt<ProductDto>());
    }
}
```

- [ ] **Step 2: GetProducts query**

```csharp
// GetProductsQuery.cs
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Queries.GetProducts;

public sealed record GetProductsQuery : IRequest<Result<List<ProductDto>>>;
```

```csharp
// GetProductsQueryHandler.cs
using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Queries.GetProducts;

public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<List<ProductDto>>>
{
    private readonly IProductRepository _repository;

    public GetProductsQueryHandler(IProductRepository repository) => _repository = repository;

    public async Task<Result<List<ProductDto>>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        var products = await _repository.GetListAsync(ct);
        return Result.Success(products.Adapt<List<ProductDto>>());
    }
}
```

- [ ] **Step 3: GetProductsByCustomer query**

```csharp
// GetProductsByCustomerQuery.cs
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Queries.GetProductsByCustomer;

public sealed record GetProductsByCustomerQuery(Guid CustomerId) : IRequest<Result<List<ProductDto>>>;
```

```csharp
// GetProductsByCustomerQueryHandler.cs
using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Queries.GetProductsByCustomer;

public sealed class GetProductsByCustomerQueryHandler : IRequestHandler<GetProductsByCustomerQuery, Result<List<ProductDto>>>
{
    private readonly IProductRepository _repository;

    public GetProductsByCustomerQueryHandler(IProductRepository repository) => _repository = repository;

    public async Task<Result<List<ProductDto>>> Handle(GetProductsByCustomerQuery request, CancellationToken ct)
    {
        var all = await _repository.GetListAsync(ct);
        var filtered = all.Where(p => p.CustomerId == request.CustomerId);
        return Result.Success(filtered.Adapt<List<ProductDto>>());
    }
}
```

- [ ] **Step 4: Commit**

```bash
git add src/Modules/Subscriptions/Obss.Subscriptions.Application/Queries/GetProductById/ src/Modules/Subscriptions/Obss.Subscriptions.Application/Queries/GetProducts/ src/Modules/Subscriptions/Obss.Subscriptions.Application/Queries/GetProductsByCustomer/
git commit -m "feat: add Product queries"
```

---

### Task 6: Application — Integration events

**Files:**
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Application/IntegrationEvents/ProductCreatedIntegrationEvent.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Application/IntegrationEvents/ProductStateChangedIntegrationEvent.cs`

- [ ] **Step 1: Create outbound integration events**

```csharp
// ProductCreatedIntegrationEvent.cs
using Obss.SharedKernel.Domain.Events;

namespace Obss.Subscriptions.Application.IntegrationEvents;

public sealed class ProductCreatedIntegrationEvent : IntegrationEvent
{
    public ProductCreatedIntegrationEvent(Guid productId, Guid customerId, Guid? productOfferingId)
    {
        ProductId = productId;
        CustomerId = customerId;
        ProductOfferingId = productOfferingId;
    }

    public Guid ProductId { get; }
    public Guid CustomerId { get; }
    public Guid? ProductOfferingId { get; }
}
```

```csharp
// ProductStateChangedIntegrationEvent.cs
using Obss.SharedKernel.Domain.Events;

namespace Obss.Subscriptions.Application.IntegrationEvents;

public sealed class ProductStateChangedIntegrationEvent : IntegrationEvent
{
    public ProductStateChangedIntegrationEvent(Guid productId, string newState)
    {
        ProductId = productId;
        NewState = newState;
    }

    public Guid ProductId { get; }
    public string NewState { get; }
}
```

- [ ] **Step 2: Commit**

```bash
git add src/Modules/Subscriptions/Obss.Subscriptions.Application/IntegrationEvents/Product*.cs
git commit -m "feat: add Product integration events"
```

---

### Task 7: Infrastructure — EF config and repository

**Files:**
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Infrastructure/Persistence/Configurations/ProductConfiguration.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Infrastructure/Persistence/Configurations/ProductRelationshipConfiguration.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Infrastructure/Persistence/Configurations/ProductCharacteristicConfiguration.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Infrastructure/Persistence/Configurations/ProductPriceConfiguration.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Infrastructure/Persistence/Configurations/ProductTermConfiguration.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Infrastructure/Persistence/Configurations/RealizingServiceConfiguration.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Infrastructure/Persistence/Configurations/RealizingResourceConfiguration.cs`
- Create: `src/Modules/Subscriptions/Obss.Subscriptions.Infrastructure/Persistence/Repositories/ProductRepository.cs`
- Modify: `src/Modules/Subscriptions/Obss.Subscriptions.Infrastructure/Persistence/SubscriptionDbContext.cs`

- [ ] **Step 1: EF config for Product**

```csharp
// ProductConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Subscriptions.Domain.Entities;

namespace Obss.Subscriptions.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(p => p.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(200);
        builder.Property(p => p.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(p => p.ProductSpecificationId).HasColumnName("product_specification_id");
        builder.Property(p => p.ProductOfferingId).HasColumnName("product_offering_id");
        builder.Property(p => p.Status).HasColumnName("status").HasMaxLength(50).IsRequired().HasConversion<string>();
        builder.Property(p => p.ActivationDate).HasColumnName("activation_date");
        builder.Property(p => p.TerminationDate).HasColumnName("termination_date");
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(p => p.Href).HasColumnName("href").HasMaxLength(500);

        builder.OwnsOne(p => p.BillingAccount, ba =>
        {
            ba.Property(b => b.AccountId).HasColumnName("billing_account_id").HasMaxLength(100);
            ba.Property(b => b.Href).HasColumnName("billing_account_href").HasMaxLength(500);
        });

        builder.OwnsOne(p => p.Place, pl =>
        {
            pl.Property(x => x.Id).HasColumnName("place_id").HasMaxLength(100);
            pl.Property(x => x.Role).HasColumnName("place_role").HasMaxLength(50);
            pl.Property(x => x.Name).HasColumnName("place_name").HasMaxLength(200);
            pl.Property(x => x.Street).HasColumnName("place_street").HasMaxLength(200);
            pl.Property(x => x.City).HasColumnName("place_city").HasMaxLength(100);
            pl.Property(x => x.State).HasColumnName("place_state").HasMaxLength(50);
            pl.Property(x => x.Zip).HasColumnName("place_zip").HasMaxLength(20);
            pl.Property(x => x.Country).HasColumnName("place_country").HasMaxLength(50);
        });

        builder.OwnsOne(p => p.Agreement, ag =>
        {
            ag.Property(a => a.AgreementId).HasColumnName("agreement_id").HasMaxLength(100);
            ag.Property(a => a.Name).HasColumnName("agreement_name").HasMaxLength(200);
            ag.Property(a => a.Href).HasColumnName("agreement_href").HasMaxLength(500);
        });

        builder.HasMany(p => p.Relationships).WithOne().HasForeignKey("ProductId").OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Characteristics).WithOne().HasForeignKey("ProductId").OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Prices).WithOne().HasForeignKey("ProductId").OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Terms).WithOne().HasForeignKey("ProductId").OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.RealizingServices).WithOne().HasForeignKey("ProductId").OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.RealizingResources).WithOne().HasForeignKey("ProductId").OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.TenantId).HasDatabaseName("ix_products_tenant_id");
        builder.HasIndex(p => p.CustomerId).HasDatabaseName("ix_products_customer_id");
        builder.HasIndex(p => p.Status).HasDatabaseName("ix_products_status");

        builder.Ignore(p => p.BillingAccount);
        builder.Ignore(p => p.Place);
        builder.Ignore(p => p.Agreement);
        builder.Navigation(p => p.Relationships).AutoInclude();
        builder.Navigation(p => p.Characteristics).AutoInclude();
        builder.Navigation(p => p.Prices).AutoInclude();
        builder.Navigation(p => p.Terms).AutoInclude();
        builder.Navigation(p => p.RealizingServices).AutoInclude();
        builder.Navigation(p => p.RealizingResources).AutoInclude();
    }
}
```

- [ ] **Step 2: EF configs for child entities**

Create standard EF configurations for each child entity:
- `ProductRelationshipConfiguration`: table `product_relationships`, HasKey(i => i.Id), Property RelatedProductId, Property Type with HasConversion<string>
- `ProductCharacteristicConfiguration`: table `product_characteristics`
- `ProductPriceConfiguration`: table `product_prices`, PriceType with HasConversion<string>
- `ProductTermConfiguration`: table `product_terms`, DurationUnit with HasConversion<string>
- `RealizingServiceConfiguration`: table `realizing_services`
- `RealizingResourceConfiguration`: table `realizing_resources`

- [ ] **Step 3: Repository implementation**

```csharp
// ProductRepository.cs
using Microsoft.EntityFrameworkCore;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Domain.Entities;
using Obss.Subscriptions.Infrastructure.Persistence;

namespace Obss.Subscriptions.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly SubscriptionDbContext _context;

    public ProductRepository(SubscriptionDbContext context) => _context = context;

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<Product>().FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<List<Product>> GetListAsync(CancellationToken ct = default)
        => await _context.Set<Product>().ToListAsync(ct);

    public async Task AddAsync(Product product, CancellationToken ct = default)
        => await _context.Set<Product>().AddAsync(product, ct);

    public Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        _context.Set<Product>().Update(product);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Product product, CancellationToken ct = default)
    {
        _context.Set<Product>().Remove(product);
        return Task.CompletedTask;
    }
}
```

- [ ] **Step 4: Register DbSet in SubscriptionDbContext**

Add to `SubscriptionDbContext.cs`:
```csharp
public DbSet<Product> Products => Set<Product>();
```

- [ ] **Step 5: Commit**

```bash
git add src/Modules/Subscriptions/Obss.Subscriptions.Infrastructure/Persistence/
git commit -m "feat: add Product EF config and repository"
```

---

### Task 8: API — Product endpoints

**Files:**
- Modify: `src/Modules/Subscriptions/Obss.Subscriptions.Api/Endpoints/SubscriptionEndpoints.cs`
- Modify: `src/Modules/Subscriptions/Obss.Subscriptions.Api/Extensions/SubscriptionModuleRegistration.cs`

- [ ] **Step 1: Add Product endpoints**

Add to `SubscriptionEndpoints.cs` inside the `Map()` method:

```csharp
// Product endpoints
group.MapPost("/products", async (CreateProductCommand command, IMediator mediator) =>
{
    var result = await mediator.Send(command);
    return result.IsSuccess
        ? (IResult)TypedResults.Created($"/api/v1/subscriptions/products/{result.Value.Id}", result.Value)
        : (IResult)TypedResults.BadRequest(result.Error);
});

group.MapGet("/products/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var result = await mediator.Send(new GetProductByIdQuery(id));
    return result.IsSuccess
        ? (IResult)TypedResults.Ok(result.Value)
        : (IResult)TypedResults.NotFound(result.Error);
});

group.MapGet("/products", async (IMediator mediator) =>
{
    var result = await mediator.Send(new GetProductsQuery());
    return result.IsSuccess
        ? (IResult)TypedResults.Ok(result.Value)
        : (IResult)TypedResults.BadRequest(result.Error);
});

group.MapGet("/customers/{customerId:guid}/products", async (Guid customerId, IMediator mediator) =>
{
    var result = await mediator.Send(new GetProductsByCustomerQuery(customerId));
    return result.IsSuccess
        ? (IResult)TypedResults.Ok(result.Value)
        : (IResult)TypedResults.BadRequest(result.Error);
});

group.MapPatch("/products/{id:guid}", async (Guid id, UpdateProductCommand command, IMediator mediator) =>
{
    if (id != command.Id)
        return (IResult)TypedResults.BadRequest();
    var result = await mediator.Send(command);
    return result.IsSuccess
        ? (IResult)TypedResults.Ok(result.Value)
        : (IResult)TypedResults.BadRequest(result.Error);
});

group.MapPost("/products/{id:guid}/activate", async (Guid id, IMediator mediator) =>
{
    var result = await mediator.Send(new ActivateProductCommand(id));
    return result.IsSuccess
        ? (IResult)TypedResults.NoContent()
        : (IResult)TypedResults.BadRequest(result.Error);
});

group.MapPost("/products/{id:guid}/suspend", async (Guid id, IMediator mediator) =>
{
    var result = await mediator.Send(new SuspendProductCommand(id));
    return result.IsSuccess
        ? (IResult)TypedResults.NoContent()
        : (IResult)TypedResults.BadRequest(result.Error);
});

group.MapPost("/products/{id:guid}/cancel", async (Guid id, IMediator mediator) =>
{
    var result = await mediator.Send(new CancelProductCommand(id));
    return result.IsSuccess
        ? (IResult)TypedResults.NoContent()
        : (IResult)TypedResults.BadRequest(result.Error);
});
```

Add using statements:
```csharp
using Obss.Subscriptions.Application.Commands.CreateProduct;
using Obss.Subscriptions.Application.Commands.UpdateProduct;
using Obss.Subscriptions.Application.Commands.ActivateProduct;
using Obss.Subscriptions.Application.Commands.SuspendProduct;
using Obss.Subscriptions.Application.Commands.CancelProduct;
using Obss.Subscriptions.Application.Queries.GetProductById;
using Obss.Subscriptions.Application.Queries.GetProducts;
using Obss.Subscriptions.Application.Queries.GetProductsByCustomer;
```

- [ ] **Step 2: Register repository in DI**

Add to `SubscriptionModuleRegistration.cs`:
```csharp
services.AddScoped<IProductRepository, ProductRepository>();
```

- [ ] **Step 3: Commit**

```bash
git add src/Modules/Subscriptions/Obss.Subscriptions.Api/
git commit -m "feat: add Product API endpoints"
```

---

### Task 9: Frontend — Products page

**Files:**
- Create: `frontend/src/app/subscriptions/products/page.tsx`
- Create: `frontend/src/app/subscriptions/products/[id]/page.tsx`
- Create: `frontend/src/app/api/products.ts`

- [ ] **Step 1: Create API client hooks**

```typescript
// frontend/src/api/products.ts
import { useQuery } from '@tanstack/react-query';
import apiClient from '@/lib/api-client';

export interface ProductDto {
  id: string;
  customerId: string;
  name: string | null;
  description: string | null;
  status: string;
  productSpecificationId: string | null;
  productOfferingId: string | null;
  activationDate: string | null;
  terminationDate: string | null;
  createdAt: string;
  updatedAt: string;
  href: string | null;
  relationships: ProductRelationshipDto[];
  characteristics: ProductCharacteristicDto[];
  prices: ProductPriceDto[];
  terms: ProductTermDto[];
  realizingServices: RealizingServiceDto[];
  realizingResources: RealizingResourceDto[];
}

export interface ProductRelationshipDto { id: string; relatedProductId: string; type: string; }
export interface ProductCharacteristicDto { id: string; name: string; value: string; valueType: string | null; }
export interface ProductPriceDto { id: string; priceType: string; name: string; amount: number; currency: string; recurringPeriod: number | null; recurringPeriodUnit: string | null; }
export interface ProductTermDto { id: string; name: string; duration: number; durationUnit: string; startDate: string; endDate: string | null; description: string | null; }
export interface RealizingServiceDto { id: string; serviceId: string; serviceType: string; status: string; }
export interface RealizingResourceDto { id: string; resourceId: string; resourceType: string; status: string; }

export function useProducts() {
  return useQuery<ProductDto[]>({
    queryKey: ['products'],
    queryFn: () => apiClient.get('/subscriptions/products').then(r => r.json()),
  });
}

export function useProduct(id: string) {
  return useQuery<ProductDto>({
    queryKey: ['product', id],
    queryFn: () => apiClient.get(`/subscriptions/products/${id}`).then(r => r.json()),
    enabled: !!id,
  });
}

export function useCustomerProducts(customerId: string) {
  return useQuery<ProductDto[]>({
    queryKey: ['customer-products', customerId],
    queryFn: () => apiClient.get(`/subscriptions/customers/${customerId}/products`).then(r => r.json()),
    enabled: !!customerId,
  });
}
```

- [ ] **Step 2: Create product list page**

```tsx
// frontend/src/app/subscriptions/products/page.tsx
'use client';

import { useProducts } from '@/api/products';
import { Card } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import Link from 'next/link';

const statusColors: Record<string, string> = {
  Created: 'bg-gray-100 text-gray-800',
  Active: 'bg-green-100 text-green-800',
  Suspended: 'bg-yellow-100 text-yellow-800',
  Cancelled: 'bg-red-100 text-red-800',
  Terminated: 'bg-gray-100 text-gray-800',
};

export default function ProductsPage() {
  const { data: products, isLoading } = useProducts();

  if (isLoading) return <div>Loading...</div>;

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold">Product Inventory</h1>
      <div className="grid gap-4">
        {products?.map(product => (
          <Link key={product.id} href={`/subscriptions/products/${product.id}`}>
            <Card className="p-4 hover:shadow-md transition-shadow">
              <div className="flex justify-between items-start">
                <div>
                  <h3 className="font-semibold">{product.name ?? 'Unnamed Product'}</h3>
                  <p className="text-sm text-gray-500">Customer: {product.customerId}</p>
                </div>
                <Badge className={statusColors[product.status]}>{product.status}</Badge>
              </div>
              {product.description && (
                <p className="text-sm mt-2 text-gray-600">{product.description}</p>
              )}
              <div className="flex gap-2 mt-2 text-xs text-gray-400">
                <span>{product.prices.length} price(s)</span>
                <span>{product.terms.length} term(s)</span>
                <span>{product.realizingServices.length} service(s)</span>
              </div>
            </Card>
          </Link>
        ))}
        {products?.length === 0 && <p className="text-gray-500">No products found.</p>}
      </div>
    </div>
  );
}
```

- [ ] **Step 3: Create product detail page**

```tsx
// frontend/src/app/subscriptions/products/[id]/page.tsx
'use client';

import { useProduct } from '@/api/products';
import { Card } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { use } from 'react';

export default function ProductDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const { data: product, isLoading } = useProduct(id);

  if (isLoading) return <div>Loading...</div>;
  if (!product) return <div>Product not found</div>;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">{product.name ?? 'Unnamed Product'}</h1>
        <Badge>{product.status}</Badge>
      </div>

      {product.description && <p className="text-gray-600">{product.description}</p>}

      {product.prices.length > 0 && (
        <section>
          <h2 className="text-lg font-semibold">Prices</h2>
          {product.prices.map(price => (
            <Card key={price.id} className="p-3 mt-2">
              <span className="font-medium">{price.name}</span>: {price.amount} {price.currency}
              <span className="text-gray-500 ml-2">({price.priceType})</span>
            </Card>
          ))}
        </section>
      )}

      {product.terms.length > 0 && (
        <section>
          <h2 className="text-lg font-semibold">Terms</h2>
          {product.terms.map(term => (
            <Card key={term.id} className="p-3 mt-2">
              <span className="font-medium">{term.name}</span> — {term.duration} {term.durationUnit}
            </Card>
          ))}
        </section>
      )}

      {product.realizingServices.length > 0 && (
        <section>
          <h2 className="text-lg font-semibold">Realizing Services</h2>
          {product.realizingServices.map(s => (
            <Card key={s.id} className="p-3 mt-2">
              Service {s.serviceId} — {s.serviceType} — {s.status}
            </Card>
          ))}
        </section>
      )}

      {product.realizingResources.length > 0 && (
        <section>
          <h2 className="text-lg font-semibold">Realizing Resources</h2>
          {product.realizingResources.map(r => (
            <Card key={r.id} className="p-3 mt-2">
              Resource {r.resourceId} — {r.resourceType} — {r.status}
            </Card>
          ))}
        </section>
      )}

      {product.characteristics.length > 0 && (
        <section>
          <h2 className="text-lg font-semibold">Characteristics</h2>
          {product.characteristics.map(c => (
            <Card key={c.id} className="p-3 mt-2">
              {c.name}: {c.value}
            </Card>
          ))}
        </section>
      )}
    </div>
  );
}
```

- [ ] **Step 4: Commit**

```bash
git add frontend/src/api/products.ts frontend/src/app/subscriptions/products/
git commit -m "feat: add Product frontend pages"
```

---

### Task 10: Data migration script

**Files:**
- Create: `scripts/migrations/subscriptions-to-products.sql`

- [ ] **Step 1: Create migration SQL**

```sql
-- scripts/migrations/subscriptions-to-products.sql
-- Creates Product records from all existing Subscription records
-- Each Subscription becomes one Product with Created status

INSERT INTO products (id, tenant_id, customer_id, name, description, status, created_at, updated_at)
SELECT
    gen_random_uuid(),
    s.tenant_id,
    s.customer_id,
    s.offer_name,
    'Migrated from subscription ' || s.id,
    CASE
        WHEN s.status = 'Active' THEN 'Active'
        WHEN s.status = 'Suspended' THEN 'Suspended'
        WHEN s.status = 'Cancelled' THEN 'Cancelled'
        WHEN s.status = 'Expired' THEN 'Terminated'
        ELSE 'Created'
    END,
    s.created_at,
    NOW()
FROM subscriptions s
WHERE NOT EXISTS (
    SELECT 1 FROM products p WHERE p.customer_id = s.customer_id AND p.name = s.offer_name
);
```

- [ ] **Step 2: Commit**

```bash
git add scripts/migrations/subscriptions-to-products.sql
git commit -m "feat: add data migration script for subscriptions to products"
```

---

### Task 11: EF Migration and build

**Files:**
- Generate: `src/Modules/Subscriptions/Obss.Subscriptions.Infrastructure/Persistence/Migrations/{timestamp}_AddProductInventoryTables.cs`

- [ ] **Step 1: Generate EF migration**

Run: `dotnet ef migrations add AddProductInventoryTables -p src/Modules/Subscriptions/Obss.Subscriptions.Infrastructure -s src/Host/Obss.Host --context SubscriptionDbContext`

Expected: New migration creates tables `products`, `product_relationships`, `product_characteristics`, `product_prices`, `product_terms`, `realizing_services`, `realizing_resources`.

- [ ] **Step 2: Build and verify**

Run: `dotnet build Obss.sln --configuration Release`

Expected: 0 errors, 0 warnings.

- [ ] **Step 3: Commit migration**

```bash
git add src/Modules/Subscriptions/Obss.Subscriptions.Infrastructure/Persistence/Migrations/
git commit -m "feat: add Product inventory database migration"
```

---

### Task 12: Update CreateSubscription flow to create Product first

**Files:**
- Modify: `src/Modules/Subscriptions/Obss.Subscriptions.Application/Commands/CreateSubscription/CreateSubscriptionCommandHandler.cs`
- Modify: `src/Modules/Subscriptions/Obss.Subscriptions.Application/IntegrationEventHandlers/OrderApprovedIntegrationEventHandler.cs`

- [ ] **Step 1: Update CreateSubscription handler to create Product first

The `CreateSubscriptionCommandHandler` should now:
1. Create a `Product` via `Product.Create()`
2. Activate the Product
3. Create the `Subscription` with the Product's `Id`
4. Save all in one transaction

Rewrite the handler:

```csharp
using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Application.DTOs;
using Obss.Subscriptions.Domain.Entities;

namespace Obss.Subscriptions.Application.Commands.CreateSubscription;

public sealed class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, Result<SubscriptionDto>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSubscriptionCommandHandler(
        ISubscriptionRepository subscriptionRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SubscriptionDto>> Handle(CreateSubscriptionCommand request, CancellationToken ct)
    {
        var product = Product.Create(
            Guid.Parse(request.TenantId),
            request.CustomerId,
            request.OfferName,
            null,
            null,
            request.OfferId);

        var subscription = Subscription.Create(
            request.TenantId,
            request.CustomerId,
            request.CustomerName,
            request.OrderId,
            request.OrderItemId,
            product.Id,
            request.OfferId,
            request.OfferName,
            request.BillingPeriod,
            request.Currency,
            request.Price,
            request.Quantity,
            request.StartDate,
            request.EndDate);

        await _productRepository.AddAsync(product, ct);
        await _subscriptionRepository.AddAsync(subscription, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(subscription.Adapt<SubscriptionDto>());
    }
}
```

- [ ] **Step 2: Update integration event handler**

Update `OrderApprovedIntegrationEventHandler.cs` to also inject `IProductRepository` and create Product before Subscription, following the same pattern as Step 1.

- [ ] **Step 3: Commit**

```bash
git add src/Modules/Subscriptions/Obss.Subscriptions.Application/Commands/CreateSubscription/ src/Modules/Subscriptions/Obss.Subscriptions.Application/IntegrationEventHandlers/
git commit -m "feat: update CreateSubscription to create Product first"
```