using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Events;

public sealed class ProductConfigurationUpdatedDomainEvent : DomainEvent
{
    public ProductConfigurationUpdatedDomainEvent(Guid productId, string updateType)
    {
        ProductId = productId;
        UpdateType = updateType;
    }

    public Guid ProductId { get; }
    public string UpdateType { get; }
}
