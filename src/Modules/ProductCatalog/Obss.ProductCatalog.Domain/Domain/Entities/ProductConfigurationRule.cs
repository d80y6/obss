using Obss.ProductCatalog.Domain.Domain.Events;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Entities;

public class ProductConfigurationRule : Entity<Guid>
{
    private ProductConfigurationRule() { }

    private ProductConfigurationRule(
        Guid id,
        Guid productId,
        RuleType ruleType,
        Guid? targetProductId,
        string? targetOption,
        string? condition)
        : base(id)
    {
        ProductId = productId;
        RuleType = ruleType;
        TargetProductId = targetProductId;
        TargetOption = targetOption;
        Condition = condition;
        IsActive = true;

        AddDomainEvent(new ProductConfigurationUpdatedDomainEvent(productId, "RuleAdded"));
    }

    public Guid ProductId { get; private set; }
    public RuleType RuleType { get; private set; }
    public Guid? TargetProductId { get; private set; }
    public string? TargetOption { get; private set; }
    public string? Condition { get; private set; }
    public bool IsActive { get; private set; }

    public static ProductConfigurationRule Create(
        Guid productId,
        RuleType ruleType,
        Guid? targetProductId,
        string? targetOption,
        string? condition)
    {
        return new ProductConfigurationRule(
            Guid.NewGuid(),
            productId,
            ruleType,
            targetProductId,
            targetOption,
            condition);
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
}
