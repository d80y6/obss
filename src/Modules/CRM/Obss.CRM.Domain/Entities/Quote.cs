using Obss.CRM.Domain.Events;
using Obss.CRM.Domain.Exceptions;
using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.CRM.Domain.Entities;

public class Quote : AggregateRoot<Guid>
{
    private readonly List<QuoteItem> _items = [];
    private readonly List<RelatedParty> _relatedParties = [];
    private readonly List<QuotePrice> _quotePrices = [];
    private readonly List<QuoteAuthorization> _authorizations = [];
    private readonly List<AccountRef> _billingAccountRefs = [];
    private readonly List<AgreementRef> _agreementRefs = [];
    private readonly List<Note> _notes = [];

    private Quote() { }

    private Quote(
        Guid id,
        string tenantId,
        Guid customerId,
        string? externalId,
        string? category,
        string? description,
        DateTime? validFrom,
        DateTime? validUntil,
        DateTime? expectedQuoteCompletionDate,
        DateTime? expectedFulfillmentStartDate)
        : base(id)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        ExternalId = externalId;
        State = QuoteState.InProgress;
        Category = category;
        Description = description;
        Version = 1;
        QuoteDate = DateTime.UtcNow;
        ValidFrom = validFrom;
        ValidUntil = validUntil;
        ExpectedQuoteCompletionDate = expectedQuoteCompletionDate;
        ExpectedFulfillmentStartDate = expectedFulfillmentStartDate;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new QuoteCreatedDomainEvent(id, customerId));
    }

    public string TenantId { get; private set; } = string.Empty;
    public string? ExternalId { get; private set; }
    public QuoteState State { get; private set; }
    public DateTime QuoteDate { get; private set; }
    public string? Category { get; private set; }
    public string? Description { get; private set; }
    public int Version { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidUntil { get; private set; }
    public DateTime? ExpectedQuoteCompletionDate { get; private set; }
    public DateTime? EffectiveQuoteCompletionDate { get; private set; }
    public DateTime? ExpectedFulfillmentStartDate { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<QuoteItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<RelatedParty> RelatedParties => _relatedParties.AsReadOnly();
    public IReadOnlyCollection<QuotePrice> QuotePrices => _quotePrices.AsReadOnly();
    public IReadOnlyCollection<QuoteAuthorization> Authorizations => _authorizations.AsReadOnly();
    public IReadOnlyCollection<AccountRef> BillingAccountRefs => _billingAccountRefs.AsReadOnly();
    public IReadOnlyCollection<AgreementRef> AgreementRefs => _agreementRefs.AsReadOnly();
    public IReadOnlyCollection<Note> Notes => _notes.AsReadOnly();

    public static Quote Create(
        string tenantId,
        Guid customerId,
        string? externalId = null,
        string? category = null,
        string? description = null,
        DateTime? validFrom = null,
        DateTime? validUntil = null,
        DateTime? expectedQuoteCompletionDate = null,
        DateTime? expectedFulfillmentStartDate = null)
    {
        return new Quote(
            Guid.NewGuid(),
            tenantId,
            customerId,
            externalId,
            category,
            description,
            validFrom,
            validUntil,
            expectedQuoteCompletionDate,
            expectedFulfillmentStartDate);
    }

    public void Submit()
    {
        if (State == QuoteState.Pending)
            return;

        if (State != QuoteState.InProgress)
            throw new InvalidQuoteStateException($"Cannot submit a quote in state '{State}'.");

        var oldState = State;
        State = QuoteState.Pending;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new QuoteStateChangedDomainEvent(Id, oldState.ToString(), State.ToString()));
    }

    public void Approve()
    {
        if (State == QuoteState.Approved)
            return;

        if (State != QuoteState.Pending)
            throw new InvalidQuoteStateException($"Cannot approve a quote in state '{State}'.");

        var oldState = State;
        State = QuoteState.Approved;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new QuoteStateChangedDomainEvent(Id, oldState.ToString(), State.ToString()));
    }

    public void Accept()
    {
        if (State == QuoteState.Accepted)
            return;

        if (State != QuoteState.Approved)
            throw new InvalidQuoteStateException($"Cannot accept a quote in state '{State}'.");

        var oldState = State;
        State = QuoteState.Accepted;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new QuoteStateChangedDomainEvent(Id, oldState.ToString(), State.ToString()));
        AddDomainEvent(new QuoteAcceptedDomainEvent(Id, CustomerId));
    }

    public void Reject()
    {
        if (State == QuoteState.Rejected)
            return;

        if (State != QuoteState.InProgress && State != QuoteState.Pending)
            throw new InvalidQuoteStateException($"Cannot reject a quote in state '{State}'.");

        var oldState = State;
        State = QuoteState.Rejected;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new QuoteStateChangedDomainEvent(Id, oldState.ToString(), State.ToString()));
    }

    public void Cancel()
    {
        if (State == QuoteState.Cancelled)
            return;

        if (State == QuoteState.Accepted || State == QuoteState.Rejected)
            throw new InvalidQuoteStateException($"Cannot cancel a quote in state '{State}'.");

        var oldState = State;
        State = QuoteState.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new QuoteStateChangedDomainEvent(Id, oldState.ToString(), State.ToString()));
    }

    public void AddItem(QuoteItem item)
    {
        if (_items.Any(i => i.Id == item.Id))
            return;

        _items.Add(item);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
            return;

        _items.Remove(item);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateItem(Guid itemId, QuoteItemAction action, int quantity, Guid? productOfferingId, string? productOfferingName, Guid? productId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
            return;

        item.UpdateDetails(action, quantity, productOfferingId, productOfferingName, productId);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddRelatedParty(RelatedParty party)
    {
        _relatedParties.Add(party);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveRelatedParty(Guid referredId)
    {
        _relatedParties.RemoveAll(r => r.ReferredId == referredId);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddQuotePrice(QuotePrice price)
    {
        _quotePrices.Add(price);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddAuthorization(QuoteAuthorization authorization)
    {
        _authorizations.Add(authorization);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddBillingAccountRef(AccountRef accountRef)
    {
        _billingAccountRefs.Add(accountRef);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveBillingAccountRef(Guid billingAccountId)
    {
        _billingAccountRefs.RemoveAll(r => r.BillingAccountId == billingAccountId);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddAgreementRef(AgreementRef agreementRef)
    {
        _agreementRefs.Add(agreementRef);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveAgreementRef(Guid agreementId)
    {
        _agreementRefs.RemoveAll(r => r.AgreementId == agreementId);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddNote(Note note)
    {
        _notes.Add(note);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetEffectiveCompletionDate(DateTime? effectiveQuoteCompletionDate)
    {
        EffectiveQuoteCompletionDate = effectiveQuoteCompletionDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string? externalId, string? category, string? description)
    {
        ExternalId = externalId;
        Category = category;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
}
