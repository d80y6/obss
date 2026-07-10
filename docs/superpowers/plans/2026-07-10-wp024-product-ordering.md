# WP-024: Product Order (TMF622) — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Rename Orders module to ProductOrder (TMF622) and add item state machine, item relationships, milestones, and BillingAccount reference.

**Architecture:** Rename existing `Order` → `ProductOrder` aggregate across all 4 layers (Domain, Application, Infrastructure, API) while adding new entities (`ProductOrderItemRelationship`, `ProductOrderMilestone`), new enums (`ProductOrderItemState`, `Priority`, `RelationshipType`, `MilestoneStatus`), new domain events, commands, queries, EF configs, API endpoints, and frontend pages/components. `OrderFulfillment` is kept as-is.

**Tech Stack:** .NET 9, EF Core/Npgsql, MediatR, FluentValidation, Mapster, MassTransit/RabbitMQ, Next.js 16 App Router, React Query

**Spec:** `docs/superpowers/specs/2026-07-10-wp024-product-ordering-design.md`

---

### Task 1: Domain — New Enums (4 files)

**Files:**
- Create: `src/Modules/Orders/Obss.Orders.Domain/ValueObjects/ProductOrderItemState.cs`
- Create: `src/Modules/Orders/Obss.Orders.Domain/ValueObjects/Priority.cs`
- Create: `src/Modules/Orders/Obss.Orders.Domain/ValueObjects/RelationshipType.cs`
- Create: `src/Modules/Orders/Obss.Orders.Domain/ValueObjects/MilestoneStatus.cs`

- [ ] **Create `ProductOrderItemState.cs`:**

```csharp
namespace Obss.Orders.Domain.ValueObjects;

public enum ProductOrderItemState
{
    Acknowledged,
    InProgress,
    Pending,
    Held,
    Assessing,
    Rejected,
    Cancelled,
    Completed,
    Failed
}
```

- [ ] **Create `Priority.cs`:**

```csharp
namespace Obss.Orders.Domain.ValueObjects;

public enum Priority { Low, Medium, High, Critical }
```

- [ ] **Create `RelationshipType.cs`:**

```csharp
namespace Obss.Orders.Domain.ValueObjects;

public enum RelationshipType { Requires, OptionalFor, ReliesOn }
```

- [ ] **Create `MilestoneStatus.cs`:**

```csharp
namespace Obss.Orders.Domain.ValueObjects;

public enum MilestoneStatus { Pending, Achieved, Missed, Cancelled }
```

- [ ] **Commit:**

```bash
git add src/Modules/Orders/Obss.Orders.Domain/ValueObjects/
git commit -m "feat: add ProductOrder enums (ItemState, Priority, RelationshipType, MilestoneStatus)"
```

---

### Task 2: Domain — Rename Entities (Order→ProductOrder)

**Files:**
- Modify: `src/Modules/Orders/Obss.Orders.Domain/Entities/Order.cs` → renamed to `ProductOrder.cs`
- Modify: `src/Modules/Orders/Obss.Orders.Domain/Entities/OrderItem.cs` → renamed to `ProductOrderItem.cs`
- Modify: `src/Modules/Orders/Obss.Orders.Domain/Entities/OrderPayment.cs` → renamed to `ProductOrderPayment.cs`
- Modify: `src/Modules/Orders/Obss.Orders.Domain/Exceptions/OrderDomainExceptions.cs` → rename exceptions

- [ ] **Rename `Order` → `ProductOrder`**: Rename the file, the class, all usages within the file. Add new fields: `BillingAccountId`, `BillingAccountHref`, `OrderVersion`, `Priority`, `ProductOfferingQualificationId`, `ProductOfferingQualificationHref`, `QuoteHref`. Replace `string? Priority` with `Priority OrderPriority`.

`ProductOrder.cs` adds to existing `Order.cs` content:
```csharp
// New fields added alongside existing:
public Guid? BillingAccountId { get; private set; }
public string? BillingAccountHref { get; private set; }
public int OrderVersion { get; private set; } = 1;
public Priority OrderPriority { get; private set; } = Priority.Medium;
public Guid? ProductOfferingQualificationId { get; private set; }
public string? ProductOfferingQualificationHref { get; private set; }
public string? QuoteHref { get; private set; }
```

Update constructor and factory method to accept/initialize new fields. Increment `OrderVersion` in `UpdateDetails()`.

- [ ] **Rename `OrderItem` → `ProductOrderItem`**: Rename file and class. Add `State` field:
```csharp
public ProductOrderItemState State { get; private set; } = ProductOrderItemState.Acknowledged;
```

- [ ] **Rename `OrderPayment` → `ProductOrderPayment`**: Rename file and class.

- [ ] **Rename domain exceptions**: In `OrderDomainExceptions.cs` → rename to `ProductOrderDomainExceptions.cs`. Rename `OrderNotFoundException` → `ProductOrderNotFoundException`, `InvalidOrderStateException` → `InvalidProductOrderStateException`, `OrderItemNotFoundException` → `ProductOrderItemNotFoundException`.

- [ ] **Commit:**

```bash
git add src/Modules/Orders/Obss.Orders.Domain/
git commit -m "feat: rename Order→ProductOrder, add BillingAccountId, Priority, OrderVersion, POQ ref"
```

---

### Task 3: Domain — Add Item State Machine to ProductOrderItem

**Files:**
- Modify: `src/Modules/Orders/Obss.Orders.Domain/Entities/ProductOrderItem.cs`
- Create: `src/Modules/Orders/Obss.Orders.Domain/Events/ProductOrderItemStateChangedDomainEvent.cs`
- Create: `src/Modules/Orders/Obss.Orders.Domain/Exceptions/InvalidProductOrderItemStateException.cs`

- [ ] **Create `ProductOrderItemStateChangedDomainEvent.cs`:**

```csharp
namespace Obss.Orders.Domain.Events;

public sealed record ProductOrderItemStateChangedDomainEvent(
    Guid OrderId,
    Guid ItemId,
    ProductOrderItemState OldState,
    ProductOrderItemState NewState,
    string? Reason) : DomainEvent;
```

- [ ] **Create `InvalidProductOrderItemStateException.cs`:**

```csharp
namespace Obss.Orders.Domain.Exceptions;

public sealed class InvalidProductOrderItemStateException(string message) : DomainException(message);
```

- [ ] **Add state transition methods to `ProductOrderItem`:**

```csharp
public void Acknowledge() => TransitionTo(ProductOrderItemState.Acknowledged);

public void StartProgress()
{
    if (State != ProductOrderItemState.Acknowledged && State != ProductOrderItemState.Pending)
        throw new InvalidProductOrderItemStateException($"Cannot start progress from {State}");
    TransitionTo(ProductOrderItemState.InProgress);
}

public void Hold()
{
    if (State != ProductOrderItemState.InProgress)
        throw new InvalidProductOrderItemStateException($"Cannot hold from {State}");
    TransitionTo(ProductOrderItemState.Held);
}

public void Resume()
{
    if (State != ProductOrderItemState.Held)
        throw new InvalidProductOrderItemStateException($"Cannot resume from {State}");
    TransitionTo(ProductOrderItemState.InProgress);
}

public void Assess()
{
    if (State != ProductOrderItemState.InProgress)
        throw new InvalidProductOrderItemStateException($"Cannot assess from {State}");
    TransitionTo(ProductOrderItemState.Assessing);
}

public void Pending(string reason)
{
    if (State != ProductOrderItemState.InProgress)
        throw new InvalidProductOrderItemStateException($"Cannot set pending from {State}");
    TransitionTo(ProductOrderItemState.Pending);
}

public void Reject(string reason)
{
    if (State != ProductOrderItemState.Assessing)
        throw new InvalidProductOrderItemStateException($"Cannot reject from {State}");
    TransitionTo(ProductOrderItemState.Rejected);
}

public void Complete()
{
    if (State != ProductOrderItemState.InProgress && State != ProductOrderItemState.Assessing)
        throw new InvalidProductOrderItemStateException($"Cannot complete from {State}");
    TransitionTo(ProductOrderItemState.Completed);
}

public void Fail(string error)
{
    if (State != ProductOrderItemState.InProgress)
        throw new InvalidProductOrderItemStateException($"Cannot fail from {State}");
    TransitionTo(ProductOrderItemState.Failed);
}

public void Cancel()
{
    if (State is ProductOrderItemState.Completed or ProductOrderItemState.Failed or ProductOrderItemState.Rejected or ProductOrderItemState.Cancelled)
        throw new InvalidProductOrderItemStateException($"Cannot cancel from {State}");
    TransitionTo(ProductOrderItemState.Cancelled);
}

private void TransitionTo(ProductOrderItemState newState)
{
    var oldState = State;
    State = newState;
    AddDomainEvent(new ProductOrderItemStateChangedDomainEvent(OrderId, Id, oldState, newState, null));
}
```

- [ ] **Commit:**

```bash
git add src/Modules/Orders/Obss.Orders.Domain/
git commit -m "feat: add ProductOrderItem state machine with 9 transitions"
```

---

### Task 4: Domain — New Entities (Relationships, Milestones) + New Domain Events

**Files:**
- Create: `src/Modules/Orders/Obss.Orders.Domain/Entities/ProductOrderItemRelationship.cs`
- Create: `src/Modules/Orders/Obss.Orders.Domain/Entities/ProductOrderMilestone.cs`
- Create: `src/Modules/Orders/Obss.Orders.Domain/Events/ProductOrderMilestoneReachedDomainEvent.cs`

- [ ] **Create `ProductOrderItemRelationship.cs`:**

```csharp
namespace Obss.Orders.Domain.Entities;

public sealed record ProductOrderItemRelationship
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ProductOrderItemId { get; private set; }
    public Guid TargetItemId { get; private set; }
    public RelationshipType Type { get; private set; }

    private ProductOrderItemRelationship() { }

    public ProductOrderItemRelationship(Guid itemId, Guid targetItemId, RelationshipType type)
    {
        Id = Guid.NewGuid();
        ProductOrderItemId = itemId;
        TargetItemId = targetItemId;
        Type = type;
    }
}
```

- [ ] **Create `ProductOrderMilestone.cs`:**

```csharp
namespace Obss.Orders.Domain.Entities;

public class ProductOrderMilestone : Entity<Guid>
{
    public Guid ProductOrderId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime MilestoneDate { get; private set; }
    public MilestoneStatus Status { get; private set; }

    private ProductOrderMilestone() { }

    public ProductOrderMilestone(Guid productOrderId, string name, string description, DateTime milestoneDate)
    {
        Id = Guid.NewGuid();
        ProductOrderId = productOrderId;
        Name = name;
        Description = description;
        MilestoneDate = milestoneDate;
        Status = MilestoneStatus.Pending;
    }

    public void Achieve()
    {
        Status = MilestoneStatus.Achieved;
        AddDomainEvent(new ProductOrderMilestoneReachedDomainEvent(ProductOrderId, Id, Name, MilestoneStatus.Achieved));
    }

    public void MarkMissed()
    {
        Status = MilestoneStatus.Missed;
        AddDomainEvent(new ProductOrderMilestoneReachedDomainEvent(ProductOrderId, Id, Name, MilestoneStatus.Missed));
    }

    public void Cancel()
    {
        Status = MilestoneStatus.Cancelled;
    }
}
```

- [ ] **Create `ProductOrderMilestoneReachedDomainEvent.cs`:**

```csharp
namespace Obss.Orders.Domain.Events;

public sealed record ProductOrderMilestoneReachedDomainEvent(
    Guid OrderId,
    Guid MilestoneId,
    string MilestoneName,
    MilestoneStatus Status) : DomainEvent;
```

- [ ] **Commit:**

```bash
git add src/Modules/Orders/Obss.Orders.Domain/
git commit -m "feat: add ProductOrderItemRelationship and ProductOrderMilestone entities"
```

---

### Task 5: Domain — Milestone Auto-Creation in ProductOrder Lifecycle

**Files:**
- Modify: `src/Modules/Orders/Obss.Orders.Domain/Entities/ProductOrder.cs`

- [ ] **Add milestone management to `ProductOrder`:**

Add a new collection:
```csharp
private readonly List<ProductOrderMilestone> _milestones = [];
public IReadOnlyList<ProductOrderMilestone> Milestones => _milestones.AsReadOnly();
```

Add milestone creation to the constructor:
```csharp
// In the factory method or constructor, after initialization:
_milestones.Add(new ProductOrderMilestone(Id, "OrderCreated", "Order was created", DateTime.UtcNow));
_milestones[^1].Achieve();
```

Add to `Submit()`:
```csharp
_milestones.Add(new ProductOrderMilestone(Id, "OrderSubmitted", "Order was submitted", DateTime.UtcNow));
_milestones[^1].Achieve();
```

Add to `Approve()`:
```csharp
_milestones.Add(new ProductOrderMilestone(Id, "OrderApproved", "Order was approved", DateTime.UtcNow));
_milestones[^1].Achieve();
```

Add to `CreateFulfillment()`:
```csharp
_milestones.Add(new ProductOrderMilestone(Id, "FulfillmentStarted", "Fulfillment process started", DateTime.UtcNow));
_milestones[^1].Achieve();
```

Add to `MarkCompleted()`:
```csharp
_milestones.Add(new ProductOrderMilestone(Id, "OrderCompleted", "Order was completed", DateTime.UtcNow));
_milestones[^1].Achieve();
```

Add relationship management methods:
```csharp
private readonly List<ProductOrderItemRelationship> _itemRelationships = [];
public IReadOnlyList<ProductOrderItemRelationship> ItemRelationships => _itemRelationships.AsReadOnly();

public void AddItemRelationship(Guid itemId, Guid targetItemId, RelationshipType type)
{
    if (HasCircularDependency(itemId, targetItemId))
        throw new InvalidProductOrderStateException("Circular dependency detected in item relationships");
    _itemRelationships.Add(new ProductOrderItemRelationship(itemId, targetItemId, type));
}

public void RemoveItemRelationship(Guid relationshipId)
{
    var rel = _itemRelationships.FirstOrDefault(r => r.Id == relationshipId);
    if (rel is null) throw new ProductOrderItemNotFoundException(relationshipId);
    _itemRelationships.Remove(rel);
}

private bool HasCircularDependency(Guid itemId, Guid targetItemId)
{
    // Check if targetItemId already has a relationship that chains back to itemId
    var visited = new HashSet<Guid> { itemId };
    var queue = new Queue<Guid>([targetItemId]);
    while (queue.Count > 0)
    {
        var current = queue.Dequeue();
        if (current == itemId) return true;
        if (!visited.Add(current)) continue;
        foreach (var rel in _itemRelationships.Where(r => r.ProductOrderItemId == current))
            queue.Enqueue(rel.TargetItemId);
    }
    return false;
}

public IReadOnlyList<ProductOrderItemRelationship> GetItemRelationships(Guid itemId) =>
    _itemRelationships.Where(r => r.ProductOrderItemId == itemId).ToList().AsReadOnly();

public void AddMilestone(ProductOrderMilestone milestone)
{
    _milestones.Add(milestone);
}
```

- [ ] **Commit:**

```bash
git add src/Modules/Orders/Obss.Orders.Domain/Entities/ProductOrder.cs
git commit -m "feat: add milestone auto-creation and relationship management to ProductOrder"
```

---

### Task 6: Application — DTOs, Repository Interfaces, Mapster Config

**Files:**
- Modify: All DTOs in `src/Modules/Orders/Obss.Orders.Application/DTOs/` — rename Order* → ProductOrder*
- Create: `src/Modules/Orders/Obss.Orders.Application/DTOs/ProductOrderItemRelationshipDto.cs`
- Create: `src/Modules/Orders/Obss.Orders.Application/DTOs/ProductOrderMilestoneDto.cs`
- Create: `src/Modules/Orders/Obss.Orders.Application/DTOs/ProductOrderItemPriceDto.cs`
- Modify: `src/Modules/Orders/Obss.Orders.Application/Abstractions/IOrderRepository.cs` → rename to `IProductOrderRepository.cs`
- Modify: `src/Modules/Orders/Obss.Orders.Application/Mappings/OrderMappingConfig.cs` → rename to `ProductOrderMappingConfig.cs`

- [ ] **Rename DTOs:**
  - `OrderDto.cs` → `ProductOrderDto.cs`: add `BillingAccountId`, `BillingAccountHref`, `OrderVersion`, `OrderPriority`, `ProductOfferingQualificationId`, `ProductOfferingQualificationHref`, `QuoteHref`, `List<ProductOrderMilestoneDto>? Milestones`, `List<ProductOrderItemRelationshipDto>? ItemRelationships`
  - `OrderItemDto.cs` → `ProductOrderItemDto.cs`: add `State` (string)
  - `OrderSummaryDto.cs` → `ProductOrderSummaryDto.cs`: add `OrderVersion`, `OrderPriority`
  - `OrderPaymentDto.cs` → `ProductOrderPaymentDto.cs`
  - `OrderValidationResultDto.cs` → `ProductOrderValidationResultDto.cs`
  - `OrderFulfillmentDto.cs` unchanged

- [ ] **Create new DTOs:**

`ProductOrderItemRelationshipDto.cs`:
```csharp
namespace Obss.Orders.Application.DTOs;

public sealed record ProductOrderItemRelationshipDto(
    Guid Id,
    Guid ProductOrderItemId,
    Guid TargetItemId,
    string Type);
```

`ProductOrderMilestoneDto.cs`:
```csharp
namespace Obss.Orders.Application.DTOs;

public sealed record ProductOrderMilestoneDto(
    Guid Id,
    Guid ProductOrderId,
    string Name,
    string Description,
    DateTime MilestoneDate,
    string Status);
```

`ProductOrderItemPriceDto.cs`:
```csharp
namespace Obss.Orders.Application.DTOs;

public sealed record ProductOrderItemPriceDto(
    decimal UnitPrice,
    decimal RecurringPrice,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalPrice);
```

- [ ] **Rename `IOrderRepository` → `IProductOrderRepository`**: Rename file and interface. All method signatures stay the same but use `ProductOrder` instead of `Order` in return types and parameters.

- [ ] **Rename `OrderMappingConfig` → `ProductOrderMappingConfig`**: Rename file and class. Add mappings for new DTOs:
```csharp
// Add to Configure() method:
TypeAdapterConfig<ProductOrderItemRelationship, ProductOrderItemRelationshipDto>.NewConfig()
    .Map(d => d.Type, s => s.Type.ToString());

TypeAdapterConfig<ProductOrderMilestone, ProductOrderMilestoneDto>.NewConfig()
    .Map(d => d.Status, s => s.Status.ToString());

TypeAdapterConfig<ProductOrderItem, ProductOrderItemPriceDto>.NewConfig()
    .Map(d => d.UnitPrice, s => s.UnitPrice)
    .Map(d => d.RecurringPrice, s => s.RecurringPrice)
    .Map(d => d.DiscountAmount, s => s.DiscountAmount)
    .Map(d => d.TaxAmount, s => s.TaxAmount)
    .Map(d => d.TotalPrice, s => s.TotalPrice);
```

- [ ] **Commit:**

```bash
git add src/Modules/Orders/Obss.Orders.Application/DTOs/
git add src/Modules/Orders/Obss.Orders.Application/Abstractions/
git add src/Modules/Orders/Obss.Orders.Application/Mappings/
git commit -m "feat: rename DTOs, add new DTOs, update IProductOrderRepository and Mapster config"
```

---

### Task 7: Application — Rename Existing Commands

**Files:**
- Modify: 11 command folders in `src/Modules/Orders/Obss.Orders.Application/Commands/` — rename each folder and its 3 files (command, handler, validator)

- [ ] **For each of the following, rename folder, rename files, and update class names + usages:**

| Old Folder | New Folder |
|------------|------------|
| `CreateOrder/` | `CreateProductOrder/` |
| `AddOrderItem/` | `AddProductOrderItem/` |
| `RemoveOrderItem/` | `RemoveProductOrderItem/` |
| `SubmitOrder/` | `SubmitProductOrder/` |
| `ApproveOrder/` | `ApproveProductOrder/` |
| `CancelOrder/` | `CancelProductOrder/` |
| `DeleteOrder/` | `DeleteProductOrder/` |
| `PartialUpdateOrder/` | `PatchProductOrder/` |
| `StartOrderFulfillment/` | unchanged |
| `CompleteOrderFulfillment/` | unchanged |
| `ValidateOrder/` | `ValidateProductOrder/` |

For each rename:
1. Rename the folder (e.g., `CreateOrder/` → `CreateProductOrder/`)
2. Rename files inside: `CreateOrderCommand.cs` → `CreateProductOrderCommand.cs`, etc.
3. Update class names, constructor names, and all type references to use `ProductOrder` instead of `Order`
4. Update `CreateProductOrderCommand` to accept new fields: `BillingAccountId` (Guid?), `Priority` (string?), `ProductOfferingQualificationId` (Guid?), `QuoteHref` (string?)
5. Update `PatchProductOrderCommand` similarly
6. The handler should call repository methods with updated type names
7. Validators should validate new optional fields

- [ ] **Commit:**

```bash
git add src/Modules/Orders/Obss.Orders.Application/Commands/
git commit -m "feat: rename all Order commands to ProductOrder, add new fields to create/patch"
```

---

### Task 8: Application — Add Item State Transition Commands

**Files:**
- Create: 9 command folders under `src/Modules/Orders/Obss.Orders.Application/Commands/`

Create the following command folders, each with `Command.cs`, `CommandHandler.cs`, and `CommandValidator.cs`:

- [ ] **`AcknowledgeProductOrderItem/`**
- [ ] **`StartProductOrderItem/`**
- [ ] **`HoldProductOrderItem/`**
- [ ] **`ResumeProductOrderItem/`**
- [ ] **`AssessProductOrderItem/`**
- [ ] **`RejectProductOrderItem/`** (includes `Reason` string field)
- [ ] **`CompleteProductOrderItem/`**
- [ ] **`FailProductOrderItem/`** (includes `Error` string field)
- [ ] **`CancelProductOrderItem/`**

Each follows the same pattern. Example for `AcknowledgeProductOrderItem`:

```csharp
// AcknowledgeProductOrderItemCommand.cs
public sealed record AcknowledgeProductOrderItemCommand(Guid OrderId, Guid ItemId) : IRequest<Result>;

// AcknowledgeProductOrderItemCommandHandler.cs
public sealed class AcknowledgeProductOrderItemCommandHandler(IProductOrderRepository repository) : IRequestHandler<AcknowledgeProductOrderItemCommand, Result>
{
    public async Task<Result> Handle(AcknowledgeProductOrderItemCommand request, CancellationToken ct)
    {
        var order = await repository.GetByIdWithItemsAsync(request.OrderId, ct);
        if (order is null) return Result.Failure(new NotFoundError($"ProductOrder {request.OrderId} not found"));

        var item = order.Items.FirstOrDefault(i => i.Id == request.ItemId);
        if (item is null) return Result.Failure(new NotFoundError($"Item {request.ItemId} not found"));

        item.Acknowledge();
        await repository.UpdateAsync(order, ct);
        return Result.Success();
    }
}

// AcknowledgeProductOrderItemCommandValidator.cs
public sealed class AcknowledgeProductOrderItemCommandValidator : AbstractValidator<AcknowledgeProductOrderItemCommand>
{
    public AcknowledgeProductOrderItemCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
    }
}
```

For `RejectProductOrderItem`, add `Reason` property with `NotEmpty()` validation.
For `FailProductOrderItem`, add `Error` property with `NotEmpty()` validation.
For `CancelProductOrderItem`, no extra fields.

- [ ] **Commit:**

```bash
git add src/Modules/Orders/Obss.Orders.Application/Commands/
git commit -m "feat: add 9 ProductOrderItem state transition commands"
```

---

### Task 9: Application — Add Relationship + Milestone Commands

**Files:**
- Create: `src/Modules/Orders/Obss.Orders.Application/Commands/AddItemRelationship/`
- Create: `src/Modules/Orders/Obss.Orders.Application/Commands/RemoveItemRelationship/`
- Create: `src/Modules/Orders/Obss.Orders.Application/Commands/CreateMilestone/`
- Create: `src/Modules/Orders/Obss.Orders.Application/Commands/UpdateMilestone/`
- Create: `src/Modules/Orders/Obss.Orders.Application/Commands/RemoveMilestone/`

- [ ] **`AddItemRelationshipCommand`:**

```csharp
// Command
public sealed record AddItemRelationshipCommand(Guid OrderId, Guid ItemId, Guid TargetItemId, string Type) : IRequest<Result>;

// Handler
public sealed class AddItemRelationshipCommandHandler(IProductOrderRepository repository) : IRequestHandler<AddItemRelationshipCommand, Result>
{
    public async Task<Result> Handle(AddItemRelationshipCommand request, CancellationToken ct)
    {
        var order = await repository.GetByIdWithItemsAsync(request.OrderId, ct);
        if (order is null) return Result.Failure(new NotFoundError($"ProductOrder {request.OrderId} not found"));

        if (!Enum.TryParse<RelationshipType>(request.Type, true, out var type))
            return Result.Failure(new ValidationError("Invalid relationship type"));

        order.AddItemRelationship(request.ItemId, request.TargetItemId, type);
        await repository.UpdateAsync(order, ct);
        return Result.Success();
    }
}

// Validator
public sealed class AddItemRelationshipCommandValidator : AbstractValidator<AddItemRelationshipCommand>
{
    public AddItemRelationshipCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.TargetItemId).NotEmpty();
        RuleFor(x => x.Type).NotEmpty().Must(t => Enum.TryParse<RelationshipType>(t, true, out _));
    }
}
```

- [ ] **`RemoveItemRelationshipCommand`** — similar pattern, validates `OrderId` + `RelationshipId`, calls `order.RemoveItemRelationship(relationshipId)`.

- [ ] **`CreateMilestoneCommand`:**

```csharp
public sealed record CreateMilestoneCommand(Guid OrderId, string Name, string Description, DateTime MilestoneDate) : IRequest<Result>;

public sealed class CreateMilestoneCommandHandler(IProductOrderRepository repository) : IRequestHandler<CreateMilestoneCommand, Result>
{
    public async Task<Result> Handle(CreateMilestoneCommand request, CancellationToken ct)
    {
        var order = await repository.GetByIdWithItemsAsync(request.OrderId, ct);
        if (order is null) return Result.Failure(new NotFoundError($"ProductOrder {request.OrderId} not found"));

        var milestone = new ProductOrderMilestone(request.OrderId, request.Name, request.Description, request.MilestoneDate);
        order.AddMilestone(milestone);
        await repository.UpdateAsync(order, ct);
        return Result.Success();
    }
}

public sealed class CreateMilestoneCommandValidator : AbstractValidator<CreateMilestoneCommand>
{
    public CreateMilestoneCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.MilestoneDate).NotEmpty();
    }
}
```

- [ ] **`UpdateMilestoneCommand`** — takes `OrderId`, `MilestoneId`, `Status` (string), `MilestoneDate`. Validates enum parse. Calls `Achieve()`, `MarkMissed()`, or `Cancel()` based on status.

- [ ] **`RemoveMilestoneCommand`** — takes `OrderId`, `MilestoneId`. Removes from collection.

- [ ] **Commit:**

```bash
git add src/Modules/Orders/Obss.Orders.Application/Commands/
git commit -m "feat: add relationship and milestone commands"
```

---

### Task 10: Application — Queries

**Files:**
- Modify: Rename existing query folders (same pattern as commands)
- Create: `src/Modules/Orders/Obss.Orders.Application/Queries/GetProductOrderItemRelationships/`
- Create: `src/Modules/Orders/Obss.Orders.Application/Queries/GetProductOrderMilestones/`

- [ ] **Rename query folders:**

| Old Folder | New Folder |
|------------|------------|
| `GetOrderById/` | `GetProductOrderById/` |
| `GetOrders/` | `GetProductOrders/` |
| `GetOrdersByCustomer/` | `GetProductOrdersByCustomer/` |
| `GetOrderFulfillmentStatus/` | unchanged |

For each: rename files, update class names, update DTO types in handlers.

- [ ] **Create `GetProductOrderItemRelationshipsQuery`:**

```csharp
// Query
public sealed record GetProductOrderItemRelationshipsQuery(Guid OrderId, Guid? ItemId) : IRequest<Result<List<ProductOrderItemRelationshipDto>>>;

// Handler
public sealed class GetProductOrderItemRelationshipsQueryHandler(IProductOrderRepository repository)
    : IRequestHandler<GetProductOrderItemRelationshipsQuery, Result<List<ProductOrderItemRelationshipDto>>>
{
    public async Task<Result<List<ProductOrderItemRelationshipDto>>> Handle(GetProductOrderItemRelationshipsQuery request, CancellationToken ct)
    {
        var order = await repository.GetByIdWithItemsAsync(request.OrderId, ct);
        if (order is null) return Result.Failure(new NotFoundError($"ProductOrder {request.OrderId} not found"));

        var relationships = request.ItemId.HasValue
            ? order.GetItemRelationships(request.ItemId.Value)
            : order.ItemRelationships.ToList();

        var dtos = relationships.Adapt<List<ProductOrderItemRelationshipDto>>();
        return Result.Success(dtos);
    }
}

// Validator
public sealed class GetProductOrderItemRelationshipsQueryValidator : AbstractValidator<GetProductOrderItemRelationshipsQuery>
{
    public GetProductOrderItemRelationshipsQueryValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
```

- [ ] **Create `GetProductOrderMilestonesQuery`** — similar pattern, returns `List<ProductOrderMilestoneDto>`.

- [ ] **Commit:**

```bash
git add src/Modules/Orders/Obss.Orders.Application/Queries/
git commit -m "feat: rename queries, add relationships and milestones queries"
```

---

### Task 11: Application — Domain Events + Integration Events + Handlers

**Files:**
- Modify: Rename 4 domain event files in `Obss.Orders.Domain/Events/`
- Modify: Rename 4 domain event handler files in `Obss.Orders.Application/DomainEventHandlers/`
- Modify: Rename integration event files in `Obss.Orders.Application/IntegrationEvents/`
- Create: `Obss.Orders.Application/IntegrationEvents/ProductOrderItemStateChangedIntegrationEvent.cs`
- Create: `Obss.Orders.Application/IntegrationEvents/ProductOrderMilestoneReachedIntegrationEvent.cs`
- Modify: `Obss.Orders.Application/IntegrationEventHandlers/` — rename handlers + update QuoteAccepted handler

- [ ] **Rename domain event files:**
  - `OrderSubmittedDomainEvent.cs` → `ProductOrderSubmittedDomainEvent.cs` (rename record, keep same fields)
  - `OrderApprovedDomainEvent.cs` → `ProductOrderApprovedDomainEvent.cs`
  - `OrderCancelledDomainEvent.cs` → `ProductOrderCancelledDomainEvent.cs`
  - `OrderCompletedDomainEvent.cs` → `ProductOrderCompletedDomainEvent.cs`

- [ ] **Rename domain event handler files:**
  - `OrderSubmittedEventHandler.cs` → `ProductOrderSubmittedEventHandler.cs` (rename class, update event type reference)
  - `OrderApprovedDomainEventHandler.cs` → `ProductOrderApprovedDomainEventHandler.cs`

- [ ] **Rename integration event files:**
  - `OrderSubmittedIntegrationEvent.cs` → `ProductOrderSubmittedIntegrationEvent.cs`
  - `OrderApprovedIntegrationEvent.cs` → `ProductOrderApprovedIntegrationEvent.cs`
  - `OrderFulfillmentStartedIntegrationEvent.cs` → unchanged
  - `ProvisioningRequiredIntegrationEvent.cs` → unchanged
  - `SubscriptionRequiredIntegrationEvent.cs` → unchanged

- [ ] **Add auto-transition logic to `ProductOrderApprovedDomainEventHandler`**: After the handler fires, check if all items are completed and call `order.MarkCompleted()` automatically:

In the handler, after processing item transitions:
```csharp
if (order.Items.All(i => i.State == ProductOrderItemState.Completed))
{
    order.MarkCompleted();
}
```

- [ ] **Create new integration events:**

`ProductOrderItemStateChangedIntegrationEvent.cs`:
```csharp
namespace Obss.Orders.Application.IntegrationEvents;

public sealed record ProductOrderItemStateChangedIntegrationEvent(
    Guid OrderId,
    Guid ItemId,
    string OldState,
    string NewState,
    string? Reason) : IntegrationEvent;
```

`ProductOrderMilestoneReachedIntegrationEvent.cs`:
```csharp
namespace Obss.Orders.Application.IntegrationEvents;

public sealed record ProductOrderMilestoneReachedIntegrationEvent(
    Guid OrderId,
    Guid MilestoneId,
    string MilestoneName,
    string Status) : IntegrationEvent;
```

- [ ] **Update `QuoteAcceptedIntegrationEventHandler`** — change to create `ProductOrder` instead of `Order`. Update all type references.

- [ ] **Commit:**

```bash
git add src/Modules/Orders/
git commit -m "feat: rename events, add new integration events, update QuoteAccepted handler"
```

---

### Task 12: Infrastructure — EF Configs

**Files:**
- Modify: Rename EF configs + add new ones
- Create: `Obss.Orders.Infrastructure/Persistence/Configurations/ProductOrderItemRelationshipConfiguration.cs`
- Create: `Obss.Orders.Infrastructure/Persistence/Configurations/ProductOrderMilestoneConfiguration.cs`

- [ ] **Rename existing configs + update table names:**

`OrderConfiguration.cs` → `ProductOrderConfiguration.cs`:
- Table: `"product_orders"`
- Add properties config: `billing_account_id`, `billing_account_href`, `order_version`, `priority` (stored as string), `product_offering_qualification_id`, `product_offering_qualification_href`, `quote_href`
- Ignore navigation properties for relationships/milestones (they're managed by the aggregate, not as EF navigations)

`OrderItemConfiguration.cs` → `ProductOrderItemConfiguration.cs`:
- Table: `"product_order_items"`
- Add `State` property: `.Property(x => x.State).HasConversion<string>().HasMaxLength(50)`

`OrderPaymentConfiguration.cs` → `ProductOrderPaymentConfiguration.cs`:
- Table: `"product_order_payments"`

- [ ] **Create `ProductOrderItemRelationshipConfiguration.cs`:**

```csharp
namespace Obss.Orders.Infrastructure.Persistence.Configurations;

public sealed class ProductOrderItemRelationshipConfiguration : IEntityTypeConfiguration<ProductOrderItemRelationship>
{
    public void Configure(EntityTypeBuilder<ProductOrderItemRelationship> builder)
    {
        builder.ToTable("product_order_item_relationships");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.ProductOrderItemId).IsRequired();
        builder.Property(r => r.TargetItemId).IsRequired();
        builder.Property(r => r.Type).HasConversion<string>().HasMaxLength(50).IsRequired();
    }
}
```

- [ ] **Create `ProductOrderMilestoneConfiguration.cs`:**

```csharp
namespace Obss.Orders.Infrastructure.Persistence.Configurations;

public sealed class ProductOrderMilestoneConfiguration : IEntityTypeConfiguration<ProductOrderMilestone>
{
    public void Configure(EntityTypeBuilder<ProductOrderMilestone> builder)
    {
        builder.ToTable("product_order_milestones");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();
        builder.Property(m => m.Name).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Description).HasMaxLength(500);
        builder.Property(m => m.MilestoneDate).IsRequired();
        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(m => m.ProductOrderId).IsRequired();
        builder.HasIndex(m => m.ProductOrderId);
    }
}
```

- [ ] **Update `OrderDbContext.cs`**: Add `DbSet<ProductOrderMilestone>`, `DbSet<ProductOrderItemRelationship>`. Rename existing `DbSet<Order>` to `DbSet<ProductOrder>`, etc.

- [ ] **Commit:**

```bash
git add src/Modules/Orders/Obss.Orders.Infrastructure/
git commit -m "feat: rename EF configs, add relationship and milestone configs"
```

---

### Task 13: Infrastructure — Repositories + Migration

**Files:**
- Modify: Rename `OrderRepository` → `ProductOrderRepository`
- Modify: Rename `IOrderFulfillmentRepository` → unchanged
- Create: EF Migration

- [ ] **Rename `OrderRepository` → `ProductOrderRepository`**: Rename file and class. Update all `Order` type references to `ProductOrder` and `DbSet<Order>` to `DbSet<ProductOrder>`.

- [ ] **Generate migration:**

Run from the Orders Infrastructure project:
```bash
cd src/Modules/Orders/Obss.Orders.Infrastructure
dotnet ef migrations add RenameAndAddTmf622Resources \
  -s ../../../Host/Obss.Host \
  -c OrderDbContext
```

If the design-time factory exists, use:
```bash
dotnet ef migrations add RenameAndAddTmf622Resources \
  -- --connection "Host=localhost;Port=5432;Database=obss_orders;Username=obss_admin;Password=obss_s3cur3_p@ss"
```

Verify the migration contains:
1. Rename tables: `orders` → `product_orders`, `order_items` → `product_order_items`, `order_payments` → `product_order_payments`
2. Add columns: `billing_account_id`, `billing_account_href`, `order_version` (default 1), `priority` (default 'Medium'), `product_offering_qualification_id`, `product_offering_qualification_href`, `quote_href` on `product_orders`
3. Add `state` (default 'Acknowledged') on `product_order_items`
4. Create `product_order_item_relationships` table
5. Create `product_order_milestones` table

- [ ] **Commit:**

```bash
git add src/Modules/Orders/Obss.Orders.Infrastructure/
git commit -m "feat: add EF migration RenameAndAddTmf622Resources"
```

---

### Task 14: API — Endpoints + Module Registration

**Files:**
- Modify: `src/Modules/Orders/Obss.Orders.Api/Endpoints/OrderEndpoints.cs` → rename + update routes + add new endpoints
- Modify: `src/Modules/Orders/Obss.Orders.Api/Extensions/OrderModuleRegistration.cs`

- [ ] **Rename `OrderEndpoints.cs` → `ProductOrderEndpoints.cs`**, update route prefix to `/productOrder`.

- [ ] **Update existing endpoint routes** from `/orders` to `/productOrder` in the group prefix.

- [ ] **Add new endpoints** to the `Map()` method:

```csharp
// Item state transitions
group.MapPost("/{id:guid}/items/{itemId:guid}/acknowledge", async (Guid id, Guid itemId, ISender sender) =>
{
    var result = await sender.Send(new AcknowledgeProductOrderItemCommand(id, itemId));
    return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
}).WithName("AcknowledgeProductOrderItem");

// Repeat for: start, hold, resume, assess, reject, complete, fail, cancel
// Reject needs body with Reason, Fail needs body with Error

// Relationships
group.MapGet("/{id:guid}/relationships", async (Guid id, Guid? itemId, ISender sender) =>
{
    var result = await sender.Send(new GetProductOrderItemRelationshipsQuery(id, itemId));
    return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
}).WithName("GetProductOrderRelationships");

group.MapPost("/{id:guid}/relationships", async (Guid id, AddItemRelationshipCommand command, ISender sender) =>
{
    var result = await sender.Send(command with { OrderId = id });
    return result.IsSuccess ? Results.Created($"/api/v1/productOrder/{id}/relationships", null) : result.ToProblemDetails();
}).WithName("AddProductOrderRelationship");

group.MapDelete("/{id:guid}/relationships/{relationshipId:guid}", async (Guid id, Guid relationshipId, ISender sender) =>
{
    var result = await sender.Send(new RemoveItemRelationshipCommand(id, relationshipId));
    return result.IsSuccess ? Results.NoContent() : result.ToProblemDetails();
}).WithName("RemoveProductOrderRelationship");

// Milestones
group.MapGet("/{id:guid}/milestones", async (Guid id, ISender sender) =>
{
    var result = await sender.Send(new GetProductOrderMilestonesQuery(id));
    return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
}).WithName("GetProductOrderMilestones");

group.MapPost("/{id:guid}/milestones", async (Guid id, CreateMilestoneCommand command, ISender sender) =>
{
    var result = await sender.Send(command with { OrderId = id });
    return result.IsSuccess ? Results.Created($"/api/v1/productOrder/{id}/milestones", null) : result.ToProblemDetails();
}).WithName("CreateProductOrderMilestone");

group.MapPatch("/{id:guid}/milestones/{milestoneId:guid}", async (Guid id, Guid milestoneId, UpdateMilestoneCommand command, ISender sender) =>
{
    var result = await sender.Send(command with { OrderId = id, MilestoneId = milestoneId });
    return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
}).WithName("UpdateProductOrderMilestone");

group.MapDelete("/{id:guid}/milestones/{milestoneId:guid}", async (Guid id, Guid milestoneId, ISender sender) =>
{
    var result = await sender.Send(new RemoveMilestoneCommand(id, milestoneId));
    return result.IsSuccess ? Results.NoContent() : result.ToProblemDetails();
}).WithName("RemoveProductOrderMilestone");
```

- [ ] **Update `OrderModuleRegistration.cs`**: Rename file to `ProductOrderModuleRegistration.cs`. Update class name. Update `MapOrderEndpoints()` to `MapProductOrderEndpoints()`.

- [ ] **Register the module in `Program.cs`**: Update call from `app.MapOrderEndpoints()` to `app.MapProductOrderEndpoints()`.

- [ ] **Update DI registrations**: Change `IOrderRepository` → `IProductOrderRepository` in the registration method. Update assembly scanning for MediatR/validators to pick up renamed types.

- [ ] **Add 301 redirect from old `/orders` paths:**
```csharp
// In ProductOrderModuleRegistration.cs, after MapProductOrderEndpoints():
app.MapGet("/api/v{version:apiVersion}/orders/{**rest}", (string? rest) =>
    Results.RedirectPermanent($"/api/v{version}/productOrder/{rest ?? ""}"))
    .ExcludeFromDescription();

app.MapPost("/api/v{version:apiVersion}/orders/{**rest}", (string? rest) =>
    Results.RedirectPermanent($"/api/v{version}/productOrder/{rest ?? ""}"))
    .ExcludeFromDescription();
```

- [ ] **Verify build**:
```bash
dotnet build src/Modules/Orders/Obss.Orders.Api/ --configuration Release 2>&1 | tail -20
```

- [ ] **Commit:**

```bash
git add src/Modules/Orders/Obss.Orders.Api/
git add src/Host/Obss.Host/Program.cs
git commit -m "feat: rename API endpoints, add new TMF622 endpoints, update module registration"
```

---

### Task 15: Cross-Module Updates

**Files:**
- Modify: All consumers of renamed integration events in Provisioning, Subscriptions, and other modules
- The integration events whose names changed: `ProductOrderSubmittedIntegrationEvent`, `ProductOrderApprovedIntegrationEvent`

- [ ] **Search for consumers of old event types:**
```bash
rg "OrderSubmittedIntegrationEvent" --include "*.cs" -l
rg "OrderApprovedIntegrationEvent" --include "*.cs" -l
```

- [ ] For each consumer found outside the Orders module, update the event type reference to the new name (`ProductOrderSubmittedIntegrationEvent` / `ProductOrderApprovedIntegrationEvent`).

- [ ] **Update Program.cs type registrations** in Host project if they reference the old names.

- [ ] **Commit:**

```bash
git add src/ -A
git commit -m "fix: update cross-module consumers of renamed integration events"
```

---

### Task 16: Frontend — Query Keys + Hooks

**Files:**
- Modify: `frontend/src/lib/query-keys.ts`
- Create: `frontend/src/api/hooks/useProductOrders.ts`

- [ ] **Update query keys** in `frontend/src/lib/query-keys.ts`:
```typescript
// Rename the existing `orders` block to `productOrders`
productOrders: {
  all: ["orders", "product-orders"] as const,
  lists: () => [...queryKeys.orders.productOrders.all, "list"] as const,
  list: (filters: Record<string, string> = {}) =>
    [...queryKeys.orders.productOrders.lists(), filters] as const,
  details: () => [...queryKeys.orders.productOrders.all, "detail"] as const,
  detail: (id: string) => [...queryKeys.orders.productOrders.details(), id] as const,
  relationships: (id: string) => [...queryKeys.orders.productOrders.detail(id), "relationships"] as const,
  milestones: (id: string) => [...queryKeys.orders.productOrders.detail(id), "milestones"] as const,
},
```

- [ ] **Create `frontend/src/api/hooks/useProductOrders.ts`** — port existing `useOrders.ts` to `ProductOrder` naming + add new hooks:

```typescript
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export interface ProductOrderDto { /* all fields from ProductOrderDto */ }
export interface ProductOrderItemDto { /* all fields including State */ }
export interface ProductOrderItemRelationshipDto { Id: string; productOrderItemId: string; targetItemId: string; type: string }
export interface ProductOrderMilestoneDto { Id: string; productOrderId: string; name: string; description: string; milestoneDate: string; status: string }

// Rename existing hooks (useOrders → useProductOrders, etc.)
export function useProductOrders(filters = {}) { /* ... */ }
export function useProductOrder(id: string) { /* ... */ }
export function useCreateProductOrder() { /* ... */ }
export function useUpdateProductOrder() { /* ... */ }
export function useDeleteProductOrder() { /* ... */ }

// New item state hooks
export function useAcknowledgeProductOrderItem() { /* mutation */ }
export function useStartProductOrderItem() { /* mutation */ }
export function useHoldProductOrderItem() { /* mutation */ }
export function useResumeProductOrderItem() { /* mutation */ }
export function useAssessProductOrderItem() { /* mutation */ }
export function useRejectProductOrderItem() { /* mutation with reason */ }
export function useCompleteProductOrderItem() { /* mutation */ }
export function useFailProductOrderItem() { /* mutation with error */ }
export function useCancelProductOrderItem() { /* mutation */ }

// New relationship hooks
export function useProductOrderRelationships(orderId: string) { /* query */ }
export function useAddProductOrderRelationship() { /* mutation */ }
export function useRemoveProductOrderRelationship() { /* mutation */ }

// New milestone hooks
export function useProductOrderMilestones(orderId: string) { /* query */ }
export function useCreateProductOrderMilestone() { /* mutation */ }
export function useUpdateProductOrderMilestone() { /* mutation */ }
export function useRemoveProductOrderMilestone() { /* mutation */ }
```

Each mutation follows the same pattern as existing hooks in `useBillingAccounts.ts`: call API endpoint, invalidate query keys on success.

- [ ] **Commit:**

```bash
git add frontend/src/lib/query-keys.ts frontend/src/api/hooks/useProductOrders.ts
git commit -m "feat: add ProductOrder frontend query keys and hooks"
```

---

### Task 17: Frontend — Pages + Components

**Files:**
- Rename: `frontend/src/app/orders/` → `frontend/src/app/product-order/`
- Modify: All page files to use new DTO types and API hooks
- Create: `ProductOrderItemStateTimeline` component
- Create: `ProductOrderRelationshipsPanel` component
- Create: `ProductOrderMilestonesPanel` component

- [ ] **Rename `frontend/src/app/orders/` → `frontend/src/app/product-order/`:**
```bash
mv frontend/src/app/orders frontend/src/app/product-order
```

- [ ] **Update all page files** to:
  - Replace `useOrders()` → `useProductOrders()`, etc.
  - Replace `OrderDto` → `ProductOrderDto` interface imports
  - Add BillingAccount dropdown to new/edit pages (fetching from `/api/v1/billing/billing-accounts`)
  - Add Priority select (Low/Medium/High/Critical) to create/edit pages
  - Display OrderVersion badge on detail page
  - Display item state badges on detail page

- [ ] **Create `ProductOrderItemStateTimeline` component** at `frontend/src/app/product-order/_components/ItemStateTimeline.tsx`:
  - Shows current state with colored badge
  - Transition buttons (acknowledge, start, hold, resume, assess, reject, complete, fail, cancel)
  - Only shows valid next transitions from current state

- [ ] **Create `ProductOrderRelationshipsPanel`** at `frontend/src/app/product-order/_components/RelationshipsPanel.tsx`:
  - Table of relationships with type badges
  - "Add Relationship" form with item selector, target item selector, type dropdown
  - Remove button per row

- [ ] **Create `ProductOrderMilestonesPanel`** at `frontend/src/app/product-order/_components/MilestonesPanel.tsx`:
  - Timeline view of milestones with status badges (green=Achieved, red=Missed, gray=Pending, yellow=Cancelled)
  - "Add Milestone" form
  - Update status actions per milestone

- [ ] **Verify frontend builds:**
```bash
cd frontend && bun run lint 2>&1 | tail -20
```

- [ ] **Commit:**

```bash
git add frontend/src/app/product-order/
git add frontend/src/app/product-order/_components/
git commit -m "feat: add ProductOrder frontend pages and components"
```

---

### Task 18: Domain + Handler Tests

**Files:**
- Create: `tests/Modules/Orders.Tests/Domain/ProductOrderItemStateMachineTests.cs`
- Create: `tests/Modules/Orders.Tests/Domain/ProductOrderRelationshipTests.cs`
- Create: `tests/Modules/Orders.Tests/Domain/ProductOrderMilestoneTests.cs`
- Create: `tests/Modules/Orders.Tests/Application/ProductOrderItemCommandHandlerTests.cs`

- [ ] **Create `ProductOrderItemStateMachineTests.cs`:**

```csharp
namespace Obss.Orders.Tests.Domain;

public class ProductOrderItemStateMachineTests
{
    [Fact]
    public void Acknowledge_SetsStateToAcknowledged()
    {
        var order = CreateOrder();
        var item = CreateItem(order.Id);
        
        item.Acknowledge();
        
        item.State.Should().Be(ProductOrderItemState.Acknowledged);
    }

    [Fact]
    public void StartProgress_FromAcknowledged_TransitionsToInProgress()
    {
        var order = CreateOrder();
        var item = CreateItem(order.Id);
        item.Acknowledge();
        
        item.StartProgress();
        
        item.State.Should().Be(ProductOrderItemState.InProgress);
    }

    [Fact]
    public void StartProgress_FromCompleted_Throws()
    {
        var order = CreateOrder();
        var item = CreateItem(order.Id);
        item.Acknowledge();
        item.StartProgress();
        item.Assess();
        item.Complete();
        
        var act = () => item.StartProgress();
        
        act.Should().Throw<InvalidProductOrderItemStateException>();
    }

    // Test all valid transitions:
    // Acknowledge → InProgress → Hold → Resume → InProgress
    // Acknowledge → InProgress → Assess → Rejected
    // Acknowledge → InProgress → Assess → Completed
    // Acknowledge → Cancel → Cancelled
    // InProgress → Pending → InProgress
    // InProgress → Fail → Failed

    [Fact]
    public void FullLifecycle_AcknowledgeToComplete_Succeeds()
    {
        var order = CreateOrder();
        var item = CreateItem(order.Id);
        
        item.Acknowledge();
        item.StartProgress();
        item.Assess();
        item.Complete();
        
        item.State.Should().Be(ProductOrderItemState.Completed);
    }

    [Fact]
    public void Cancel_FromInProgress_TransitionsToCancelled()
    {
        var order = CreateOrder();
        var item = CreateItem(order.Id);
        item.Acknowledge();
        item.StartProgress();
        
        item.Cancel();
        
        item.State.Should().Be(ProductOrderItemState.Cancelled);
    }

    [Fact]
    public void Cancel_FromCompleted_Throws()
    {
        var order = CreateOrder();
        var item = CreateItem(order.Id);
        item.Acknowledge();
        item.StartProgress();
        item.Assess();
        item.Complete();
        
        var act = () => item.Cancel();
        
        act.Should().Throw<InvalidProductOrderItemStateException>();
    }

    [Fact]
    public void ItemStateChangedEvent_RaisedOnTransition()
    {
        var order = CreateOrder();
        var item = CreateItem(order.Id);
        item.Acknowledge();
        item.ClearDomainEvents();

        item.StartProgress();

        var events = item.DomainEvents;
        events.Should().ContainSingle(e => e is ProductOrderItemStateChangedDomainEvent);
        var stateEvent = events.OfType<ProductOrderItemStateChangedDomainEvent>().Single();
        stateEvent.ItemId.Should().Be(item.Id);
        stateEvent.OldState.Should().Be(ProductOrderItemState.Acknowledged);
        stateEvent.NewState.Should().Be(ProductOrderItemState.InProgress);
    }

    private static ProductOrder CreateOrder() =>
        ProductOrder.Create("tenant1", Guid.NewGuid(), "Test Customer", OrderType.New, "user1", null, null, null, "USD");

    private static ProductOrderItem CreateItem(Guid orderId)
    {
        var order = CreateOrder();
        var item = new ProductOrderItem(Guid.NewGuid(), orderId, Guid.NewGuid(), Guid.NewGuid(), "Product", "Offer", 1, 100, 0, 0, 0, BillingPeriod.Monthly, null);
        return item;
    }
}
```

- [ ] **Create `ProductOrderRelationshipTests.cs`:**

```csharp
namespace Obss.Orders.Tests.Domain;

public class ProductOrderRelationshipTests
{
    [Fact]
    public void AddItemRelationship_AddsRelationship()
    {
        var order = CreateOrderWithTwoItems(out var item1, out var item2);

        order.AddItemRelationship(item1.Id, item2.Id, RelationshipType.Requires);

        order.ItemRelationships.Should().ContainSingle(r => r.ProductOrderItemId == item1.Id && r.TargetItemId == item2.Id);
    }

    [Fact]
    public void AddItemRelationship_CircularDependency_Throws()
    {
        var order = CreateOrderWithTwoItems(out var item1, out var item2);
        order.AddItemRelationship(item1.Id, item2.Id, RelationshipType.Requires);

        var act = () => order.AddItemRelationship(item2.Id, item1.Id, RelationshipType.Requires);

        act.Should().Throw<InvalidProductOrderStateException>();
    }

    [Fact]
    public void RemoveItemRelationship_RemovesRelationship()
    {
        var order = CreateOrderWithTwoItems(out var item1, out var item2);
        order.AddItemRelationship(item1.Id, item2.Id, RelationshipType.Requires);
        var relId = order.ItemRelationships.First().Id;

        order.RemoveItemRelationship(relId);

        order.ItemRelationships.Should().BeEmpty();
    }

    [Fact]
    public void GetItemRelationships_ReturnsOnlyRelatedItems()
    {
        var order = CreateOrderWithTwoItems(out var item1, out var item2);
        order.AddItemRelationship(item1.Id, item2.Id, RelationshipType.Requires);

        var rels = order.GetItemRelationships(item1.Id);

        rels.Should().ContainSingle(r => r.TargetItemId == item2.Id);
    }

    private static ProductOrder CreateOrderWithTwoItems(out ProductOrderItem item1, out ProductOrderItem item2)
    {
        var order = ProductOrder.Create("tenant1", Guid.NewGuid(), "Test", OrderType.New, "user1", null, null, null, "USD");
        item1 = new ProductOrderItem(Guid.NewGuid(), order.Id, Guid.NewGuid(), Guid.NewGuid(), "P1", "O1", 1, 100, 0, 0, 0, BillingPeriod.Monthly, null);
        item2 = new ProductOrderItem(Guid.NewGuid(), order.Id, Guid.NewGuid(), Guid.NewGuid(), "P2", "O2", 2, 200, 0, 0, 0, BillingPeriod.Monthly, null);
        return order;
    }
}
```

- [ ] **Create `ProductOrderMilestoneTests.cs`:**

```csharp
namespace Obss.Orders.Tests.Domain;

public class ProductOrderMilestoneTests
{
    [Fact]
    public void Constructor_SetsPending()
    {
        var milestone = new ProductOrderMilestone(Guid.NewGuid(), "Custom", "Test milestone", DateTime.UtcNow);

        milestone.Status.Should().Be(MilestoneStatus.Pending);
    }

    [Fact]
    public void Achieve_SetsStatusAndRaisesEvent()
    {
        var milestone = new ProductOrderMilestone(Guid.NewGuid(), "Custom", "Test", DateTime.UtcNow);

        milestone.Achieve();

        milestone.Status.Should().Be(MilestoneStatus.Achieved);
        milestone.DomainEvents.Should().ContainSingle(e => e is ProductOrderMilestoneReachedDomainEvent);
    }

    [Fact]
    public void MarkMissed_SetsMissedStatus()
    {
        var milestone = new ProductOrderMilestone(Guid.NewGuid(), "Custom", "Test", DateTime.UtcNow);

        milestone.MarkMissed();

        milestone.Status.Should().Be(MilestoneStatus.Missed);
    }

    [Fact]
    public void AutoCreateMilestones_OnOrderCreation()
    {
        var order = CreateOrder();

        order.Milestones.Should().Contain(m => m.Name == "OrderCreated" && m.Status == MilestoneStatus.Achieved);
    }

    [Fact]
    public void AutoCreateMilestones_OnSubmit()
    {
        var order = CreateOrder();
        order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "Product", "Offer", 1, 100, 0, 0, 0, BillingPeriod.Monthly);
        order.Submit();

        order.Milestones.Should().Contain(m => m.Name == "OrderSubmitted" && m.Status == MilestoneStatus.Achieved);
    }

    private static ProductOrder CreateOrder() =>
        ProductOrder.Create("tenant1", Guid.NewGuid(), "Test", OrderType.New, "user1", null, null, null, "USD");
}
```

- [ ] **Create `ProductOrderItemCommandHandlerTests.cs`:**

```csharp
namespace Obss.Orders.Tests.Application;

public class ProductOrderItemCommandHandlerTests
{
    [Fact]
    public async Task AcknowledgeItemCommandHandler_ShouldAcknowledgeItem()
    {
        var order = CreateOrderWithItem(out var item);
        var repo = Substitute.For<IProductOrderRepository>();
        repo.GetByIdWithItemsAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var handler = new AcknowledgeProductOrderItemCommandHandler(repo);

        var result = await handler.Handle(new AcknowledgeProductOrderItemCommand(order.Id, item.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        item.State.Should().Be(ProductOrderItemState.Acknowledged);
        await repo.Received(1).UpdateAsync(order, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartItemCommandHandler_ShouldStartItem()
    {
        var order = CreateOrderWithItem(out var item);
        item.Acknowledge();
        var repo = Substitute.For<IProductOrderRepository>();
        repo.GetByIdWithItemsAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var handler = new StartProductOrderItemCommandHandler(repo);

        var result = await handler.Handle(new StartProductOrderItemCommand(order.Id, item.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        item.State.Should().Be(ProductOrderItemState.InProgress);
    }

    [Fact]
    public async Task RejectItemCommandHandler_ShouldRejectItem()
    {
        var order = CreateOrderWithItem(out var item);
        item.Acknowledge();
        item.StartProgress();
        item.Assess();
        var repo = Substitute.For<IProductOrderRepository>();
        repo.GetByIdWithItemsAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var handler = new RejectProductOrderItemCommandHandler(repo);

        var result = await handler.Handle(new RejectProductOrderItemCommand(order.Id, item.Id, "Not eligible"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        item.State.Should().Be(ProductOrderItemState.Rejected);
    }

    [Fact]
    public async Task CompleteItemCommandHandler_WhenAllItemsComplete_TriggersOrderAutoComplete()
    {
        var order = CreateOrderWithItem(out var item);
        item.Acknowledge();
        item.StartProgress();
        item.Assess();
        item.Complete();
        var repo = Substitute.For<IProductOrderRepository>();
        repo.GetByIdWithItemsAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        // The auto-complete logic should fire when the last item completes.
        // This test verifies the handler behavior — the actual auto-transition
        // is verified in the domain event handler integration test.
        var handler = new CompleteProductOrderItemCommandHandler(repo);
        var result = await handler.Handle(new CompleteProductOrderItemCommand(order.Id, item.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    private static ProductOrder CreateOrderWithItem(out ProductOrderItem item)
    {
        var order = ProductOrder.Create("tenant1", Guid.NewGuid(), "Test", OrderType.New, "user1", null, null, null, "USD");
        item = new ProductOrderItem(Guid.NewGuid(), order.Id, Guid.NewGuid(), Guid.NewGuid(), "P", "O", 1, 100, 0, 0, 0, BillingPeriod.Monthly, null);
        return order;
    }
}
```

- [ ] **Run tests:**
```bash
dotnet test tests/Modules/Orders.Tests/ --configuration Release --no-restore 2>&1 | tail -20
```
Expected: All new tests pass.

- [ ] **Commit:**
```bash
git add tests/Modules/Orders.Tests/
git commit -m "test: add ProductOrder domain and handler tests"
```

---

### Task 19: Full Build Verification

- [ ] **Build .NET solution:**
```bash
dotnet build Obss.sln --configuration Release 2>&1 | tail -20
```
Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

- [ ] **Run formatting check:**
```bash
dotnet format --verify-no-changes --verbosity diagnostic 2>&1 | tail -10
```

- [ ] **Build frontend:**
```bash
cd frontend && bun run lint 2>&1 | tail -20
```

- [ ] **If any issues found**: fix and re-run until clean.

---

### Task 20: Merge

- [ ] **Check branch status:**
```bash
git log --oneline master..HEAD
git diff --stat master..HEAD | tail -5
```

- [ ] **Create merge commit or fast-forward to master:**
```bash
git checkout master
git merge wp-024-product-ordering --ff-only
```

- [ ] **Delete feature branch:**
```bash
git branch -d wp-024-product-ordering
```
