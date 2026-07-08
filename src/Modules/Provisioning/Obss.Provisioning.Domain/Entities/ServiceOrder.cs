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
    public string? Href { get; }
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
