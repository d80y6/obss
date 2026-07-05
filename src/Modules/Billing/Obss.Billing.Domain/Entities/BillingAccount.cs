using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Billing.Domain.Entities;

public class BillingAccount : AggregateRoot<Guid>
{
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

    public void UpdateDetails(string name, decimal creditLimit, string currency, string? description)
    {
        Name = name;
        CreditLimit = creditLimit;
        Currency = currency;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
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
}
