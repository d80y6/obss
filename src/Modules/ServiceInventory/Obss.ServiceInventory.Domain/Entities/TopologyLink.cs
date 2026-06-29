using Obss.ServiceInventory.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ServiceInventory.Domain.Entities;

public class TopologyLink : Entity<Guid>
{
    private TopologyLink() { }

    private TopologyLink(
        Guid id,
        Guid serviceTopologyId,
        Guid sourceServiceId,
        Guid targetServiceId,
        LinkType linkType,
        Direction direction,
        string? attributes)
        : base(id)
    {
        ServiceTopologyId = serviceTopologyId;
        SourceServiceId = sourceServiceId;
        TargetServiceId = targetServiceId;
        LinkType = linkType;
        Direction = direction;
        Attributes = attributes;
    }

    public Guid ServiceTopologyId { get; private set; }
    public Guid SourceServiceId { get; private set; }
    public Guid TargetServiceId { get; private set; }
    public LinkType LinkType { get; private set; }
    public Direction Direction { get; private set; }
    public string? Attributes { get; private set; }

    public static TopologyLink Create(
        Guid serviceTopologyId,
        Guid sourceServiceId,
        Guid targetServiceId,
        LinkType linkType,
        Direction direction,
        string? attributes = null)
    {
        return new TopologyLink(
            Guid.NewGuid(),
            serviceTopologyId,
            sourceServiceId,
            targetServiceId,
            linkType,
            direction,
            attributes);
    }
}
