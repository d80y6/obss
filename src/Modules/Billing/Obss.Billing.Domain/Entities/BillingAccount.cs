using Obss.Billing.Domain.Events;
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Billing.Domain.Entities;

public class BillingAccount : AggregateRoot<Guid>
{
    private readonly List<RelatedParty> _relatedParties = [];
    private readonly List<BillPresentationMedia> _billPresentationMedia = [];

    private BillingAccount() { }

    public BillingAccount(Guid customerId, AccountType accountType, string name, decimal creditLimit, string currency)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        AccountType = accountType;
        Name = name;
        CreditLimit = creditLimit;
        Currency = currency;
        Status = "Active";
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new BillingAccountCreatedEvent(Id, customerId, accountType.ToString()));
    }

    public Guid CustomerId { get; private set; }
    public AccountType AccountType { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public decimal CreditLimit { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidUntil { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public string? Href { get; private set; }
    public string? AtType { get; private set; } = "BillingAccount";
    public string? AtBaseType { get; private set; } = "PartyAccount";
#pragma warning disable S1144 // Used by EF Core via reflection
    public string? AtSchemaLocation { get; private set; }
#pragma warning restore S1144
#pragma warning disable S1144 // Used by EF Core via reflection
    public string? ExternalId { get; private set; }
#pragma warning restore S1144

    public AccountHolder? AccountHolder { get; private set; }
    public string? PaymentMethodId { get; private set; }
    public IReadOnlyCollection<BillPresentationMedia> BillPresentationMedia => _billPresentationMedia.AsReadOnly();

    public IReadOnlyCollection<RelatedParty> RelatedParties => _relatedParties.AsReadOnly();

    public void UpdateDetails(string name, decimal creditLimit, string currency, string? description)
    {
        Name = name;
        CreditLimit = creditLimit;
        Currency = currency;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new BillingAccountUpdatedEvent(Id, name, Status));
    }

    public void SetStatus(string status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetValidFor(DateTime? validFrom, DateTime? validUntil)
    {
        ValidFrom = validFrom;
        ValidUntil = validUntil;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetHref(string href) => Href = href;

    public void AddRelatedParty(string partyId, string partyName, string role) => _relatedParties.Add(new RelatedParty(partyId, partyName, role));

    public void SetAccountHolder(AccountHolder accountHolder)
    {
        AccountHolder = accountHolder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPaymentMethodId(string? paymentMethodId)
    {
        PaymentMethodId = paymentMethodId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddBillPresentationMedia(BillPresentationMedia media) => _billPresentationMedia.Add(media);

    public void RemoveBillPresentationMedia(Guid mediaId)
    {
        var media = _billPresentationMedia.FirstOrDefault(m => m.Id == mediaId);
        if (media is not null)
            _billPresentationMedia.Remove(media);
    }

    public void MarkDeleted()
    {
        IsActive = false;
        Status = "Deleted";
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new BillingAccountDeletedEvent(Id));
    }

    public void RemoveRelatedParty(RelatedParty party) => _relatedParties.Remove(party);
}
