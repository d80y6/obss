using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Events;

public sealed class CatalogCreatedDomainEvent : DomainEvent
{
    public CatalogCreatedDomainEvent(Guid catalogId, string tenantId, string name)
    {
        CatalogId = catalogId;
        TenantId = tenantId;
        Name = name;
    }

    public Guid CatalogId { get; }
    public string TenantId { get; }
    public string Name { get; }
}
