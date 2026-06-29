using Obss.SharedKernel.Domain.Common;
using Obss.Ticketing.Domain.Events;
using Obss.Ticketing.Domain.Exceptions;
using Obss.Ticketing.Domain.ValueObjects;

namespace Obss.Ticketing.Domain.Entities;

public class Ticket : AggregateRoot<Guid>
{
    private readonly List<TicketComment> _comments = [];
    private readonly List<TicketAttachment> _attachments = [];

    private Ticket() { }

    private Ticket(
        Guid id,
        string tenantId,
        string ticketNumber,
        Guid customerId,
        string customerName,
        string subject,
        string description,
        TicketPriority priority,
        TicketCategory category,
        TicketSource source)
        : base(id)
    {
        TenantId = tenantId;
        TicketNumber = ticketNumber;
        CustomerId = customerId;
        CustomerName = customerName;
        Subject = subject;
        Description = description;
        Priority = priority;
        Category = category;
        Source = source;
        Status = TicketStatus.Open;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TicketCreatedDomainEvent(id, ticketNumber, tenantId, customerId, subject, priority.ToString()));
    }

    public string TenantId { get; private set; } = string.Empty;
    public string TicketNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TicketPriority Priority { get; private set; }
    public TicketCategory Category { get; private set; }
    public TicketStatus Status { get; private set; }
    public TicketSource Source { get; private set; }
    public string? AssignedTo { get; private set; }
    public string? AssignedGroup { get; private set; }
    public string? Resolution { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public DateTime? FirstResponseAt { get; private set; }
    public DateTime? SlaDeadline { get; private set; }
    public DateTime? SlaResponseDeadline { get; private set; }
    public DateTime? SlaBreachedAt { get; private set; }
    public Guid? SlaDefinitionId { get; private set; }

    public bool IsSlaBreached => SlaBreachedAt.HasValue;

    public IReadOnlyCollection<TicketComment> Comments => _comments.AsReadOnly();
    public IReadOnlyCollection<TicketAttachment> Attachments => _attachments.AsReadOnly();

    public static Ticket Create(
        string tenantId,
        string ticketNumber,
        Guid customerId,
        string customerName,
        string subject,
        string description,
        TicketPriority priority,
        TicketCategory category,
        TicketSource source)
    {
        return new Ticket(
            Guid.NewGuid(),
            tenantId,
            ticketNumber,
            customerId,
            customerName,
            subject,
            description,
            priority,
            category,
            source);
    }

    public void Assign(string userId, string? group = null)
    {
        if (Status == TicketStatus.Closed)
            throw new InvalidTicketStateException("Cannot assign a closed ticket.");

        if (Status == TicketStatus.Resolved)
            throw new InvalidTicketStateException("Cannot assign a resolved ticket.");

        AssignedTo = userId;
        AssignedGroup = group;
        UpdatedAt = DateTime.UtcNow;

        if (Status == TicketStatus.Open)
        {
            Status = TicketStatus.InProgress;
            FirstResponseAt ??= DateTime.UtcNow;
        }

        AddDomainEvent(new TicketAssignedDomainEvent(Id, TicketNumber, userId, userId));
    }

    public void ChangePriority(TicketPriority newPriority)
    {
        if (Status == TicketStatus.Closed)
            throw new InvalidTicketStateException("Cannot change priority of a closed ticket.");

        Priority = newPriority;
        UpdatedAt = DateTime.UtcNow;
    }

    public TicketComment AddComment(string content, bool isInternal, string authorId, string authorName)
    {
        var comment = new TicketComment(Guid.NewGuid(), Id, content, isInternal, authorId, authorName);
        _comments.Add(comment);

        if (Status == TicketStatus.Open && _comments.Count == 1)
        {
            FirstResponseAt ??= DateTime.UtcNow;
        }

        UpdatedAt = DateTime.UtcNow;
        return comment;
    }

    public void Resolve(string resolution)
    {
        if (Status == TicketStatus.Closed)
            throw new InvalidTicketStateException("Cannot resolve a closed ticket.");

        if (Status == TicketStatus.Resolved)
            throw new TicketAlreadyResolvedException(TicketNumber);

        Resolution = resolution;
        Status = TicketStatus.Resolved;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TicketResolvedDomainEvent(Id, TicketNumber, resolution, AssignedTo ?? "system"));
    }

    public void Close()
    {
        if (Status == TicketStatus.Closed)
            throw new TicketAlreadyClosedException(TicketNumber);

        if (Status != TicketStatus.Resolved)
            throw new TicketNotResolvedException(TicketNumber);

        Status = TicketStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reopen()
    {
        if (Status != TicketStatus.Resolved && Status != TicketStatus.Closed)
            return;

        Status = TicketStatus.Open;
        ClosedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public TicketAttachment AddAttachment(string fileName, string contentType, long fileSize, string storagePath, string uploadedById)
    {
        var attachment = new TicketAttachment(Guid.NewGuid(), Id, fileName, contentType, fileSize, storagePath, uploadedById);
        _attachments.Add(attachment);
        UpdatedAt = DateTime.UtcNow;
        return attachment;
    }

    public void Escalate(string escalatedBy, string reason)
    {
        if (Status == TicketStatus.Closed)
            throw new InvalidTicketStateException("Cannot escalate a closed ticket.");

        if (Status != TicketStatus.InProgress && Status != TicketStatus.WaitingThirdParty)
            Status = TicketStatus.InProgress;

        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TicketEscalatedDomainEvent(Id, TicketNumber, escalatedBy, reason));
    }

    public void SetWaitingCustomer()
    {
        if (Status != TicketStatus.InProgress) return;
        Status = TicketStatus.WaitingCustomer;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetWaitingThirdParty()
    {
        if (Status != TicketStatus.InProgress) return;
        Status = TicketStatus.WaitingThirdParty;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ApplySlaDefinition(SlaDefinition sla)
    {
        if (sla is null)
            throw new ArgumentNullException(nameof(sla));

        SlaDefinitionId = sla.Id;
        SlaResponseDeadline = CreatedAt.AddHours(sla.ResponseTimeHours);
        SlaDeadline = CreatedAt.AddHours(sla.ResolutionTimeHours);
        SlaBreachedAt = null;
    }

    public void MarkFirstResponse()
    {
        FirstResponseAt ??= DateTime.UtcNow;
    }

    public void CheckSlaBreach()
    {
        if (SlaDeadline.HasValue && !SlaBreachedAt.HasValue && DateTime.UtcNow > SlaDeadline.Value)
        {
            SlaBreachedAt = DateTime.UtcNow;
        }
    }
}
