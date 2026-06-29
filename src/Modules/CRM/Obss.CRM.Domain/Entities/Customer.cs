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

    public IReadOnlyCollection<CustomerNote> Notes => _notes.AsReadOnly();
    public IReadOnlyCollection<Contact> Contacts => _contacts.AsReadOnly();

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
}
