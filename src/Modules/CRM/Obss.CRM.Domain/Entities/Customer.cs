using Obss.CRM.Domain.Events;
using Obss.CRM.Domain.Exceptions;
using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.CRM.Domain.Entities;

public class Customer : AggregateRoot<Guid>
{
    private readonly List<CustomerNote> _notes = [];
    private readonly List<Contact> _contacts = [];
    private readonly List<CharValue> _characteristics = [];
    private readonly List<CreditProfile> _creditProfiles = [];
    private readonly List<RelatedParty> _relatedParties = [];
    private readonly List<NotificationHub> _notificationHubs = [];
    private readonly List<ContactMedium> _contactMedia = [];
    private readonly List<AccountRef> _accountRefs = [];
    private readonly List<AgreementRef> _agreementRefs = [];
    private readonly List<PaymentMethodRef> _paymentMethodRefs = [];

    private Customer() { }

    private Customer(
        Guid id,
        string tenantId,
        CustomerType customerType,
        string? companyName,
        string displayName,
        string? taxNumber,
        string? registrationNumber,
        Email email,
        PhoneNumber? phoneNumber,
        string? website,
        string currency)
        : base(id)
    {
        TenantId = tenantId;
        CustomerType = customerType;
        Status = CustomerStatus.Lead;
        CompanyName = companyName;
        DisplayName = displayName;
        TaxNumber = taxNumber;
        RegistrationNumber = registrationNumber;
        Email = email;
        PhoneNumber = phoneNumber;
        Website = website;
        IsActive = true;
        CreditLimit = 0;
        Currency = currency;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new CustomerCreatedDomainEvent(
            string.Empty, TenantId, id, customerType, displayName));
    }

    public string TenantId { get; private set; } = string.Empty;
    public CustomerType CustomerType { get; private set; }
    public CustomerStatus Status { get; private set; }
    public string? CompanyName { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public string? TaxNumber { get; private set; }
    public string? RegistrationNumber { get; private set; }
    public Email Email { get; private set; } = default!;
    public PhoneNumber? PhoneNumber { get; private set; }
    public string? Website { get; private set; }
    public bool IsActive { get; private set; }
    public decimal CreditLimit { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid? IndividualId { get; private set; }
    public Guid? OrganizationId { get; private set; }
    public Individual? Individual { get; private set; }
    public Organization? Organization { get; private set; }
    public string? Description { get; private set; }
    public string? StatusReason { get; private set; }
    public string? ExternalId { get; private set; }
    public string? Href { get; private set; }
    public TimePeriod? ValidFor { get; private set; }

    public IReadOnlyCollection<CustomerNote> Notes => _notes.AsReadOnly();
    public IReadOnlyCollection<Contact> Contacts => _contacts.AsReadOnly();

    public IReadOnlyCollection<CharValue> Characteristics => _characteristics.AsReadOnly();
    public IReadOnlyCollection<CreditProfile> CreditProfiles => _creditProfiles.AsReadOnly();
    public IReadOnlyCollection<RelatedParty> RelatedParties => _relatedParties.AsReadOnly();
    public IReadOnlyCollection<NotificationHub> NotificationHubs => _notificationHubs.AsReadOnly();
    public IReadOnlyCollection<ContactMedium> ContactMedia => _contactMedia.AsReadOnly();
    public IReadOnlyCollection<AccountRef> AccountRefs => _accountRefs.AsReadOnly();
    public IReadOnlyCollection<AgreementRef> AgreementRefs => _agreementRefs.AsReadOnly();
    public IReadOnlyCollection<PaymentMethodRef> PaymentMethodRefs => _paymentMethodRefs.AsReadOnly();

    public static Customer Create(
        string tenantId,
        CustomerType customerType,
        string? companyName,
        string displayName,
        string? taxNumber,
        string? registrationNumber,
        Email email,
        PhoneNumber? phoneNumber,
        string? website,
        string currency)
    {
        return new Customer(
            Guid.NewGuid(),
            tenantId,
            customerType,
            companyName,
            displayName,
            taxNumber,
            registrationNumber,
            email,
            phoneNumber,
            website,
            currency);
    }

    public void Activate()
    {
        if (Status == CustomerStatus.Active)
            return;

        if (Status == CustomerStatus.Terminated)
            throw new InvalidCustomerStateException("Cannot activate a terminated customer.");

        var oldStatus = Status;
        Status = CustomerStatus.Active;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CustomerStatusChangedDomainEvent(Id, oldStatus, Status));
    }

    public void Suspend(string reason)
    {
        if (Status == CustomerStatus.Suspended)
            return;

        if (Status == CustomerStatus.Terminated)
            throw new InvalidCustomerStateException("Cannot suspend a terminated customer.");

        var oldStatus = Status;
        Status = CustomerStatus.Suspended;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CustomerStatusChangedDomainEvent(Id, oldStatus, Status));
        AddDomainEvent(new CustomerSuspendedDomainEvent(Id, reason));
    }

    public void Terminate()
    {
        if (Status == CustomerStatus.Terminated)
            return;

        var oldStatus = Status;
        Status = CustomerStatus.Terminated;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CustomerStatusChangedDomainEvent(Id, oldStatus, Status));
    }

    public void UpdateDetails(
        string? companyName,
        string displayName,
        string? taxNumber,
        string? registrationNumber,
        Email email,
        PhoneNumber? phoneNumber,
        string? website)
    {
        CompanyName = companyName;
        DisplayName = displayName;
        TaxNumber = taxNumber;
        RegistrationNumber = registrationNumber;
        Email = email;
        PhoneNumber = phoneNumber;
        Website = website;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddContact(Contact contact)
    {
        if (_contacts.Any(c => c.Id == contact.Id))
            return;

        _contacts.Add(contact);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveContact(Guid contactId)
    {
        var contact = _contacts.FirstOrDefault(c => c.Id == contactId);
        if (contact is null)
            return;

        _contacts.Remove(contact);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddNote(CustomerNote note)
    {
        _notes.Add(note);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeType(CustomerType newType)
    {
        if (CustomerType == newType)
            return;

        CustomerType = newType;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCreditLimit(decimal creditLimit, string currency)
    {
        if (creditLimit < 0)
            throw new ArgumentException("Credit limit cannot be negative.", nameof(creditLimit));

        CreditLimit = creditLimit;
        Currency = currency;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetEngagedParty(Individual individual)
    {
        Individual = individual;
        IndividualId = individual.Id;
        Organization = null;
        OrganizationId = null;
    }

    public void SetEngagedParty(Organization organization)
    {
        Organization = organization;
        OrganizationId = organization.Id;
        Individual = null;
        IndividualId = null;
    }

    public void UpdateTmfDetails(
        string? description = null,
        string? statusReason = null,
        string? externalId = null,
        string? href = null,
        TimePeriod? validFor = null)
    {
        if (description is not null) Description = description;
        if (statusReason is not null) StatusReason = statusReason;
        if (externalId is not null) ExternalId = externalId;
        if (href is not null) Href = href;
        if (validFor is not null) ValidFor = validFor;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStatus(CustomerStatus newStatus, string? reason = null)
    {
        if (newStatus == Status)
            return;

        Status = newStatus;
        if (reason is not null)
            StatusReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddCharacteristic(CharValue characteristic)
    {
        _characteristics.Add(characteristic);
    }

    public void RemoveCharacteristic(string key)
    {
        _characteristics.RemoveAll(c => c.Key == key);
    }

    public void AddCreditProfile(CreditProfile profile)
    {
        _creditProfiles.Add(profile);
    }

    public void AddRelatedParty(RelatedParty party)
    {
        _relatedParties.Add(party);
    }

    public void RemoveRelatedParty(Guid referredId)
    {
        _relatedParties.RemoveAll(r => r.ReferredId == referredId);
    }

    public void AddNotificationHub(HubType hubType, string identifier, bool isOptIn, DateTime? validFrom, DateTime? validUntil)
    {
        _notificationHubs.Add(new NotificationHub(hubType, identifier, isOptIn, validFrom, validUntil));
    }

    public void RemoveNotificationHub(HubType hubType, string identifier)
    {
        _notificationHubs.RemoveAll(h => h.HubType == hubType && h.Identifier == identifier);
    }

    public void SetNotificationHubOptIn(HubType hubType, string identifier, bool isOptIn)
    {
        var hub = _notificationHubs.FirstOrDefault(h => h.HubType == hubType && h.Identifier == identifier);
        if (hub is null) throw new InvalidOperationException("Notification hub not found.");
        hub.SetOptIn(isOptIn);
    }

    public void AddContactMedium(ContactMediumType mediumType, bool isPreferred, DateTime? validFrom, DateTime? validUntil)
    {
        _contactMedia.Add(new ContactMedium(mediumType, isPreferred, validFrom, validUntil));
    }

    public void RemoveContactMedium(ContactMediumType mediumType)
    {
        _contactMedia.RemoveAll(m => m.MediumType == mediumType);
    }

    public void AddAccountRef(Guid billingAccountId, string name, string accountType, string role, string? href)
        => _accountRefs.Add(new AccountRef(billingAccountId, name, accountType, role, href));

    public void RemoveAccountRef(Guid billingAccountId)
        => _accountRefs.RemoveAll(r => r.BillingAccountId == billingAccountId);

    public void AddAgreementRef(Guid agreementId, string name, string agreementType, string role, string? href)
        => _agreementRefs.Add(new AgreementRef(agreementId, name, agreementType, role, href));

    public void RemoveAgreementRef(Guid agreementId)
        => _agreementRefs.RemoveAll(r => r.AgreementId == agreementId);

    public void AddPaymentMethodRef(Guid paymentMethodId, string name, string? href)
        => _paymentMethodRefs.Add(new PaymentMethodRef(paymentMethodId, name, href));

    public void RemovePaymentMethodRef(Guid paymentMethodId)
        => _paymentMethodRefs.RemoveAll(r => r.PaymentMethodId == paymentMethodId);
}
