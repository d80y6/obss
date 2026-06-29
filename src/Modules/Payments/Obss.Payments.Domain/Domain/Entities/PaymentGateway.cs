using Obss.Payments.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Payments.Domain.Entities;

public class PaymentGateway : AggregateRoot<Guid>
{
    private readonly List<string> _supportedCurrencies = [];

    private PaymentGateway() { }

    private PaymentGateway(
        Guid id,
        string tenantId,
        string name,
        PaymentProvider provider,
        string configuration,
        IEnumerable<string> supportedCurrencies,
        decimal? minAmount,
        decimal? maxAmount,
        decimal transactionFee,
        FeeType feeType)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Provider = provider;
        IsActive = true;
        Configuration = configuration;
        _supportedCurrencies = supportedCurrencies?.ToList() ?? [];
        MinAmount = minAmount;
        MaxAmount = maxAmount;
        TransactionFee = transactionFee;
        FeeType = feeType;
        CreatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public PaymentProvider Provider { get; private set; }
    public bool IsActive { get; private set; }
    public string Configuration { get; private set; } = string.Empty;
    public IReadOnlyCollection<string> SupportedCurrencies => _supportedCurrencies.AsReadOnly();
    public decimal? MinAmount { get; private set; }
    public decimal? MaxAmount { get; private set; }
    public decimal TransactionFee { get; private set; }
    public FeeType FeeType { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static PaymentGateway Create(
        string tenantId,
        string name,
        PaymentProvider provider,
        string configuration,
        IEnumerable<string> supportedCurrencies,
        decimal? minAmount,
        decimal? maxAmount,
        decimal transactionFee,
        FeeType feeType)
    {
        return new PaymentGateway(
            Guid.NewGuid(),
            tenantId,
            name,
            provider,
            configuration,
            supportedCurrencies,
            minAmount,
            maxAmount,
            transactionFee,
            feeType);
    }

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

    public void UpdateConfiguration(string configuration)
    {
        Configuration = configuration ?? string.Empty;
    }
}
