using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Events;

public sealed class ProductSpecificationCreatedDomainEvent : DomainEvent
{
    public ProductSpecificationCreatedDomainEvent(Guid productSpecificationId, string tenantId, string name)
    {
        ProductSpecificationId = productSpecificationId;
        TenantId = tenantId;
        Name = name;
    }

    public Guid ProductSpecificationId { get; }
    public string TenantId { get; }
    public string Name { get; }
}
