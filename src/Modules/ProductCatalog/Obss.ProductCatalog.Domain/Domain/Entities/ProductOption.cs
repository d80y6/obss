using Obss.ProductCatalog.Domain.Domain.Events;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Entities;

public class ProductOption : Entity<Guid>
{
    private readonly List<OptionValue> _values = [];

    private ProductOption() { }

    private ProductOption(
        Guid id,
        Guid productId,
        string name,
        string? description,
        OptionType optionType,
        bool isRequired,
        bool isMultiSelect,
        int sortOrder)
        : base(id)
    {
        ProductId = productId;
        Name = name;
        Description = description;
        OptionType = optionType;
        IsRequired = isRequired;
        IsMultiSelect = isMultiSelect;
        SortOrder = sortOrder;
        IsActive = true;

        AddDomainEvent(new ProductConfigurationUpdatedDomainEvent(productId, "OptionAdded"));
    }

    public Guid ProductId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public OptionType OptionType { get; private set; }
    public bool IsRequired { get; private set; }
    public bool IsMultiSelect { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<OptionValue> Values => _values.AsReadOnly();

    public static ProductOption Create(
        Guid productId,
        string name,
        string? description,
        OptionType optionType,
        bool isRequired,
        bool isMultiSelect,
        int sortOrder)
    {
        return new ProductOption(
            Guid.NewGuid(),
            productId,
            name,
            description,
            optionType,
            isRequired,
            isMultiSelect,
            sortOrder);
    }

    public void AddValue(OptionValue value)
    {
        _values.Add(value);
    }

    public void RemoveValue(OptionValue value)
    {
        _values.Remove(value);
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
