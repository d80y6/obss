# WP-018: ServiceOrder (TMF641) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use subagent-driven-development or executing-plans to implement this plan task-by-task.

**Goal:** Add a TMF641-compliant ServiceOrder aggregate root to the Provisioning module with lifecycle management, cancellation, item tracking, and bidirectional integration with Orders.

**Architecture:** Layered approach — ServiceOrder and ServiceOrderItem are new domain entities. Existing ProvisioningJob/Task remains as the execution engine underneath. ServiceOrderItem triggers 1+ ProvisioningJobs. Outbound integration events notify Orders.

**Tech Stack:** .NET 9, EF Core/Npgsql, MediatR, Mapster, FluentValidation

---

### Task 1: Domain — ServiceOrder entity

**Files:**
- Create: `src/Modules/Provisioning/Obss.Provisioning.Domain/Entities/ServiceOrder.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Domain/Entities/ServiceOrderItem.cs`

- [ ] **Step 1: Create ServiceOrder aggregate root**

```csharp
using Obss.Provisioning.Domain.Events;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Entities;

public class ServiceOrder : AggregateRoot<Guid>
{
    private readonly List<ServiceOrderItem> _items = [];
    private readonly List<ServiceOrderRelatedParty> _relatedParties = [];
    private readonly List<ServiceOrderCharacteristic> _characteristics = [];
    private readonly List<ServiceOrderMilestone> _milestones = [];
    private readonly List<ServiceOrderNote> _notes = [];

    private ServiceOrder() { }

    private ServiceOrder(
        Guid id,
        Guid tenantId,
        string? externalId,
        ServiceOrderState state,
        string? priority,
        string? description,
        string? category)
        : base(id)
    {
        TenantId = tenantId;
        ExternalId = externalId;
        State = state;
        Priority = priority;
        Description = description;
        Category = category;
        OrderDate = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid TenantId { get; private set; }
    public string? ExternalId { get; private set; }
    public ServiceOrderState State { get; private set; }
    public string? Priority { get; private set; }
    public string? Description { get; private set; }
    public string? Category { get; private set; }
    public DateTime? RequestedStartDate { get; private set; }
    public DateTime? RequestedCompletionDate { get; private set; }
    public DateTime OrderDate { get; private set; }
    public DateTime? StatusChangeDate { get; private set; }
    public string? CompletionMessage { get; private set; }
    public string? Href { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<ServiceOrderItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<ServiceOrderRelatedParty> RelatedParties => _relatedParties.AsReadOnly();
    public IReadOnlyCollection<ServiceOrderCharacteristic> Characteristics => _characteristics.AsReadOnly();
    public IReadOnlyCollection<ServiceOrderMilestone> Milestones => _milestones.AsReadOnly();
    public IReadOnlyCollection<ServiceOrderNote> Notes => _notes.AsReadOnly();
    public CancelServiceOrder? CancelRequest { get; private set; }

    public static ServiceOrder Create(
        Guid tenantId,
        string? externalId,
        string? description,
        string? category,
        string? priority)
    {
        var order = new ServiceOrder(
            Guid.NewGuid(), tenantId, externalId,
            ServiceOrderState.Acknowledged, priority, description, category);

        order.AddDomainEvent(new ServiceOrderSubmittedDomainEvent(order.Id, tenantId, externalId));
        return order;
    }

    public ServiceOrderItem AddItem(
        Guid? serviceId,
        ServiceOrderAction action,
        int quantity,
        string? description,
        DateTime? requestedStartDate,
        DateTime? requestedCompletionDate)
    {
        var item = new ServiceOrderItem(
            Guid.NewGuid(), Id, serviceId, action, quantity, description,
            requestedStartDate, requestedCompletionDate);

        _items.Add(item);
        UpdatedAt = DateTime.UtcNow;
        return item;
    }

    public void Start()
    {
        if (State != ServiceOrderState.Acknowledged)
            return;

        State = ServiceOrderState.InProgress;
        StatusChangeDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ServiceOrderStateChangedDomainEvent(Id, ExternalId, "InProgress"));
    }

    public void Hold()
    {
        if (State != ServiceOrderState.InProgress)
            return;

        State = ServiceOrderState.Held;
        StatusChangeDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ServiceOrderStateChangedDomainEvent(Id, ExternalId, "Held"));
    }

    public void Resume()
    {
        if (State != ServiceOrderState.Held)
            return;

        State = ServiceOrderState.InProgress;
        StatusChangeDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ServiceOrderStateChangedDomainEvent(Id, ExternalId, "InProgress"));
    }

    public void Complete()
    {
        if (State != ServiceOrderState.InProgress)
            return;

        State = _items.All(i => i.State == ServiceOrderItemState.Completed)
            ? ServiceOrderState.Completed
            : ServiceOrderState.PartiallyCompleted;

        StatusChangeDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ServiceOrderStateChangedDomainEvent(Id, ExternalId, State.ToString()));
    }

    public void Fail(string? message = null)
    {
        if (State is ServiceOrderState.Completed or ServiceOrderState.Cancelled)
            return;

        State = ServiceOrderState.Failed;
        CompletionMessage = message;
        StatusChangeDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ServiceOrderStateChangedDomainEvent(Id, ExternalId, "Failed"));
    }

    public void Reject(string? message = null)
    {
        if (State != ServiceOrderState.Acknowledged)
            return;

        State = ServiceOrderState.Rejected;
        CompletionMessage = message;
        StatusChangeDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ServiceOrderStateChangedDomainEvent(Id, ExternalId, "Rejected"));
    }

    public void RequestCancellation(string reason)
    {
        if (State is ServiceOrderState.Completed or ServiceOrderState.Cancelled or ServiceOrderState.Rejected)
            return;

        State = ServiceOrderState.PendingCancellation;
        CancelRequest = new CancelServiceOrder(Guid.NewGuid(), reason);
        StatusChangeDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ServiceOrderCancellationRequestedDomainEvent(Id, ExternalId, reason));
    }

    public void Cancel()
    {
        if (State != ServiceOrderState.PendingCancellation)
            return;

        State = ServiceOrderState.Cancelled;
        if (CancelRequest is not null)
            CancelRequest = CancelRequest with { CompletedDate = DateTime.UtcNow };

        StatusChangeDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ServiceOrderStateChangedDomainEvent(Id, ExternalId, "Cancelled"));
    }

    public void CompleteItem(Guid itemId, Guid? serviceId = null)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
            return;

        item.Complete(serviceId);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ServiceOrderItemCompletedDomainEvent(Id, itemId, serviceId));
    }

    public void FailItem(Guid itemId, string? error = null)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
            return;

        item.Fail(error);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(
        string? description = null,
        string? category = null,
        string? priority = null,
        DateTime? requestedStartDate = null,
        DateTime? requestedCompletionDate = null)
    {
        if (description is not null) Description = description;
        if (category is not null) Category = category;
        if (priority is not null) Priority = priority;
        if (requestedStartDate is not null) RequestedStartDate = requestedStartDate;
        if (requestedCompletionDate is not null) RequestedCompletionDate = requestedCompletionDate;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

- [ ] **Step 2: Create ServiceOrderItem entity**

```csharp
using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Entities;

public class ServiceOrderItem : Entity<Guid>
{
    internal ServiceOrderItem(
        Guid id,
        Guid serviceOrderId,
        Guid? serviceId,
        ServiceOrderAction action,
        int quantity,
        string? description,
        DateTime? requestedStartDate,
        DateTime? requestedCompletionDate)
        : base(id)
    {
        ServiceOrderId = serviceOrderId;
        ServiceId = serviceId;
        Action = action;
        Quantity = quantity;
        Description = description;
        State = ServiceOrderItemState.Acknowledged;
        RequestedStartDate = requestedStartDate;
        RequestedCompletionDate = requestedCompletionDate;
    }

    private ServiceOrderItem() { }

    public Guid ServiceOrderId { get; private set; }
    public Guid? ServiceId { get; private set; }
    public ServiceOrderAction Action { get; private set; }
    public int Quantity { get; private set; }
    public string? Description { get; private set; }
    public ServiceOrderItemState State { get; private set; }
    public DateTime? RequestedStartDate { get; private set; }
    public DateTime? RequestedCompletionDate { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public string? ErrorMessage { get; private set; }

    public void Complete(Guid? serviceId = null)
    {
        State = ServiceOrderItemState.Completed;
        CompletedDate = DateTime.UtcNow;
        if (serviceId.HasValue)
            ServiceId = serviceId;
    }

    public void Fail(string? error = null)
    {
        State = ServiceOrderItemState.Failed;
        ErrorMessage = error;
        CompletedDate = DateTime.UtcNow;
    }

    public void Hold()
    {
        if (State is ServiceOrderItemState.Completed or ServiceOrderItemState.Failed)
            return;
        State = ServiceOrderItemState.Held;
    }
}
```

- [ ] **Step 3: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Domain/Entities/ServiceOrder.cs src/Modules/Provisioning/Obss.Provisioning.Domain/Entities/ServiceOrderItem.cs
git commit -m "feat: add ServiceOrder and ServiceOrderItem domain entities"
```

---

### Task 2: Domain — Value objects and enums

**Files:**
- Create: `src/Modules/Provisioning/Obss.Provisioning.Domain/Entities/ServiceOrderState.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Domain/Entities/ServiceOrderItemState.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Domain/Entities/ServiceOrderAction.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Domain/Entities/CancelServiceOrder.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Domain/Entities/ServiceOrderRelatedParty.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Domain/Entities/ServiceOrderCharacteristic.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Domain/Entities/ServiceOrderMilestone.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Domain/Entities/ServiceOrderNote.cs`

- [ ] **Step 1: Create enums**

```csharp
// ServiceOrderState.cs
namespace Obss.Provisioning.Domain.Entities;

public enum ServiceOrderState
{
    Acknowledged,
    InProgress,
    Held,
    Completed,
    Failed,
    PartiallyCompleted,
    Cancelled,
    Rejected,
    PendingCancellation
}
```

```csharp
// ServiceOrderItemState.cs
namespace Obss.Provisioning.Domain.Entities;

public enum ServiceOrderItemState
{
    Acknowledged,
    InProgress,
    Completed,
    Failed,
    Held,
    Cancelled
}
```

```csharp
// ServiceOrderAction.cs
namespace Obss.Provisioning.Domain.Entities;

public enum ServiceOrderAction
{
    Add,
    Modify,
    Delete
}
```

- [ ] **Step 2: Create value objects/entities**

```csharp
// CancelServiceOrder.cs
namespace Obss.Provisioning.Domain.Entities;

public record CancelServiceOrder
{
    public CancelServiceOrder(Guid id, string reason)
    {
        Id = id;
        Reason = reason;
        State = "Pending";
    }

    public Guid Id { get; init; }
    public string Reason { get; init; }
    public DateTime? CompletedDate { get; init; }
    public string State { get; init; }
}
```

```csharp
// ServiceOrderRelatedParty.cs
using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Entities;

public class ServiceOrderRelatedParty : Entity<Guid>
{
    public ServiceOrderRelatedParty(Guid id, string? name, string? role, string? partyId)
        : base(id)
    {
        Name = name;
        Role = role;
        PartyId = partyId;
    }

    private ServiceOrderRelatedParty() { }

    public string? Name { get; private set; }
    public string? Role { get; private set; }
    public string? PartyId { get; private set; }
}
```

```csharp
// ServiceOrderCharacteristic.cs
using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Entities;

public class ServiceOrderCharacteristic : Entity<Guid>
{
    public ServiceOrderCharacteristic(Guid id, string key, string value)
        : base(id)
    {
        Key = key;
        Value = value;
    }

    private ServiceOrderCharacteristic() { }

    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
}
```

```csharp
// ServiceOrderMilestone.cs
using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Entities;

public class ServiceOrderMilestone : Entity<Guid>
{
    public ServiceOrderMilestone(
        Guid id, string name, string? description,
        DateTime date, MilestoneStatus status)
        : base(id)
    {
        Name = name;
        Description = description;
        Date = date;
        Status = status;
    }

    private ServiceOrderMilestone() { }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime Date { get; private set; }
    public MilestoneStatus Status { get; private set; }
}

public enum MilestoneStatus
{
    Pending,
    Reached,
    Missed,
    Cancelled
}
```

```csharp
// ServiceOrderNote.cs
using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Entities;

public class ServiceOrderNote : Entity<Guid>
{
    public ServiceOrderNote(Guid id, string text, string? author)
        : base(id)
    {
        Text = text;
        Author = author;
        CreatedAt = DateTime.UtcNow;
    }

    private ServiceOrderNote() { }

    public string Text { get; private set; } = string.Empty;
    public string? Author { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
```

- [ ] **Step 3: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Domain/Entities/ServiceOrderState.cs src/Modules/Provisioning/Obss.Provisioning.Domain/Entities/ServiceOrderItemState.cs src/Modules/Provisioning/Obss.Provisioning.Domain/Entities/ServiceOrderAction.cs src/Modules/Provisioning/Obss.Provisioning.Domain/Entities/CancelServiceOrder.cs src/Modules/Provisioning/Obss.Provisioning.Domain/Entities/ServiceOrderRelatedParty.cs src/Modules/Provisioning/Obss.Provisioning.Domain/Entities/ServiceOrderCharacteristic.cs src/Modules/Provisioning/Obss.Provisioning.Domain/Entities/ServiceOrderMilestone.cs src/Modules/Provisioning/Obss.Provisioning.Domain/Entities/ServiceOrderNote.cs
git commit -m "feat: add ServiceOrder value objects and enums"
```

---

### Task 3: Domain — Domain events

**Files:**
- Create: `src/Modules/Provisioning/Obss.Provisioning.Domain/Events/ServiceOrderSubmittedDomainEvent.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Domain/Events/ServiceOrderStateChangedDomainEvent.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Domain/Events/ServiceOrderItemCompletedDomainEvent.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Domain/Events/ServiceOrderCancellationRequestedDomainEvent.cs`

- [ ] **Step 1: Create domain events**

```csharp
// ServiceOrderSubmittedDomainEvent.cs
using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Events;

public sealed class ServiceOrderSubmittedDomainEvent : DomainEvent
{
    public ServiceOrderSubmittedDomainEvent(Guid serviceOrderId, Guid tenantId, string? externalId)
    {
        ServiceOrderId = serviceOrderId;
        TenantId = tenantId;
        ExternalId = externalId;
    }

    public Guid ServiceOrderId { get; }
    public Guid TenantId { get; }
    public string? ExternalId { get; }
}
```

```csharp
// ServiceOrderStateChangedDomainEvent.cs
using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Events;

public sealed class ServiceOrderStateChangedDomainEvent : DomainEvent
{
    public ServiceOrderStateChangedDomainEvent(Guid serviceOrderId, string? externalId, string newState)
    {
        ServiceOrderId = serviceOrderId;
        ExternalId = externalId;
        NewState = newState;
    }

    public Guid ServiceOrderId { get; }
    public string? ExternalId { get; }
    public string NewState { get; }
}
```

```csharp
// ServiceOrderItemCompletedDomainEvent.cs
using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Events;

public sealed class ServiceOrderItemCompletedDomainEvent : DomainEvent
{
    public ServiceOrderItemCompletedDomainEvent(Guid serviceOrderId, Guid itemId, Guid? serviceId)
    {
        ServiceOrderId = serviceOrderId;
        ItemId = itemId;
        ServiceId = serviceId;
    }

    public Guid ServiceOrderId { get; }
    public Guid ItemId { get; }
    public Guid? ServiceId { get; }
}
```

```csharp
// ServiceOrderCancellationRequestedDomainEvent.cs
using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Events;

public sealed class ServiceOrderCancellationRequestedDomainEvent : DomainEvent
{
    public ServiceOrderCancellationRequestedDomainEvent(Guid serviceOrderId, string? externalId, string reason)
    {
        ServiceOrderId = serviceOrderId;
        ExternalId = externalId;
        Reason = reason;
    }

    public Guid ServiceOrderId { get; }
    public string? ExternalId { get; }
    public string Reason { get; }
}
```

- [ ] **Step 2: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Domain/Events/ServiceOrder*.cs
git commit -m "feat: add ServiceOrder domain events"
```

---

### Task 4: Application — Abstractions and DTOs

**Files:**
- Create: `src/Modules/Provisioning/Obss.Provisioning.Application/Abstractions/IServiceOrderRepository.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Application/DTOs/ServiceOrderDto.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Application/DTOs/ServiceOrderItemDto.cs`

- [ ] **Step 1: Create repository interface**

```csharp
// IServiceOrderRepository.cs
using Obss.Provisioning.Domain.Entities;
using Obss.SharedKernel.Domain.Specifications;

namespace Obss.Provisioning.Application.Abstractions;

public interface IServiceOrderRepository
{
    Task<ServiceOrder?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ServiceOrder>> GetListAsync(Specification<ServiceOrder>? spec = null, CancellationToken ct = default);
    Task AddAsync(ServiceOrder order, CancellationToken ct = default);
    Task UpdateAsync(ServiceOrder order, CancellationToken ct = default);
    Task DeleteAsync(ServiceOrder order, CancellationToken ct = default);
    Task<int> CountAsync(Specification<ServiceOrder>? spec = null, CancellationToken ct = default);
}
```

- [ ] **Step 2: Create DTOs**

```csharp
// ServiceOrderDto.cs
namespace Obss.Provisioning.Application.DTOs;

public sealed record ServiceOrderDto(
    Guid Id,
    string State,
    string? ExternalId,
    string? Priority,
    string? Description,
    string? Category,
    DateTime? RequestedStartDate,
    DateTime? RequestedCompletionDate,
    DateTime OrderDate,
    DateTime? StatusChangeDate,
    string? CompletionMessage,
    List<ServiceOrderItemDto> Items,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record ServiceOrderItemDto(
    Guid Id,
    Guid ServiceOrderId,
    Guid? ServiceId,
    string Action,
    int Quantity,
    string? Description,
    string State,
    DateTime? RequestedStartDate,
    DateTime? RequestedCompletionDate,
    DateTime? CompletedDate,
    string? ErrorMessage);
```

- [ ] **Step 3: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Application/Abstractions/IServiceOrderRepository.cs src/Modules/Provisioning/Obss.Provisioning.Application/DTOs/ServiceOrder*.cs
git commit -m "feat: add IServiceOrderRepository and DTOs"
```

---

### Task 5: Application — CreateServiceOrder command

**Files:**
- Create: `src/Modules/Provisioning/Obss.Provisioning.Application/Commands/CreateServiceOrder/CreateServiceOrderCommand.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Application/Commands/CreateServiceOrder/CreateServiceOrderCommandHandler.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Application/Commands/CreateServiceOrder/CreateServiceOrderCommandValidator.cs`

- [ ] **Step 1: Create command**

```csharp
// CreateServiceOrderCommand.cs
using MediatR;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Commands.CreateServiceOrder;

public sealed record CreateServiceOrderCommand(
    Guid TenantId,
    string? ExternalId,
    string? Description,
    string? Category,
    string? Priority,
    List<CreateServiceOrderItemRequest> Items) : IRequest<Result<ServiceOrderDto>>;

public sealed record CreateServiceOrderItemRequest(
    Guid? ServiceId,
    string Action,
    int Quantity,
    string? Description,
    DateTime? RequestedStartDate,
    DateTime? RequestedCompletionDate);
```

- [ ] **Step 2: Create handler**

```csharp
// CreateServiceOrderCommandHandler.cs
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Application.DTOs;
using Obss.Provisioning.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Provisioning.Application.Commands.CreateServiceOrder;

public sealed class CreateServiceOrderCommandHandler : IRequestHandler<CreateServiceOrderCommand, Result<ServiceOrderDto>>
{
    private readonly IServiceOrderRepository _repository;
    private readonly IProvisioningJobRepository _jobRepository;
    private readonly IProvisioningTemplateRepository _templateRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateServiceOrderCommandHandler> _logger;

    public CreateServiceOrderCommandHandler(
        IServiceOrderRepository repository,
        IProvisioningJobRepository jobRepository,
        IProvisioningTemplateRepository templateRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateServiceOrderCommandHandler> logger)
    {
        _repository = repository;
        _jobRepository = jobRepository;
        _templateRepository = templateRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ServiceOrderDto>> Handle(CreateServiceOrderCommand request, CancellationToken ct)
    {
        var order = ServiceOrder.Create(
            request.TenantId,
            request.ExternalId,
            request.Description,
            request.Category,
            request.Priority);

        foreach (var itemRequest in request.Items)
        {
            if (!Enum.TryParse<ServiceOrderAction>(itemRequest.Action, out var action))
                return Result.Failure<ServiceOrderDto>(Error.Validation($"Invalid action: '{itemRequest.Action}'"));

            var item = order.AddItem(
                itemRequest.ServiceId,
                action,
                itemRequest.Quantity,
                itemRequest.Description,
                itemRequest.RequestedStartDate,
                itemRequest.RequestedCompletionDate);
        }

        await _repository.AddAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Created ServiceOrder {Id} with {ItemCount} items", order.Id, order.Items.Count);

        return Result.Success(order.Adapt<ServiceOrderDto>());
    }
}
```

- [ ] **Step 3: Create validator**

```csharp
// CreateServiceOrderCommandValidator.cs
using FluentValidation;

namespace Obss.Provisioning.Application.Commands.CreateServiceOrder;

public sealed class CreateServiceOrderCommandValidator : AbstractValidator<CreateServiceOrderCommand>
{
    public CreateServiceOrderCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item is required.");

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.Quantity)
                    .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
            });
    }
}
```

- [ ] **Step 4: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Application/Commands/CreateServiceOrder/
git commit -m "feat: add CreateServiceOrder command"
```

---

### Task 6: Application — Update/Cancel/CompleteItem commands

**Files:**
- Create: `src/Modules/Provisioning/Obss.Provisioning.Application/Commands/UpdateServiceOrder/UpdateServiceOrderCommand.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Application/Commands/UpdateServiceOrder/UpdateServiceOrderCommandHandler.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Application/Commands/CancelServiceOrder/CancelServiceOrderCommand.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Application/Commands/CancelServiceOrder/CancelServiceOrderCommandHandler.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Application/Commands/CompleteServiceOrderItem/CompleteServiceOrderItemCommand.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Application/Commands/CompleteServiceOrderItem/CompleteServiceOrderItemCommandHandler.cs`

- [ ] **Step 1: Update command**

```csharp
// UpdateServiceOrderCommand.cs
using MediatR;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Commands.UpdateServiceOrder;

public sealed record UpdateServiceOrderCommand(
    Guid Id,
    string? Description,
    string? Category,
    string? Priority,
    DateTime? RequestedStartDate,
    DateTime? RequestedCompletionDate) : IRequest<Result<ServiceOrderDto>>;
```

```csharp
// UpdateServiceOrderCommandHandler.cs
using Mapster;
using MediatR;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Commands.UpdateServiceOrder;

public sealed class UpdateServiceOrderCommandHandler : IRequestHandler<UpdateServiceOrderCommand, Result<ServiceOrderDto>>
{
    private readonly IServiceOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateServiceOrderCommandHandler(IServiceOrderRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ServiceOrderDto>> Handle(UpdateServiceOrderCommand request, CancellationToken ct)
    {
        var order = await _repository.GetByIdAsync(request.Id, ct);
        if (order is null)
            return Result.Failure<ServiceOrderDto>(Error.NotFound("ServiceOrder not found"));

        order.UpdateDetails(
            request.Description, request.Category, request.Priority,
            request.RequestedStartDate, request.RequestedCompletionDate);

        await _repository.UpdateAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(order.Adapt<ServiceOrderDto>());
    }
}
```

- [ ] **Step 2: Cancel command**

```csharp
// CancelServiceOrderCommand.cs
using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Commands.CancelServiceOrder;

public sealed record CancelServiceOrderCommand(Guid Id, string Reason) : IRequest<Result>; // NOTE: Result is non-generic
```

```csharp
// CancelServiceOrderCommandHandler.cs
using MediatR;
using Obss.Provisioning.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Commands.CancelServiceOrder;

public sealed class CancelServiceOrderCommandHandler : IRequestHandler<CancelServiceOrderCommand, Result>
{
    private readonly IServiceOrderRepository _repository;
    private readonly IProvisioningJobRepository _jobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelServiceOrderCommandHandler(
        IServiceOrderRepository repository,
        IProvisioningJobRepository jobRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _jobRepository = jobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CancelServiceOrderCommand request, CancellationToken ct)
    {
        var order = await _repository.GetByIdAsync(request.Id, ct);
        if (order is null)
            return Result.Failure(Error.NotFound("ServiceOrder not found"));

        order.RequestCancellation(request.Reason);

        await _repository.UpdateAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
```

- [ ] **Step 3: CompleteItem command**

```csharp
// CompleteServiceOrderItemCommand.cs
using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Commands.CompleteServiceOrderItem;

public sealed record CompleteServiceOrderItemCommand(
    Guid ServiceOrderId,
    Guid ItemId,
    Guid? ServiceId) : IRequest<Result>;
```

```csharp
// CompleteServiceOrderItemCommandHandler.cs
using MediatR;
using Obss.Provisioning.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Commands.CompleteServiceOrderItem;

public sealed class CompleteServiceOrderItemCommandHandler : IRequestHandler<CompleteServiceOrderItemCommand, Result>
{
    private readonly IServiceOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteServiceOrderItemCommandHandler(IServiceOrderRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CompleteServiceOrderItemCommand request, CancellationToken ct)
    {
        var order = await _repository.GetByIdAsync(request.ServiceOrderId, ct);
        if (order is null)
            return Result.Failure(Error.NotFound("ServiceOrder not found"));

        order.CompleteItem(request.ItemId, request.ServiceId);
        order.Complete();

        await _repository.UpdateAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
```

- [ ] **Step 4: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Application/Commands/UpdateServiceOrder/ src/Modules/Provisioning/Obss.Provisioning.Application/Commands/CancelServiceOrder/ src/Modules/Provisioning/Obss.Provisioning.Application/Commands/CompleteServiceOrderItem/
git commit -m "feat: add Update/Cancel/CompleteItem commands for ServiceOrder"
```

---

### Task 7: Application — Queries

**Files:**
- Create: `src/Modules/Provisioning/Obss.Provisioning.Application/Queries/GetServiceOrderById/GetServiceOrderByIdQuery.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Application/Queries/GetServiceOrderById/GetServiceOrderByIdQueryHandler.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Application/Queries/GetServiceOrders/GetServiceOrdersQuery.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Application/Queries/GetServiceOrders/GetServiceOrdersQueryHandler.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Application/Queries/GetServiceOrderItems/GetServiceOrderItemsQuery.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Application/Queries/GetServiceOrderItems/GetServiceOrderItemsQueryHandler.cs`

- [ ] **Step 1: Get by ID query**

```csharp
// GetServiceOrderByIdQuery.cs
using MediatR;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Queries.GetServiceOrderById;

public sealed record GetServiceOrderByIdQuery(Guid Id) : IRequest<Result<ServiceOrderDto>>;
```

```csharp
// GetServiceOrderByIdQueryHandler.cs
using Mapster;
using MediatR;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Queries.GetServiceOrderById;

public sealed class GetServiceOrderByIdQueryHandler : IRequestHandler<GetServiceOrderByIdQuery, Result<ServiceOrderDto>>
{
    private readonly IServiceOrderRepository _repository;

    public GetServiceOrderByIdQueryHandler(IServiceOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ServiceOrderDto>> Handle(GetServiceOrderByIdQuery request, CancellationToken ct)
    {
        var order = await _repository.GetByIdAsync(request.Id, ct);
        if (order is null)
            return Result.Failure<ServiceOrderDto>(Error.NotFound("ServiceOrder not found"));

        return Result.Success(order.Adapt<ServiceOrderDto>());
    }
}
```

- [ ] **Step 2: List query**

```csharp
// GetServiceOrdersQuery.cs
using MediatR;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Queries.GetServiceOrders;

public sealed record GetServiceOrdersQuery(
    string? State,
    string? ExternalId,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PaginatedResult<ServiceOrderDto>>>;

public sealed record PaginatedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);
```

```csharp
// GetServiceOrdersQueryHandler.cs
using Mapster;
using MediatR;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Domain.Specifications;

namespace Obss.Provisioning.Application.Queries.GetServiceOrders;

public sealed class GetServiceOrdersQueryHandler : IRequestHandler<GetServiceOrdersQuery, Result<PaginatedResult<ServiceOrderDto>>>
{
    private readonly IServiceOrderRepository _repository;

    public GetServiceOrdersQueryHandler(IServiceOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PaginatedResult<ServiceOrderDto>>> Handle(GetServiceOrdersQuery request, CancellationToken ct)
    {
        var spec = new ServiceOrderSpecification(request.State, request.ExternalId);
        var items = await _repository.GetListAsync(spec, ct);
        var total = await _repository.CountAsync(spec, ct);

        var paged = items
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Adapt<List<ServiceOrderDto>>();

        return Result.Success(new PaginatedResult<ServiceOrderDto>(paged, total, request.Page, request.PageSize));
    }
}

internal sealed class ServiceOrderSpecification : Specification<ServiceOrder>
{
    public ServiceOrderSpecification(string? state, string? externalId)
    {
        if (!string.IsNullOrWhiteSpace(state))
            AddFilter(o => o.State.ToString() == state);
        if (!string.IsNullOrWhiteSpace(externalId))
            AddFilter(o => o.ExternalId == externalId);
    }
}
```

- [ ] **Step 3: Items query**

```csharp
// GetServiceOrderItemsQuery.cs
using MediatR;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Queries.GetServiceOrderItems;

public sealed record GetServiceOrderItemsQuery(Guid ServiceOrderId) : IRequest<Result<List<ServiceOrderItemDto>>>;
```

```csharp
// GetServiceOrderItemsQueryHandler.cs
using Mapster;
using MediatR;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Queries.GetServiceOrderItems;

public sealed class GetServiceOrderItemsQueryHandler : IRequestHandler<GetServiceOrderItemsQuery, Result<List<ServiceOrderItemDto>>>
{
    private readonly IServiceOrderRepository _repository;

    public GetServiceOrderItemsQueryHandler(IServiceOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ServiceOrderItemDto>>> Handle(GetServiceOrderItemsQuery request, CancellationToken ct)
    {
        var order = await _repository.GetByIdAsync(request.ServiceOrderId, ct);
        if (order is null)
            return Result.Failure<List<ServiceOrderItemDto>>(Error.NotFound("ServiceOrder not found"));

        return Result.Success(order.Items.Adapt<List<ServiceOrderItemDto>>());
    }
}
```

- [ ] **Step 4: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Application/Queries/GetServiceOrderById/ src/Modules/Provisioning/Obss.Provisioning.Application/Queries/GetServiceOrders/ src/Modules/Provisioning/Obss.Provisioning.Application/Queries/GetServiceOrderItems/
git commit -m "feat: add ServiceOrder queries"
```

---

### Task 8: Infrastructure — EF Core configuration and repository

**Files:**
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Persistence/Configurations/ServiceOrderConfiguration.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Persistence/Configurations/ServiceOrderItemConfiguration.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Persistence/Repositories/ServiceOrderRepository.cs`

- [ ] **Step 1: EF configuration for ServiceOrder**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Provisioning.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Provisioning.Infrastructure.Persistence.Configurations;

public sealed class ServiceOrderConfiguration : IEntityTypeConfiguration<ServiceOrder>
{
    public void Configure(EntityTypeBuilder<ServiceOrder> builder)
    {
        builder.ToTable("service_orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .ValueGeneratedNever();

        builder.Property(o => o.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion<GuidToStringConverter>()
            .HasMaxLength(36)
            .IsRequired();

        builder.Property(o => o.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(100);

        builder.Property(o => o.State)
            .HasColumnName("state")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(o => o.Priority)
            .HasColumnName("priority")
            .HasMaxLength(50);

        builder.Property(o => o.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(o => o.Category)
            .HasColumnName("category")
            .HasMaxLength(100);

        builder.Property(o => o.RequestedStartDate)
            .HasColumnName("requested_start_date");

        builder.Property(o => o.RequestedCompletionDate)
            .HasColumnName("requested_completion_date");

        builder.Property(o => o.OrderDate)
            .HasColumnName("order_date")
            .IsRequired();

        builder.Property(o => o.StatusChangeDate)
            .HasColumnName("status_change_date");

        builder.Property(o => o.CompletionMessage)
            .HasColumnName("completion_message")
            .HasMaxLength(2000);

        builder.Property(o => o.Href)
            .HasColumnName("href")
            .HasMaxLength(500);

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.ServiceOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.RelatedParties)
            .WithOne()
            .HasForeignKey("ServiceOrderId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Characteristics)
            .WithOne()
            .HasForeignKey("ServiceOrderId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Milestones)
            .WithOne()
            .HasForeignKey("ServiceOrderId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Notes)
            .WithOne()
            .HasForeignKey("ServiceOrderId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(o => o.CancelRequest, cancel =>
        {
            cancel.WithOwner();
            cancel.Property(c => c.Id).HasColumnName("cancel_id");
            cancel.Property(c => c.Reason).HasColumnName("cancel_reason").HasMaxLength(1000);
            cancel.Property(c => c.CompletedDate).HasColumnName("cancel_completed_date");
            cancel.Property(c => c.State).HasColumnName("cancel_state").HasMaxLength(50);
        });

        builder.HasIndex(o => o.TenantId)
            .HasDatabaseName("ix_service_orders_tenant_id");

        builder.HasIndex(o => o.State)
            .HasDatabaseName("ix_service_orders_state");

        builder.HasIndex(o => o.ExternalId)
            .HasDatabaseName("ix_service_orders_external_id");

        builder.Navigation(o => o.Items)
            .AutoInclude();
    }
}
```

- [ ] **Step 2: EF configuration for ServiceOrderItem**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Provisioning.Domain.Entities;

namespace Obss.Provisioning.Infrastructure.Persistence.Configurations;

public sealed class ServiceOrderItemConfiguration : IEntityTypeConfiguration<ServiceOrderItem>
{
    public void Configure(EntityTypeBuilder<ServiceOrderItem> builder)
    {
        builder.ToTable("service_order_items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(i => i.ServiceOrderId)
            .HasColumnName("service_order_id")
            .IsRequired();

        builder.Property(i => i.ServiceId)
            .HasColumnName("service_id");

        builder.Property(i => i.Action)
            .HasColumnName("action")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(i => i.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(i => i.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(i => i.State)
            .HasColumnName("state")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(i => i.RequestedStartDate)
            .HasColumnName("requested_start_date");

        builder.Property(i => i.RequestedCompletionDate)
            .HasColumnName("requested_completion_date");

        builder.Property(i => i.CompletedDate)
            .HasColumnName("completed_date");

        builder.Property(i => i.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(2000);

        builder.HasIndex(i => i.ServiceOrderId)
            .HasDatabaseName("ix_service_order_items_service_order_id");
    }
}
```

- [ ] **Step 3: Repository implementation**

```csharp
using Microsoft.EntityFrameworkCore;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Infrastructure.Persistence;
using Obss.SharedKernel.Domain.Specifications;

namespace Obss.Provisioning.Infrastructure.Persistence.Repositories;

public sealed class ServiceOrderRepository : IServiceOrderRepository
{
    private readonly ProvisioningDbContext _context;

    public ServiceOrderRepository(ProvisioningDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceOrder?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<ServiceOrder>()
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<List<ServiceOrder>> GetListAsync(Specification<ServiceOrder>? spec = null, CancellationToken ct = default)
    {
        var query = _context.Set<ServiceOrder>().AsQueryable();
        if (spec is not null)
            query = query.Where(spec.ToExpression());
        return await query.ToListAsync(ct);
    }

    public async Task AddAsync(ServiceOrder order, CancellationToken ct = default)
    {
        await _context.Set<ServiceOrder>().AddAsync(order, ct);
    }

    public Task UpdateAsync(ServiceOrder order, CancellationToken ct = default)
    {
        _context.Set<ServiceOrder>().Update(order);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ServiceOrder order, CancellationToken ct = default)
    {
        _context.Set<ServiceOrder>().Remove(order);
        return Task.CompletedTask;
    }

    public async Task<int> CountAsync(Specification<ServiceOrder>? spec = null, CancellationToken ct = default)
    {
        var query = _context.Set<ServiceOrder>().AsQueryable();
        if (spec is not null)
            query = query.Where(spec.ToExpression());
        return await query.CountAsync(ct);
    }
}
```

- [ ] **Step 4: Register DbSet in ProvisioningDbContext**

```csharp
// Add to ProvisioningDbContext.cs
public DbSet<ServiceOrder> ServiceOrders => Set<ServiceOrder>();
```

- [ ] **Step 5: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Persistence/Configurations/ServiceOrder*.cs src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Persistence/Repositories/ServiceOrderRepository.cs
git commit -m "feat: add ServiceOrder EF config and repository"
```

---

### Task 9: API — ServiceOrder endpoints

**Files:**
- Modify: `src/Modules/Provisioning/Obss.Provisioning.Api/Endpoints/ProvisioningEndpoints.cs`
- Modify: `src/Modules/Provisioning/Obss.Provisioning.Api/Extensions/ProvisioningModuleRegistration.cs`

- [ ] **Step 1: Add ServiceOrder endpoints**

Add to `ProvisioningEndpoints.cs`:

```csharp
// ServiceOrder endpoints
group.MapPost("/service-orders", async (CreateServiceOrderCommand command, IMediator mediator) =>
{
    var result = await mediator.Send(command);
    return result.IsSuccess
        ? (IResult)TypedResults.Created($"/api/v1/provisioning/service-orders/{result.Value.Id}", result.Value)
        : (IResult)TypedResults.BadRequest(result.Error);
});

group.MapGet("/service-orders/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var result = await mediator.Send(new GetServiceOrderByIdQuery(id));
    return result.IsSuccess
        ? (IResult)TypedResults.Ok(result.Value)
        : (IResult)TypedResults.NotFound(result.Error);
});

group.MapGet("/service-orders", async ([AsParameters] GetServiceOrdersQuery query, IMediator mediator) =>
{
    var result = await mediator.Send(query);
    return result.IsSuccess
        ? (IResult)TypedResults.Ok(result.Value)
        : (IResult)TypedResults.BadRequest(result.Error);
});

group.MapPatch("/service-orders/{id:guid}", async (Guid id, UpdateServiceOrderCommand command, IMediator mediator) =>
{
    if (id != command.Id)
        return (IResult)TypedResults.BadRequest();
    var result = await mediator.Send(command);
    return result.IsSuccess
        ? (IResult)TypedResults.Ok(result.Value)
        : (IResult)TypedResults.BadRequest(result.Error);
});

group.MapDelete("/service-orders/{id:guid}", async (Guid id, IServiceOrderRepository repository, IUnitOfWork unitOfWork) =>
{
    var order = await repository.GetByIdAsync(id);
    if (order is null)
        return (IResult)TypedResults.NotFound();
    if (order.State != ServiceOrderState.Acknowledged)
        return (IResult)TypedResults.BadRequest("Can only delete orders in Acknowledged state.");
    await repository.DeleteAsync(order);
    await unitOfWork.SaveChangesAsync();
    return (IResult)TypedResults.NoContent();
});

group.MapPost("/service-orders/{id:guid}/cancel", async (Guid id, CancelServiceOrderCommand command, IMediator mediator) =>
{
    if (id != command.Id)
        return (IResult)TypedResults.BadRequest();
    var result = await mediator.Send(command);
    return result.IsSuccess
        ? (IResult)TypedResults.NoContent()
        : (IResult)TypedResults.BadRequest(result.Error);
});

group.MapGet("/service-orders/{id:guid}/items", async (Guid id, IMediator mediator) =>
{
    var result = await mediator.Send(new GetServiceOrderItemsQuery(id));
    return result.IsSuccess
        ? (IResult)TypedResults.Ok(result.Value)
        : (IResult)TypedResults.NotFound(result.Error);
});

group.MapGet("/service-orders/{id:guid}/items/{itemId:guid}", async (Guid id, Guid itemId, IServiceOrderRepository repository) =>
{
    var order = await repository.GetByIdAsync(id);
    if (order is null)
        return (IResult)TypedResults.NotFound();
    var item = order.Items.FirstOrDefault(i => i.Id == itemId);
    return item is not null
        ? (IResult)TypedResults.Ok(item.Adapt<ServiceOrderItemDto>())
        : (IResult)TypedResults.NotFound();
});
```

- [ ] **Step 2: Register repository in DI**

Add to `ProvisioningModuleRegistration.cs`:

```csharp
services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
```

And add using for the repository namespace.

- [ ] **Step 3: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Api/Endpoints/ProvisioningEndpoints.cs src/Modules/Provisioning/Obss.Provisioning.Api/Extensions/ProvisioningModuleRegistration.cs
git commit -m "feat: add ServiceOrder API endpoints"
```

---

### Task 10: Integration events and handler

**Files:**
- Modify: `src/Modules/Provisioning/Obss.Provisioning.Application/IntegrationEventHandlers/OrderApprovedIntegrationEventHandler.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Application/IntegrationEvents/ServiceOrderSubmittedIntegrationEvent.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Application/IntegrationEvents/ServiceOrderStateChangedIntegrationEvent.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Application/IntegrationEvents/ServiceOrderItemCompletedIntegrationEvent.cs`

- [ ] **Step 1: Create outbound integration events**

```csharp
// ServiceOrderSubmittedIntegrationEvent.cs
using Obss.SharedKernel.Domain.Events;

namespace Obss.Provisioning.Application.IntegrationEvents;

public sealed class ServiceOrderSubmittedIntegrationEvent : IntegrationEvent
{
    public ServiceOrderSubmittedIntegrationEvent(
        Guid serviceOrderId,
        Guid tenantId,
        string? externalId)
    {
        ServiceOrderId = serviceOrderId;
        TenantId = tenantId;
        ExternalId = externalId;
    }

    public Guid ServiceOrderId { get; }
    public Guid TenantId { get; }
    public string? ExternalId { get; }
}
```

```csharp
// ServiceOrderStateChangedIntegrationEvent.cs
using Obss.SharedKernel.Domain.Events;

namespace Obss.Provisioning.Application.IntegrationEvents;

public sealed class ServiceOrderStateChangedIntegrationEvent : IntegrationEvent
{
    public ServiceOrderStateChangedIntegrationEvent(
        Guid serviceOrderId,
        string? externalId,
        string newState)
    {
        ServiceOrderId = serviceOrderId;
        ExternalId = externalId;
        NewState = newState;
    }

    public Guid ServiceOrderId { get; }
    public string? ExternalId { get; }
    public string NewState { get; }
}
```

```csharp
// ServiceOrderItemCompletedIntegrationEvent.cs
using Obss.SharedKernel.Domain.Events;

namespace Obss.Provisioning.Application.IntegrationEvents;

public sealed class ServiceOrderItemCompletedIntegrationEvent : IntegrationEvent
{
    public ServiceOrderItemCompletedIntegrationEvent(
        Guid serviceOrderId,
        Guid itemId,
        Guid? serviceId,
        string? externalId)
    {
        ServiceOrderId = serviceOrderId;
        ItemId = itemId;
        ServiceId = serviceId;
        ExternalId = externalId;
    }

    public Guid ServiceOrderId { get; }
    public Guid ItemId { get; }
    public Guid? ServiceId { get; }
    public string? ExternalId { get; }
}
```

- [ ] **Step 2: Migrate OrderApprovedIntegrationEventHandler to use ServiceOrder**

Modify the existing handler (`OrderApprovedIntegrationEventHandler.cs` or rename/retarget it from `ProvisioningRequiredIntegrationEventHandler.cs`) to create a ServiceOrder with items instead of directly creating a ProvisioningJob:

```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Orders.Application.IntegrationEvents;
using Obss.Provisioning.Application.Commands.CreateServiceOrder;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Provisioning.Application.IntegrationEventHandlers;

public sealed class ProvisioningRequiredIntegrationEventHandler : INotificationHandler<ProvisioningRequiredIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<ProvisioningRequiredIntegrationEventHandler> _logger;

    public ProvisioningRequiredIntegrationEventHandler(
        IMediator mediator,
        ICurrentTenant currentTenant,
        ILogger<ProvisioningRequiredIntegrationEventHandler> logger)
    {
        _mediator = mediator;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task Handle(ProvisioningRequiredIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId ?? string.Empty;
        var parsedTenantId = Guid.TryParse(tenantId, out var tid) ? tid : Guid.Empty;

        _logger.LogInformation(
            "Creating ServiceOrder for order {OrderId}, item {OrderItemId}",
            notification.OrderId,
            notification.OrderItemId);

        var command = new CreateServiceOrderCommand(
            parsedTenantId,
            notification.OrderId.ToString(),
            $"Provisioning for order {notification.OrderId}",
            null,
            null,
        [
            new CreateServiceOrderItemRequest(
                null,
                notification.Action,
                1,
                $"Service: {notification.ServiceType}",
                null,
                null)
        ]);

        await _mediator.Send(command, cancellationToken);
    }
}
```

- [ ] **Step 3: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Application/IntegrationEvents/ServiceOrder*.cs
git commit -m "feat: add ServiceOrder integration events and update inbound handler"
```

---

### Task 11: Migration

- [ ] **Step 1: Generate EF migration**

```bash
~/.dotnet/tools/dotnet-ef migrations add AddServiceOrderTables -p src/Modules/Provisioning/Obss.Provisioning.Infrastructure -s src/Host/Obss.Host --context ProvisioningDbContext
```

Expected: Migration generated with new tables `service_orders` and `service_order_items`.

- [ ] **Step 2: Build and verify**

```bash
dotnet build Obss.sln --configuration Release
```

Expected: 0 errors, 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Persistence/Migrations/
git commit -m "feat: add ServiceOrder database migration"
```

---

### Self-Review

**Spec coverage:**
- Task 1-3: ServiceOrder aggregate, ServiceOrderItem entity, value objects, enums, domain events ✅
- Task 4-7: Repository interface, DTOs, Create/Update/Cancel/CompleteItem commands, queries ✅
- Task 8: EF Core configuration and repository implementation ✅
- Task 9: API endpoints (POST/GET/PATCH/DELETE + cancel + items) ✅
- Task 10: Outbound integration events, inbound handler update ✅
- Task 11: EF migration ✅

**Missing from spec:** CI test setup for state machine. Adding frontend pages is explicitly out of scope.

**Placeholder scan:** No TBD, TODO, or incomplete sections. All code is concrete.

**Type consistency:** ServiceOrderAction, ServiceOrderState, ServiceOrderItemState match across all tasks. Result non-generic (for CancelServiceOrder) and generic variants are consistent with existing patterns.
