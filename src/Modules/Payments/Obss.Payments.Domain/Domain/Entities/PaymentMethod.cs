using Obss.Payments.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Payments.Domain.Entities;

public class PaymentMethod : Entity<Guid>
{
    private PaymentMethod() { }

    public PaymentMethod(
        Guid id,
        string tenantId,
        Guid customerId,
        PaymentMethodType methodType,
        string provider,
        string accountNumber,
        DateTime? expiryDate)
        : base(id)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        MethodType = methodType;
        IsDefault = false;
        Provider = provider;
        AccountNumber = MaskAccountNumber(accountNumber);
        ExpiryDate = expiryDate;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public PaymentMethodType MethodType { get; private set; }
    public bool IsDefault { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string AccountNumber { get; private set; } = string.Empty;
    public DateTime? ExpiryDate { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
    }

    public void UnsetDefault()
    {
        IsDefault = false;
    }

    private static string MaskAccountNumber(string accountNumber)
    {
        if (string.IsNullOrEmpty(accountNumber) || accountNumber.Length < 4)
            return accountNumber;

        return new string('*', accountNumber.Length - 4) + accountNumber[^4..];
    }
}
