using Obss.Invoices.Domain.Events;
using Obss.Invoices.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Invoices.Domain.Entities;

public class InvoiceDispute : AggregateRoot<Guid>
{
    private readonly List<DisputeAttachment> _attachments = [];

    private InvoiceDispute() { }

    private InvoiceDispute(
        Guid id,
        Guid invoiceId,
        Guid customerId,
        string reason,
        string description,
        decimal disputedAmount)
        : base(id)
    {
        InvoiceId = invoiceId;
        CustomerId = customerId;
        Reason = reason;
        Description = description;
        DisputedAmount = disputedAmount;
        Status = DisputeStatus.Open;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid InvoiceId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DisputeStatus Status { get; private set; }
    public decimal DisputedAmount { get; private set; }
    public string? Resolution { get; private set; }
    public Guid? ResolvedById { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<DisputeAttachment> Attachments => _attachments.AsReadOnly();

    public static InvoiceDispute Submit(Guid invoiceId, Guid customerId, string reason, string description, decimal disputedAmount)
    {
        var dispute = new InvoiceDispute(Guid.NewGuid(), invoiceId, customerId, reason, description, disputedAmount);
        dispute.AddDomainEvent(new InvoiceDisputeOpenedDomainEvent(dispute.Id, invoiceId, customerId, disputedAmount, reason));
        return dispute;
    }

    public void AcceptResolution(string resolution, Guid resolvedById)
    {
        if (Status != DisputeStatus.Open && Status != DisputeStatus.InReview)
            throw new InvalidOperationException($"Cannot accept resolution on dispute in '{Status}' state.");

        Status = DisputeStatus.Resolved;
        Resolution = resolution;
        ResolvedById = resolvedById;
        ResolvedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new InvoiceDisputeResolvedDomainEvent(Id, InvoiceId, resolution));
    }

    public void RejectResolution(string reason)
    {
        if (Status != DisputeStatus.Open && Status != DisputeStatus.InReview)
            throw new InvalidOperationException($"Cannot reject resolution on dispute in '{Status}' state.");

        Status = DisputeStatus.Rejected;
        Resolution = reason;
        ResolvedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddAttachment(string fileName, string contentType, long fileSize, string storagePath)
    {
        var attachment = new DisputeAttachment(Guid.NewGuid(), Id, fileName, contentType, fileSize, storagePath);
        _attachments.Add(attachment);
        UpdatedAt = DateTime.UtcNow;
    }
}
