using Obss.ServiceInventory.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ServiceInventory.Domain.Entities;

public class ServiceTopology : AggregateRoot<Guid>
{
    private readonly List<TopologyLink> _links = [];

    private ServiceTopology() { }

    private ServiceTopology(
        Guid id,
        Guid serviceId,
        TopologyType topologyType)
        : base(id)
    {
        ServiceId = serviceId;
        TopologyType = topologyType;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid ServiceId { get; private set; }
    public TopologyType TopologyType { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<TopologyLink> Links => _links.AsReadOnly();

    public static ServiceTopology Create(Guid serviceId, TopologyType topologyType)
    {
        return new ServiceTopology(Guid.NewGuid(), serviceId, topologyType);
    }

    public void AddLink(TopologyLink link)
    {
        _links.Add(link);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveLink(Guid linkId)
    {
        var link = _links.FirstOrDefault(l => l.Id == linkId);
        if (link is not null)
        {
            _links.Remove(link);
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
