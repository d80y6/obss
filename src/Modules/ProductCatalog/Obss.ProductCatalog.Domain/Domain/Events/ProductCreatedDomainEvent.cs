using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Events;

public sealed class ProductCreatedDomainEvent : DomainEvent
{
    public ProductCreatedDomainEvent(Guid productId, string tenantId, string name, string categoryId)
    {
        ProductId = productId;
        TenantId = tenantId;
        Name = name;
        CategoryId = categoryId;
    }

    public Guid ProductId { get; }
    public string TenantId { get; }
    public string Name { get; }
    public string CategoryId { get; }
}
